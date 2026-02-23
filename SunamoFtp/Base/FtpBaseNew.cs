namespace SunamoFtp.Base;

public abstract class FtpBaseNew : FtpAbstract, IDisposable
{
    public abstract void Dispose();
    public abstract void DebugAllEntries();
    public abstract void DebugDirChmod(string directoryName);

    public abstract
#if ASYNC
        Task
#else
void
#endif
        UploadFile(string path);
}