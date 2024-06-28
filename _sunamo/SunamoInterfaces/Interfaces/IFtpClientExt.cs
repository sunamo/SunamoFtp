namespace SunamoFtp;


internal interface IFtpClientExt
{
    bool IsInFormatOfAlbum(string folderName);
    string SlashWwwSlash { get; }
    string WwwSlash { get; }
    string Www { get; }
}