namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    /// <summary>
    /// Recursively retrieves all file system entries (files and folders) from the FTP server starting at the current path.
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during traversal</param>
    /// <returns>Dictionary mapping folder paths to lists of entry details</returns>
    public override Dictionary<string, List<string>> GetFSEntriesListRecursively(List<string> foldersToSkip)
    {
        if (!IsLoggedIn)
            Login();
        // Must store path to entire folder, not just current folder name
        var visitedFolders = new List<string>();
        var result = new Dictionary<string, List<string>>();
        var ftpEntries = ListDirectoryDetails();
        var actualPath = PathSelector.ActualPath;
        OnNewStatus("Getting recursive file list from folder" + " " + actualPath);
        foreach (var item in ftpEntries)
        {
            var firstChar = item[0];
            if (firstChar == '-')
            {
                if (result.ContainsKey(actualPath))
                {
                    result[actualPath].Add(item);
                }
                else
                {
                    var entries = new List<string>();
                    entries.Add(item);
                    result.Add(actualPath, entries);
                }
            }
            else if (firstChar == 'd')
            {
                var folderName = SHJoin.JoinFromIndex(8, ' ', item.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList());
                ////DebugLogger.Instance.WriteLine("Název alba22: " + folderName);
                if (!FtpHelper.IsThisOrUp(folderName))
                {
                    if (result.ContainsKey(actualPath))
                    {
                        result[actualPath].Add(item + "/");
                    }
                    else
                    {
                        var entries = new List<string>();
                        entries.Add(item + "/");
                        result.Add(actualPath, entries);
                    }
                //GetFSEntriesListRecursively(foldersToSkip, visitedFolders, result, PathSelector.ActualPath, folderName);
                }
            }
            else
            {
                throw new Exception("Unsupported object type");
            }
        }

        if (PathSelector.IndexZero > 0)
        {
            SetRemotePath(ftpClient.WwwSlash);
            // TODO: Zatím mi to bude stačit jen o 1 úrověň výše
            if (PathSelector.IndexZero == 1)
            {
                GoToUpFolderForce();
                ftpEntries = ListDirectoryDetails();
                actualPath = PathSelector.ActualPath;
                foreach (var item in ftpEntries)
                {
                    var firstChar = item[0];
                    if (firstChar == '-')
                    {
                    }
                    else if (firstChar == 'd')
                    {
                        var folderName = SHJoin.JoinFromIndex(8, ' ', item.Split(' '));
                        if (!FtpHelper.IsThisOrUp(folderName))
                        {
                            if (result.ContainsKey(actualPath))
                            {
                                result[actualPath].Add(item);
                            }
                            else
                            {
                                var entries = new List<string>();
                                entries.Add(item);
                                result.Add(actualPath, entries);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Unsupported object type");
                    }
                }
            }
            else
            {
                ThrowEx.NotSupported();
            }
        }

        return result;
    }

    /// <summary>
    /// Navigates to the specified remote folder path on the FTP server, creating directories if needed.
    /// </summary>
    /// <param name="remoteFolder">The full path to the remote folder to navigate to</param>
    public override void GoToPath(string remoteFolder)
    {
        if (remoteFolder.Contains("/" + "Kocicky" + "/"))
        {
            var i = 0;
            var ii = i;
        }

        if (!IsLoggedIn)
            Login();
        if (FtpLogging.GoToFolder)
            OnNewStatus("Navigating to folder" + " " + remoteFolder);
        var actualPath = PathSelector.ActualPath;
        var lastCharIndex = remoteFolder.Length - 1;
        if (actualPath == remoteFolder)
            return;
        SetRemotePath(ftpClient.WwwSlash);
        actualPath = PathSelector.ActualPath;
        // Remote folder starts with current path == remote folder is longer. Just go deeper
        if (remoteFolder.StartsWith(actualPath))
        {
            remoteFolder = remoteFolder.Substring(actualPath.Length);
            var tokens = remoteFolder.Split(new[] { PathSelector.Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in tokens)
                CreateDirectoryIfNotExists(item);
        }
        // Remote folder does not start with current path,
        else
        {
            SetRemotePath(ftpClient.WwwSlash);
            var tokens = remoteFolder.Split(new[] { PathSelector.Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (var i = PathSelector.IndexZero; i < tokens.Count; i++)
                CreateDirectoryIfNotExists(tokens[i]);
        }
    }

    /// <summary>
    /// Gets the size of a remote file by sending the SIZE command. Logs in if not already authenticated.
    /// </summary>
    /// <param name="fileName">The name of the file to get the size of</param>
    /// <returns>The size of the file in bytes</returns>
    public override long GetFileSize(string fileName)
    {
        OnNewStatus("Getting file size" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
        if (!IsLoggedIn)
            Login();
        SendCommand("SIZE" + " " + fileName);
        long size = 0;
        if (retValue == 213)
            size = long.Parse(reply.Substring(4));
        else
            throw new Exception(reply.Substring(4));
        return size;
    }

    /// <summary>
    /// Logs in to the FTP server using the configured credentials.
    /// This method should be called immediately after setting the connection variables.
    /// If not connected, connects to the server first, then sends USER command, and PASS command if required.
    /// Response code 230 means login successful without password, otherwise sends password with PASS command.
    /// </summary>
    public void Login()
    {
        //SslStream sslStream = new SslStream(client.GetStream(), false);
        OnNewStatus("Logging in to FTP Server");
#region If not connected, přihlásím se to server bez uživatele
        try
        {
            if (clientSocket == null || clientSocket.Connected == false)
                LoginWithoutUser();
        }
        catch (Exception ex)
        {
            OnNewStatus("Couldn't connect to remote server" + ": " + ex.Message);
            return;
        //throw new Exception("Couldn't connect to remote server" + ": " + ex.Message);
        }

#endregion
#region Then login with RemoteUser command
        if (isDebug)
            OnNewStatus("USER" + " " + RemoteUser);
        SendCommand2("USER" + " " + RemoteUser);
        if (!(retValue == 331 || retValue == 230))
        {
            Cleanup();
            throw new Exception(reply.Substring(4));
        }

#endregion
#region Hodnota 230 znamená že mohu pokračovat bez hesla. Pokud ne, pošlu své heslo příkazem PASS
        var bad = false;
        if (retValue != 230)
        {
            if (isDebug)
                OnNewStatus("PASS xxx");
            SendCommand2("PASS" + " " + RemotePass);
            if (!(retValue == 230 || retValue == 202))
            {
                Cleanup();
                bad = true;
            //throw new Exception(reply.Substring(4));
            }
        }

#endregion
        IsLoggedIn = !bad;
        if (IsLoggedIn)
            OnNewStatus("IsLoggedIn to" + " " + RemoteHost);
        else
            OnNewStatus("Not IsLoggedIn to" + " " + RemoteHost);
    }

    /// <summary>
    /// Establishes a TCP socket connection to the FTP server without user authentication.
    /// Creates a Stream-type TCP socket and connects to the remote server.
    /// Throws IOException if the response code is not 220.
    /// This method must always be called before authenticating with user credentials.
    /// </summary>
    public void LoginWithoutUser()
    {
        OnNewStatus("Connecting to FTP Server without user");
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipv4Address = null;
        var remoteHost2 = RemoteHost;
        if (FtpHelper.IsSchemaFtp(RemoteHost))
            remoteHost2 = FtpHelper.ReplaceSchemaFtp(remoteHost2);
        if (!IPAddress.TryParse(remoteHost2, out ipv4Address))
            ipv4Address = Dns.GetHostEntry(RemoteHost).AddressList[0].MapToIPv4();
        var data = ipv4Address.ToString();
        var endPoint = new IPEndPoint(ipv4Address, RemotePort);
        try
        {
            clientSocket.Connect(endPoint);
        }
        catch (Exception ex)
        {
            // During first attemp to connect to sunamo.cz Message = "A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond 185.8.239.101:21"
            throw new Exception("Couldn't connect to remote server");
        }

        ReadReply();
        if (retValue != 220)
        {
            Close();
            throw new Exception(reply.Substring(4));
        }
    }

    /// <summary>
    /// Outputs errors from certificate validation
    /// </summary>
    /// <param name = "sender"></param>
    /// <param name = "certificate"></param>
    /// <param name = "chain"></param>
    /// <param name = "errors"></param>
    private bool OnCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        OnNewStatus("Server Certificate Issued To: {0}", certificate.GetName());
        OnNewStatus("Server Certificate Issued By: {0}", certificate.GetIssuerName());
        if (errors != SslPolicyErrors.None)
        {
            OnNewStatus("Server Certificate Validation Error");
            OnNewStatus(errors.ToString());
            return false;
        }

        OnNewStatus("No Certificate Validation Errors");
        return true;
    }

    /// <summary>
    /// Outputs certificate information. Parameter indicates whether to output verbose info.
    /// </summary>
    /// <param name = "remoteCertificate"></param>
    /// <param name = "verbose"></param>
    private void ShowCertificateInfo(X509Certificate remoteCertificate, bool isVerbose)
    {
        OnNewStatus("Certficate Information for:\n{0}\n", remoteCertificate.GetName());
        OnNewStatus("Valid From: \n{0}", remoteCertificate.GetEffectiveDateString());
        OnNewStatus("Valid To: \n{0}", remoteCertificate.GetExpirationDateString());
        OnNewStatus("Certificate Format: \n{0}\n", remoteCertificate.GetFormat());
        OnNewStatus("Issuer Name: \n{0}", remoteCertificate.GetIssuerName());
        if (isVerbose)
        {
            OnNewStatus("Serial Number: \n{0}", remoteCertificate.GetSerialNumberString());
            OnNewStatus("Hash: \n{0}", remoteCertificate.GetCertHashString());
            OnNewStatus("Key Algorithm: \n{0}", remoteCertificate.GetKeyAlgorithm());
            OnNewStatus("Key Algorithm Parameters: \n{0}", remoteCertificate.GetKeyAlgorithmParametersString());
            OnNewStatus("Public Key: \n{0}", remoteCertificate.GetPublicKeyString());
        }
    }
}