namespace SunamoFtp.Base;

public abstract class FtpBaseNew : FtpAbstract, IDisposable
{
    public abstract void DebugAllEntries();
    public abstract void DebugDirChmod(string dir);

    public abstract void Dispose();

    public abstract
#if ASYNC
 Task
#else
void  
#endif
UploadFile(string path);
}
