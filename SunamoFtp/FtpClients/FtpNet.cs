namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FtpNet : FtpBase
{
    private static Type type = typeof(FtpNet);
    public override void LoginIfIsNot(bool startup)
    {
        this.startup = startup;
    // Není potřeba se přihlašovat, přihlašovácí údaje posílám při každém příkazu
    }

    public override void goToPath(string remoteFolder)
    {
        if (FtpLogging.GoToFolder)
            OnNewStatus("Přecházím do složky" + " " + remoteFolder);
        var actualPath = ps.ActualPath;
        var dd = remoteFolder.Length - 1;
        if (actualPath == remoteFolder)
            return;
        // Vzdálená složka začíná s aktuální cestou == vzdálená složka je delší. Pouze přejdi hloubš
        if (remoteFolder.StartsWith(actualPath))
        {
            remoteFolder = remoteFolder.Substring(actualPath.Length);
            var tokens = SHSplit.Split(remoteFolder, ps.Delimiter);
            foreach (var item in tokens)
                CreateDirectoryIfNotExists(item);
        }
        // Vzdálená složka nezačíná aktuální cestou,
        else
        {
            ps.ActualPath = "";
            var tokens = SHSplit.Split(remoteFolder, ps.Delimiter);
            var pridat = 0;
            for (var i = 0 + pridat; i < tokens.Count; i++)
                CreateDirectoryIfNotExists(tokens[i]);
        }
    }

    /// <summary>
    ///     RENAME
    ///     Pošlu příkaz RNFR A1 a když bude odpoveď 350, tak RNTO
    /// </summary>
    /// <param name = "oldFileName"></param>
    /// <param name = "newFileName"></param>
    public override void renameRemoteFile(string oldFileName, string newFileName)
    {
        OnNewStatus("Ve složce" + " " + ps.ActualPath + " " + "přejmenovávám soubor" + " " + oldFileName + " na " + newFileName);
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
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (response != null)
                    response.Dispose();
            }
        }

        pocetExc = 0;
    }

    /// <summary>
    /// Před zavoláním této metody se musí musí zjistit zda první znak je d(adresář) nebo -(soubor)
    /// </summary>
    /// <param name = "entry"></param>
     
    /// <summary>
    ///     OK
    ///     RMD
    ///     Smaže v akt. složce adr. A1 příkazem RMD
    ///     Tato metoda se může volat pouze když se bude vědět se složka je prázdná, jinak se program nesmaže a program vypíše
    ///     chybu 550
    /// </summary>
    /// <param name = "dirName"></param>
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
                if (sr != null)
                    sr.Dispose();
                if (datastream != null)
                    datastream.Dispose();
                if (response != null)
                    response.Dispose();
                OnNewStatus("Error delete folder" + ": " + ex.Message);
                return rmdir(slozkyNeuploadovatAVS, dirName);
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
                if (datastream != null)
                    datastream.Dispose();
                if (response != null)
                    response.Dispose();
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
    /// <param name = "slozkyNeuploadovatAVS"></param>
    /// <param name = "dirName"></param>
    public override void DeleteRecursively(List<string> slozkyNeuploadovatAVS, string dirName, int i, List<DirectoriesToDeleteFtp> td)
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
                                while (!deleteRemoteFile(item3))
                                {
                                }

                            goToUpFolderForce();
                            rmdir(new List<string>(), Path.GetFileName(item2.Key.TrimEnd('/')));
                        }
                    }
        }
    }
}