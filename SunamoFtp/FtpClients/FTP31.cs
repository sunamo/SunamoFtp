// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
namespace SunamoFtp.FtpClients;
public partial class FTP : FtpBase
{
    /// <summary>
    /// Zjistím si bajty z O clientSocket nebo stream a převedu je na ASCII string
    /// Rozdělím získaný string \n a vezmu předposlední prvek, nebo první, který pak vrátím
    /// Když na 3. straně není mezera, zavolám tuto M znovu
    /// </summary>
    private string readLine()
    {
        // Zjistím si bajty z O clientSocket nebo stream a převedu je na ASCII string
        while (true)
        {
            // Zjistím si bajty
            if (useStream)
                bytes = stream.Read(buffer, buffer.Length, 0);
            else
                // TODO: Tento řádek způsobuje chybu při ukončení po dlouhé nečinnosti
                bytes = clientSocket.Receive(buffer, buffer.Length, 0);
            // Ty převedu na string metodou ASCII.GetString. Pokud bylo načteno bajtů méně než je velikost bufferu, breaknu to
            mes += ASCII.GetString(buffer, 0, bytes);
            if (bytes < buffer.Length)
                break;
        }

        var mess = SHSplit.SplitChar(mes, '\n');
        // Rozdělím získaný string \n a vezmu předposlední prvek, nebo první, který pak vrátím
        if (mes.Length > 2)
            mes = mess[mess.Count - 2];
        else
            mes = mess[0];
        //Když na 3. straně není mezera, zavolám tuto M znovu
        if (!mes.Substring(3, 1).Equals(""))
            return readLine();
        if (debug)
            for (var k = 0; k < mess.Count - 1; k++)
                OnNewStatus(mess[k]);
        return mes;
    }
}