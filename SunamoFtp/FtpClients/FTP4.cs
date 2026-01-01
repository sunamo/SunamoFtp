namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FTP : FtpBase
{
    /// <summary>
    /// Writes a message to the stream.
    /// Converts the message to ASCII bytes and writes them to the stream.
    /// </summary>
    /// <param name="message">The message to write to the stream</param>
    private void WriteMsg(string message)
    {
        var en = new ASCIIEncoding();
        var WriteBuffer = new byte[1024];
        WriteBuffer = en.GetBytes(message);
        stream.Write(WriteBuffer, 0, WriteBuffer.Length);
    //NewStatus(" WRITE:" + message);
    }

    /// <summary>
    /// Reads a response message from the FTP server stream.
    /// Reads all bytes from the stream until newline character is encountered.
    /// Stores the response code in retValue and returns the entire output.
    /// </summary>
    /// <returns>The complete response message from the server</returns>
    private string ResponseMsg()
    {
        var enc = new ASCIIEncoding();
        var serverbuff = new byte[1024];
        var count = 0;
        while (true)
        {
            var buff = new byte[2];
            var bytes = stream.Read(buff, 0, 1);
            if (bytes == 1)
            {
                serverbuff[count] = buff[0];
                count++;
                if (buff[0] == '\n')
                    break;
            }
            else
            {
                break;
            };
        };
        var retval = enc.GetString(serverbuff, 0, count);
        //NewStatus(" READ:" + retval);
        retValue = int.Parse(retval.Substring(0, 3));
        return retval;
    }

    /// <summary>
    /// Sends a command to the FTP server.
    /// Converts the command to ASCII bytes, sends it via stream or socket, and reads the server's reply.
    /// Stores the response in reply and retValue properties.
    /// </summary>
    /// <param name="command">The FTP command to send (without CRLF terminator)</param>
    public void sendCommand(string command)
    {
#region Původní metoda sendCommand
        var cmdBytes = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());
        if (useStream)
            WriteMsg(command + "\r\n");
        else
            clientSocket.Send(cmdBytes, cmdBytes.Length, 0);
        readReply();
#endregion
    }

    /// <summary>
    /// Sends a command to the FTP server (alternate implementation).
    /// Identical to sendCommand - converts command to ASCII bytes, sends via stream or socket, and reads reply.
    /// Stores the response in reply and retValue properties.
    /// </summary>
    /// <param name="command">The FTP command to send (without CRLF terminator)</param>
    private void sendCommand2(string command)
    {
#region Původní metoda sendCommand
        var cmdBytes = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());
        if (useStream)
            WriteMsg(command + "\r\n");
        else
            clientSocket.Send(cmdBytes, cmdBytes.Length, 0);
        readReply();
#endregion
    }

    /// <summary>
    /// Creates a data socket for passive mode FTP transfer.
    /// Sends PASV command, parses the IP address and port from the server's reply.
    /// Extracts the IP address parts and joins them with dots.
    /// Calculates the port by bit-shifting the 5th part by 8 and adding the 6th part.
    /// Creates a Socket, IPEndPoint and attempts to connect to the server.
    /// </summary>
    /// <returns>A connected socket ready for data transfer</returns>
    public Socket createDataSocket()
    {
#region Nastavím pasivní způsob přenosu(příkaz PASV)
        sendCommand("PASV");
        if (retValue != 227)
            throw new Exception(reply.Substring(4));
#endregion
#region Získám IP adresu v řetězci z reply
        var index1 = reply.IndexOf('(');
        var index2 = reply.IndexOf(')');
        var ipData = reply.Substring(index1 + 1, index2 - index1 - 1);
        var parts = new int[6];
        var len = ipData.Length;
        var partCount = 0;
        var buf = "";
#endregion
#region Získám do pole intů jednotlivé části IP adresy a spojím je do řetězce text tečkama
        for (var i = 0; i < len && partCount <= 6; i++)
        {
            var ch = char.Parse(ipData.Substring(i, 1));
            if (char.IsDigit(ch))
                buf += ch;
            else if (ch != ',')
                throw new Exception("Malformed PASV reply" + ": " + reply);
#region Pokud je poslední znak čárka,
            if (ch == ',' || i + 1 == len)
                try
                {
                    parts[partCount++] = int.Parse(buf);
                    buf = "";
                }
                catch (Exception ex)
                {
                    throw new Exception("Malformed PASV reply" + ": " + reply);
                }
#endregion
        }

        var ipAddress = parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3];
#endregion
#region Port získám tak čtvrtou část ip adresy bitově posunu o 8 a sečtu text pátou částí. Získám Socket, O IPEndPoint a pokusím se připojit na tento objekt.
        var port = (parts[4] << 8) + parts[5];
        var text = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var ep = new IPEndPoint(Dns.Resolve(ipAddress).AddressList[0], port);
        try
        {
            text.Connect(ep);
        }
        catch (Exception ex)
        {
            throw new Exception("Can't connect to remoteserver");
        }

        return text;
#endregion
    }

    /// <summary>
    /// Placeholder method for uploading a folder securely.
    /// Currently empty - not implemented. Should verify that _.txt file is uploaded first.
    /// </summary>
    public void uploadSecureFolder()
    {
        OnNewStatus("Byla volána metoda uploadSecureFolder která je prázdná");
    // Zkontrolovat zda se první nauploadoval _.txt
    }

    /// <summary>
    /// Recursively deletes a directory and all its contents from the FTP server.
    /// Changes to the directory, lists all contents, deletes files and recursively deletes subdirectories.
    /// After all contents are deleted, goes to parent folder and removes the now-empty directory.
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during deletion</param>
    /// <param name="dirName">The name of the directory to delete</param>
    /// <param name="i">Recursion depth level (currently unused)</param>
    /// <param name="td">List of directories to delete (currently unused)</param>
    public override void DeleteRecursively(List<string> foldersToSkip, string dirName, int i, List<DirectoriesToDeleteFtp> td)
    {
        chdirLite(dirName);
        var toDelete = ListDirectoryDetails();
        foreach (var item2 in toDelete)
        {
            var fst = FtpHelper.IsFile(item2, out var fn);
            if (fst == FileSystemType.File)
                deleteRemoteFile(fn);
            else if (fst == FileSystemType.Folder)
                DeleteRecursively(foldersToSkip, fn, i, td);
        //////DebugLogger.Instance.WriteLine(item2);
        }

        goToUpFolderForce();
        rmdir(foldersToSkip, dirName);
    }

    /// <summary>
    /// Outputs debug information about the current folder.
    /// Not implemented - throws NotImplementedMethod exception.
    /// </summary>
    public override void DebugActualFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Debug output method.
    /// Not implemented - throws NotImplementedMethod exception.
    /// </summary>
    /// <param name="what">What to debug</param>
    /// <param name="text">Format string for output</param>
    /// <param name="args">Arguments for the format string</param>
    public override void D(string what, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Connects to the FTP server.
    /// Not implemented - throws NotImplementedMethod exception.
    /// </summary>
    public override void Connect()
    {
        ThrowEx.NotImplementedMethod();
    }
}