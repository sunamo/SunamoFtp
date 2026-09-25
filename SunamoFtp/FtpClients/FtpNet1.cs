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
                reader?.Dispose();
                response?.Dispose();
                OnNewStatus("Error get filelist" + ": " + ex.Message);
                if (ExceptionCount == 2)
                {
                    ExceptionCount = 0;
                    return new List<string>();
                }
                else
                {
                    return GetFileList(mask);
                }
            }
            finally
            {
                reader?.Dispose();
                response?.Dispose();
            }
        }

        {
            ExceptionCount = 0;
            return new List<string>();
        }
    }

    /// <summary>
    ///     OK
    ///     MKD
    ///     Creates directory if it does not exist
    /// </summary>
    /// <param name = "directoryName"></param>
    public override void CreateDirectoryIfNotExists(string directoryName)
    {
        if (directoryName != "")
        {
            directoryName = Path.GetFileName(directoryName.TrimEnd('/'));
            if (directoryName[directoryName.Length - 1] == "/"[0])
                directoryName = directoryName.Substring(0, directoryName.Length - 1);
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
                if (fn == directoryName)
                {
                    directoryFound = true;
                    break;
                }
        }

        if (!directoryFound)
        {
            if (Mkdir(directoryName))
            {
            }
        }
        else
        {
            PathSelector.AddToken(directoryName);
        }
    }

    /// <summary>
    /// Changes current directory on FTP server (lightweight version without full navigation)
    /// </summary>
    /// <param name="directoryName">Directory name to change to</param>
    public override void ChdirLite(string directoryName)
    {
        // Trim slash from end in directoryName variable
        if (directoryName != "")
        {
            if (directoryName[directoryName.Length - 1] == "/"[0])
                directoryName = directoryName.Substring(0, directoryName.Length - 1);
        }
        else
        {
            directoryName = MainWindow.Www;
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
                if (fn == directoryName)
                {
                    directoryFound = true;
                    break;
                }
        }

        if (!directoryFound)
        {
            if (Mkdir(directoryName))
            {
            //this.remotePath = directoryName;
            }
        }
        else
        {
            if (directoryName == "..")
                PathSelector.RemoveLastToken();
            else
                PathSelector.AddToken(directoryName);
        }
    }

    /// <summary>
    ///     OK
    ///     MKD
    ///     Vytvoří v akt. složce A1 adresář A1 příkazem MKD
    /// </summary>
    /// <param name = "directoryName"></param>
    public override bool Mkdir(string directoryName)
    {
        if (ExceptionCount < MaxExceptionCount)
        {
            var adr = UH.Combine(true, PathSelector.ActualPath, directoryName);
            OnNewStatus("Creating directory" + " " + adr);
            FtpWebRequest reqFTP = null;
            FtpWebResponse response = null;
            Stream ftpStream = null;
            try
            {
                // directoryName = name of the directory to create.
                var uri = new Uri(GetActualPath(directoryName));
                reqFTP = (FtpWebRequest)WebRequest.Create(uri);
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(RemoteUser, RemotePass);
                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();
                PathSelector.AddToken(directoryName);
                ftpStream.Dispose();
                response.Dispose();
                ExceptionCount = 0;
                return true;
            }
            catch (Exception ex)
            {
                ftpStream?.Dispose();
                response?.Dispose();
                ExceptionCount++;
                OnNewStatus("Error creating new directory" + ": " + ex.Message);
                return Mkdir(directoryName);
            }
            finally
            {
                ftpStream?.Dispose();
                response?.Dispose();
            }
        }

        ExceptionCount = 0;
        return false;
    }
}