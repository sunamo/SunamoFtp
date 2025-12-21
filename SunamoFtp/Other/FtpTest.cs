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
    public static void SetConnectionInfo(FtpAbstract ftpBase, string un, string pw)
    {
        // Wedos
        ftpBase.setRemoteHost("185.8.239.101");
        ftpBase.setRemoteUser(un);
        ftpBase.setRemotePass(pw);
    }


}