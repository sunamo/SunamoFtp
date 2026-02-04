namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    /// <summary>
    /// Writes a message to the stream.
    /// Converts the message to ASCII bytes and writes them to the stream.
    /// </summary>
    /// <param name="message">The message to write to the stream</param>
    private void WriteMsg(string message)
    {
        var encoding = new ASCIIEncoding();
        var WriteBuffer = new byte[1024];
        WriteBuffer = encoding.GetBytes(message);
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

    /// <summary>
    /// Sends a command to the FTP server.
    /// Converts the command to ASCII bytes, sends it via stream or socket, and reads the server's reply.
    /// Stores the response in reply and retValue properties.
    /// </summary>
    /// <param name="command">The FTP command to send (without CRLF terminator)</param>
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

    /// <summary>
    /// Sends a command to the FTP server (alternate implementation).
    /// Identical to SendCommand - converts command to ASCII bytes, sends via stream or socket, and reads reply.
    /// Stores the response in reply and retValue properties.
    /// </summary>
    /// <param name="command">The FTP command to send (without CRLF terminator)</param>
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

    /// <summary>
    /// Creates a data socket for passive mode FTP transfer.
    /// Sends PASV command, parses the IP address and port from the server's reply.
    /// Extracts the IP address parts and joins them with dots.
    /// Calculates the port by bit-shifting the 5th part by 8 and adding the 6th part.
    /// Creates a Socket, IPEndPoint and attempts to connect to the server.
    /// </summary>
    /// <returns>A connected socket ready for data transfer</returns>
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
                catch (Exception ex)
                {
                    throw new Exception("Malformed PASV reply" + ": " + reply);
                }
#endregion
        }

        var ipAddress = parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3];
#endregion
#region Gets port by bit-shifting fourth IP part by 8 and adding fifth part. Creates Socket, IPEndPoint and attempts to connect to this object.
        var port = (parts[4] << 8) + parts[5];
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endPoint = new IPEndPoint(Dns.Resolve(ipAddress).AddressList[0], port);
        try
        {
            socket.Connect(endPoint);
        }
        catch (Exception ex)
        {
            throw new Exception("Can't connect to remoteserver");
        }

        return socket;
#endregion
    }

    /// <summary>
    /// Placeholder method for uploading a folder securely.
    /// Currently empty - not implemented. Should verify that _.txt file is uploaded first.
    /// </summary>
    public void uploadSecureFolder()
    {
        OnNewStatus("Method uploadSecureFolder was called but is empty");
    // Check if _.txt was uploaded first
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
    public override void DeleteRecursively(List<string> foldersToSkip, string dirName, int i, List<DirectoriesToDeleteFtp> directoriesToDelete)
    {
        ChdirLite(dirName);
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
        Rmdir(foldersToSkip, dirName);
    }

    /// <summary>
    /// Outputs isDebug information about the current folder.
    /// Not implemented - throws NotImplementedMethod exception.
    /// </summary>
    public override void DebugActualFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// isDebug output method.
    /// Not implemented - throws NotImplementedMethod exception.
    /// </summary>
    /// <param name="context">What to isDebug</param>
    /// <param name="text">Format string for output</param>
    /// <param name="args">Arguments for the format string</param>
    public override void WriteDebugLog(string context, string text, params object[] args)
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