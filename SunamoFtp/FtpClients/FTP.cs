namespace SunamoFtp.FtpClients;

/// <summary>
/// FTP client implementation using raw socket commands
/// </summary>
public partial class FTP : FtpBase
{
    /// <summary>
    /// Block size for reading.
    /// </summary>
    private static readonly int BLOCK_SIZE = 1024;
    /// <summary>
    /// Constant containing ASCII encoding
    /// </summary>
    private readonly Encoding ASCII = Encoding.ASCII;
    /// <summary>
    /// Buffer is only 1KB
    /// </summary>
    private readonly byte[] buffer = new byte[BLOCK_SIZE];
    /// <summary>
    ///
    /// </summary>
    private int bytes;
    /// <summary>
    ///
    /// </summary>
    private Socket clientSocket;
    /// <summary>
    /// Indicates whether to output commands to console.
    /// </summary>
    private bool isDebug;
    private readonly IFtpClientExt ftpClient;
    private bool isUpload;
    private string message;
    private string reply;
    /// <summary>
    /// Value stored by ReadReply, which is called by SendCommand
    /// </summary>
    private int retValue;
    // Removed unused field: isStartupPhase
    /// <summary>
    /// Stream used for download.
    /// </summary>
    private Stream stream;
    /// <summary>
    /// Stream used for upload by writing to it via Write method
    /// </summary>
    private Stream stream2;
    /// <summary>
    /// Indicates whether to use stream (binary transfer) instead of clientSocket (ASCII conversion)
    /// </summary>
    private bool useStream;
    /// <summary>
    /// IK, OOP.
    /// </summary>
    public FTP(IFtpClientExt ftpClient)
    {
        this.ftpClient = ftpClient;
        isDebug = false;
    }

    /// <summary>
    /// current remote directory.
    /// </summary>
     //string remotePath;
    private string remotePath
    {
        get => PathSelector.ActualPath;
        set
        {
        }
    }

    /// <summary>
    /// Sets whether to use binary transfer mode
    /// </summary>
    /// <param name="value">True to enable binary transfer, false for ASCII</param>
    public void SetUseStream(bool value)
    {
        useStream = value;
    }

    /// <summary>
    /// Sets the remote FTP path and navigates to it
    /// </summary>
    /// <param name="remotePath">Remote FTP path to navigate to</param>
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

    /// <summary>
    /// Gets current remote directory.
    /// </summary>
    public string GetRemotePath()
    {
        return remotePath;
    }

    public override void LoginIfIsNot(bool isStartup)
    {
        base.IsStartup = isStartup;
        if (!IsLoggedIn)
            Login();
    }

    /// <summary>
    /// Gets list of files matching the specified mask from current FTP directory
    /// </summary>
    /// <param name="mask">File mask pattern</param>
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