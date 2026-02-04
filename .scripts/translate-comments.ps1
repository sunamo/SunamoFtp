# Script to translate Czech comments to English
$rootPath = "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp"

# Get all .cs files except in obj folders
$files = Get-ChildItem -Path $rootPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }

$translations = @(
    # XML comment translations
    @{Old = 'Zda se vypisují příkazy to konzoli\.'; New = 'Indicates whether to output commands to console.'},
    @{Old = 'Zda se vypisují příkazy na konzoli\.'; New = 'Indicates whether to output commands to console.'},
    @{Old = 'Hodnotu kterou ukládá třeba readReply, který volá třeba sendCommand'; New = 'Value stored by readReply, which is called by sendCommand'},
    @{Old = 'Pokud nejsem přihlášený, přihlásím se M login'; New = 'If not logged in, log in using login method'},
    @{Old = 'příkazem NLST'; New = 'using NLST command'},
    @{Old = 'Musí to tu být právě kvůli předchozímu řádku.*kdy získávám seznam souborů to FTP serveru'; New = 'This is required due to previous line where we get file list from FTP server'},
    @{Old = 'Uploaduji všechny files'; New = 'Uploading all files'},
    @{Old = 'do složky ftp serveru'; New = 'to FTP server folder'},
    @{Old = 'bezpečnou metodou'; New = 'using safe method'},
    @{Old = 'Nová složka je'; New = 'New folder is'},
    @{Old = 'Pokud chceš uploadovat soubor do aktuální složky a zvlolit pouze název souboru to disku, použij metodu UploadFile\.'; New = 'To upload a file to current folder and specify only file name on disk, use UploadFile method.'},
    @{Old = 'aktuální vzdálený adresář\.'; New = 'current remote directory.'},
    @{Old = 'G aktuální vzdálený adresář\.'; New = 'Gets current remote directory.'},
    @{Old = 'Byl nastavena cesta ftp to'; New = 'FTP path was set to'},
    @{Old = 'Velikost bloku po které se čte\.'; New = 'Block size for reading.'},
    @{Old = 'Konstanta obsahující ASCII kódování'; New = 'Constant containing ASCII encoding'},
    @{Old = 'Buffer je pouhý 1KB'; New = 'Buffer is only 1KB'},
    @{Old = 'Stream který se používá při downloadu\.'; New = 'Stream used for download.'},
    @{Old = 'Stream který se používá při uploadu a to tak že do něho zapíšu jeho M Write'; New = 'Stream used for upload by writing to it via Write method'},
    @{Old = 'Zda se používá PP stream\(binární přenos\) místo clientSocket\(ascii převod\)'; New = 'Indicates whether to use stream (binary transfer) instead of clientSocket (ASCII conversion)'},
    @{Old = 'Vypíše chyby to K z A4'; New = 'Outputs errors from certificate validation'},
    @{Old = 'Vypíšu to K info o certifikátu A1\. A2 zda vypsat podrobně\.'; New = 'Outputs certificate information. Parameter indicates whether to output verbose info.'},
    @{Old = 'Musí se do ní ukládat cesta k celé složce, nikoliv jen název aktuální složky'; New = 'Must store path to entire folder, not just current folder name'},
    @{Old = 'Vzdálená složka začíná s aktuální cestou == vzdálená složka je delší\. Pouze přejdi hloubš'; New = 'Remote folder starts with current path == remote folder is longer. Just go deeper'},
    @{Old = 'Vzdálená složka nezačíná aktuální cestou,'; New = 'Remote folder does not start with current path,'},
    @{Old = 'Vzdálená složka začíná text aktuální cestou == vzdálená složka je delší\. Pouze přejdi hloubš'; New = 'Remote folder starts with current path == remote folder is longer. Just go deeper'},
    @{Old = 'Tuto metodu nepoužívej, protože fakticky způsobuje neošetřenou výjimku, pokud již cesta bude skutečně / a a nebude\s*moci se přesunout nikde výš'; New = 'Do not use this method as it causes unhandled exception if path is already / and cannot move higher'},
    @{Old = 'Vrátí pouze názvy souborů, bez složek nebo linků'; New = 'Returns only file names, without folders or links'},
    @{Old = 'Vrátí složky, files i Linky'; New = 'Returns folders, files and links'},
    @{Old = 'Toto je vstupní metoda, metodu getFSEntriesListRecursively s 5ti parametry nevolej, ač má stejný název'; New = 'This is entry method, do not call getFSEntriesListRecursively with 5 parameters even though it has same name'},
    @{Old = 'Vrátí files i složky, ale pozor, složky jsou vždycky až po souborech'; New = 'Returns files and folders, but note that folders are always after files'},
    @{Old = 'Adresář vytvoří pokud nebude existovat'; New = 'Creates directory if it does not exist'},
    @{Old = 'Odstraním vzdálený soubor jména A1\.'; New = 'Deletes remote file with specified name.'},
    @{Old = 'Posílám příkaz SIZE\. Pokud nejsem nalogovaný, přihlásím se\.'; New = 'Sends SIZE command. If not logged in, logs in.'},
    @{Old = 'Přečtu do PP reply M ResponseMsg když používám Stream nebo readLine'; New = 'Reads reply using ResponseMsg when using Stream or readLine'},
    @{Old = 'Zavřu, nulluji clientSocket a nastavím logined to false\.'; New = 'Closes, nulls clientSocket and sets IsLoggedIn to false.'},
    @{Old = 'Původní metoda sendCommand'; New = 'Original sendCommand method'},
    @{Old = 'Nastavím pasivní způsob přenosu\(příkaz PASV\)'; New = 'Sets passive transfer mode (PASV command)'},
    @{Old = 'Získám IP adresu v řetězci z reply'; New = 'Gets IP address as string from reply'},
    @{Old = 'Získám do pole intů jednotlivé části IP adresy a spojím je do řetězce text tečkama'; New = 'Gets individual IP address parts into int array and joins them with dots'},
    @{Old = 'Pokud je poslední znak čárka,'; New = 'If last character is comma,'},
    @{Old = 'Port získám tak čtvrtou část ip adresy bitově posunu o 8 a sečtu text pátou částí\. Získám Socket, O IPEndPoint a pokusím se připojit to tento objekt\.'; New = 'Gets port by bit-shifting fourth IP part by 8 and adding fifth part. Creates Socket, IPEndPoint and attempts to connect to this object.'},
    @{Old = 'Zkontrolovat zda se první nauploadoval _\.txt'; New = 'Check if _.txt was uploaded first'}
)

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $modified = $false

    foreach ($translation in $translations) {
        $newContent = $content -replace $translation.Old, $translation.New
        if ($newContent -ne $content) {
            $content = $newContent
            $modified = $true
        }
    }

    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Translated comments: $($file.FullName)"
    }
}

Write-Host "Comment translation complete!"
