// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy

namespace SunamoFtp.FtpClients;

public class FtpNet : FtpBase
{
    private static Type type = typeof(FtpNet);

    public override void LoginIfIsNot(bool startup)
    {
        this.startup = startup;
        // Není potřeba se přihlašovat, přihlašovácí údaje posílám při každém příkazu
    }

    public override void goToPath(string remoteFolder)
    {
        if (FtpLogging.GoToFolder) OnNewStatus("Přecházím do složky" + " " + remoteFolder);

        var actualPath = ps.ActualPath;
        var dd = remoteFolder.Length - 1;
        if (actualPath == remoteFolder) return;

        // Vzdálená složka začíná s aktuální cestou == vzdálená složka je delší. Pouze přejdi hloubš
        if (remoteFolder.StartsWith(actualPath))
        {
            remoteFolder = remoteFolder.Substring(actualPath.Length);
            var tokens = SHSplit.Split(remoteFolder, ps.Delimiter);
            foreach (var item in tokens) CreateDirectoryIfNotExists(item);
        }
        // Vzdálená složka nezačíná aktuální cestou,
        else
        {
            ps.ActualPath = "";
            var tokens = SHSplit.Split(remoteFolder, ps.Delimiter);
            var pridat = 0;
            for (var i = 0 + pridat; i < tokens.Count; i++) CreateDirectoryIfNotExists(tokens[i]);
        }
    }


    /// <summary>
    ///     RENAME
    ///     Pošlu příkaz RNFR A1 a když bude odpoveď 350, tak RNTO
    /// </summary>
    /// <param name="oldFileName"></param>
    /// <param name="newFileName"></param>
    public override void renameRemoteFile(string oldFileName, string newFileName)
    {
        OnNewStatus("Ve složce" + " " + ps.ActualPath + " " + "přejmenovávám soubor" + " " + oldFileName +
                    " na " + newFileName);

        if (pocetExc < maxPocetExc)
        {
            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;
            FtpWebResponse response = null;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(GetActualPath(oldFileName)));
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFileName;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(remoteUser, remotePass);
                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();
            }
            catch (Exception ex)
            {
                OnNewStatus("Error rename file" + ": " + ex.Message);
                pocetExc++;
                renameRemoteFile(oldFileName, newFileName);
            }
            finally
            {
                if (ftpStream != null) ftpStream.Dispose();
                if (response != null) response.Dispose();
            }
        }

        pocetExc = 0;
    }

    #region Zakomentované metody

    /// <summary>
    /// Před zavoláním této metody se musí musí zjistit zda první znak je d(adresář) nebo -(soubor)
    /// </summary>
    /// <param name="entry"></param>

    #endregion

    #region OK Metody

    /// <summary>
    ///     OK
    ///     RMD
    ///     Smaže v akt. složce adr. A1 příkazem RMD
    ///     Tato metoda se může volat pouze když se bude vědět se složka je prázdná, jinak se program nesmaže a program vypíše
    ///     chybu 550
    /// </summary>
    /// <param name="dirName"></param>
    public override bool rmdir(List<string> slozkyNeuploadovatAVS, string dirName)
    {
        if (pocetExc < maxPocetExc)
        {
            var ma = GetActualPath(dirName).TrimEnd('/');
            OnNewStatus("Mažu adresář" + " " + ma);

            FtpWebRequest clsRequest = null;
            StreamReader sr = null;
            Stream datastream = null;
            FtpWebResponse response = null;
            try
            {
                clsRequest = (FtpWebRequest)WebRequest.Create(new Uri(ma));
                clsRequest.Credentials = new NetworkCredential(remoteUser, remotePass);

                clsRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;

                var result = string.Empty;
                response = (FtpWebResponse)clsRequest.GetResponse();

                var size = response.ContentLength;
                datastream = response.GetResponseStream();
                sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                pocetExc++;
                if (sr != null) sr.Dispose();
                if (datastream != null) datastream.Dispose();
                if (response != null) response.Dispose();
                OnNewStatus("Error delete folder" + ": " + ex.Message);
                return rmdir(slozkyNeuploadovatAVS, dirName);
            }
            finally
            {
                if (sr != null) sr.Dispose();
                if (datastream != null) datastream.Dispose();
                if (response != null) response.Dispose();
            }

            pocetExc = 0;
            return true;
        }

        pocetExc = 0;
        return false;
    }


    /// <summary>
    ///     OK
    ///     DELE + RMD
    /// </summary>
    /// <param name="slozkyNeuploadovatAVS"></param>
    /// <param name="dirName"></param>
    public override void DeleteRecursively(List<string> slozkyNeuploadovatAVS, string dirName, int i,
        List<DirectoriesToDeleteFtp> td)
    {
        i++;
        var smazat = ListDirectoryDetails();
        //bool pridano = false;
        td.Add(new DirectoriesToDeleteFtp { hloubka = i });
        Dictionary<string, List<string>> ds = null;
        foreach (var item in td)
            if (item.hloubka == i)
            {
                if (item.adresare.Count != 0)
                {
                    foreach (var item2 in item.adresare)
                        foreach (var item3 in item2)
                            if (item3.Key == ps.ActualPath)
                                ds = item2;
                }
                else
                {
                    ds = new Dictionary<string, List<string>>();
                }
                //ds = ;
            }

        for (var zValue = 0; zValue < td.Count; zValue++)
        {
            var item = td[zValue];


            if (item.hloubka == i)
                //ds.Add(ps.ActualPath, new List<string>());
                foreach (var item2 in smazat)
                {
                    var fn = "";
                    var fst = FtpHelper.IsFile(item2, out fn);
                    if (fst == FileSystemType.File)
                    {
                        if (ds.ContainsKey(ps.ActualPath))
                        {
                        }
                        else
                        {
                            ds.Add(ps.ActualPath, new List<string>());
                        }

                        var f = ds[ps.ActualPath];
                        f.Add(fn);
                    }
                    else if (fst == FileSystemType.Folder)
                    {
                        ps.AddToken(fn);
                        ds.Add(ps.ActualPath, new List<string>());
                        //pridano = true;
                        DeleteRecursively(slozkyNeuploadovatAVS, fn, i, td);
                    }
                    ////DebugLogger.Instance.WriteLine(item2);
                }
            //item.adresare.Add(ds);
        }

        if (true)
            foreach (var item in td)
                if (item.hloubka == i)
                    item.adresare.Add(ds);
        if (i == 1)
        {
            var smazaneAdresare = new List<string>();
            for (var yValue = td.Count - 1; yValue >= 0; yValue--)
                foreach (var item in td[yValue].adresare)
                    foreach (var item2 in item)
                    {
                        ps.ActualPath = item2.Key;
                        var sa = item2.Key;
                        if (!smazaneAdresare.Contains(sa))
                        {
                            smazaneAdresare.Add(sa);
                            foreach (var item3 in item2.Value)
                                while (!
                                       deleteRemoteFile(item3))
                                {
                                }

                            goToUpFolderForce();
                            rmdir(new List<string>(), Path.GetFileName(item2.Key.TrimEnd('/')));
                        }
                    }
        }
    }

    /// <summary>
    ///     OK
    ///     NLST
    ///     Vrátí pouze názvy souborů, bez složek nebo linků
    ///     Pokud nejsem přihlášený, přihlásím se M login
    ///     Vytvořím objekt Socket metodou createDataSocket ze které budu přidávat znaky
    ///     Zavolám příkaz NLST s A1,
    ///     Skrz objekt Socket získám bajty, které okamžitě přidávám do řetězce
    ///     Odpověď získám M readReply a G
    /// </summary>
    /// <param name="mask"></param>
    public List<string> getFileList(string mask)
    {
        if (pocetExc < maxPocetExc)
        {
            OnNewStatus("Získávám seznam souborů ze složky" + " " + ps.ActualPath + " " + "příkazem NLST");


            var result = new StringBuilder();
            FtpWebRequest reqFTP = null;
            StreamReader reader = null;
            WebResponse response = null;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(GetActualPath()));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(remoteUser, remotePass);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;

                response = reqFTP.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1250"));
                var line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }

                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                return SHSplit.SplitChar(result.ToString(), '\n');
            }
            catch (Exception ex)
            {
                if (reader != null) reader.Dispose();
                if (response != null) response.Dispose();
                OnNewStatus("Error get filelist" + ": " + ex.Message);
                if (pocetExc == 2)
                {
                    pocetExc = 0;
                    var downloadFiles = new List<string>();
                    return downloadFiles;
                }
                else
                {
                    return getFileList(mask);
                }
            }
            finally
            {
                if (reader != null) reader.Dispose();
                if (response != null) response.Dispose();
            }
        }

        {
            pocetExc = 0;
            var downloadFiles = new List<string>();
            return downloadFiles;
        }
    }

    /// <summary>
    ///     OK
    ///     MKD
    ///     Adresář vytvoří pokud nebude existovat
    /// </summary>
    /// <param name="dirName"></param>
    public override void CreateDirectoryIfNotExists(string dirName)
    {
        if (dirName != "")
        {
            dirName = Path.GetFileName(dirName.TrimEnd('/'));
            if (dirName[dirName.Length - 1] == "/"[0]) dirName = dirName.Substring(0, dirName.Length - 1);
        }
        else
        {
            OnNewStatus("Nemohl být vytvořen nový adresář, protože nebyl zadán jeho název");
            return;
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
            }
        }
        else
        {
            ps.AddToken(dirName);
        }
    }

    public override void chdirLite(string dirName)
    {
        // Trim slash from end in dirName variable
        if (dirName != "")
        {
            if (dirName[dirName.Length - 1] == "/"[0]) dirName = dirName.Substring(0, dirName.Length - 1);
        }
        else
        {
            dirName = MainWindow.Www;
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
                var tokens = SHSplit.Split(item, " ").Count;
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
            if (dirName == "..")
                ps.RemoveLastToken();
            else
                ps.AddToken(dirName);
        }
    }

    /// <summary>
    ///     OK
    ///     MKD
    ///     Vytvoří v akt. složce A1 adresář A1 příkazem MKD
    /// </summary>
    /// <param name="dirName"></param>
    public override bool mkdir(string dirName)
    {
        if (pocetExc < maxPocetExc)
        {
            var adr = UH.Combine(true, ps.ActualPath, dirName);

            OnNewStatus("Vytvářím adresář" + " " + adr);
            FtpWebRequest reqFTP = null;
            FtpWebResponse response = null;
            Stream ftpStream = null;
            try
            {
                // dirName = name of the directory to create.
                var uri = new Uri(GetActualPath(dirName));
                reqFTP = (FtpWebRequest)WebRequest.Create(uri);
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(remoteUser, remotePass);
                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();

                ps.AddToken(dirName);

                ftpStream.Dispose();
                response.Dispose();
                pocetExc = 0;
                return true;
            }
            catch (Exception ex)
            {
                if (ftpStream != null) ftpStream.Dispose();
                if (response != null) response.Dispose();
                pocetExc++;
                OnNewStatus("Error create new dir" + ": " + ex.Message);
                return mkdir(dirName);
            }
            finally
            {
                if (ftpStream != null) ftpStream.Dispose();
                if (response != null) response.Dispose();
            }
        }

        pocetExc = 0;
        return false;
    }

    /// <summary>
    ///     OK
    ///     LIST
    ///     Vrátí složky, soubory i Linky
    /// </summary>
    public override List<string> ListDirectoryDetails()
    {
        var vr = new List<string>();
        if (pocetExc < maxPocetExc)
        {
            StreamReader reader = null;
            FtpWebResponse response = null;

            var _Path = UH.Combine(true, remoteHost + ":" + remotePort, ps.ActualPath);
            try
            {
                // Get the object used to communicate with the server.
                var request = (FtpWebRequest)WebRequest.Create(_Path);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(remoteUser, remotePass);

                response = (FtpWebResponse)request.GetResponse();

                var responseStream = response.GetResponseStream();
                reader = new StreamReader(responseStream, Encoding.GetEncoding("windows-1250"));
                if (reader != null)
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        vr.Add(line);
                    }

                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (response != null) response.Dispose();
                pocetExc++;
                OnNewStatus("Command LIST error" + ": " + ex.Message);
                return ListDirectoryDetails();
            }
            finally
            {
                if (response != null) response.Dispose();
            }

            pocetExc = 0;
            return vr;
        }

        pocetExc = 0;
        return vr;
    }

    /// <summary>
    ///     OK
    ///     DELE
    ///     Odstraním vzdálený soubor jména A1.
    /// </summary>
    /// <param name="fileName"></param>
    public override bool deleteRemoteFile(string fileName)
    {
        var vr = true;
        if (pocetExc < maxPocetExc)
        {
            OnNewStatus("Odstraňuji ze ftp serveru soubor" + " " + UH.Combine(false, ps.ActualPath, fileName));
            FtpWebRequest reqFTP = null;
            StreamReader sr = null;
            Stream datastream = null;
            FtpWebResponse response = null;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(GetActualPath(fileName)));

                reqFTP.Credentials = new NetworkCredential(remoteUser, remotePass);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;

                var result = string.Empty;
                response = (FtpWebResponse)reqFTP.GetResponse();
                var size = response.ContentLength;
                datastream = response.GetResponseStream();
                sr = new StreamReader(datastream);
                result = sr.ReadToEnd();

                sr.Dispose();
                datastream.Dispose();
                response.Dispose();
            }
            catch (Exception ex)
            {
                //vr = false;
                pocetExc++;
                OnNewStatus("Error delete file" + ": " + ex.Message);
                if (sr != null) sr.Dispose();
                if (datastream != null) datastream.Dispose();
                if (response != null) response.Dispose();

                return deleteRemoteFile(fileName);
            }
            finally
            {
                if (sr != null) sr.Dispose();
                if (datastream != null) datastream.Dispose();
                if (response != null) response.Dispose();
            }

            pocetExc = 0;
            return vr;
        }

        pocetExc = 0;
        return false;
    }

    /// <summary>
    ///     OK
    ///     SIZE
    ///     Posílám příkaz SIZE. Pokud nejsem nalogovaný, přihlásím se.
    /// </summary>
    /// <param name="fileName"></param>
    public override long getFileSize(string fileName)
    {
        long fileSize = 0;
        if (pocetExc < maxPocetExc)
        {
            OnNewStatus("Pokouším se získat velikost souboru" + " " + UH.Combine(false, ps.ActualPath, fileName));

            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;
            FtpWebResponse response = null;

            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(GetActualPath(fileName)));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(remoteUser, remotePass);
                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();
                fileSize = response.ContentLength;
            }
            catch (Exception ex)
            {
                OnNewStatus("Error get filesize" + ": " + ex.Message);
                if (ftpStream != null) ftpStream.Dispose();
                if (response != null) response.Dispose();
                pocetExc++;
                return getFileSize(fileName);
            }
            finally
            {
                if (ftpStream != null) ftpStream.Dispose();
                if (response != null) response.Dispose();
            }

            pocetExc = 0;
            return fileSize;
        }

        pocetExc = 0;
        return fileSize;
    }

    /// <summary>
    ///     OK
    ///     RETR
    ///     Stáhne soubor A1 do lok. souboru A2. Navazuje pokud A3.
    ///     Pokud A2 bude null, M vyhodí výjimku
    ///     Pokud neexistuje, vytvořím jej a hned zavřu. Načtu jej do FS s FileMode Open
    ///     Pokud otevřený soubor nemá velikost 0, pošlu příkaz REST čímž nastavím offset
    ///     Pokud budeme navazovat, posunu v otevřeném souboru na konec
    ///     Pošlu příkaz RETR a všechny přijaté bajty zapíšu
    /// </summary>
    /// <param name="remFileName"></param>
    /// <param name="locFileName"></param>
    /// <param name="resume"></param>
    public override bool download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        if (!FtpHelper.IsSchemaFtp(remFileName)) remFileName = GetActualPath(remFileName);

        if (string.IsNullOrEmpty(locFileName))
        {
            OnNewStatus("Do metody download byl předán prázdný parametr locFileName");
            return false;
        }

        OnNewStatus("Stahuji" + " " + remFileName);

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
                    OnNewStatus("Soubor" + " " + remFileName + " " + "nemohl být stažen, protože soubor" + " " +
                                locFileName + " " + "nešel smazat");
                    return false;
                }
            }
            else
            {
                OnNewStatus("Soubor" + " " + remFileName + " " + "nemohl být stažen, protože soubor" + " " +
                            locFileName + " " + "existoval již na disku a nebylo povoleno jeho smazání");
                return false;
            }
        }

        if (pocetExc < maxPocetExc)
        {
            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;
            FileStream outputStream = null;
            FtpWebResponse response = null;

            try
            {
                outputStream = new FileStream(locFileName, FileMode.Create);

                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(remFileName));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(remoteUser, remotePass);
                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();
                var cl = response.ContentLength;
                var bufferSize = 2048;
                int readCount;
                var buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
            }
            catch (Exception ex)
            {
                OnNewStatus("Error download file" + ": " + ex.Message);
                if (ftpStream != null) ftpStream.Dispose();
                if (outputStream != null) outputStream.Dispose();
                if (response != null) response.Dispose();
                pocetExc++;
                return download(remFileName, locFileName, deleteLocalIfExists);
            }
            finally
            {
                if (ftpStream != null) ftpStream.Dispose();
                if (outputStream != null) outputStream.Dispose();
                if (response != null) response.Dispose();
            }

            pocetExc = 0;
            return true;
        }

        pocetExc = 0;
        return false;
    }

    /// <summary>
    ///     OK
    ///     LIST
    ///     Toto je vstupní metoda, metodu getFSEntriesListRecursively s 5ti parametry nevolej, ač má stejný název
    ///     Vrátí soubory i složky, ale pozor, složky jsou vždycky až po souborech
    /// </summary>
    /// <param name="slozkyNeuploadovatAVS"></param>
    public override Dictionary<string, List<string>> getFSEntriesListRecursively(List<string> slozkyNeuploadovatAVS)
    {
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
                var folderName = SHJoin.JoinFromIndex(8, ' ', SHSplit.Split(item, ""));

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

        return vr;
    }

    /// <summary>
    ///     OK
    ///     Tuto metodu nepoužívej, protože fakticky způsobuje neošetřenou výjimku, pokud již cesta bude skutečně / a a nebude
    ///     moci se přesunout nikde výš
    /// </summary>
    public override void goToUpFolderForce()
    {
        if (FtpLogging.GoToUpFolder) OnNewStatus("Přecházím do nadsložky" + " " + ps.ActualPath);

        ps.RemoveLastTokenForce();
        OnNewStatusNewFolder();
    }

    /// <summary>
    ///     OK
    /// </summary>
    public override void goToUpFolder()
    {
        if (ps.CanGoToUpFolder)
        {
            ps.RemoveLastToken();
            OnNewStatusNewFolder();
        }
        else
        {
            OnNewStatus("Nemohl jsem přejít do nadsložky" + ".");
        }
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