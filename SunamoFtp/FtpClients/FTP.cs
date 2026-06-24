namespace SunamoFtp.FtpClients;

public partial class FTP : FtpBase
{
    private static readonly int BLOCK_SIZE = 1024;
    private readonly Encoding ASCII = Encoding.ASCII;
    // Buffer is only 1KB
    private readonly byte[] buffer = new byte[BLOCK_SIZE];
    private int bytes;
    private Socket clientSocket;
    // Indicates whether to output commands to console.
    private bool isDebug;
    private readonly IFtpClientExt ftpClient;
    private bool isUpload;
    private string message;
    private string reply;
    // Value stored by ReadReply, which is called by SendCommand
    private int retValue;
    // Removed unused field: isStartupPhase
    // Stream used for download.
    private Stream stream;
    // Stream used for upload by writing to it via Write method
    private Stream stream2;
    // Indicates whether to use stream (binary transfer) instead of clientSocket (ASCII conversion)
    private bool useStream;

    public FTP(IFtpClientExt ftpClient)
    {
        this.ftpClient = ftpClient;
        isDebug = false;
    }

    // current remote directory.
     //string remotePath;
    private string remotePath
    {
        get => PathSelector.ActualPath;
        set
        {
        }
    }

    public void SetUseStream(bool useBinaryMode)
    {
        useStream = useBinaryMode;
    }

    public void SetRemotePath(string remotePath)
    {
        OnNewStatus("FTP path set to" + " " + remotePath);
        if (remotePath == ftpClient.WwwSlash)
        {
            if (PathSelector.ActualPath != ftpClient.WwwSlash)
                while (PathSelector.CanGoToUpFolder)
                    //PathSelector.RemoveLastToken();
                    GoToUpFolder();
        //ChdirLite("www");
        }
        else
        {
            PathSelector.ActualPath = remotePath;
        }
    }

    public string GetRemotePath() => remotePath;

    public override void LoginIfIsNot(bool isInitialLogin)
    {
        base.IsInitialLogin = isInitialLogin;
        if (!IsLoggedIn)
            Login();
    }

    public List<string> GetFileList(string mask)
    {
        OnNewStatus("Getting file list from folder" + " " + PathSelector.ActualPath + " " + "using NLST command");
#region MyRegion
        if (!IsLoggedIn)
            Login();
        var clientSocket = CreateDataSocket();
        SendCommand("NLST" + " " + mask);
        if (!(retValue == 150 || retValue == 125))
            throw new Exception(reply.Substring(4));
        message = "";
#endregion
#region MyRegion
        while (true)
        {
            var bytes = clientSocket.Receive(buffer, buffer.Length, 0);
            message += ASCII.GetString(buffer, 0, bytes);
            if (bytes < buffer.Length)
                break;
        }

        string[] seperator = ["\r\n"];
        var mess = message.Split(seperator, StringSplitOptions.RemoveEmptyEntries).ToList();
        clientSocket.Close();
#endregion
        ReadReply();
        if (retValue != 226)
            throw new Exception(reply.Substring(4));
        return mess;
    }

    public override void GoToUpFolderForce()
    {
        if (FtpLogging.GoToUpFolder)
            OnNewStatus("Navigating to parent folder" + " " + PathSelector.ActualPath);
        SendCommand("CWD " + "..");
        PathSelector.RemoveLastTokenForce();
        NewStatusNewFolder();
    }

    private void NewStatusNewFolder()
    {
        OnNewStatus("New folder is" + " " + PathSelector.ActualPath);
    }

    public override void GoToUpFolder()
    {
        if (PathSelector.CanGoToUpFolder)
        {
            SendCommand("CWD " + "..");
            PathSelector.RemoveLastToken();
        }
        else
        {
            OnNewStatus("Could not navigate to parent folder" + ".");
        }
    }
}
