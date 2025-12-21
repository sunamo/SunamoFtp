namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FTP : FtpBase
{
    /// <summary>
    /// Zapíšu do PP stream A1.
    /// </summary>
    /// <param name = "message"></param>
    private void WriteMsg(string message)
    {
        var en = new ASCIIEncoding();
        var WriteBuffer = new byte[1024];
        WriteBuffer = en.GetBytes(message);
        stream.Write(WriteBuffer, 0, WriteBuffer.Length);
    //NewStatus(" WRITE:" + message);
    }

    /// <summary>
    /// Přečtu všechny bajty z PP stream.
    /// Uložím do retValue a G celý výstup.
    /// </summary>
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
    /// Získám bajty z A1, pošlu odpověď a uložím PP reply a retValue
    /// </summary>
    /// <param name = "command"></param>
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
    /// Nastavím pasivní způsob přenosu(příkaz PASV)
    /// Získám IP adresu v řetězci z reply
    /// Získám do pole intů jednotlivé části IP adresy a spojím je do řetězce text tečkama
    /// Port získám tak čtvrtou část ip adresy bitově posunu o 8 a sečtu text pátou částí. Získám Socket, O IPEndPoint a pokusím se připojit na tento objekt.
    /// </summary>
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

    public void uploadSecureFolder()
    {
        OnNewStatus("Byla volána metoda uploadSecureFolder která je prázdná");
    // Zkontrolovat zda se první nauploadoval _.txt
    }

    /// <summary>
    /// OK
    /// </summary>
    /// <param name = "slozkyNeuploadovatAVS"></param>
    /// <param name = "dirName"></param>
    /// <param name = "i"></param>
    /// <param name = "td"></param>
    public override void DeleteRecursively(List<string> slozkyNeuploadovatAVS, string dirName, int i, List<DirectoriesToDeleteFtp> td)
    {
        chdirLite(dirName);
        var smazat = ListDirectoryDetails();
        foreach (var item2 in smazat)
        {
            var fst = FtpHelper.IsFile(item2, out var fn);
            if (fst == FileSystemType.File)
                deleteRemoteFile(fn);
            else if (fst == FileSystemType.Folder)
                DeleteRecursively(slozkyNeuploadovatAVS, fn, i, td);
        //////DebugLogger.Instance.WriteLine(item2);
        }

        goToUpFolderForce();
        rmdir(slozkyNeuploadovatAVS, dirName);
    }

    public override void DebugActualFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void D(string what, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void Connect()
    {
        ThrowEx.NotImplementedMethod();
    }
}