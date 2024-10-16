namespace SunamoFtp.Other;

public class FtpTest
{
    /// <summary>
    ///     AppData.ci.GetCommonSettings(CommonSettingsKeys.ftp_wedos_user)
    ///     AppData.ci.GetCommonSettings(CommonSettingsKeys.ftp_wedos_pw)
    /// </summary>
    /// <param name="ftpBase"></param>
    /// <param name="un"></param>
    /// <param name="pw"></param>
    private static void SetConnectionInfo(FtpAbstract ftpBase, string un, string pw)
    {
        // Wedos
        ftpBase.setRemoteHost("185.8.239.101");
        ftpBase.setRemoteUser(un);
        ftpBase.setRemotePass(pw);
    }

    public static void FtpDll(string un, string pw)
    {
        var ftpDll = new FtpDllWrapper(new Ftp());
        SetConnectionInfo(ftpDll, un, pw);
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