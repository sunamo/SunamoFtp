namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FtpNet : FtpBase
{
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
                if (response != null)
                    response.Dispose();
                pocetExc++;
                OnNewStatus("Command LIST error" + ": " + ex.Message);
                return ListDirectoryDetails();
            }
            finally
            {
                if (response != null)
                    response.Dispose();
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
    /// <param name = "fileName"></param>
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
                if (sr != null)
                    sr.Dispose();
                if (datastream != null)
                    datastream.Dispose();
                if (response != null)
                    response.Dispose();
                return deleteRemoteFile(fileName);
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
    /// <param name = "fileName"></param>
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
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (response != null)
                    response.Dispose();
                pocetExc++;
                return getFileSize(fileName);
            }
            finally
            {
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (response != null)
                    response.Dispose();
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
    /// <param name = "remFileName"></param>
    /// <param name = "locFileName"></param>
    /// <param name = "resume"></param>
    public override bool download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        if (!FtpHelper.IsSchemaFtp(remFileName))
            remFileName = GetActualPath(remFileName);
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
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (outputStream != null)
                    outputStream.Dispose();
                if (response != null)
                    response.Dispose();
                pocetExc++;
                return download(remFileName, locFileName, deleteLocalIfExists);
            }
            finally
            {
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (outputStream != null)
                    outputStream.Dispose();
                if (response != null)
                    response.Dispose();
            }

            pocetExc = 0;
            return true;
        }

        pocetExc = 0;
        return false;
    }
}