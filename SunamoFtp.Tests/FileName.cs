using Limilabs.FTP.Client;
using SunamoFtp.FtpClients;
using SunamoFtp.Other;

public class MyClass
{
    public static void FtpDll(string un, string pw)
    {
        var ftpDll = new FtpDllWrapper(new Ftp());
        FtpTest.SetConnectionInfo(ftpDll, un, pw);
        var ftp = ftpDll.Client;

        ftp.Connect(ftpDll.remoteHost);
        ftp.Login(ftpDll.remoteUser, ftpDll.remotePass);

        var folder = "a";
        ftpDll.DebugActualFolder();
        ftpDll.DebugAllEntries();

        ftp.CreateFolder("/" + folder);
        ftp.ChangeFolder(folder);
        ftpDll.DebugActualFolder();
        ftp.UploadFiles("D:\a.txt");

        ftp.Close();
    }
}