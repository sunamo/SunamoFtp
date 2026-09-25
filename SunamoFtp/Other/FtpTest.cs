namespace SunamoFtp.Other;

public class FtpTest
{
    public static void SetConnectionInfo(FtpAbstract ftpBase, string username, string password)
    {
        // Wedos server configuration
        ftpBase.SetRemoteHost("185.8.239.101");
        ftpBase.SetRemoteUser(username);
        ftpBase.SetRemotePass(password);
    }
}
