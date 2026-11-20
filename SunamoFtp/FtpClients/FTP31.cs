namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    private string ReadLine()
    {
        // I get the bytes from O clientSocket or stream and convert them to an ASCII string
        while (true)
        {
            // I get the bytes
            if (useStream)
                bytes = stream.Read(buffer, buffer.Length, 0);
            else
                // TODO: This line causes an error on shutdown after a long idle period
                bytes = clientSocket.Receive(buffer, buffer.Length, 0);
            // I convert them to string with the ASCII.GetString method. If fewer bytes were read than the buffer size, I break
            message += ASCII.GetString(buffer, 0, bytes);
            if (bytes < buffer.Length)
                break;
        }

        var mess = SHSplit.SplitChar(message, '\n');
        // I split the obtained string by \n and take the second-to-last element, or the first one, which I then return
        if (message.Length > 2)
            message = mess[mess.Count - 2];
        else
            message = mess[0];
        // If the 3rd side is not a space, I call this M again
        if (!message.Substring(3, 1).Equals(""))
            return ReadLine();
        if (isDebug)
            for (var k = 0; k < mess.Count - 1; k++)
                OnNewStatus(mess[k]);
        return message;
    }
}