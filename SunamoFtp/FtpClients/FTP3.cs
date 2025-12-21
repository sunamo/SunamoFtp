namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FTP : FtpBase
{
    /// <summary>
    /// Pokud nejsem nalogovaný, přihlásím se.
    /// Pokud mám navazovat, zjistím si veliksot vzdáleného souboru.
    /// Pošlu příkaz REST text offsetem a poté už STOR
    /// Pokud byl offset, seeknu se v souboru a čtu bajty a zapisuji je na server metodou cSocket.Send
    /// Pokud jsem připojený, zavřu objekt cSocket a zavřu návratovou hodnotu
    /// </summary>
    /// <param name = "fileName"></param>
    /// <param name = "resume"></param>
    public void upload(string fileName, bool resume, byte[] buffer)
    {
        OnNewStatus("Uploaduji" + " " + UH.Combine(false, ps.ActualPath, fileName));
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
    /// Odstraním vzdálený soubor jména A1.
    /// </summary>
    /// <param name = "fileName"></param>
    public override bool deleteRemoteFile(string fileName)
    {
        OnNewStatus("Odstraňuji ze ftp serveru soubor" + " " + UH.Combine(false, ps.ActualPath, fileName));
        if (!logined)
            login();
        sendCommand("DELE" + " " + fileName);
        if (retValue != 250)
            sendCommand("DELE" + " " + WebUtility.UrlDecode(fileName));
        return true;
    }

    /// <summary>
    /// Pošlu příkaz RNFR A1 a když bude odpoveď 350, tak RNTO
    /// </summary>
    /// <param name = "oldFileName"></param>
    /// <param name = "newFileName"></param>
    public override void renameRemoteFile(string oldFileName, string newFileName)
    {
        OnNewStatus("Ve složce" + " " + ps.ActualPath + " " + "přejmenovávám soubor" + " " + oldFileName + " na " + newFileName);
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
    /// Vytvoří v akt. složce A1 adresář A1 příkazem MKD
    /// </summary>
    /// <param name = "dirName"></param>
    public override bool mkdir(string dirName)
    {
        OnNewStatus("Vytvářím adresář" + " " + UH.Combine(true, ps.ActualPath, dirName));
        if (!logined)
            login();
        sendCommand("MKD " + dirName);
        if (retValue != 250 && retValue != 257)
            throw new Exception(reply.Substring(4));
        chdirLite(dirName);
        return true;
    }

    /// <summary>
    /// Smaže v akt. složce adr. A1 příkazem RMD
    /// </summary>
    /// <param name = "dirName"></param>
    public override bool rmdir(List<string> slozkyNeuploadovatAVS, string dirName)
    {
        OnNewStatus("Mažu adresář" + " " + UH.Combine(true, ps.ActualPath, dirName));
        if (!logined)
            login();
        sendCommand("RMD " + dirName);
        if (retValue != 250)
        {
            if (retValue == 550)
                DeleteRecursively(slozkyNeuploadovatAVS, dirName, 0, new List<DirectoriesToDeleteFtp>());
            else
                throw new Exception(reply.Substring(4));
        }

        return true;
    }

    /// <summary>
    /// Změním akt. adresář na A1 a text remotePath A1 příkazem CWD
    /// </summary>
    /// <param name = "dirName"></param>
    public override void CreateDirectoryIfNotExists(string dirName)
    {
        if (dirName == "." || dirName == "..")
            return;
        if (!ExistsFolder(dirName))
            mkdir(dirName);
        else
            chdirLite(dirName);
    //ps.AddToken(dirName);
    }

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
        List<string> fse = null;
        var vseMa8 = false;
        while (!vseMa8)
        {
            vseMa8 = true;
            fse = ListDirectoryDetails();
            foreach (var item in fse)
            {
                var tokens = item.Split(' ').Length; //SHSplit.Split(item, "").Count;
                if (tokens < 8)
                    vseMa8 = false;
            }
        }

        foreach (var item in fse)
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
                ps.RemoveLastToken();
            else
                ps.AddToken(dirName);
        }
    }

    /// <summary>
    /// Pošlu příkaz QUIT pokud clientSocket není null
    /// Zavřu, nulluji clientSocket a nastavím logined na false.
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
    /// Nastavím debugovací mod
    /// </summary>
    /// <param name = "debug"></param>
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