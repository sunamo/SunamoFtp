namespace SunamoFtp.FtpClients;

public class FTP : FtpBase
{
    private static Type type = typeof(FTP);
    /// <summary>
    /// Velikost bloku po které se čte.
    /// </summary>
    private static readonly int BLOCK_SIZE = 1024;
    /// <summary>
    /// Konstanta obsahující ASCII kódování
    /// </summary>
    private readonly Encoding ASCII = Encoding.ASCII;
    /// <summary>
    /// Buffer je pouhý 1KB
    /// </summary>
    private readonly byte[] buffer = new byte[BLOCK_SIZE];
    /// <summary>
    ///
    /// </summary>
    private int bytes;
    /// <summary>
    ///
    /// </summary>
    private Socket clientSocket;
    /// <summary>
    /// Zda se vypisují příkazy na konzoli.
    /// </summary>
    private bool debug;
    private readonly IFtpClientExt ftpClient;
    private bool isUpload;
    private string mes;
    private string reply;
    /// <summary>
    /// Hodnotu kterou ukládá třeba readReply, který volá třeba sendCommand
    /// </summary>
    private int retValue;
    private bool startup = true;
    /// <summary>
    /// Stream který se používá při downloadu.
    /// </summary>
    private Stream stream;
    /// <summary>
    /// Stream který se používá při uploadu a to tak že do něho zapíšu jeho M Write
    /// </summary>
    private Stream stream2;
    /// <summary>
    /// Zda se používá PP stream(binární přenos) místo clientSocket(ascii převod)
    /// </summary>
    private bool useStream;
    /// <summary>
    /// IK, OOP.
    /// </summary>
    public FTP(IFtpClientExt ftpClient)
    {
        this.ftpClient = ftpClient;
        debug = false;
    }
    /// <summary>
    /// aktuální vzdálený adresář.
    /// </summary>
    //string remotePath;
    private string remotePath
    {
        get => ps.ActualPath;
        set
        {
        }
    }
    /// <summary>
    /// Nastaví zda se používá binární přenos.
    /// </summary>
    /// <param name="value"></param>
    public void setUseStream(bool value)
    {
        useStream = value;
    }
    /// <summary>
    /// S PP remotePath na A1
    /// </summary>
    /// <param name="remotePath"></param>
    public void setRemotePath(string remotePath)
    {
        OnNewStatus("Byl nastavena cesta ftp na" + " " + remotePath);
        if (remotePath == ftpClient.WwwSlash)
        {
            if (ps.ActualPath != ftpClient.WwwSlash)
                while (ps.CanGoToUpFolder)
                    //ps.RemoveLastToken();
                    goToUpFolder();
            //chdirLite("www");
        }
        else
        {
            ps.ActualPath = remotePath;
        }
    }
    /// <summary>
    /// G aktuální vzdálený adresář.
    /// </summary>
    public string getRemotePath()
    {
        return remotePath;
    }
    public override void LoginIfIsNot(bool startup)
    {
        base.startup = startup;
        if (!logined) login();
    }
    /// <summary>
    /// Pokud nejsem přihlášený, přihlásím se M login
    /// Vytvořím objekt Socket metodou createDataSocket ze které budu přidávat znaky
    /// Zavolám příkaz NLST s A1,
    /// Skrz objekt Socket získám bajty, které okamžitě přidávám do řetězce
    /// Odpověď získám M readReply a G
    /// </summary>
    /// <param name="mask"></param>
    public List<string> getFileList(string mask)
    {
        OnNewStatus("Získávám seznam souborů ze složky" + " " + ps.ActualPath + " " + "příkazem NLST");
        #region MyRegion
        if (!logined) login();
        var cSocket = createDataSocket();
        sendCommand("NLST" + " " + mask);
        if (!(retValue == 150 || retValue == 125)) throw new Exception(reply.Substring(4));
        mes = "";
        #endregion
        #region MyRegion
        while (true)
        {
            var bytes = cSocket.Receive(buffer, buffer.Length, 0);
            mes += ASCII.GetString(buffer, 0, bytes);
            if (bytes < buffer.Length) break;
        }
        string[] seperator = ["\r\n"];
        var mess = mes.Split(seperator, StringSplitOptions.RemoveEmptyEntries).ToList();
        cSocket.Close();
        #endregion
        readReply();
        if (retValue != 226) throw new Exception(reply.Substring(4));
        return mess;
    }
    public override void goToUpFolderForce()
    {
        if (FtpLogging.GoToUpFolder) OnNewStatus("Přecházím do nadsložky" + " " + ps.ActualPath);
        sendCommand("CWD " + "..");
        ps.RemoveLastTokenForce();
        NewStatusNewFolder();
    }
    private void NewStatusNewFolder()
    {
        OnNewStatus("Nová složka je" + " " + ps.ActualPath);
    }
    public override void goToUpFolder()
    {
        if (ps.CanGoToUpFolder)
        {
            sendCommand("CWD " + "..");
            ps.RemoveLastToken();
        }
        else
        {
            OnNewStatus("Program nemohl přejít do nadsložky" + ".");
        }
    }
    public override Dictionary<string, List<string>> getFSEntriesListRecursively(List<string> slozkyNeuploadovatAVS)
    {
        if (!logined) login();
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
        if (!logined) login();
        if (FtpLogging.GoToFolder) OnNewStatus("Přecházím do složky" + " " + remoteFolder);
        var actualPath = ps.ActualPath;
        var dd = remoteFolder.Length - 1;
        if (actualPath == remoteFolder) return;
        setRemotePath(ftpClient.WwwSlash);
        actualPath = ps.ActualPath;
        // Vzdálená složka začíná s aktuální cestou == vzdálená složka je delší. Pouze přejdi hloubš
        if (remoteFolder.StartsWith(actualPath))
        {
            remoteFolder = remoteFolder.Substring(actualPath.Length);
            var tokens = remoteFolder.Split(new[] { ps.Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in tokens) CreateDirectoryIfNotExists(item);
        }
        // Vzdálená složka nezačíná aktuální cestou,
        else
        {
            setRemotePath(ftpClient.WwwSlash);
            var tokens = remoteFolder.Split(new[] { ps.Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (var i = ps.indexZero; i < tokens.Count; i++) CreateDirectoryIfNotExists(tokens[i]);
        }
    }
    /// <summary>
    /// Před zavoláním této metody se musí musí zjistit zda první znak je d(adresář) nebo -(soubor)
    /// </summary>
    /// <param name="entry"></param>
    /// <summary>
    /// Posílám příkaz SIZE. Pokud nejsem nalogovaný, přihlásím se.
    /// </summary>
    /// <param name="fileName"></param>
    public override long getFileSize(string fileName)
    {
        OnNewStatus("Pokouším se získat velikost souboru" + " " + UH.Combine(false, ps.ActualPath, fileName));
        if (!logined) login();
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
        if (FtpHelper.IsSchemaFtp(remoteHost)) remoteHost2 = FtpHelper.ReplaceSchemaFtp(remoteHost2);
        if (!IPAddress.TryParse(remoteHost2, out v4)) v4 = Dns.GetHostEntry(remoteHost).AddressList[0].MapToIPv4();
        var d = v4.ToString();
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
    /// <param name="sender"></param>
    /// <param name="certificate"></param>
    /// <param name="chain"></param>
    /// <param name="errors"></param>
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
    /// <param name="remoteCertificate"></param>
    /// <param name="verbose"></param>
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
    /// <summary>
    /// Vypíšu velmi pokročilé informace o certifikaci
    /// </summary>
    /// <param name="serverName"></param>
    /// <param name="sslStream"></param>
    /// <param name="verbose"></param>
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
    /// <param name="Csocket"></param>
    public void getSslStream(Socket Csocket)
    {
        RemoteCertificateValidationCallback callback = OnCertificateValidation;
        var _sslStream = new SslStream(new NetworkStream(Csocket));//,new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
        try
        {
            _sslStream.AuthenticateAsClient(
                remoteHost,
                null,
                SslProtocols.Ssl3 | SslProtocols.Tls,
                true);
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
    /// <param name="mode"></param>
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
        if (retValue != 200) throw new Exception(reply.Substring(4));
    }
    /// <summary>
    /// Stáhne soubor A1 do lok. souboru A2. Navazuje pokud A3.
    /// Pokud A2 bude null, M vyhodí výjimku
    /// Pokud neexistuje, vytvořím jej a hned zavřu. Načtu jej do FS s FileMode Open
    /// Pokud otevřený soubor nemá velikost 0, pošlu příkaz REST čímž nastavím offset
    /// Pokud budeme navazovat, posunu v otevřeném souboru na konec
    /// Pošlu příkaz RETR a všechny přijaté bajty zapíšu
    /// </summary>
    /// <param name="remFileName"></param>
    /// <param name="locFileName"></param>
    /// <param name="resume"></param>
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
        if (string.IsNullOrEmpty(locFileName)) throw new Exception("Musíte zadat jméno souboru do kterého chcete stáhnout");
        if (!logined) login();
        setBinaryMode(true);
        #endregion
        #region Pokud neexistuje, vytvořím jej a hned zavřu. Načtu jej do FS s FileMode Open
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
                if (retValue != 350) offset = 0;
            }
            #endregion
            #region Pokud budeme navazovat, posunu v otevřeném souboru na konec
            if (offset > 0)
            {
                if (debug) OnNewStatus("seeking to" + " " + offset);
                var npos = output.Seek(offset, SeekOrigin.Begin);
                OnNewStatus("new pos=" + npos);
            }
            #endregion
        }
        #region Pošlu příkaz RETR a všechny přijaté bajty zapíšu
        sendCommand("RETR" + " " + UH.GetFileName(remFileName));
        if (!(retValue == 150 || retValue == 125)) throw new Exception(reply.Substring(4));
        while (true)
        {
            bytes = cSocket.Receive(buffer, buffer.Length, 0);
            output.Write(buffer, 0, bytes);
            if (bytes <= 0) break;
        }
        output.Close();
        if (cSocket.Connected) cSocket.Close();
        OnNewStatus("");
        readReply();
        if (!(retValue == 226 || retValue == 250)) throw new Exception(reply.Substring(4));
        #endregion
        return true;
    }
    /// <summary>
    /// Pošlu příkaz PASV a příhlásím se pokud nejsem
    /// Získám socket, z něho stream a pokud navazuzuji, pokusím se nastavit binární mód a offset podle toho kolik dat už na serveru bylo.
    /// Pokud je tam nějaký offset, pošlu opět příkaz rest s offsetem, abych nastavil od čeho budu uploadovat
    /// Pošlu příkaz STOR s jménem souboru a zapíšu všechny bajty z souboru do bufferu byte[]
    /// Nastavím offset v lokálním souboru.  I když nevím prož když pak uploaduji M stream2.Write s offsetem 0. Zavřu socket i proud a přečtu odpověď serveru. Pokud nebyla 226 nebo 250, VV
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="resume"></param>
    public void uploadSecure(string fileName, bool resume)
    {
        var path = UH.Combine(false, ps.ActualPath, fileName);
        OnUploadingNewStatus(path);
        #region Pošlu příkaz PASV a příhlásím se pokud nejsem
        sendCommand("PASV");
        if (retValue != 227) throw new Exception(reply.Substring(4));
        if (!logined) login();
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
        #region Pokud je tam nějaký offset, pošlu opět příkaz rest s offsetem, abych nastavil od čeho budu uploadovat
        if (offset > 0)
        {
            sendCommand("REST" + " " + offset);
            if (retValue != 350) offset = 0;
        }
        #endregion
        #region Pošlu příkaz STOR s jménem souboru a zapíšu všechny bajty z souboru do bufferu byte[]
        sendCommand("STOR" + " " + Path.GetFileName(fileName));
        if (!(retValue == 125 || retValue == 150)) throw new Exception(reply.Substring(4));
        var input = File.OpenRead(fileName);
        var bufferFile = new byte[input.Length];
        input.Read(bufferFile, 0, bufferFile.Length);
        input.Close();
        #endregion
        #region Nastavím offset v lokálním souboru.  I když nevím prož když pak uploaduji M stream2.Write s offsetem 0. Zavřu socket i proud a přečtu odpověď serveru. Pokud nebyla 226 nebo 250, VV
        if (offset != 0)
        {
            if (debug) OnNewStatus("seeking to" + " " + offset);
            input.Seek(offset, SeekOrigin.Begin);
        }
        OnNewStatus("Uploading file" + " " + fileName + " to " + remotePath);
        if (cSocket.Connected)
        {
            stream2.Write(bufferFile, 0, bufferFile.Length);
            OnNewStatus("File Upload");
        }
        stream2.Close();
        if (cSocket.Connected) cSocket.Close();
        readReply();
        if (!(retValue == 226 || retValue == 250)) throw new Exception(reply.Substring(4));
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
        while (!reader.EndOfStream) vr.Add(reader.ReadLine());
        reader.Close();
        response.Close();
        return vr;
    }
    /// <summary>
    /// Pokud nejsem nalogovaný, přihlásím se.
    /// Pokud mám navazovat, zjistím si veliksot vzdáleného souboru.
    /// Pošlu příkaz REST s offsetem a poté už STOR
    /// Pokud byl offset, seeknu se v souboru a čtu bajty a zapisuji je na server metodou cSocket.Send
    /// Pokud jsem připojený, zavřu objekt cSocket a zavřu návratovou hodnotu
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="resume"></param>
    public void upload(string fileName, bool resume, byte[] buffer)
    {
        OnNewStatus("Uploaduji" + " " + UH.Combine(false, ps.ActualPath, fileName));
        #region Tento kód mi nedovolil často nauploadovat ani jeden soubor, takže ho nahradím speciálními třídami .net
        #region Pokud nejsem nalogovaný, přihlásím se.
        if (!logined) login();
        sendCommand("PASV");
        #endregion
        #region Pokud mám navazovat, zjistím si veliksot vzdáleného souboru.
        var cSocket = createDataSocket();
        long offset = 0;
        isUpload = true;
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
        #region Pošlu příkaz REST s offsetem a poté už STOR
        if (offset > 0)
        {
            sendCommand("REST" + " " + offset);
            if (retValue != 350) offset = 0;
        }
        sendCommand("STOR" + " " + Path.GetFileName(fileName));
        if (!(retValue == 125 || retValue == 150)) throw new Exception(reply.Substring(4));
        #endregion
        #region Pokud byl offset, seeknu se v souboru a čtu bajty a zapisuji je na server metodou cSocket.Send
        // open input stream to read source file
        var input = new FileStream(fileName, FileMode.Open);
        if (offset != 0)
        {
            if (debug) OnNewStatus("seeking to" + " " + offset);
            input.Seek(offset, SeekOrigin.Begin);
        }
        OnNewStatus("Uploading file" + " " + fileName + " to " + remotePath);
        while ((bytes = input.Read(buffer, 0, buffer.Length)) > 0) cSocket.Send(buffer, bytes, 0);
        input.Close();
        #endregion
        #region Pokud jsem připojený, zavřu objekt cSocket a zavřu návratovou hodnotu
        if (cSocket.Connected) cSocket.Close();
        readReply();
        if (!(retValue == 226 || retValue == 250)) throw new Exception(reply.Substring(4));
        #endregion
        #endregion
        #region MyRegion
        #endregion
    }
    /// <summary>
    /// Odstraním vzdálený soubor jména A1.
    /// </summary>
    /// <param name="fileName"></param>
    public override bool deleteRemoteFile(string fileName)
    {
        OnNewStatus("Odstraňuji ze ftp serveru soubor" + " " + UH.Combine(false, ps.ActualPath, fileName));
        if (!logined) login();
        sendCommand("DELE" + " " + fileName);
        if (retValue != 250) sendCommand("DELE" + " " + WebUtility.UrlDecode(fileName));
        return true;
    }
    /// <summary>
    /// Pošlu příkaz RNFR A1 a když bude odpoveď 350, tak RNTO
    /// </summary>
    /// <param name="oldFileName"></param>
    /// <param name="newFileName"></param>
    public override void renameRemoteFile(string oldFileName, string newFileName)
    {
        OnNewStatus("Ve složce" + " " + ps.ActualPath + " " + "přejmenovávám soubor" + " " + oldFileName + " na " + newFileName);
        if (!logined) login();
        sendCommand("RNFR" + " " + oldFileName);
        if (retValue != 350) throw new Exception(reply.Substring(4));
        sendCommand("RNTO" + " " + newFileName);
        if (retValue != 250) throw new Exception(reply.Substring(4));
    }
    /// <summary>
    /// Vytvoří v akt. složce A1 adresář A1 příkazem MKD
    /// </summary>
    /// <param name="dirName"></param>
    public override bool mkdir(string dirName)
    {
        OnNewStatus("Vytvářím adresář" + " " + UH.Combine(true, ps.ActualPath, dirName));
        if (!logined) login();
        sendCommand("MKD " + dirName);
        if (retValue != 250 && retValue != 257) throw new Exception(reply.Substring(4));
        chdirLite(dirName);
        return true;
    }
    /// <summary>
    /// Smaže v akt. složce adr. A1 příkazem RMD
    /// </summary>
    /// <param name="dirName"></param>
    public override bool rmdir(List<string> slozkyNeuploadovatAVS, string dirName)
    {
        OnNewStatus("Mažu adresář" + " " + UH.Combine(true, ps.ActualPath, dirName));
        if (!logined) login();
        sendCommand("RMD " + dirName);
        if (retValue != 250)
        {
            if (retValue == 550)
                DeleteRecursively(slozkyNeuploadovatAVS, dirName, 0, new List<DirectoriesToDeleteFtp>());
            else
                throw new Exception(reply.Substring(4));
        }
        return true;
    }
    /// <summary>
    /// Změním akt. adresář na A1 a S remotePath A1 příkazem CWD
    /// </summary>
    /// <param name="dirName"></param>
    public override void CreateDirectoryIfNotExists(string dirName)
    {
        if (dirName == "." || dirName == "..") return;
        if (!ExistsFolder(dirName))
            mkdir(dirName);
        else
            chdirLite(dirName);
        //ps.AddToken(dirName);
    }
    public override void chdirLite(string dirName)
    {
        if (!logined) login();
        if (dirName != "")
        {
            if (dirName[dirName.Length - 1] == "/"[0]) dirName = dirName.Substring(0, dirName.Length - 1);
        }
        else
        {
            dirName = ftpClient.Www;
        }
        var nalezenAdresar = false;
        List<string> fse = null;
        var vseMa8 = false;
        while (!vseMa8)
        {
            vseMa8 = true;
            fse = ListDirectoryDetails();
            foreach (var item in fse)
            {
                var tokens = item.Split(' ').Length; //SHSplit.Split(item, "").Count;
                if (tokens < 8) vseMa8 = false;
            }
        }
        foreach (var item in fse)
        {
            string fn = null;
            if (FtpHelper.IsFile(item, out fn) == FileSystemType.Folder)
                if (fn == dirName)
                {
                    nalezenAdresar = true;
                    break;
                }
        }
        if (!nalezenAdresar)
        {
            if (mkdir(dirName))
            {
                //this.remotePath = dirName;
            }
        }
        else
        {
            sendCommand("CWD " + dirName);
            if (retValue != 250) throw new Exception(reply.Substring(4));
            if (dirName == "..")
                ps.RemoveLastToken();
            else
                ps.AddToken(dirName);
        }
    }
    /// <summary>
    /// Pošlu příkaz QUIT pokud clientSocket není null
    /// Zavřu, nulluji clientSocket a nastavím logined na false.
    /// </summary>
    public void close()
    {
        OnNewStatus("Uzavírám ftp relaci");
        if (clientSocket != null) sendCommand("QUIT");
        cleanup();
        OnNewStatus("Closing" + "." + "..");
    }
    /// <summary>
    /// Nastavím debugovací mod
    /// </summary>
    /// <param name="debug"></param>
    public void setDebug(bool debug)
    {
        this.debug = debug;
    }
    /// <summary>
    /// Přečtu do PP reply M ResponseMsg když používám Stream nebo readLine
    /// </summary>
    private void readReply()
    {
        if (useStream)
        {
            reply = ResponseMsg();
        }
        else
        {
            mes = "";
            reply = readLine();
            retValue = int.Parse(reply.Substring(0, 3));
        }
    }
    /// <summary>
    /// Zavřu, nulluji clientSocket a nastavím logined na false.
    /// </summary>
    private void cleanup()
    {
        if (clientSocket != null)
        {
            clientSocket.Close();
            clientSocket = null;
        }
        logined = false;
    }
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
            if (bytes < buffer.Length) break;
        }
        var mess = SHSplit.SplitChar(mes, '\n');
        // Rozdělím získaný string \n a vezmu předposlední prvek, nebo první, který pak vrátím
        if (mes.Length > 2)
            mes = mess[mess.Count - 2];
        else
            mes = mess[0];
        //Když na 3. straně není mezera, zavolám tuto M znovu
        if (!mes.Substring(3, 1).Equals("")) return readLine();
        if (debug)
            for (var k = 0; k < mess.Count - 1; k++) OnNewStatus(mess[k]);
        return mes;
    }
    /// <summary>
    /// Zapíšu do PP stream A1.
    /// </summary>
    /// <param name="message"></param>
    private void WriteMsg(string message)
    {
        var en = new ASCIIEncoding();
        var WriteBuffer = new byte[1024];
        WriteBuffer = en.GetBytes(message);
        stream.Write(WriteBuffer, 0, WriteBuffer.Length);
        //NewStatus(" WRITE:" + message);
    }
    /// <summary>
    /// Přečtu všechny bajty z PP stream.
    /// Uložím do retValue a G celý výstup.
    /// </summary>
    private string ResponseMsg()
    {
        var enc = new ASCIIEncoding();
        var serverbuff = new byte[1024];
        var count = 0;
        while (true)
        {
            var buff = new byte[2];
            var bytes = stream.Read(buff, 0, 1);
            if (bytes == 1)
            {
                serverbuff[count] = buff[0];
                count++;
                if (buff[0] == '\n') break;
            }
            else
            {
                break;
            }
            ;
        }
        ;
        var retval = enc.GetString(serverbuff, 0, count);
        //NewStatus(" READ:" + retval);
        retValue = int.Parse(retval.Substring(0, 3));
        return retval;
    }
    /// <summary>
    /// Získám bajty z A1, pošlu odpověď a uložím PP reply a retValue
    /// </summary>
    /// <param name="command"></param>
    public void sendCommand(string command)
    {
        #region Původní metoda sendCommand
        var cmdBytes = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());
        if (useStream)
            WriteMsg(command + "\r\n");
        else
            clientSocket.Send(cmdBytes, cmdBytes.Length, 0);
        readReply();
        #endregion
    }
    private void sendCommand2(string command)
    {
        #region Původní metoda sendCommand
        var cmdBytes = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());
        if (useStream)
            WriteMsg(command + "\r\n");
        else
            clientSocket.Send(cmdBytes, cmdBytes.Length, 0);
        readReply();
        #endregion
    }
    /// <summary>
    /// Nastavím pasivní způsob přenosu(příkaz PASV)
    /// Získám IP adresu v řetězci z reply
    /// Získám do pole intů jednotlivé části IP adresy a spojím je do řetězce s tečkama
    /// Port získám tak čtvrtou část ip adresy bitově posunu o 8 a sečtu s pátou částí. Získám Socket, O IPEndPoint a pokusím se připojit na tento objekt.
    /// </summary>
    public Socket createDataSocket()
    {
        #region Nastavím pasivní způsob přenosu(příkaz PASV)
        sendCommand("PASV");
        if (retValue != 227) throw new Exception(reply.Substring(4));
        #endregion
        #region Získám IP adresu v řetězci z reply
        var index1 = reply.IndexOf('(');
        var index2 = reply.IndexOf(')');
        var ipData = reply.Substring(index1 + 1, index2 - index1 - 1);
        var parts = new int[6];
        var len = ipData.Length;
        var partCount = 0;
        var buf = "";
        #endregion
        #region Získám do pole intů jednotlivé části IP adresy a spojím je do řetězce s tečkama
        for (var i = 0; i < len && partCount <= 6; i++)
        {
            var ch = char.Parse(ipData.Substring(i, 1));
            if (char.IsDigit(ch))
                buf += ch;
            else if (ch != ',') throw new Exception("Malformed PASV reply" + ": " + reply);
            #region Pokud je poslední znak čárka,
            if (ch == ',' || i + 1 == len)
                try
                {
                    parts[partCount++] = int.Parse(buf);
                    buf = "";
                }
                catch (Exception ex)
                {
                    throw new Exception("Malformed PASV reply" + ": " + reply);
                }
            #endregion
        }
        var ipAddress = parts[0] + "." + parts[1] + "." +
                        parts[2] + "." + parts[3];
        #endregion
        #region Port získám tak čtvrtou část ip adresy bitově posunu o 8 a sečtu s pátou částí. Získám Socket, O IPEndPoint a pokusím se připojit na tento objekt.
        var port = (parts[4] << 8) + parts[5];
        var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var ep = new IPEndPoint(Dns.Resolve(ipAddress).AddressList[0], port);
        try
        {
            s.Connect(ep);
        }
        catch (Exception ex)
        {
            throw new Exception("Can't connect to remoteserver");
        }
        return s;
        #endregion
    }
    public void uploadSecureFolder()
    {
        OnNewStatus("Byla volána metoda uploadSecureFolder která je prázdná");
        // Zkontrolovat zda se první nauploadoval _.txt
    }
    #region IPv6
    #endregion
    #region OK metody
    /// <summary>
    /// OK
    /// </summary>
    /// <param name="slozkyNeuploadovatAVS"></param>
    /// <param name="dirName"></param>
    /// <param name="i"></param>
    /// <param name="td"></param>
    public override void DeleteRecursively(List<string> slozkyNeuploadovatAVS, string dirName, int i, List<DirectoriesToDeleteFtp> td)
    {
        chdirLite(dirName);
        var smazat = ListDirectoryDetails();
        foreach (var item2 in smazat)
        {
            var fst = FtpHelper.IsFile(item2, out var fn);
            if (fst == FileSystemType.File)
                deleteRemoteFile(fn);
            else if (fst == FileSystemType.Folder) DeleteRecursively(slozkyNeuploadovatAVS, fn, i, td);
            //////DebugLogger.Instance.WriteLine(item2);
        }
        goToUpFolderForce();
        rmdir(slozkyNeuploadovatAVS, dirName);
    }
    public override void DebugActualFolder()
    {
        ThrowEx.NotImplementedMethod();
    }
    public override void D(string what, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }
    public override void Connect()
    {
        ThrowEx.NotImplementedMethod();
    }
    #endregion
}