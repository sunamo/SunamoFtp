namespace SunamoFtp.Other;

/// <summary>
/// Represents the ftp test type.
/// </summary>
public class FtpTest
{
    /// <summary>
    /// Set connection info.
    /// </summary>
    /// <param name="ftpBase">The ftp base.</param>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    public static void SetConnectionInfo(FtpAbstract ftpBase, string username, string password)
    {
        // Wedos server configuration
        ftpBase.SetRemoteHost("185.8.239.101");
        ftpBase.SetRemoteUser(username);
        ftpBase.SetRemotePass(password);
    }
}
