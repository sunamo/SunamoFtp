namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    /// <summary>
    /// Reads a line of response from the FTP server.
    /// Gets bytes from the client socket or stream and converts them to ASCII string.
    /// Splits the string by newline characters and takes the second-to-last or first element.
    /// If the character at position 3 is not a space, recursively calls this method again.
    /// </summary>
    /// <returns>The response line from the FTP server</returns>
    private string ReadLine()
    {
        // Zjistím si bajty z O clientSocket nebo stream a převedu je to ASCII string
        while (true)
        {
            // Zjistím si bajty
            if (useStream)
                bytes = stream.Read(buffer, buffer.Length, 0);
            else
                // TODO: Tento řádek způsobuje chybu při ukončení po dlouhé nečinnosti
                bytes = clientSocket.Receive(buffer, buffer.Length, 0);
            // Ty převedu to string metodou ASCII.GetString. Pokud bylo načteno bajtů méně než je velikost bufferu, breaknu to
            message += ASCII.GetString(buffer, 0, bytes);
            if (bytes < buffer.Length)
                break;
        }

        var mess = SHSplit.SplitChar(message, '\n');
        // Rozdělím získaný string \n a vezmu předposlední prvek, nebo první, který pak vrátím
        if (message.Length > 2)
            message = mess[mess.Count - 2];
        else
            message = mess[0];
        //Když to 3. straně není mezera, zavolám tuto M znovu
        if (!message.Substring(3, 1).Equals(""))
            return ReadLine();
        if (isDebug)
            for (var k = 0; k < mess.Count - 1; k++)
                OnNewStatus(mess[k]);
        return message;
    }
}