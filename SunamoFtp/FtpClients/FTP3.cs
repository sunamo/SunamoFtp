namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    /// <summary>
    /// Uploads a file to the FTP server, with optional resume capability.
    /// Logs in if not authenticated, sends PASV command, and creates data socket.
    /// If resuming, sets binary mode and gets remote file size to determine offset.
    /// Sends REST command with offset if resuming, then STOR command with file name.
    /// Reads bytes from file and sends them via socket, then Closes socket and verifies server response.
    /// </summary>
    /// <param name="fileName">The name of the file to upload</param>
    /// <param name="resume">Whether to resume a previous upload from the last position</param>
    /// <param name="buffer">The byte buffer to use for reading the file</param>
    public void Upload(string fileName, bool resume, byte[] buffer)
    {
        OnNewStatus("Uploading" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
#region Tento kód mi nedovolil často nauploadovat ani jeden soubor, takže ho nahradím speciálními třídami .net
#region Pokud nejsem nalogovaný, přihlásím se.
        if (!IsLoggedIn)
            Login();
        SendCommand("PASV");
#endregion
#region Pokud mám navazovat, zjistím si veliksot vzdáleného souboru.
        var clientSocket = CreateDataSocket();
        long offset = 0;
        isUpload = true;
        if (resume)
            try
            {
                SetBinaryMode(true);
                offset = GetFileSize(fileName);
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

        SendCommand("STOR" + " " + Path.GetFileName(fileName));
        if (!(retValue == 125 || retValue == 150))
            throw new Exception(reply.Substring(4));
#endregion
#region Pokud byl offset, seeknu se v souboru a čtu bajty a zapisuji je to server metodou clientSocket.Send
        // open input stream to read source file
        var input = new FileStream(fileName, FileMode.Open);
        if (offset != 0)
        {
            if (isDebug)
                OnNewStatus("seeking to" + " " + offset);
            input.Seek(offset, SeekOrigin.Begin);
        }

        OnNewStatus("Uploading file" + " " + fileName + " to " + remotePath);
        while ((bytes = input.Read(buffer, 0, buffer.Length)) > 0)
            clientSocket.Send(buffer, bytes, 0);
        input.Close();
#endregion
#region Pokud jsem připojený, zavřu objekt clientSocket a zavřu návratovou hodnotu
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
    /// Deletes a remote file from the FTP server.
    /// Logs in if not authenticated, then sends DELE command with the file name.
    /// If the first attempt fails, tries again with URL-decoded file name.
    /// </summary>
    /// <param name="fileName">The name of the file to delete</param>
    /// <returns>Always returns true (throws exception on failure)</returns>
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
    /// Renames a file on the FTP server.
    /// Sends RNFR command with old file name, waits for 350 response, then sends RNTO command with new file name.
    /// Logs in if not authenticated before executing the rename operation.
    /// </summary>
    /// <param name="oldFileName">The current name of the file</param>
    /// <param name="newFileName">The new name for the file</param>
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
    /// Creates a directory in the current folder on the FTP server.
    /// Sends MKD command with directory name, then changes to the new directory.
    /// Logs in if not authenticated before creating the directory.
    /// </summary>
    /// <param name="dirName">The name of the directory to create</param>
    /// <returns>Always returns true (throws exception on failure)</returns>
    public override bool Mkdir(string dirName)
    {
        OnNewStatus("Creating directory" + " " + UH.Combine(true, PathSelector.ActualPath, dirName));
        if (!IsLoggedIn)
            Login();
        SendCommand("MKD " + dirName);
        if (retValue != 250 && retValue != 257)
            throw new Exception(reply.Substring(4));
        ChdirLite(dirName);
        return true;
    }

    /// <summary>
    /// Removes a directory from the current folder on the FTP server.
    /// Sends RMD command with directory name. If the directory is not empty (error 550), deletes it recursively.
    /// Logs in if not authenticated before removing the directory.
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during recursive deletion</param>
    /// <param name="dirName">The name of the directory to remove</param>
    /// <returns>Always returns true (throws exception on failure)</returns>
    public override bool Rmdir(List<string> foldersToSkip, string dirName)
    {
        OnNewStatus("Deleting directory" + " " + UH.Combine(true, PathSelector.ActualPath, dirName));
        if (!IsLoggedIn)
            Login();
        SendCommand("RMD " + dirName);
        if (retValue != 250)
        {
            if (retValue == 550)
                DeleteRecursively(foldersToSkip, dirName, 0, new List<DirectoriesToDeleteFtp>());
            else
                throw new Exception(reply.Substring(4));
        }

        return true;
    }

    /// <summary>
    /// Changes to the specified directory, creating it if it doesn't exist.
    /// Skips "." and ".." directory references.
    /// If the directory doesn't exist, creates it with mkdir, otherwise changes to it with ChdirLite.
    /// </summary>
    /// <param name="dirName">The name of the directory to change to or create</param>
    public override void CreateDirectoryIfNotExists(string dirName)
    {
        if (dirName == "." || dirName == "..")
            return;
        if (!ExistsFolder(dirName))
            Mkdir(dirName);
        else
            ChdirLite(dirName);
    //PathSelector.AddToken(dirName);
    }

    /// <summary>
    /// Changes the current directory on the FTP server, creating it if necessary.
    /// Removes trailing slash from directory name if present.
    /// Lists directory contents to verify the directory exists before changing to it.
    /// If directory doesn't exist, creates it with mkdir.
    /// Updates the PathSelector when changing directories.
    /// </summary>
    /// <param name="dirName">The name of the directory to change to. Empty string changes to www root.</param>
    public override void ChdirLite(string dirName)
    {
        if (!IsLoggedIn)
            Login();
        if (dirName != "")
        {
            if (dirName[dirName.Length - 1] == "/"[0])
                dirName = dirName.Substring(0, dirName.Length - 1);
        }
        else
        {
            dirName = ftpClient.Www;
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
            string fileName = null;
            if (FtpHelper.IsFile(item, out fileName) == FileSystemType.Folder)
                if (fileName == dirName)
                {
                    directoryFound = true;
                    break;
                }
        }

        if (!directoryFound)
        {
            if (Mkdir(dirName))
            {
            //this.remotePath = dirName;
            }
        }
        else
        {
            SendCommand("CWD " + dirName);
            if (retValue != 250)
                throw new Exception(reply.Substring(4));
            if (dirName == "..")
                PathSelector.RemoveLastToken();
            else
                PathSelector.AddToken(dirName);
        }
    }

    /// <summary>
    /// Closes the FTP connection and cleans up resources.
    /// Sends QUIT command if client socket is not null.
    /// Closes and nullifies the client socket, and sets IsLoggedIn to false.
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
    /// Sets the isDebug mode for the FTP client.
    /// When enabled, outputs detailed command and response information.
    /// </summary>
    /// <param name="isDebug">True to enable isDebug mode, false to disable</param>
    public void SetDebug(bool isDebug)
    {
        this.isDebug = isDebug;
    }

    /// <summary>
    /// Reads reply using ResponseMsg when using Stream or ReadLine
    /// </summary>
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

    /// <summary>
    /// Zavřu, nulluji clientSocket a nastavím IsLoggedIn to false.
    /// </summary>
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