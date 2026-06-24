namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    private void WriteMsg(string message)
    {
        var encoding = new ASCIIEncoding();
        var WriteBuffer = encoding.GetBytes(message);
        stream.Write(WriteBuffer, 0, WriteBuffer.Length);
    //NewStatus(" WRITE:" + message);
    }

    private string ResponseMsg()
    {
        var encoding = new ASCIIEncoding();
        var serverbuff = new byte[1024];
        var count = 0;
        while (true)
        {
            var buffer = new byte[2];
            var bytes = stream.Read(buffer, 0, 1);
            if (bytes == 1)
            {
                serverbuff[count] = buffer[0];
                count++;
                if (buffer[0] == '\n')
                    break;
            }
            else
            {
                break;
            };
        };
        var retval = encoding.GetString(serverbuff, 0, count);
        //NewStatus(" READ:" + retval);
        retValue = int.Parse(retval.Substring(0, 3));
        return retval;
    }

    public void SendCommand(string command)
    {
#region Original SendCommand method
        var cmdBytes = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());
        if (useStream)
            WriteMsg(command + "\r\n");
        else
            clientSocket.Send(cmdBytes, cmdBytes.Length, 0);
        ReadReply();
#endregion
    }

    private void SendCommand2(string command)
    {
#region Original SendCommand method
        var cmdBytes = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());
        if (useStream)
            WriteMsg(command + "\r\n");
        else
            clientSocket.Send(cmdBytes, cmdBytes.Length, 0);
        ReadReply();
#endregion
    }

    public Socket CreateDataSocket()
    {
#region Sets passive transfer mode (PASV command)
        SendCommand("PASV");
        if (retValue != 227)
            throw new Exception(reply.Substring(4));
#endregion
#region Gets IP address as string from reply
        var index1 = reply.IndexOf('(');
        var index2 = reply.IndexOf(')');
        var ipData = reply.Substring(index1 + 1, index2 - index1 - 1);
        var parts = new int[6];
        var len = ipData.Length;
        var partCount = 0;
        var buffer = "";
#endregion
#region Gets individual IP address parts into int array and joins them with dots
        for (var i = 0; i < len && partCount <= 6; i++)
        {
            var character = char.Parse(ipData.Substring(i, 1));
            if (char.IsDigit(character))
                buffer += character;
            else if (character != ',')
                throw new Exception("Malformed PASV reply" + ": " + reply);
#region If last character is comma,
            if (character == ',' || i + 1 == len)
                try
                {
                    parts[partCount++] = int.Parse(buffer);
                    buffer = "";
                }
                catch (Exception)
                {
                    throw new Exception("Malformed PASV reply" + ": " + reply);
                }
#endregion
        }

        var ipAddress = $"{parts[0]}.{parts[1]}.{parts[2]}.{parts[3]}";
#endregion
#region Gets port by bit-shifting fourth IP part by 8 and adding fifth part. Creates Socket, IPEndPoint and attempts to connect to this object.
        var port = (parts[4] << 8) + parts[5];
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endPoint = new IPEndPoint(Dns.Resolve(ipAddress).AddressList[0], port);
        try
        {
            socket.Connect(endPoint);
        }
        catch (Exception)
        {
            throw new Exception("Can't connect to remoteserver");
        }

        return socket;
#endregion
    }

    public void uploadSecureFolder()
    {
        OnNewStatus("Method uploadSecureFolder was called but is empty");
    // Check if _.txt was uploaded first
    }

    public override void DeleteRecursively(List<string> foldersToSkip, string directoryName, int i, List<DirectoriesToDeleteFtp> directoriesToDelete)
    {
        ChdirLite(directoryName);
        var toDelete = ListDirectoryDetails();
        foreach (var item2 in toDelete)
        {
            var fst = FtpHelper.IsFile(item2, out var fn);
            if (fst == FileSystemType.File)
                DeleteRemoteFile(fn);
            else if (fst == FileSystemType.Folder)
                DeleteRecursively(foldersToSkip, fn, i, directoriesToDelete);
        //////DebugLogger.Instance.WriteLine(item2);
        }

        GoToUpFolderForce();
        Rmdir(foldersToSkip, directoryName);
    }

    public override void DebugActualFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void WriteDebugLog(string context, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void Connect()
    {
        ThrowEx.NotImplementedMethod();
    }
}
