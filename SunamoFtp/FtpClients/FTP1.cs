namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FTP : FtpBase
{
    public override Dictionary<string, List<string>> getFSEntriesListRecursively(List<string> slozkyNeuploadovatAVS)
    {
        if (!logined)
            login();
        // Musí se do ní ukládat cesta k celé složce, nikoliv jen název aktuální složky
        var projeteSlozky = new List<string>();
        var vr = new Dictionary<string, List<string>>();
        var fse = ListDirectoryDetails();
        var actualPath = ps.ActualPath;
        OnNewStatus("Získávám rekurzivní seznam souborů ze složky" + " " + actualPath);
        foreach (var item in fse)
        {
            var fz = item[0];
            if (fz == '-')
            {
                if (vr.ContainsKey(actualPath))
                {
                    vr[actualPath].Add(item);
                }
                else
                {
                    var ppk = new List<string>();
                    ppk.Add(item);
                    vr.Add(actualPath, ppk);
                }
            }
            else if (fz == 'd')
            {
                var folderName = SHJoin.JoinFromIndex(8, ' ', item.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList());
                ////DebugLogger.Instance.WriteLine("Název alba22: " + folderName);
                if (!FtpHelper.IsThisOrUp(folderName))
                {
                    if (vr.ContainsKey(actualPath))
                    {
                        vr[actualPath].Add(item + "/");
                    }
                    else
                    {
                        var ppk = new List<string>();
                        ppk.Add(item + "/");
                        vr.Add(actualPath, ppk);
                    }
                //getFSEntriesListRecursively(slozkyNeuploadovatAVS, projeteSlozky, vr, ps.ActualPath, folderName);
                }
            }
            else
            {
                throw new Exception("Nepodporovaný typ objektu");
            }
        }

        if (ps.indexZero > 0)
        {
            setRemotePath(ftpClient.WwwSlash);
            // TODO: Zatím mi to bude stačit jen o 1 úrověň výše
            if (ps.indexZero == 1)
            {
                goToUpFolderForce();
                fse = ListDirectoryDetails();
                actualPath = ps.ActualPath;
                foreach (var item in fse)
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
                            if (vr.ContainsKey(actualPath))
                            {
                                vr[actualPath].Add(item);
                            }
                            else
                            {
                                var ppk = new List<string>();
                                ppk.Add(item);
                                vr.Add(actualPath, ppk);
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

        return vr;
    }

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
        var actualPath = ps.ActualPath;
        var dd = remoteFolder.Length - 1;
        if (actualPath == remoteFolder)
            return;
        setRemotePath(ftpClient.WwwSlash);
        actualPath = ps.ActualPath;
        // Vzdálená složka začíná text aktuální cestou == vzdálená složka je delší. Pouze přejdi hloubš
        if (remoteFolder.StartsWith(actualPath))
        {
            remoteFolder = remoteFolder.Substring(actualPath.Length);
            var tokens = remoteFolder.Split(new[] { ps.Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in tokens)
                CreateDirectoryIfNotExists(item);
        }
        // Vzdálená složka nezačíná aktuální cestou,
        else
        {
            setRemotePath(ftpClient.WwwSlash);
            var tokens = remoteFolder.Split(new[] { ps.Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (var i = ps.indexZero; i < tokens.Count; i++)
                CreateDirectoryIfNotExists(tokens[i]);
        }
    }

    /// <summary>
    /// Před zavoláním této metody se musí musí zjistit zda první znak je data(adresář) nebo -(soubor)
    /// </summary>
    /// <param name = "entry"></param>
    /// <summary>
    /// Posílám příkaz SIZE. Pokud nejsem nalogovaný, přihlásím se.
    /// </summary>
    /// <param name = "fileName"></param>
    public override long getFileSize(string fileName)
    {
        OnNewStatus("Pokouším se získat velikost souboru" + " " + UH.Combine(false, ps.ActualPath, fileName));
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
    /// Tuto metodu je třeba nutno zavolat ihned po nastavení proměnných.
    /// Pokud nejsem připojený, přihlásím se na server bez uživatele
    /// Poté se přihlásím příkazem remoteUser
    /// Hodnota 230 znamená že mohu pokračovat bez hesla. Pokud ne, pošlu své heslo příkazem PASS
    /// Nastavím že jsem přihlášený a pokusím se nastavit vzdálený adresář remotePath
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
    /// Získám socket TCP typu Stream. Toto je důležité a proto se tato metoda musí vždy volat.
    /// Naloguji se na vzdálený server - zatím bez uživatele.
    /// Vyhodím výjimku IOException pokud vrácená hodnota nebude 220 a pošlu příkaz quit
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