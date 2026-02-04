# Script to translate Czech comments and strings to English
$rootPath = "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp"

# Get all .cs files except in obj folders and excluding already marked files
$files = Get-ChildItem -Path $rootPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }

$translations = @(
    # OnNewStatus translations
    @{Old = 'OnNewStatus\("Uploaduji"'; New = 'OnNewStatus("Uploading"'},
    @{Old = 'OnNewStatus\("Stahuji"'; New = 'OnNewStatus("Downloading"'},
    @{Old = 'OnNewStatus\("Přihlašuji se na FTP Server"'; New = 'OnNewStatus("Logging in to FTP Server"'},
    @{Old = 'OnNewStatus\("Přihlašuji se na FTP Server bez uživatele"'; New = 'OnNewStatus("Connecting to FTP Server without user"'},
    @{Old = 'OnNewStatus\("Přecházím do složky"'; New = 'OnNewStatus("Navigating to folder"'},
    @{Old = 'OnNewStatus\("Přecházím do nadsložky"'; New = 'OnNewStatus("Navigating to parent folder"'},
    @{Old = 'OnNewStatus\("Vytvářím adresář"'; New = 'OnNewStatus("Creating directory"'},
    @{Old = 'OnNewStatus\("Mažu adresář"'; New = 'OnNewStatus("Deleting directory"'},
    @{Old = 'OnNewStatus\("Získávám seznam souborů ze složky"'; New = 'OnNewStatus("Getting file list from folder"'},
    @{Old = 'OnNewStatus\("Získávám rekurzivní seznam souborů ze složky"'; New = 'OnNewStatus("Getting recursive file list from folder"'},
    @{Old = 'OnNewStatus\("Odstraňuji ze ftp serveru soubor"'; New = 'OnNewStatus("Deleting file from FTP server"'},
    @{Old = 'OnNewStatus\("Ve složce"'; New = 'OnNewStatus("In folder"'},
    @{Old = 'přejmenovávám soubor'; New = 'renaming file'},
    @{Old = ' na '; New = ' to '},
    @{Old = 'OnNewStatus\("Pokouším se získat velikost souboru"'; New = 'OnNewStatus("Getting file size"'},
    @{Old = 'OnNewStatus\("Uzavírám ftp relaci"'; New = 'OnNewStatus("Closing FTP session"'},
    @{Old = 'OnNewStatus\("Nová složka je"'; New = 'OnNewStatus("New folder is"'},
    @{Old = 'OnNewStatus\("Program nemohl přejít do nadsložky"'; New = 'OnNewStatus("Could not navigate to parent folder"'},
    @{Old = 'OnNewStatus\("Nemohl jsem přejít do nadsložky"'; New = 'OnNewStatus("Could not navigate to parent folder"'},
    @{Old = 'OnNewStatus\("Byla volána metoda uploadSecureFolder která je prázdná"'; New = 'OnNewStatus("Method uploadSecureFolder was called but is empty"'},
    @{Old = 'OnNewStatus\("Bylo nastavena cesta ftp na"'; New = 'OnNewStatus("FTP path was set to"'},
    @{Old = 'OnNewStatus\("Nastavuji binární mód přenosu"'; New = 'OnNewStatus("Setting binary transfer mode"'},
    @{Old = 'OnNewStatus\("Nastavuji ASCII mód přenosu"'; New = 'OnNewStatus("Setting ASCII transfer mode"'},

    # Error messages
    @{Old = 'OnNewStatus\("Error'; New = 'OnNewStatus("Error'},
    @{Old = 'OnNewStatus\("Nemohl být vytvořen nový adresář, protože nebyl zadán jeho název"'; New = 'OnNewStatus("Could not create new directory because no name was specified"'},
    @{Old = '"Do metody download byl předán prázdný parametr locFileName"'; New = '"Empty locFileName parameter was passed to download method"'},
    @{Old = '"Soubor" \+ " " \+ remFileName \+ " " \+ "nemohl být stažen, protože soubor" \+ " " \+ locFileName \+ " " \+ "nešel toDelete"'; New = '"File " + remFileName + " could not be downloaded because file " + locFileName + " could not be deleted"'},
    @{Old = '"Soubor" \+ " " \+ remFileName \+ " " \+ "nemohl být stažen, protože soubor" \+ " " \+ locFileName \+ " " \+ "existoval již na disku a nebylo povoleno jeho smazání"'; New = '"File " + remFileName + " could not be downloaded because file " + locFileName + " already exists on disk and deletion was not allowed"'},
    @{Old = 'Musíte zadat jméno souboru do kterého chcete stáhnout'; New = 'You must specify a file name to download to'},
    @{Old = '"Nepodporovaný typ objektu"'; New = '"Unsupported object type"'},
    @{Old = 'Logined to'; New = 'Logged in to'},
    @{Old = 'Not logined to'; New = 'Not logged in to'}
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
        Write-Host "Translated: $($file.FullName)"
    }
}

Write-Host "Translation complete!"
