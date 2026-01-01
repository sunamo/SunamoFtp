namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FTP : FtpBase
{
    /// <summary>
    /// Uploads a file to the FTP server, with optional resume capability.
    /// Logs in if not authenticated, sends PASV command, and creates data socket.
    /// If resuming, sets binary mode and gets remote file size to determine offset.
    /// Sends REST command with offset if resuming, then STOR command with file name.
    /// Reads bytes from file and sends them via socket, then closes socket and verifies server response.
    /// </summary>
    /// <param name="fileName">The name of the file to upload</param>
    /// <param name="resume">Whether to resume a previous upload from the last position</param>
    /// <param name="buffer">The byte buffer to use for reading the file</param>
    public void upload(string fileName, bool resume, byte[] buffer)
    {
        OnNewStatus("Uploaduji" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
#region Tento kód mi nedovolil často nauploadovat ani jeden soubor, takže ho nahradím speciálními třídami .net
#region Pokud nejsem nalogovaný, přihlásím se.
        if (!logined)
            login();
        sendCommand("PASV");
#endregion
#region Pokud mám navazovat, zjistím si veliksot vzdáleného souboru.
        var cSocket = createDataSocket();
        long offset = 0;
        isUpload = true;
        if (resume)
            try
            {
                setBinaryMode(true);
                offset = getFileSize(fileName);
            }
            catch (Exception ex)
            {
                offset = 0;
            }

#endregion
#region Pošlu příkaz REST text offsetem a poté už STOR
        if (offset > 0)
        {
            sendCommand("REST" + " " + offset);
            if (retValue != 350)
                offset = 0;
        }

        sendCommand("STOR" + " " + Path.GetFileName(fileName));
        if (!(retValue == 125 || retValue == 150))
            throw new Exception(reply.Substring(4));
#endregion
#region Pokud byl offset, seeknu se v souboru a čtu bajty a zapisuji je na server metodou cSocket.Send
        // open input stream to read source file
        var input = new FileStream(fileName, FileMode.Open);
        if (offset != 0)
        {
            if (debug)
                OnNewStatus("seeking to" + " " + offset);
            input.Seek(offset, SeekOrigin.Begin);
        }

        OnNewStatus("Uploading file" + " " + fileName + " to " + remotePath);
        while ((bytes = input.Read(buffer, 0, buffer.Length)) > 0)
            cSocket.Send(buffer, bytes, 0);
        input.Close();
#endregion
#region Pokud jsem připojený, zavřu objekt cSocket a zavřu návratovou hodnotu
        if (cSocket.Connected)
            cSocket.Close();
        readReply();
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
    public override bool deleteRemoteFile(string fileName)
    {
        OnNewStatus("Odstraňuji ze ftp serveru soubor" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
        if (!logined)
            login();
        sendCommand("DELE" + " " + fileName);
        if (retValue != 250)
            sendCommand("DELE" + " " + WebUtility.UrlDecode(fileName));
        return true;
    }

    /// <summary>
    /// Renames a file on the FTP server.
    /// Sends RNFR command with old file name, waits for 350 response, then sends RNTO command with new file name.
    /// Logs in if not authenticated before executing the rename operation.
    /// </summary>
    /// <param name="oldFileName">The current name of the file</param>
    /// <param name="newFileName">The new name for the file</param>
    public override void renameRemoteFile(string oldFileName, string newFileName)
    {
        OnNewStatus("Ve složce" + " " + PathSelector.ActualPath + " " + "přejmenovávám soubor" + " " + oldFileName + " na " + newFileName);
        if (!logined)
            login();
        sendCommand("RNFR" + " " + oldFileName);
        if (retValue != 350)
            throw new Exception(reply.Substring(4));
        sendCommand("RNTO" + " " + newFileName);
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
    public override bool mkdir(string dirName)
    {
        OnNewStatus("Vytvářím adresář" + " " + UH.Combine(true, PathSelector.ActualPath, dirName));
        if (!logined)
            login();
        sendCommand("MKD " + dirName);
        if (retValue != 250 && retValue != 257)
            throw new Exception(reply.Substring(4));
        chdirLite(dirName);
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
    public override bool rmdir(List<string> foldersToSkip, string dirName)
    {
        OnNewStatus("Mažu adresář" + " " + UH.Combine(true, PathSelector.ActualPath, dirName));
        if (!logined)
            login();
        sendCommand("RMD " + dirName);
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
    /// If the directory doesn't exist, creates it with mkdir, otherwise changes to it with chdirLite.
    /// </summary>
    /// <param name="dirName">The name of the directory to change to or create</param>
    public override void CreateDirectoryIfNotExists(string dirName)
    {
        if (dirName == "." || dirName == "..")
            return;
        if (!ExistsFolder(dirName))
            mkdir(dirName);
        else
            chdirLite(dirName);
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
    public override void chdirLite(string dirName)
    {
        if (!logined)
            login();
        if (dirName != "")
        {
            if (dirName[dirName.Length - 1] == "/"[0])
                dirName = dirName.Substring(0, dirName.Length - 1);
        }
        else
        {
            dirName = ftpClient.Www;
        }

        var nalezenAdresar = false;
        List<string> ftpEntries = null;
        var vseMa8 = false;
        while (!vseMa8)
        {
            vseMa8 = true;
            ftpEntries = ListDirectoryDetails();
            foreach (var item in ftpEntries)
            {
                var tokens = item.Split(' ').Length; //SHSplit.Split(item, "").Count;
                if (tokens < 8)
                    vseMa8 = false;
            }
        }

        foreach (var item in ftpEntries)
        {
            string fn = null;
            if (FtpHelper.IsFile(item, out fn) == FileSystemType.Folder)
                if (fn == dirName)
                {
                    nalezenAdresar = true;
                    break;
                }
        }

        if (!nalezenAdresar)
        {
            if (mkdir(dirName))
            {
            //this.remotePath = dirName;
            }
        }
        else
        {
            sendCommand("CWD " + dirName);
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
    /// Closes and nullifies the client socket, and sets logined to false.
    /// </summary>
    public void close()
    {
        OnNewStatus("Uzavírám ftp relaci");
        if (clientSocket != null)
            sendCommand("QUIT");
        cleanup();
        OnNewStatus("Closing" + "." + "..");
    }

    /// <summary>
    /// Sets the debug mode for the FTP client.
    /// When enabled, outputs detailed command and response information.
    /// </summary>
    /// <param name="debug">True to enable debug mode, false to disable</param>
    public void setDebug(bool debug)
    {
        this.debug = debug;
    }

    /// <summary>
    /// Přečtu do PP reply M ResponseMsg když používám Stream nebo readLine
    /// </summary>
    private void readReply()
    {
        if (useStream)
        {
            reply = ResponseMsg();
        }
        else
        {
            mes = "";
            reply = readLine();
            retValue = int.Parse(reply.Substring(0, 3));
        }
    }

    /// <summary>
    /// Zavřu, nulluji clientSocket a nastavím logined na false.
    /// </summary>
    private void cleanup()
    {
        if (clientSocket != null)
        {
            clientSocket.Close();
            clientSocket = null;
        }

        logined = false;
    }
}