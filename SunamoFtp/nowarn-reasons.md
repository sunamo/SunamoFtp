# NoWarn — důvody

## CS8600, CS8602, CS8625 — nullable reference warningy
**Zdroj:** starý FTP klientský kód (FTP1-4.cs, FtpNet.cs) s `Nullable=enable` zapnutým dodatečně, multi-target net8/9/10.
**Proč nelze opravit plošně:** vyžaduje individuální ověření nullability u desítek call sites přes 3 target frameworky.
**Kdy přehodnotit:** při postupné revizi nullability jednotlivých FTP klientů.

## CA2022 — Avoid inexact read with FileStream.Read
**Zdroj:** FTP2.cs čte data do bufferu bez ověření počtu skutečně přečtených bytů.
**Proč nelze opravit bez rizika:** oprava vyžaduje smyčku okolo Read() a ověření chování při přenosu souborů přes FTP - riziko regrese v produkčním přenosovém kódu bez možnosti reálného otestování.
**Kdy přehodnotit:** při refaktoringu FTP přenosové logiky s reálným testováním.

## SYSLIB0014 — WebRequest.Create je obsolete
**Zdroj:** starší FTP klienti (FtpNet1/2.cs, FTP2.cs) používají WebRequest pro FTP protokol.
**Proč nelze opravit bez rizika:** HttpClient nepodporuje FTP protokol nativně, migrace by vyžadovala jiný přístup (FtpWebRequest náhradu), riziko regrese bez reálného testování proti FTP serveru.
**Kdy přehodnotit:** při refaktoringu FTP klientů na modernější knihovnu.

## SYSLIB0039, CS0618 — SslProtocols.Tls/Ssl3 jsou obsolete
**Zdroj:** FTP2.cs explicitně nastavuje staré TLS/SSL protokoly pro kompatibilitu se staršími FTP servery.
**Proč nelze opravit bez rizika:** cílové FTP servery mohou vyžadovat právě tyto starší protokoly, změna by mohla rozbít připojení.
**Kdy přehodnotit:** při ověření, které FTP servery se skutečně používají a jaké protokoly podporují.

## CS8604, CS8618, CS8622 — další nullable warningy
**Zdroj:** stejné jako CS8600/CS8602/CS8625 výše.
**Proč nelze opravit plošně:** viz výše.
**Kdy přehodnotit:** při postupné revizi nullability jednotlivých FTP klientů.

## SYSLIB0058 — SslStream diagnostické property jsou obsolete
**Zdroj:** FTP2.cs vypisuje diagnostiku SSL spojení (HashAlgorithm, CipherAlgorithm atd.) pro debug účely.
**Proč nelze opravit bez rizika:** NegotiatedCipherSuite náhrada má jinou strukturu, změna by vyžadovala přepis debug výpisu bez možnosti reálného otestování proti FTP serveru.
**Kdy přehodnotit:** při refaktoringu FTP diagnostiky.
