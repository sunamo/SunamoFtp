// variables names: ok
using Limilabs.FTP.Client;
using SunamoFtp.FtpClients;
using SunamoFtp.Other;

/// <summary>
/// Test class for FTP operations
/// </summary>
public class MyClass
{
    /// <summary>
    /// Tests FTP DLL wrapper functionality
    /// </summary>
    /// <param name="username">FTP username</param>
    /// <param name="password">FTP password</param>
    public static void FtpDll(string username, string password)
    {
        var ftpDll = new FtpDllWrapper(new Ftp());
        FtpTest.SetConnectionInfo(ftpDll, username, password);
        var ftp = ftpDll.Client;

        ftp.Connect(ftpDll.RemoteHost);
        ftp.Login(ftpDll.RemoteUser, ftpDll.RemotePass);

        var folder = "a";
        ftpDll.DebugActualFolder();
        ftpDll.DebugAllEntries();

        ftp.CreateFolder("/" + folder);
        ftp.ChangeFolder(folder);
        ftpDll.DebugActualFolder();
        ftp.UploadFiles("D:\\a.txt");

        ftp.Close();
    }
}
