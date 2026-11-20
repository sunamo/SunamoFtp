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
#region If I am not logged in, I log in and set binary mode
        if (string.IsNullOrEmpty(locFileName))
            throw new Exception("You must specify a file name to download to");
        if (!IsLoggedIn)
            Login();
        SetBinaryMode(true);
#endregion
#region If it does not exist, I create it and close it immediately. I load it into FS text FileMode Open
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
#region If the opened file does not have size 0, I send the REST command to set the offset
            offset = downloadStream.Length;
            if (offset > 0)
            {
                SendCommand("REST" + " " + offset);
                if (retValue != 350)
                    offset = 0;
            }

#endregion
#region If we are resuming, I move to the end of the opened file
            if (offset > 0)
            {
                if (isDebug)
                    OnNewStatus("seeking to" + " " + offset);
                var newPosition = downloadStream.Seek(offset, SeekOrigin.Begin);
                OnNewStatus("new pos=" + newPosition);
            }
#endregion
        }

#region I send the RETR command and write all received bytes
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
#region I send the PASV command and log in if I am not logged in
        SendCommand("PASV");
        if (retValue != 227)
            throw new Exception(reply.Substring(4));
        if (!IsLoggedIn)
            Login();
#endregion
#region I get the socket, from it the stream, and if I am resuming, I try to set binary mode and the offset according to how much data was already sent to the server.
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
#region If there is an offset, I send the rest command with the offset again to set where to upload from
        if (offset > 0)
        {
            SendCommand("REST" + " " + offset);
            if (retValue != 350)
                offset = 0;
        }

#endregion
#region I send the STOR command with the file name and write all bytes from the file into a byte[] buffer
        SendCommand("STOR" + " " + Path.GetFileName(filePath));
        if (!(retValue == 125 || retValue == 150))
            throw new Exception(reply.Substring(4));
        var input = File.OpenRead(filePath);
        var fileBuffer = new byte[input.Length];
        input.Read(fileBuffer, 0, fileBuffer.Length);
        input.Close();
#endregion
#region I set the offset in the local file. Although I do not know why, since I then upload with M stream2.Write text with offset 0. I close the socket and the stream and read the server response. If it was not 226 or 250, VV
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
