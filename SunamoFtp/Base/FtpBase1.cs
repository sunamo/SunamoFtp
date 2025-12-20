// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
namespace SunamoFtp.Base;
public abstract partial class FtpBase : FtpAbstract
{
    /// <summary>
    ///     OK
    ///     LIST
    ///     Tato metoda není vstupní, nevolej ji, zavolej místo toho getFSEntriesListRecursively text 1 parametrem
    /// </summary>
    /// <param name = "projiteSlozky"></param>
    /// <param name = "vr"></param>
    /// <param name = "p"></param>
    /// <param name = "folderName"></param>
    public void getFSEntriesListRecursively(List<string> slozkyNeuploadovatAVS, List<string> projiteSlozky, Dictionary<string, List<string>> vr, string folderName)
    {
        LoginIfIsNot(startup);
        var nextPath = UH.Combine(true, ps.ActualPath, folderName);
        if (!projiteSlozky.Contains(nextPath))
        {
            NewStatus("Složka do které se mělo přejít" + " " + nextPath + " " + "ještě nebyla v projeté kolekci", []);
            ps.AddToken(folderName);
            projiteSlozky.Add(nextPath);
            var fse = ListDirectoryDetails();
            var actualPath = ps.ActualPath;
            foreach (var item in fse)
            {
                var size = SHJoin.JoinFromIndex(4, ' ', item.Split(' ').ToList());
                var fz = item[0];
                if (fz == '-')
                {
                    if (size != "0")
                        folderSizeRec += ulong.Parse(size.Substring(0, size.IndexOf(' ') + 1));
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
                    var folderName2 = SHJoin.JoinFromIndex(8, ' ', item.Split(' '));
                    if (!FtpHelper.IsThisOrUp(folderName2))
                    {
                        if (slozkyNeuploadovatAVS.Contains(folderName2) && ps.ActualPath == MainWindow.WwwSlash)
                            continue;
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
                    //getFSEntriesListRecursively(slozkyNeuploadovatAVS, projiteSlozky, vr, ps.ActualPath,folderName2);
                    }
                }
                else
                {
                    throw new Exception("Nepodporovaný typ objektu");
                }
            }

            if (ps.CanGoToUpFolder)
                goToUpFolder();
        //ps.RemoveLastToken();
        }
        else
        {
            NewStatus("Složka do které se mělo přejít" + " " + nextPath + " " + "již byla v projeté kolekci", []);
        }
    //ps.ActualPath = p;
    }

    private static Type type = typeof(FtpBase);
    /// <summary>
    ///     OK
    ///     RETR
    ///     Stáhne soubor A1 do lok. souboru A2. Nenavazuje
    /// </summary>
    /// <param name = "remFileName"></param>
    /// <param name = "locFileName"></param>
    public void download(string remFileName, string locFileName)
    {
        download(remFileName, locFileName, true);
    }

    /// <summary>
    ///     OK
    ///     STOR
    ///     Před použitím této metody se musím přesunout do složky do které chci uploadovat.
    ///     Methods to upload file to FTP Server
    /// </summary>
    /// <param name = "_FileName">local source file name</param>
    /// <param name = "_UploadPath">Upload FTP path including Host name</param>
    /// <param name = "_FTPUser">FTP login username</param>
    /// <param name = "_FTPPass">FTP login password</param>
    public void UploadFile(string _FileName)
    {
        var _UploadPath = UH.Combine(false, remoteHost + ":" + remotePort + "/", UH.Combine(true, ps.ActualPath, Path.GetFileName(_FileName)));
        if (reallyUpload)
            UploadFileMain(_FileName, _UploadPath);
    //MainWindow.FileUploaded(_FileName);
    }

    /// <summary>
    ///     OK
    ///     STOR
    ///     Metoda text druhým argumentem, pokud chci uploadovat do jiné složky, než ve které teď jsem
    /// </summary>
    /// <param name = "fullFilePath"></param>
    /// <param name = "actualFtpPath"></param>
    public bool UploadFile(string fullFilePath, string actualFtpPath)
    {
        var _UploadPath = UH.Combine(false, remoteHost + ":" + remotePort + "/" + "/", UH.Combine(false, actualFtpPath, Path.GetFileName(fullFilePath)));
        var vr = true;
        if (reallyUpload)
            vr = UploadFileMain(fullFilePath, _UploadPath);
        return vr;
    }

    /// <summary>
    ///     OK
    ///     STOR
    /// </summary>
    /// <param name = "slozkaTo"></param>
    /// <param name = "iw"></param>
    public bool uploadFolderShared(string slozkaFrom, bool rek, IWorking working)
    {
        var nazevSlozky = Path.GetFileName(slozkaFrom);
        var pathFolder = UH.Combine(true, ps.ActualPath, nazevSlozky);
        slozkaFrom = slozkaFrom.TrimEnd('\\');
        var soubory = Directory.GetFiles(slozkaFrom).ToList();
        var slozky = Directory.GetDirectories(slozkaFrom);
        NewStatus("Uploaduji všechny soubory" + " " + soubory.Count() + " " + "do složky ftp serveru" + " " + pathFolder, []);
        CreateDirectoryIfNotExists(nazevSlozky);
        foreach (var item in soubory)
        {
            if (!working.IsWorking)
                return false;
            UploadFile(item);
        }

        if (rek)
        {
            if (slozky.Count() == 0)
            {
                goToUpFolder();
            }
            else
            {
                foreach (var item in slozky)
                    uploadFolderShared(item, rek, working);
                if (slozky.Count() != 0)
                    goToUpFolder();
            }
        }

        return true;
    }

    /// <summary>
    ///     OK
    ///     Do A1 se zadává název souboru bez cesty
    /// </summary>
    /// <param name = "folder"></param>
    public bool ExistsFolder(string folder)
    {
        var fse = ListDirectoryDetails();
        var data = new List<string>(FtpHelper.GetDirectories(fse));
        return data.Contains(folder);
    }
}