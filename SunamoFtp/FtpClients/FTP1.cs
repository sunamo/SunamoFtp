namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FTP : FtpBase
{
    /// <summary>
    /// Recursively retrieves all file system entries (files and folders) from the FTP server starting at the current path.
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during traversal</param>
    /// <returns>Dictionary mapping folder paths to lists of entry details</returns>
    public override Dictionary<string, List<string>> getFSEntriesListRecursively(List<string> foldersToSkip)
    {
        if (!logined)
            login();
        // Musí se do ní ukládat cesta k celé složce, nikoliv jen název aktuální složky
        var visitedFolders = new List<string>();
        var result = new Dictionary<string, List<string>>();
        var ftpEntries = ListDirectoryDetails();
        var actualPath = PathSelector.ActualPath;
        OnNewStatus("Získávám rekurzivní seznam souborů ze složky" + " " + actualPath);
        foreach (var item in ftpEntries)
        {
            var fz = item[0];
            if (fz == '-')
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
            else if (fz == 'd')
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
                //getFSEntriesListRecursively(foldersToSkip, visitedFolders, result, PathSelector.ActualPath, folderName);
                }
            }
            else
            {
                throw new Exception("Nepodporovaný typ objektu");
            }
        }

        if (PathSelector.indexZero > 0)
        {
            setRemotePath(ftpClient.WwwSlash);
            // TODO: Zatím mi to bude stačit jen o 1 úrověň výše
            if (PathSelector.indexZero == 1)
            {
                goToUpFolderForce();
                ftpEntries = ListDirectoryDetails();
                actualPath = PathSelector.ActualPath;
                foreach (var item in ftpEntries)
                {
                    var fz = item[0];
                    if (fz == '-')
                    {
                    }
                    else if (fz == 'd')
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
                        throw new Exception("Nepodporovaný typ objektu");
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
    public override void goToPath(string remoteFolder)
    {
        if (remoteFolder.Contains("/" + "Kocicky" + "/"))
        {
            var i = 0;
            var ii = i;
        }

        if (!logined)
            login();
        if (FtpLogging.GoToFolder)
            OnNewStatus("Přecházím do složky" + " " + remoteFolder);
        var actualPath = PathSelector.ActualPath;
        var dd = remoteFolder.Length - 1;
        if (actualPath == remoteFolder)
            return;
        setRemotePath(ftpClient.WwwSlash);
        actualPath = PathSelector.ActualPath;
        // Vzdálená složka začíná text aktuální cestou == vzdálená složka je delší. Pouze přejdi hloubš
        if (remoteFolder.StartsWith(actualPath))
        {
            remoteFolder = remoteFolder.Substring(actualPath.Length);
            var tokens = remoteFolder.Split(new[] { PathSelector.Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in tokens)
                CreateDirectoryIfNotExists(item);
        }
        // Vzdálená složka nezačíná aktuální cestou,
        else
        {
            setRemotePath(ftpClient.WwwSlash);
            var tokens = remoteFolder.Split(new[] { PathSelector.Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (var i = PathSelector.indexZero; i < tokens.Count; i++)
                CreateDirectoryIfNotExists(tokens[i]);
        }
    }

    /// <summary>
    /// Gets the size of a remote file by sending the SIZE command. Logs in if not already authenticated.
    /// </summary>
    /// <param name="fileName">The name of the file to get the size of</param>
    /// <returns>The size of the file in bytes</returns>
    public override long getFileSize(string fileName)
    {
        OnNewStatus("Pokouším se získat velikost souboru" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
        if (!logined)
            login();
        sendCommand("SIZE" + " " + fileName);
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
    public void login()
    {
        //SslStream sslStream = new SslStream(client.GetStream(), false);
        OnNewStatus("Přihlašuji se na FTP Server");
#region Pokud nejsem připojený, přihlásím se na server bez uživatele
        try
        {
            if (clientSocket == null || clientSocket.Connected == false)
                loginWithoutUser();
        }
        catch (Exception ex)
        {
            OnNewStatus("Couldn't connect to remote server" + ": " + ex.Message);
            return;
        //throw new Exception("Couldn't connect to remote server" + ": " + ex.Message);
        }

#endregion
#region Poté se přihlásím příkazem remoteUser
        if (debug)
            OnNewStatus("USER" + " " + remoteUser);
        sendCommand2("USER" + " " + remoteUser);
        if (!(retValue == 331 || retValue == 230))
        {
            cleanup();
            throw new Exception(reply.Substring(4));
        }

#endregion
#region Hodnota 230 znamená že mohu pokračovat bez hesla. Pokud ne, pošlu své heslo příkazem PASS
        var bad = false;
        if (retValue != 230)
        {
            if (debug)
                OnNewStatus("PASS xxx");
            sendCommand2("PASS" + " " + remotePass);
            if (!(retValue == 230 || retValue == 202))
            {
                cleanup();
                bad = true;
            //throw new Exception(reply.Substring(4));
            }
        }

#endregion
        logined = !bad;
        if (logined)
            OnNewStatus("Logined to" + " " + remoteHost);
        else
            OnNewStatus("Not logined to" + " " + remoteHost);
    }

    /// <summary>
    /// Establishes a TCP socket connection to the FTP server without user authentication.
    /// Creates a Stream-type TCP socket and connects to the remote server.
    /// Throws IOException if the response code is not 220.
    /// This method must always be called before authenticating with user credentials.
    /// </summary>
    public void loginWithoutUser()
    {
        OnNewStatus("Přihlašuji se na FTP Server bez uživatele");
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress v4 = null;
        var remoteHost2 = remoteHost;
        if (FtpHelper.IsSchemaFtp(remoteHost))
            remoteHost2 = FtpHelper.ReplaceSchemaFtp(remoteHost2);
        if (!IPAddress.TryParse(remoteHost2, out v4))
            v4 = Dns.GetHostEntry(remoteHost).AddressList[0].MapToIPv4();
        var data = v4.ToString();
        var ep = new IPEndPoint(v4, remotePort);
        try
        {
            clientSocket.Connect(ep);
        }
        catch (Exception ex)
        {
            // During first attemp to connect to sunamo.cz Message = "A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond 185.8.239.101:21"
            throw new Exception("Couldn't connect to remote server");
        }

        readReply();
        if (retValue != 220)
        {
            close();
            throw new Exception(reply.Substring(4));
        }
    }

    /// <summary>
    /// Vypíše chyby na K z A4
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
    /// Vypíšu na K info o certifikátu A1. A2 zda vypsat podrobně.
    /// </summary>
    /// <param name = "remoteCertificate"></param>
    /// <param name = "verbose"></param>
    private void showCertificateInfo(X509Certificate remoteCertificate, bool verbose)
    {
        OnNewStatus("Certficate Information for:\n{0}\n", remoteCertificate.GetName());
        OnNewStatus("Valid From: \n{0}", remoteCertificate.GetEffectiveDateString());
        OnNewStatus("Valid To: \n{0}", remoteCertificate.GetExpirationDateString());
        OnNewStatus("Certificate Format: \n{0}\n", remoteCertificate.GetFormat());
        OnNewStatus("Issuer Name: \n{0}", remoteCertificate.GetIssuerName());
        if (verbose)
        {
            OnNewStatus("Serial Number: \n{0}", remoteCertificate.GetSerialNumberString());
            OnNewStatus("Hash: \n{0}", remoteCertificate.GetCertHashString());
            OnNewStatus("Key Algorithm: \n{0}", remoteCertificate.GetKeyAlgorithm());
            OnNewStatus("Key Algorithm Parameters: \n{0}", remoteCertificate.GetKeyAlgorithmParametersString());
            OnNewStatus("Public Key: \n{0}", remoteCertificate.GetPublicKeyString());
        }
    }
}