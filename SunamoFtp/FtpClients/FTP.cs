namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy

/// <summary>
/// FTP client implementation using raw socket commands
/// </summary>
public partial class FTP : FtpBase
{
    /// <summary>
    /// Velikost bloku po které se čte.
    /// </summary>
    private static readonly int BLOCK_SIZE = 1024;
    /// <summary>
    /// Konstanta obsahující ASCII kódování
    /// </summary>
    private readonly Encoding ASCII = Encoding.ASCII;
    /// <summary>
    /// Buffer je pouhý 1KB
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
    /// Zda se vypisují příkazy na konzoli.
    /// </summary>
    private bool debug;
    private readonly IFtpClientExt ftpClient;
    private bool isUpload;
    private string mes;
    private string reply;
    /// <summary>
    /// Hodnotu kterou ukládá třeba readReply, který volá třeba sendCommand
    /// </summary>
    private int retValue;
    private new bool startup = true;
    /// <summary>
    /// Stream který se používá při downloadu.
    /// </summary>
    private Stream stream;
    /// <summary>
    /// Stream který se používá při uploadu a to tak že do něho zapíšu jeho M Write
    /// </summary>
    private Stream stream2;
    /// <summary>
    /// Zda se používá PP stream(binární přenos) místo clientSocket(ascii převod)
    /// </summary>
    private bool useStream;
    /// <summary>
    /// IK, OOP.
    /// </summary>
    public FTP(IFtpClientExt ftpClient)
    {
        this.ftpClient = ftpClient;
        debug = false;
    }

    /// <summary>
    /// aktuální vzdálený adresář.
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
    public void setUseStream(bool value)
    {
        useStream = value;
    }

    /// <summary>
    /// Sets the remote FTP path and navigates to it
    /// </summary>
    /// <param name="remotePath">Remote FTP path to navigate to</param>
    public void setRemotePath(string remotePath)
    {
        OnNewStatus("Byl nastavena cesta ftp na" + " " + remotePath);
        if (remotePath == ftpClient.WwwSlash)
        {
            if (PathSelector.ActualPath != ftpClient.WwwSlash)
                while (PathSelector.CanGoToUpFolder)
                    //PathSelector.RemoveLastToken();
                    goToUpFolder();
        //chdirLite("www");
        }
        else
        {
            PathSelector.ActualPath = remotePath;
        }
    }

    /// <summary>
    /// G aktuální vzdálený adresář.
    /// </summary>
    public string getRemotePath()
    {
        return remotePath;
    }

    public override void LoginIfIsNot(bool startup)
    {
        base.startup = startup;
        if (!logined)
            login();
    }

    /// <summary>
    /// Pokud nejsem přihlášený, přihlásím se M login
    /// Vytvořím objekt Socket metodou createDataSocket ze které budu přidávat znaky
    /// Zavolám příkaz NLST text A1,
    /// Skrz objekt Socket získám bajty, které okamžitě přidávám do řetězce
    /// Odpověď získám M readReply a G
    /// </summary>
    /// <param name = "mask"></param>
    public List<string> getFileList(string mask)
    {
        OnNewStatus("Získávám seznam souborů ze složky" + " " + PathSelector.ActualPath + " " + "příkazem NLST");
#region MyRegion
        if (!logined)
            login();
        var cSocket = createDataSocket();
        sendCommand("NLST" + " " + mask);
        if (!(retValue == 150 || retValue == 125))
            throw new Exception(reply.Substring(4));
        mes = "";
#endregion
#region MyRegion
        while (true)
        {
            var bytes = cSocket.Receive(buffer, buffer.Length, 0);
            mes += ASCII.GetString(buffer, 0, bytes);
            if (bytes < buffer.Length)
                break;
        }

        string[] seperator = ["\r\n"];
        var mess = mes.Split(seperator, StringSplitOptions.RemoveEmptyEntries).ToList();
        cSocket.Close();
#endregion
        readReply();
        if (retValue != 226)
            throw new Exception(reply.Substring(4));
        return mess;
    }

    public override void goToUpFolderForce()
    {
        if (FtpLogging.GoToUpFolder)
            OnNewStatus("Přecházím do nadsložky" + " " + PathSelector.ActualPath);
        sendCommand("CWD " + "..");
        PathSelector.RemoveLastTokenForce();
        NewStatusNewFolder();
    }

    private void NewStatusNewFolder()
    {
        OnNewStatus("Nová složka je" + " " + PathSelector.ActualPath);
    }

    public override void goToUpFolder()
    {
        if (PathSelector.CanGoToUpFolder)
        {
            sendCommand("CWD " + "..");
            PathSelector.RemoveLastToken();
        }
        else
        {
            OnNewStatus("Program nemohl přejít do nadsložky" + ".");
        }
    }
}