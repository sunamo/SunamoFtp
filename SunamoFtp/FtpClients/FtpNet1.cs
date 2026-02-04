namespace SunamoFtp.FtpClients;

public partial class FtpNet : FtpBase
{
    /// <summary>
    ///     OK
    ///     NLST
    ///     Returns only file names, without folders or links
    ///     If not logged in, log in using login method
    ///     Vytvořím objekt Socket metodou CreateDataSocket ze které budu přidávat znaky
    ///     Zavolám příkaz NLST s A1,
    ///     Skrz objekt Socket získám bajty, které okamžitě přidávám do řetězce
    ///     Odpověď získám M ReadReply a G
    /// </summary>
    /// <param name = "mask"></param>
    public List<string> GetFileList(string mask)
    {
        if (ExceptionCount < MaxExceptionCount)
        {
            OnNewStatus("Getting file list from folder" + " " + PathSelector.ActualPath + " " + "using NLST command");
            var result = new StringBuilder();
            FtpWebRequest reqFTP = null;
            StreamReader reader = null;
            WebResponse response = null;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(GetActualPath()));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(RemoteUser, RemotePass);
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
                if (reader != null)
                    reader.Dispose();
                if (response != null)
                    response.Dispose();
                OnNewStatus("Error get filelist" + ": " + ex.Message);
                if (ExceptionCount == 2)
                {
                    ExceptionCount = 0;
                    var downloadFiles = new List<string>();
                    return downloadFiles;
                }
                else
                {
                    return GetFileList(mask);
                }
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
                if (response != null)
                    response.Dispose();
            }
        }

        {
            ExceptionCount = 0;
            var downloadFiles = new List<string>();
            return downloadFiles;
        }
    }

    /// <summary>
    ///     OK
    ///     MKD
    ///     Creates directory if it does not exist
    /// </summary>
    /// <param name = "dirName"></param>
    public override void CreateDirectoryIfNotExists(string dirName)
    {
        if (dirName != "")
        {
            dirName = Path.GetFileName(dirName.TrimEnd('/'));
            if (dirName[dirName.Length - 1] == "/"[0])
                dirName = dirName.Substring(0, dirName.Length - 1);
        }
        else
        {
            OnNewStatus("Could not create new directory because no name was specified");
            return;
        }

        var directoryFound = false;
        List<string> ftpEntries = null;
        var allHaveEightTokens = false;
        while (!allHaveEightTokens)
        {
            allHaveEightTokens = true;
            ftpEntries = ListDirectoryDetails();
            foreach (var item in ftpEntries)
            {
                var tokens = item.Split(' ').Length; //SHSplit.Split(item, "").Count;
                if (tokens < 8)
                    allHaveEightTokens = false;
            }
        }

        foreach (var item in ftpEntries)
        {
            string fn = null;
            if (FtpHelper.IsFile(item, out fn) == FileSystemType.Folder)
                if (fn == dirName)
                {
                    directoryFound = true;
                    break;
                }
        }

        if (!directoryFound)
        {
            if (Mkdir(dirName))
            {
            }
        }
        else
        {
            PathSelector.AddToken(dirName);
        }
    }

    /// <summary>
    /// Changes current directory on FTP server (lightweight version without full navigation)
    /// </summary>
    /// <param name="dirName">Directory name to change to</param>
    public override void ChdirLite(string dirName)
    {
        // Trim slash from end in dirName variable
        if (dirName != "")
        {
            if (dirName[dirName.Length - 1] == "/"[0])
                dirName = dirName.Substring(0, dirName.Length - 1);
        }
        else
        {
            dirName = MainWindow.Www;
        }

        var directoryFound = false;
        List<string> ftpEntries = null;
        var allHaveEightTokens = false;
        while (!allHaveEightTokens)
        {
            allHaveEightTokens = true;
            ftpEntries = ListDirectoryDetails();
            foreach (var item in ftpEntries)
            {
                var tokens = SHSplit.Split(item, " ").Count;
                if (tokens < 8)
                    allHaveEightTokens = false;
            }
        }

        foreach (var item in ftpEntries)
        {
            string fn = null;
            if (FtpHelper.IsFile(item, out fn) == FileSystemType.Folder)
                if (fn == dirName)
                {
                    directoryFound = true;
                    break;
                }
        }

        if (!directoryFound)
        {
            if (Mkdir(dirName))
            {
            //this.remotePath = dirName;
            }
        }
        else
        {
            if (dirName == "..")
                PathSelector.RemoveLastToken();
            else
                PathSelector.AddToken(dirName);
        }
    }

    /// <summary>
    ///     OK
    ///     MKD
    ///     Vytvoří v akt. složce A1 adresář A1 příkazem MKD
    /// </summary>
    /// <param name = "dirName"></param>
    public override bool Mkdir(string dirName)
    {
        if (ExceptionCount < MaxExceptionCount)
        {
            var adr = UH.Combine(true, PathSelector.ActualPath, dirName);
            OnNewStatus("Creating directory" + " " + adr);
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
                reqFTP.Credentials = new NetworkCredential(RemoteUser, RemotePass);
                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();
                PathSelector.AddToken(dirName);
                ftpStream.Dispose();
                response.Dispose();
                ExceptionCount = 0;
                return true;
            }
            catch (Exception ex)
            {
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (response != null)
                    response.Dispose();
                ExceptionCount++;
                OnNewStatus("Error create new dir" + ": " + ex.Message);
                return Mkdir(dirName);
            }
            finally
            {
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (response != null)
                    response.Dispose();
            }
        }

        ExceptionCount = 0;
        return false;
    }
}