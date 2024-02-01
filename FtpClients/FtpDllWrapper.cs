
namespace SunamoFtp.FtpClients;
using SunamoData.Data;
using SunamoExceptions.OnlyInSE;
using SunamoLogger;


public class FtpDllWrapper : FtpBaseNew
{
    public Ftp Client = null;
    static Type type = typeof(FtpDllWrapper);
    public FtpDllWrapper(Ftp ftp)
    {
        Client = ftp;
    }

    public override void chdirLite(string dirName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void CreateDirectoryIfNotExists(string dirName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void D(string what, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void DebugActualFolder()
    {
        InitApp.Logger.WriteLine("Actual dir" + ":", Client.GetCurrentFolder());
    }

    public override void DebugAllEntries()
    {
        InitApp.Logger.WriteLine("All file entries" + ":");
        Client.GetList().ForEach(d => InitApp.Logger.WriteLine(d.Name));

    }

    public override void DebugDirChmod(string dir)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void DeleteRecursively(List<string> slozkyNeuploadovatAVS, string dirName, int i, List<DirectoriesToDelete> td)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override bool deleteRemoteFile(string fileName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override bool download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override long getFileSize(string filename)
    {
        ThrowEx.NotImplementedMethod();
        return 0;
    }

    public override Dictionary<string, List<string>> getFSEntriesListRecursively(List<string> slozkyNeuploadovatAVS)
    {
        ThrowEx.NotImplementedMethod();
        return null;
    }

    public override void goToPath(string slozkaNaHostingu)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void goToUpFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void goToUpFolderForce()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override List<string> ListDirectoryDetails()
    {
        ThrowEx.NotImplementedMethod();
        return null;
    }

    public override void LoginIfIsNot(bool startup)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override bool mkdir(string dirName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override void renameRemoteFile(string oldFileName, string newFileName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override bool rmdir(List<string> slozkyNeuploadovatAVS, string dirName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override
#if ASYNC
async Task
#else
void
#endif
UploadFile(string path)
    {
        throw new NotImplementedException();
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }

    public override void Connect()
    {
        throw new NotImplementedException();
    }
}
