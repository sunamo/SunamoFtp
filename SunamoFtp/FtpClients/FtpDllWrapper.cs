namespace SunamoFtp.FtpClients;

public class FtpDllWrapper : FtpBaseNew
{
    public Ftp Client;

    public FtpDllWrapper(Ftp ftp)
    {
        Client = ftp;
    }

    public override void ChdirLite(string directoryName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void CreateDirectoryIfNotExists(string directoryName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void WriteDebugLog(string context, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void DebugActualFolder()
    {
        //InitApp.Logger.WriteLine("Actual directory" + ":", Client.GetCurrentFolder());
    }

    public override void DebugAllEntries()
    {
        //InitApp.Logger.WriteLine("All file entries" + ":");
        //Client.GetList().ForEach(d => InitApp.Logger.WriteLine(d.Name));
    }

    public override void DebugDirChmod(string directoryName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void DeleteRecursively(List<string> foldersToSkip, string directoryName, int i,
        List<DirectoriesToDeleteFtp> directoriesToDelete)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override bool DeleteRemoteFile(string fileName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override bool Download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override long GetFileSize(string filename)
    {
        ThrowEx.NotImplementedMethod();
        return 0;
    }

    public override Dictionary<string, List<string>> GetFSEntriesListRecursively(List<string> foldersToSkip)
    {
        ThrowEx.NotImplementedMethod();
        return null;
    }

    public override void GoToPath(string remoteFolder)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void GoToUpFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void GoToUpFolderForce()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override List<string> ListDirectoryDetails()
    {
        ThrowEx.NotImplementedMethod();
        return null;
    }

    public override void LoginIfIsNot(bool isInitialLogin)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override bool Mkdir(string directoryName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override void RenameRemoteFile(string oldFileName, string newFileName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override bool Rmdir(List<string> foldersToSkip, string directoryName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override
        async Task
        UploadFile(string path)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void Dispose()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void Connect()
    {
        ThrowEx.NotImplementedMethod();
    }
}
