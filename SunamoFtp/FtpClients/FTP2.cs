namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    private void ShowSslInfo(string serverName, SslStream sslStream, bool isVerbose)
    {
        ShowCertificateInfo(sslStream.RemoteCertificate, isVerbose);
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

    // Convenience method that calls GetSslStream(Socket) with the clientSocket.
    public void GetSslStream()
    {
        GetSslStream(clientSocket);
    }

    public void GetSslStream(Socket clientSocket)
    {
        RemoteCertificateValidationCallback callback = OnCertificateValidation;
        var sslStream = new SslStream(new NetworkStream(clientSocket)); //,new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
        try
        {
            sslStream.AuthenticateAsClient(RemoteHost, null, SslProtocols.Ssl3 | SslProtocols.Tls, true);
            if (sslStream.IsAuthenticated)
                if (isUpload)
                    stream2 = sslStream;
                else
                    stream = sslStream;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

        ShowSslInfo(RemoteHost, sslStream, true);
    }

    public void SetBinaryMode(bool isBinary)
    {
        if (isBinary)
        {
            OnNewStatus("Setting binary transfer mode");
            SendCommand("TYPE" + " ");
        }
        else
        {
            OnNewStatus("Setting ASCII transfer mode");
            SendCommand("TYPE" + " ");
        }

        if (retValue != 200)
            throw new Exception(reply.Substring(4));
    }

    public override bool Download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        OnNewStatus("Downloading" + " " + UH.Combine(false, PathSelector.ActualPath, remFileName));
        if (File.Exists(locFileName))
        {
            if (deleteLocalIfExists)
            {
                try
                {
                    File.Delete(locFileName);
                }
                catch (Exception)
                {
                    OnNewStatus("File " + remFileName + " could not be downloaded because file " + locFileName + " could not be deleted");
                    return false;
                }
            }
            else
            {
                OnNewStatus("Soubor" + " " + remFileName + " " + "nemohl být stažen, protože soubor" + " " + locFileName + " " + "existoval již to disku a nebylo povoleno jeho smazání");
                return false;
            }
        }

        var resume = false;
#region Pokud nejsem přihlášený, přihlásím se to nastavím binární mód
        if (string.IsNullOrEmpty(locFileName))
            throw new Exception("You must specify a file name to download to");
        if (!IsLoggedIn)
            Login();
        SetBinaryMode(true);
#endregion
#region Pokud neexistuje, vytvořím jej a hned zavřu. Načtu jej do FS text FileMode Open
        OnNewStatus("Downloading file" + " " + remFileName + " " + "from" + " " + RemoteHost + "/" + remotePath);
        if (!File.Exists(locFileName))
        {
            Stream createdFileStream = File.Create(locFileName);
            createdFileStream.Close();
        }

        var downloadStream = new FileStream(locFileName, FileMode.Open);
#endregion
        var clientSocket = CreateDataSocket();
        long offset = 0;
        if (resume)
        {
#region Pokud otevřený soubor nemá velikost 0, pošlu příkaz REST čímž nastavím offset
            offset = downloadStream.Length;
            if (offset > 0)
            {
                SendCommand("REST" + " " + offset);
                if (retValue != 350)
                    offset = 0;
            }

#endregion
#region Pokud budeme navazovat, posunu v otevřeném souboru to konec
            if (offset > 0)
            {
                if (isDebug)
                    OnNewStatus("seeking to" + " " + offset);
                var newPosition = downloadStream.Seek(offset, SeekOrigin.Begin);
                OnNewStatus("new pos=" + newPosition);
            }
#endregion
        }

#region Pošlu příkaz RETR a všechny přijaté bajty zapíšu
        SendCommand("RETR" + " " + UH.GetFileName(remFileName));
        if (!(retValue == 150 || retValue == 125))
            throw new Exception(reply.Substring(4));
        while (true)
        {
            bytes = clientSocket.Receive(buffer, buffer.Length, 0);
            downloadStream.Write(buffer, 0, bytes);
            if (bytes <= 0)
                break;
        }

        downloadStream.Close();
        if (clientSocket.Connected)
            clientSocket.Close();
        OnNewStatus("");
        ReadReply();
        if (!(retValue == 226 || retValue == 250))
            throw new Exception(reply.Substring(4));
#endregion
        return true;
    }

    public void UploadSecure(string filePath, bool isResume)
    {
        var path = UH.Combine(false, PathSelector.ActualPath, filePath);
        OnUploadingNewStatus(path);
#region Pošlu příkaz PASV a příhlásím se pokud nejsem
        SendCommand("PASV");
        if (retValue != 227)
            throw new Exception(reply.Substring(4));
        if (!IsLoggedIn)
            Login();
#endregion
#region Získám socket, z něho stream a pokud navazuzuji, pokusím se nastavit binární mód a offset podle toho kolik dat už to serveru bylo.
        var clientSocket = CreateDataSocket();
        isUpload = true;
        GetSslStream(clientSocket);
        long offset = 0;
        if (isResume)
            try
            {
                SetBinaryMode(true);
                offset = GetFileSize(filePath);
            }
            catch (Exception)
            {
                offset = 0;
            }

#endregion
#region Pokud je tam nějaký offset, pošlu opět příkaz rest text offsetem, abych nastavil od čeho budu uploadovat
        if (offset > 0)
        {
            SendCommand("REST" + " " + offset);
            if (retValue != 350)
                offset = 0;
        }

#endregion
#region Pošlu příkaz STOR text jménem souboru a zapíšu všechny bajty z souboru do bufferu byte[]
        SendCommand("STOR" + " " + Path.GetFileName(filePath));
        if (!(retValue == 125 || retValue == 150))
            throw new Exception(reply.Substring(4));
        var input = File.OpenRead(filePath);
        var fileBuffer = new byte[input.Length];
        input.Read(fileBuffer, 0, fileBuffer.Length);
        input.Close();
#endregion
#region Nastavím offset v lokálním souboru.  I když nevím prož když pak uploaduji M stream2.Write text offsetem 0. Zavřu socket i proud a přečtu odpověď serveru. Pokud nebyla 226 nebo 250, VV
        if (offset != 0)
        {
            if (isDebug)
                OnNewStatus("seeking to" + " " + offset);
            input.Seek(offset, SeekOrigin.Begin);
        }

        OnNewStatus("Uploading file" + " " + filePath + " to " + remotePath);
        if (clientSocket.Connected)
        {
            stream2.Write(fileBuffer, 0, fileBuffer.Length);
            OnNewStatus("File Upload");
        }

        stream2.Close();
        if (clientSocket.Connected)
            clientSocket.Close();
        ReadReply();
        if (!(retValue == 226 || retValue == 250))
            throw new Exception(reply.Substring(4));
#endregion
    }

    public override List<string> ListDirectoryDetails()
    {
        var result = new List<string>();
        var path = UH.Combine(true, RemoteHost, PathSelector.ActualPath);
        // Get the object used to communicate with the server.
        var request = (FtpWebRequest)WebRequest.Create(path);
        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
        // This example assumes the FTP site uses anonymous logon.
        request.Credentials = new NetworkCredential(RemoteUser, RemotePass);
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
