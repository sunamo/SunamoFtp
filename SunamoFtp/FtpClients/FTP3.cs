namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    public void Upload(string filePath, bool resume, byte[] buffer)
    {
        OnNewStatus("Uploading" + " " + UH.Combine(false, PathSelector.ActualPath, filePath));
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
#region Pokud byl offset, seeknu se v souboru a čtu bajty a zapisuji je to server metodou clientSocket.Send
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

    public override void CreateDirectoryIfNotExists(string directoryName)
    {
        if (directoryName == "." || directoryName == "..")
            return;
        if (!ExistsFolder(directoryName))
            Mkdir(directoryName);
        else
            ChdirLite(directoryName);
    //PathSelector.AddToken(directoryName);
    }

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
            //this.remotePath = directoryName;
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

    public void Close()
    {
        OnNewStatus("Closing FTP session");
        if (clientSocket != null)
            SendCommand("QUIT");
        Cleanup();
        OnNewStatus("Closing" + "." + "..");
    }

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

    // Zavřu, nulluji clientSocket a nastavím IsLoggedIn to false.
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
