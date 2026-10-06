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
                response?.Dispose();
                ExceptionCount++;
                OnNewStatus("Command LIST error" + ": " + ex.Message);
                return ListDirectoryDetails();
            }
            finally
            {
                response?.Dispose();
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
    /// <param name = "fileName"></param>@@
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
                sr?.Dispose();
                datastream?.Dispose();
                response?.Dispose();
                return DeleteRemoteFile(fileName);
            }
            finally
            {
                sr?.Dispose();
                datastream?.Dispose();
                response?.Dispose();
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
                ftpStream?.Dispose();
                response?.Dispose();
                ExceptionCount++;
                return GetFileSize(fileName);
            }
            finally
            {
                ftpStream?.Dispose();
                response?.Dispose();
            }

            ExceptionCount = 0;
            return fileSize;
        }

        ExceptionCount = 0;
        return fileSize;
    }

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
                catch (Exception)
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
                ftpStream?.Dispose();
                outputStream?.Dispose();
                response?.Dispose();
                ExceptionCount++;
                return Download(remFileName, locFileName, deleteLocalIfExists);
            }
            finally
            {
                ftpStream?.Dispose();
                outputStream?.Dispose();
                response?.Dispose();
            }

            ExceptionCount = 0;
            return true;
        }

        ExceptionCount = 0;
        return false;
    }
}
