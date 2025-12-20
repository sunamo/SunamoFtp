// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
namespace SunamoFtp.FtpClients;
public partial class FTP : FtpBase
{
    /// <summary>
    /// Vypíšu velmi pokročilé informace o certifikaci
    /// </summary>
    /// <param name = "serverName"></param>
    /// <param name = "sslStream"></param>
    /// <param name = "verbose"></param>
    private void showSslInfo(string serverName, SslStream sslStream, bool verbose)
    {
        showCertificateInfo(sslStream.RemoteCertificate, verbose);
        OnNewStatus("\n\nSSL Connect Report for : {0}\n", serverName);
        OnNewStatus("Is Authenticated: {0}", sslStream.IsAuthenticated);
        OnNewStatus("Is Encrypted: {0}", sslStream.IsEncrypted);
        OnNewStatus("Is Signed: {0}", sslStream.IsSigned);
        OnNewStatus("Is Mutually Authenticated: {0}\n", sslStream.IsMutuallyAuthenticated);
        OnNewStatus("Hash Algorithm: {0}", sslStream.HashAlgorithm);
        OnNewStatus("Hash Strength: {0}", sslStream.HashStrength);
        OnNewStatus("Cipher Algorithm: {0}", sslStream.CipherAlgorithm);
        OnNewStatus("Cipher Strength: {0}\n", sslStream.CipherStrength);
        OnNewStatus("Key Exchange Algorithm: {0}", sslStream.KeyExchangeAlgorithm);
        OnNewStatus("Key Exchange Strength: {0}\n", sslStream.KeyExchangeStrength);
        OnNewStatus("SSL Protocol: {0}", sslStream.SslProtocol);
    }

    /// <summary>
    /// Získám stream na objektu clientSocket
    /// </summary>
    public void getSslStream()
    {
        getSslStream(clientSocket);
    }

    /// <summary>
    /// Získám stream ze socketu do T SslStream.
    /// Pokud se autentizuji, vložím SslStream do stream2(při uploadu) nebo stream.
    /// Výjimkky zachycuje
    /// </summary>
    /// <param name = "Csocket"></param>
    public void getSslStream(Socket Csocket)
    {
        RemoteCertificateValidationCallback callback = OnCertificateValidation;
        var _sslStream = new SslStream(new NetworkStream(Csocket)); //,new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
        try
        {
            _sslStream.AuthenticateAsClient(remoteHost, null, SslProtocols.Ssl3 | SslProtocols.Tls, true);
            if (_sslStream.IsAuthenticated)
                if (isUpload)
                    stream2 = _sslStream;
                else
                    stream = _sslStream;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

        showSslInfo(remoteHost, _sslStream, true);
    }

    /// <summary>
    /// Pošlu příkaz TYPE I pro binární mod nebo TYPE A pro ASCII
    /// </summary>
    /// <param name = "mode"></param>
    public void setBinaryMode(bool mode)
    {
        if (mode)
        {
            OnNewStatus("Nastavuji binární mód přenosu");
            sendCommand("TYPE" + " ");
        }
        else
        {
            OnNewStatus("Nastavuji ASCII mód přenosu");
            sendCommand("TYPE" + " ");
        }

        if (retValue != 200)
            throw new Exception(reply.Substring(4));
    }

    /// <summary>
    /// Stáhne soubor A1 do lok. souboru A2. Navazuje pokud A3.
    /// Pokud A2 bude null, M vyhodí výjimku
    /// Pokud neexistuje, vytvořím jej a hned zavřu. Načtu jej do FS text FileMode Open
    /// Pokud otevřený soubor nemá velikost 0, pošlu příkaz REST čímž nastavím offset
    /// Pokud budeme navazovat, posunu v otevřeném souboru na konec
    /// Pošlu příkaz RETR a všechny přijaté bajty zapíšu
    /// </summary>
    /// <param name = "remFileName"></param>
    /// <param name = "locFileName"></param>
    /// <param name = "resume"></param>
    public override bool download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        OnNewStatus("Stahuji" + " " + UH.Combine(false, ps.ActualPath, remFileName));
        if (File.Exists(locFileName))
        {
            if (deleteLocalIfExists)
            {
                try
                {
                    File.Delete(locFileName);
                }
                catch (Exception ex)
                {
                    OnNewStatus("Soubor" + " " + remFileName + " " + "nemohl být stažen, protože soubor" + " " + locFileName + " " + "nešel smazat");
                    return false;
                }
            }
            else
            {
                OnNewStatus("Soubor" + " " + remFileName + " " + "nemohl být stažen, protože soubor" + " " + locFileName + " " + "existoval již na disku a nebylo povoleno jeho smazání");
                return false;
            }
        }

        var resume = false;
#region Pokud nejsem přihlášený, přihlásím se na nastavím binární mód
        if (string.IsNullOrEmpty(locFileName))
            throw new Exception("Musíte zadat jméno souboru do kterého chcete stáhnout");
        if (!logined)
            login();
        setBinaryMode(true);
#endregion
#region Pokud neexistuje, vytvořím jej a hned zavřu. Načtu jej do FS text FileMode Open
        OnNewStatus("Downloading file" + " " + remFileName + " " + "from" + " " + remoteHost + "/" + remotePath);
        if (!File.Exists(locFileName))
        {
            Stream st = File.Create(locFileName);
            st.Close();
        }

        var output = new FileStream(locFileName, FileMode.Open);
#endregion
        var cSocket = createDataSocket();
        long offset = 0;
        if (resume)
        {
#region Pokud otevřený soubor nemá velikost 0, pošlu příkaz REST čímž nastavím offset
            offset = output.Length;
            if (offset > 0)
            {
                sendCommand("REST" + " " + offset);
                if (retValue != 350)
                    offset = 0;
            }

#endregion
#region Pokud budeme navazovat, posunu v otevřeném souboru na konec
            if (offset > 0)
            {
                if (debug)
                    OnNewStatus("seeking to" + " " + offset);
                var npos = output.Seek(offset, SeekOrigin.Begin);
                OnNewStatus("new pos=" + npos);
            }
#endregion
        }

#region Pošlu příkaz RETR a všechny přijaté bajty zapíšu
        sendCommand("RETR" + " " + UH.GetFileName(remFileName));
        if (!(retValue == 150 || retValue == 125))
            throw new Exception(reply.Substring(4));
        while (true)
        {
            bytes = cSocket.Receive(buffer, buffer.Length, 0);
            output.Write(buffer, 0, bytes);
            if (bytes <= 0)
                break;
        }

        output.Close();
        if (cSocket.Connected)
            cSocket.Close();
        OnNewStatus("");
        readReply();
        if (!(retValue == 226 || retValue == 250))
            throw new Exception(reply.Substring(4));
#endregion
        return true;
    }

    /// <summary>
    /// Pošlu příkaz PASV a příhlásím se pokud nejsem
    /// Získám socket, z něho stream a pokud navazuzuji, pokusím se nastavit binární mód a offset podle toho kolik dat už na serveru bylo.
    /// Pokud je tam nějaký offset, pošlu opět příkaz rest text offsetem, abych nastavil od čeho budu uploadovat
    /// Pošlu příkaz STOR text jménem souboru a zapíšu všechny bajty z souboru do bufferu byte[]
    /// Nastavím offset v lokálním souboru.  I když nevím prož když pak uploaduji M stream2.Write text offsetem 0. Zavřu socket i proud a přečtu odpověď serveru. Pokud nebyla 226 nebo 250, VV
    /// </summary>
    /// <param name = "fileName"></param>
    /// <param name = "resume"></param>
    public void uploadSecure(string fileName, bool resume)
    {
        var path = UH.Combine(false, ps.ActualPath, fileName);
        OnUploadingNewStatus(path);
#region Pošlu příkaz PASV a příhlásím se pokud nejsem
        sendCommand("PASV");
        if (retValue != 227)
            throw new Exception(reply.Substring(4));
        if (!logined)
            login();
#endregion
#region Získám socket, z něho stream a pokud navazuzuji, pokusím se nastavit binární mód a offset podle toho kolik dat už na serveru bylo.
        var cSocket = createDataSocket();
        isUpload = true;
        getSslStream(cSocket);
        long offset = 0;
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
#region Pokud je tam nějaký offset, pošlu opět příkaz rest text offsetem, abych nastavil od čeho budu uploadovat
        if (offset > 0)
        {
            sendCommand("REST" + " " + offset);
            if (retValue != 350)
                offset = 0;
        }

#endregion
#region Pošlu příkaz STOR text jménem souboru a zapíšu všechny bajty z souboru do bufferu byte[]
        sendCommand("STOR" + " " + Path.GetFileName(fileName));
        if (!(retValue == 125 || retValue == 150))
            throw new Exception(reply.Substring(4));
        var input = File.OpenRead(fileName);
        var bufferFile = new byte[input.Length];
        input.Read(bufferFile, 0, bufferFile.Length);
        input.Close();
#endregion
#region Nastavím offset v lokálním souboru.  I když nevím prož když pak uploaduji M stream2.Write text offsetem 0. Zavřu socket i proud a přečtu odpověď serveru. Pokud nebyla 226 nebo 250, VV
        if (offset != 0)
        {
            if (debug)
                OnNewStatus("seeking to" + " " + offset);
            input.Seek(offset, SeekOrigin.Begin);
        }

        OnNewStatus("Uploading file" + " " + fileName + " to " + remotePath);
        if (cSocket.Connected)
        {
            stream2.Write(bufferFile, 0, bufferFile.Length);
            OnNewStatus("File Upload");
        }

        stream2.Close();
        if (cSocket.Connected)
            cSocket.Close();
        readReply();
        if (!(retValue == 226 || retValue == 250))
            throw new Exception(reply.Substring(4));
#endregion
    }

    public override List<string> ListDirectoryDetails()
    {
        var vr = new List<string>();
        var _Path = UH.Combine(true, remoteHost, ps.ActualPath);
        // Get the object used to communicate with the server.
        var request = (FtpWebRequest)WebRequest.Create(_Path);
        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
        // This example assumes the FTP site uses anonymous logon.
        request.Credentials = new NetworkCredential(remoteUser, remotePass);
        var response = (FtpWebResponse)request.GetResponse();
        var responseStream = response.GetResponseStream();
        var reader = new StreamReader(responseStream);
        while (!reader.EndOfStream)
            vr.Add(reader.ReadLine());
        reader.Close();
        response.Close();
        return vr;
    }
}