namespace SunamoFtp._public.SunamoInterfaces.Interfaces;


public interface IFtpClientExt
{
    bool IsInFormatOfAlbum(string folderName);
    string SlashWwwSlash { get; }
    string WwwSlash { get; }
    string Www { get; }
}