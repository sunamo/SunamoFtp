namespace SunamoFtp.Other;

/// <summary>
/// Helper class for FTP testing and connection setup
/// </summary>
public class FtpTest
{
    /// <summary>
    /// Sets connection information for FTP client
    /// Example: AppData.ci.GetCommonSettings(CommonSettingsKeys.ftp_wedos_user)
    ///          AppData.ci.GetCommonSettings(CommonSettingsKeys.ftp_wedos_pw)
    /// </summary>
    /// <param name="ftpBase">FTP client instance</param>
    /// <param name="username">FTP username</param>
    /// <param name="password">FTP password</param>
    public static void SetConnectionInfo(FtpAbstract ftpBase, string username, string password)
    {
        // Wedos server configuration
        ftpBase.SetRemoteHost("185.8.239.101");
        ftpBase.SetRemoteUser(username);
        ftpBase.SetRemotePass(password);
    }
}