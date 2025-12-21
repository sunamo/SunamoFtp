namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FTP : FtpBase
{
    private static Type type = typeof(FTP);
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
        get => ps.ActualPath;
        set
        {
        }
    }

    /// <summary>
    /// Nastaví zda se používá binární přenos.
    /// </summary>
    /// <param name = "value"></param>
    public void setUseStream(bool value)
    {
        useStream = value;
    }

    /// <summary>
    /// text PP remotePath na A1
    /// </summary>
    /// <param name = "remotePath"></param>
    public void setRemotePath(string remotePath)
    {
        OnNewStatus("Byl nastavena cesta ftp na" + " " + remotePath);
        if (remotePath == ftpClient.WwwSlash)
        {
            if (ps.ActualPath != ftpClient.WwwSlash)
                while (ps.CanGoToUpFolder)
                    //ps.RemoveLastToken();
                    goToUpFolder();
        //chdirLite("www");
        }
        else
        {
            ps.ActualPath = remotePath;
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
        OnNewStatus("Získávám seznam souborů ze složky" + " " + ps.ActualPath + " " + "příkazem NLST");
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
            OnNewStatus("Přecházím do nadsložky" + " " + ps.ActualPath);
        sendCommand("CWD " + "..");
        ps.RemoveLastTokenForce();
        NewStatusNewFolder();
    }

    private void NewStatusNewFolder()
    {
        OnNewStatus("Nová složka je" + " " + ps.ActualPath);
    }

    public override void goToUpFolder()
    {
        if (ps.CanGoToUpFolder)
        {
            sendCommand("CWD " + "..");
            ps.RemoveLastToken();
        }
        else
        {
            OnNewStatus("Program nemohl přejít do nadsložky" + ".");
        }
    }
}