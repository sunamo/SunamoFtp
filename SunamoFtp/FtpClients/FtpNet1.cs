namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FtpNet : FtpBase
{
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
    /// <param name = "mask"></param>
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
                if (reader != null)
                    reader.Dispose();
                if (response != null)
                    response.Dispose();
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
                if (reader != null)
                    reader.Dispose();
                if (response != null)
                    response.Dispose();
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
                if (tokens < 8)
                    vseMa8 = false;
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
            if (dirName[dirName.Length - 1] == "/"[0])
                dirName = dirName.Substring(0, dirName.Length - 1);
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
                if (tokens < 8)
                    vseMa8 = false;
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
    /// <param name = "dirName"></param>
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
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (response != null)
                    response.Dispose();
                pocetExc++;
                OnNewStatus("Error create new dir" + ": " + ex.Message);
                return mkdir(dirName);
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
        return false;
    }
}