namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    /// <summary>
    /// Upload.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="resume">The resume.</param>
    /// <param name="buffer">The buffer.</param>
    public void Upload(string filePath, bool resume, byte[] buffer)
    {
        OnNewStatus("Uploading" + " " + UH.Combine(false, PathSelector.ActualPath, filePath));
#region This code did not let me upload even a single file often, so I replace it with special .net classes
#region If I am not logged in, I log in.
        if (!IsLoggedIn)
            Login();
        SendCommand("PASV");
#endregion
#region If I am to resume, I find out the size of the remote file.
        var clientSocket = CreateDataSocket();
        long offset = 0;
        isUpload = true;
        if (resume)
            try
            {
                SetBinaryMode(true);
                offset = GetFileSize(filePath);
            }
            catch (Exception)
            {
                offset = 0;
            }

#endregion
#region Pošlu příkaz REST text offsetem a poté už STOR
        if (offset > 0)
        {
            SendCommand("REST" + " " + offset);
            if (retValue != 350)
                offset = 0;
        }

        SendCommand("STOR" + " " + Path.GetFileName(filePath));
        if (!(retValue == 125 || retValue == 150))
            throw new Exception(reply.Substring(4));
#endregion
#region If there was an offset, I seek in the file, read the bytes and write them to the server using clientSocket.Send
        // open input stream to read source file
        var input = new FileStream(filePath, FileMode.Open);
        if (offset != 0)
        {
            if (isDebug)
                OnNewStatus("seeking to" + " " + offset);
            input.Seek(offset, SeekOrigin.Begin);
        }

        OnNewStatus("Uploading file" + " " + filePath + " to " + remotePath);
        while ((bytes = input.Read(buffer, 0, buffer.Length)) > 0)
            clientSocket.Send(buffer, bytes, 0);
        input.Close();
#endregion
#region If I am connected, I close the clientSocket object and close the return value
        if (clientSocket.Connected)
            clientSocket.Close();
        ReadReply();
        if (!(retValue == 226 || retValue == 250))
            throw new Exception(reply.Substring(4));
#endregion
#endregion
#region MyRegion
#endregion
    }

    /// <summary>
    /// Delete remote file.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The delete remote file result.</returns>
    public override bool DeleteRemoteFile(string fileName)
    {
        OnNewStatus("Deleting file from FTP server" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
        if (!IsLoggedIn)
            Login();
        SendCommand("DELE" + " " + fileName);
        if (retValue != 250)
            SendCommand("DELE" + " " + WebUtility.UrlDecode(fileName));
        return true;
    }

    /// <summary>
    /// Rename remote file.
    /// </summary>
    /// <param name="oldFileName">The old file name.</param>
    /// <param name="newFileName">The new file name.</param>
    public override void RenameRemoteFile(string oldFileName, string newFileName)
    {
        OnNewStatus("In folder" + " " + PathSelector.ActualPath + " " + "renaming file" + " " + oldFileName + " to " + newFileName);
        if (!IsLoggedIn)
            Login();
        SendCommand("RNFR" + " " + oldFileName);
        if (retValue != 350)
            throw new Exception(reply.Substring(4));
        SendCommand("RNTO" + " " + newFileName);
        if (retValue != 250)
            throw new Exception(reply.Substring(4));
    }

    /// <summary>
    /// Mkdir.
    /// </summary>
    /// <param name="directoryName">The directory name.</param>
    /// <returns>The mkdir result.</returns>
    public override bool Mkdir(string directoryName)
    {
        OnNewStatus("Creating directory" + " " + UH.Combine(true, PathSelector.ActualPath, directoryName));
        if (!IsLoggedIn)
            Login();
        SendCommand("MKD " + directoryName);
        if (retValue != 250 && retValue != 257)
            throw new Exception(reply.Substring(4));
        ChdirLite(directoryName);
        return true;
    }

    /// <summary>
    /// Rmdir.
    /// </summary>
    /// <param name="foldersToSkip">The folders to skip.</param>
    /// <param name="directoryName">The directory name.</param>
    /// <returns>The rmdir result.</returns>
    public override bool Rmdir(List<string> foldersToSkip, string directoryName)
    {
        OnNewStatus("Deleting directory" + " " + UH.Combine(true, PathSelector.ActualPath, directoryName));
        if (!IsLoggedIn)
            Login();
        SendCommand("RMD " + directoryName);
        if (retValue != 250)
        {
            if (retValue == 550)
                DeleteRecursively(foldersToSkip, directoryName, 0, new List<DirectoriesToDeleteFtp>());
            else
                throw new Exception(reply.Substring(4));
        }

        return true;
    }

    /// <summary>
    /// Create directory if not exists.
    /// </summary>
    /// <param name="directoryName">The directory name.</param>
    public override void CreateDirectoryIfNotExists(string directoryName)
    {
        if (directoryName == "." || directoryName == "..")
            return;
        if (!ExistsFolder(directoryName))
            Mkdir(directoryName);
        else
            ChdirLite(directoryName);
    }

    /// <summary>
    /// Chdir lite.
    /// </summary>
    /// <param name="directoryName">The directory name.</param>
    public override void ChdirLite(string directoryName)
    {
        if (!IsLoggedIn)
            Login();
        if (directoryName != "")
        {
            if (directoryName[directoryName.Length - 1] == "/"[0])
                directoryName = directoryName.Substring(0, directoryName.Length - 1);
        }
        else
        {
            directoryName = ftpClient.Www;
        }

        var directoryFound = false;
        List<string> ftpEntries = null;
        var allHaveEightTokens = false;
        while (!allHaveEightTokens)
        {
            allHaveEightTokens = true;
            ftpEntries = ListDirectoryDetails();
            foreach (var item in ftpEntries)
            {
                var tokens = item.Split(' ').Length; //SHSplit.Split(item, "").Count;
                if (tokens < 8)
                    allHaveEightTokens = false;
            }
        }

        foreach (var item in ftpEntries)
        {
            if (FtpHelper.IsFile(item, out var fileName) == FileSystemType.Folder)
                if (fileName == directoryName)
                {
                    directoryFound = true;
                    break;
                }
        }

        if (!directoryFound)
        {
            if (Mkdir(directoryName))
            {
            }
        }
        else
        {
            SendCommand("CWD " + directoryName);
            if (retValue != 250)
                throw new Exception(reply.Substring(4));
            if (directoryName == "..")
                PathSelector.RemoveLastToken();
            else
                PathSelector.AddToken(directoryName);
        }
    }

    /// <summary>
    /// Close.
    /// </summary>
    public void Close()
    {
        OnNewStatus("Closing FTP session");
        if (clientSocket != null)
            SendCommand("QUIT");
        Cleanup();
        OnNewStatus("Closing" + "." + "..");
    }

    /// <summary>
    /// Set debug.
    /// </summary>
    /// <param name="isDebug">The is debug.</param>
    public void SetDebug(bool isDebug)
    {
        this.isDebug = isDebug;
    }

    private void ReadReply()
    {
        if (useStream)
        {
            reply = ResponseMsg();
        }
        else
        {
            message = "";
            reply = ReadLine();
            retValue = int.Parse(reply.Substring(0, 3));
        }
    }

    // I close and null clientSocket and set IsLoggedIn to false.
    private void Cleanup()
    {
        if (clientSocket != null)
        {
            clientSocket.Close();
            clientSocket = null;
        }

        IsLoggedIn = false;
    }
}
