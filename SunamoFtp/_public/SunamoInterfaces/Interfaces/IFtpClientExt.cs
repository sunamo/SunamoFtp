namespace SunamoFtp._public.SunamoInterfaces.Interfaces;

public interface IFtpClientExt
{
    string SlashWwwSlash { get; }
    string WwwSlash { get; }
    string Www { get; }
    bool IsInFormatOfAlbum(string folderName);
}