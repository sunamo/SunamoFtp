namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FTP : FtpBase
{
    /// <summary>
    /// Displays detailed SSL/TLS connection information including authentication, encryption, and certificate details.
    /// </summary>
    /// <param name="serverName">The name of the server being connected to</param>
    /// <param name="sslStream">The SSL stream to retrieve information from</param>
    /// <param name="verbose">Whether to display verbose certificate information</param>
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
    /// Gets an SSL stream on the client socket. This is a convenience method that calls getSslStream(Socket) with the clientSocket.
    /// </summary>
    public void getSslStream()
    {
        getSslStream(clientSocket);
    }

    /// <summary>
    /// Creates an SSL stream from the specified socket and authenticates as client.
    /// If authentication succeeds, assigns the SSL stream to stream2 (for upload) or stream (for download).
    /// Catches and re-throws exceptions with message details.
    /// </summary>
    /// <param name="Csocket">The socket to create the SSL stream from</param>
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
    /// Sets the file transfer mode on the FTP server.
    /// Sends TYPE I command for binary mode or TYPE A command for ASCII mode.
    /// </summary>
    /// <param name="mode">True for binary mode, false for ASCII mode</param>
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
    /// Downloads a file from the FTP server to a local file.
    /// If the local file exists and deleteLocalIfExists is true, deletes it first.
    /// Creates the local file if it doesn't exist, then sends RETR command and writes all received bytes.
    /// </summary>
    /// <param name="remFileName">The name of the remote file to download</param>
    /// <param name="locFileName">The local file path to save to. Throws exception if null.</param>
    /// <param name="deleteLocalIfExists">Whether to delete the local file if it already exists</param>
    /// <returns>True if download succeeded, false if file couldn't be deleted or already exists</returns>
    public override bool download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        OnNewStatus("Stahuji" + " " + UH.Combine(false, PathSelector.ActualPath, remFileName));
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
                    OnNewStatus("Soubor" + " " + remFileName + " " + "nemohl být stažen, protože soubor" + " " + locFileName + " " + "nešel toDelete");
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
    /// Uploads a file to the FTP server using a secure SSL connection.
    /// Sends PASV command, creates secure data socket, and optionally resumes from previous upload.
    /// If resuming, sets binary mode and gets remote file size to determine offset.
    /// Sends STOR command with file name and writes all bytes from file to the secure stream.
    /// Closes socket and stream after upload and verifies server response.
    /// </summary>
    /// <param name="fileName">The name of the file to upload</param>
    /// <param name="resume">Whether to resume a previous upload from the last position</param>
    public void uploadSecure(string fileName, bool resume)
    {
        var path = UH.Combine(false, PathSelector.ActualPath, fileName);
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

    /// <summary>
    /// Lists all files and directories in the current remote directory with detailed information.
    /// Uses FtpWebRequest with ListDirectoryDetails method.
    /// </summary>
    /// <returns>List of strings containing detailed directory entry information</returns>
    public override List<string> ListDirectoryDetails()
    {
        var result = new List<string>();
        var _Path = UH.Combine(true, remoteHost, PathSelector.ActualPath);
        // Get the object used to communicate with the server.
        var request = (FtpWebRequest)WebRequest.Create(_Path);
        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
        // This example assumes the FTP site uses anonymous logon.
        request.Credentials = new NetworkCredential(remoteUser, remotePass);
        var response = (FtpWebResponse)request.GetResponse();
        var responseStream = response.GetResponseStream();
        var reader = new StreamReader(responseStream);
        while (!reader.EndOfStream)
            result.Add(reader.ReadLine());
        reader.Close();
        response.Close();
        return result;
    }
}