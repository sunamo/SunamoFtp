namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FtpNet : FtpBase
{
    /// <summary>
    ///     OK
    ///     LIST
    ///     Vrátí složky, files i Linky
    /// </summary>
    public override List<string> ListDirectoryDetails()
    {
        var result = new List<string>();
        if (ExceptionCount < MaxExceptionCount)
        {
            StreamReader reader = null;
            FtpWebResponse response = null;
            var _Path = UH.Combine(true, remoteHost + ":" + remotePort, PathSelector.ActualPath);
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
                        result.Add(line);
                    }

                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (response != null)
                    response.Dispose();
                ExceptionCount++;
                OnNewStatus("Command LIST error" + ": " + ex.Message);
                return ListDirectoryDetails();
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }

            ExceptionCount = 0;
            return result;
        }

        ExceptionCount = 0;
        return result;
    }

    /// <summary>
    ///     OK
    ///     DELE
    ///     Odstraním vzdálený soubor jména A1.
    /// </summary>
    /// <param name = "fileName"></param>
    public override bool deleteRemoteFile(string fileName)
    {
        var result = true;
        if (ExceptionCount < MaxExceptionCount)
        {
            OnNewStatus("Odstraňuji ze ftp serveru soubor" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
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
                var responseText = string.Empty;
                response = (FtpWebResponse)reqFTP.GetResponse();
                var size = response.ContentLength;
                datastream = response.GetResponseStream();
                sr = new StreamReader(datastream);
                responseText = sr.ReadToEnd();
                sr.Dispose();
                datastream.Dispose();
                response.Dispose();
            }
            catch (Exception ex)
            {
                //result = false;
                ExceptionCount++;
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

            ExceptionCount = 0;
            return result;
        }

        ExceptionCount = 0;
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
        if (ExceptionCount < MaxExceptionCount)
        {
            OnNewStatus("Pokouším se získat velikost souboru" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
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
                ExceptionCount++;
                return getFileSize(fileName);
            }
            finally
            {
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (response != null)
                    response.Dispose();
            }

            ExceptionCount = 0;
            return fileSize;
        }

        ExceptionCount = 0;
        return fileSize;
    }

    /// <summary>
    /// Downloads file from FTP server using RETR command. If local file exists and deleteLocalIfExists is true, deletes it first.
    /// </summary>
    /// <param name="remFileName">Remote file name to download</param>
    /// <param name="locFileName">Local file path to save to</param>
    /// <param name="deleteLocalIfExists">Whether to delete local file if it already exists</param>
    /// <returns>True if download was successful</returns>
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
                    OnNewStatus("Soubor" + " " + remFileName + " " + "nemohl být stažen, protože soubor" + " " + locFileName + " " + "nešel toDelete");
                    return false;
                }
            }
            else
            {
                OnNewStatus("Soubor" + " " + remFileName + " " + "nemohl být stažen, protože soubor" + " " + locFileName + " " + "existoval již na disku a nebylo povoleno jeho smazání");
                return false;
            }
        }

        if (ExceptionCount < MaxExceptionCount)
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
                ExceptionCount++;
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

            ExceptionCount = 0;
            return true;
        }

        ExceptionCount = 0;
        return false;
    }
}