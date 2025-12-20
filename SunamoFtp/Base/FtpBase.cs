// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
namespace SunamoFtp.Base;
public abstract partial class FtpBase : FtpAbstract
{
    /// <summary>
    ///     IK, OOP.
    /// </summary>
    public FtpBase()
    {
        ps = new PathSelector("");
        remoteHost = string.Empty;
        //remotePath = ".";
        remoteUser = string.Empty;
        remotePass = string.Empty;
        remotePort = 21;
        logined = false;
    }

    //public abstract void DeleteRecursively(List<string> slozkyNeuploadovatAVS, string dirName, int i, List<DirectoriesToDelete> td);
    public void OnNewStatusNewFolder()
    {
        NewStatus("Nová složka je" + " " + ps.ActualPath, []);
    }

    /// <summary>
    ///     Upload file by FtpWebRequest
    ///     OK
    ///     STOR
    ///     Pokud chceš uploadovat soubor do aktuální složky a zvlolit pouze název souboru na disku, použij metodu UploadFile.
    /// </summary>
    /// <param name = "local"></param>
    /// <param name = "_UploadPath"></param>
    public virtual bool UploadFileMain(string local, string _UploadPath)
    {
        if (pocetExc < maxPocetExc)
        {
            OnNewStatus("Uploaduji" + " " + _UploadPath);
            var _FileInfo = new FileInfo(local);
            Stream _Stream = null;
            FileStream _FileStream = null;
            try
            {
                // Create FtpWebRequest object from the Uri provided
                var _FtpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri(_UploadPath));
                // Provide the WebPermission Credintials
                _FtpWebRequest.Credentials = new NetworkCredential(remoteUser, remotePass);
                _FtpWebRequest.KeepAlive = false;
                // set timeout for 20 seconds
                _FtpWebRequest.Timeout = 20000;
                // Specify the command to be executed.
                _FtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                // Specify the data transfer type.
                _FtpWebRequest.UseBinary = true;
                // Notify the server about the size of the uploaded file
                _FtpWebRequest.ContentLength = _FileInfo.Length;
                // The buffer size is set to 2kb
                var buffLength = 2048;
                var buff = new byte[buffLength];
                // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
                _FileStream = _FileInfo.OpenRead();
                // Stream to which the file to be upload is written
                _Stream = _FtpWebRequest.GetRequestStream();
                // Read from the file stream 2kb at a time
                var contentLen = _FileStream.Read(buff, 0, buffLength);
                // Till Stream content ends
                while (contentLen != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream
                    _Stream.Write(buff, 0, contentLen);
                    contentLen = _FileStream.Read(buff, 0, buffLength);
                }

                // Close the file stream and the Request Stream
                _Stream.Close();
                _Stream.Dispose();
                _FileStream.Close();
                _FileStream.Dispose();
                pocetExc = 0;
            // Close the file stream and the Request Stream
            }
            catch (Exception ex)
            {
                pocetExc++;
                //CleanUp.Streams(_Stream, _FileStream);
                _Stream.Dispose();
                _FileStream.Dispose();
                OnNewStatus("Upload file error" + ": " + ex.Message);
                return UploadFileMain(local, _UploadPath);
            }
            finally
            {
                //CleanUp.Streams(_Stream, _FileStream);
                _Stream.Dispose();
                _FileStream.Dispose();
            }

            pocetExc = 0;
            return true;
        }

        pocetExc = 0;
        return false;
    }

    public void OnUploadingNewStatus(string path)
    {
        OnNewStatus("Uploaduji" + " " + path + " " + "bezpečnou metodou");
    }

    public static event Action<object, object[]> NewStatus;
    /// <summary>
    ///     OK
    /// </summary>
    /// <param name = "s"></param>
    /// <param name = "p"></param>
    public static void OnNewStatus(string text, params object[] p)
    {
        NewStatus(text, p);
    }

    /// <summary>
    ///     STOR
    ///     Nauploaduje pouze soubory které ještě v adresáři nejsou
    /// </summary>
    /// <param name = "files"></param>
    /// <param name = "iw"></param>
    public bool UploadFiles(List<string> files)
    {
        var fse = ListDirectoryDetails();
        foreach (var item in files)
        {
            var fi = new FileInfo(item);
            var fileSize = fi.Length;
            if (!FtpHelper.IsFileOnHosting(item, fse, fileSize))
                UploadFile(item);
        }

        return true;
    }

    public string GetActualPath()
    {
        return UH.Combine(true, remoteHost + ":" + remotePort, ps.ActualPath);
    }

    /// <summary>
    ///     A1 musí být vždy pouze název adresáře/souboru, nikdy to nemůže být plná cesta
    /// </summary>
    /// <param name = "dirName"></param>
    public string GetActualPath(string dirName)
    {
        var text = /*UH.Combine(true,*/ remoteHost + ":" + remotePort + ps.ActualPath + dirName;
        return text.TrimEnd('/');
    }

    /// <summary>
    ///     OK
    ///     Po zavolání této metody v třídě FTP, pokud chceš do adresáře, kde jsi byl před jejím zavoláním, musíš zavolat
    ///     goToUpFolder
    /// </summary>
    /// <param name = "slozkaFrom"></param>
    public bool uploadFolder(string slozkaFrom, bool FTPclass, IWorking working)
    {
        var actPath = ps.ActualPath;
        var vr = uploadFolderShared(slozkaFrom, false, working);
        if (FTPclass)
            goToPath(actPath);
        return vr;
    }

    /// <summary>
    ///     STOR
    /// </summary>
    /// <param name = "slozkaNaLocalu"></param>
    /// <param name = "slozkaNaHostingu"></param>
    /// <param name = "iw"></param>
    public bool uploadFolderRek(string slozkaNaLocalu, string slozkaNaHostingu)
    {
        // Musí to tu být právě kvůli předchozímu řádku List<string> fse = getFSEntriesList(); kdy získávám seznam souborů na FTP serveru
        goToPath(slozkaNaHostingu);
        var directories = Directory.GetDirectories(slozkaNaLocalu);
        var files = Directory.GetFiles(slozkaNaLocalu).ToList();
        OnNewStatus("Uploaduji všechny soubory" + " " + files.Count() + " " + "do složky ftp serveru" + " " + ps.ActualPath);
        if (!UploadFiles(files))
            return false;
        foreach (var item in directories)
            if (!uploadFolderRek(item, UH.Combine(false, slozkaNaHostingu, Path.GetFileName(item))))
                return false;
        return true;
    }

    /// <summary>
    ///     OK
    /// </summary>
    /// <param name = "slozkaNaLocalu"></param>
    public bool uploadFolderRek(string slozkaNaLocalu, IWorking iw)
    {
        return uploadFolderShared(slozkaNaLocalu, true, iw);
    }
}