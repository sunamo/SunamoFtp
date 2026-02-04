namespace SunamoFtp.FtpClients;

public partial class FtpNet : FtpBase
{
    /// <summary>
    ///     OK
    ///     LIST
    ///     Returns folders, files and links
    /// </summary>
    public override List<string> ListDirectoryDetails()
    {
        var result = new List<string>();
        if (ExceptionCount < MaxExceptionCount)
        {
            StreamReader reader = null;
            FtpWebResponse response = null;
            var path = UH.Combine(true, RemoteHost + ":" + RemotePort, PathSelector.ActualPath);
            try
            {
                // Get the object used to communicate with the server.
                var request = (FtpWebRequest)WebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(RemoteUser, RemotePass);
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
    ///     Deletes remote file with specified name.
    /// </summary>
    /// <param name = "fileName"></param>
    public override bool DeleteRemoteFile(string fileName)
    {
        var result = true;
        if (ExceptionCount < MaxExceptionCount)
        {
            OnNewStatus("Deleting file from FTP server" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
            FtpWebRequest reqFTP = null;
            StreamReader sr = null;
            Stream datastream = null;
            FtpWebResponse response = null;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(GetActualPath(fileName)));
                reqFTP.Credentials = new NetworkCredential(RemoteUser, RemotePass);
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
                return DeleteRemoteFile(fileName);
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
    ///     Sends SIZE command. If not logged in, logs in.
    /// </summary>
    /// <param name = "fileName"></param>
    public override long GetFileSize(string fileName)
    {
        long fileSize = 0;
        if (ExceptionCount < MaxExceptionCount)
        {
            OnNewStatus("Getting file size" + " " + UH.Combine(false, PathSelector.ActualPath, fileName));
            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;
            FtpWebResponse response = null;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(GetActualPath(fileName)));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(RemoteUser, RemotePass);
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
                return GetFileSize(fileName);
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
    public override bool Download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        if (!FtpHelper.IsSchemaFtp(remFileName))
            remFileName = GetActualPath(remFileName);
        if (string.IsNullOrEmpty(locFileName))
        {
            OnNewStatus("Empty locFileName parameter was passed to download method");
            return false;
        }

        OnNewStatus("Downloading" + " " + remFileName);
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
                    OnNewStatus("File " + remFileName + " could not be downloaded because file " + locFileName + " could not be deleted");
                    return false;
                }
            }
            else
            {
                OnNewStatus("Soubor" + " " + remFileName + " " + "nemohl být stažen, protože soubor" + " " + locFileName + " " + "existoval již to disku a nebylo povoleno jeho smazání");
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
                reqFTP.Credentials = new NetworkCredential(RemoteUser, RemotePass);
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
                return Download(remFileName, locFileName, deleteLocalIfExists);
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