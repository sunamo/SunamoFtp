# Script to rename all FTP method names from camelCase to PascalCase
# and fix variable names across all FTP client files

$ftpFiles = @(
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FTP.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FTP1.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FTP2.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FTP3.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FTP31.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FTP4.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FtpDllWrapper.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FtpNet.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FtpNet1.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FtpNet2.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FtpNet3.cs"
)

# Method name mappings from camelCase to PascalCase
$methodMappings = @{
    'createDataSocket' = 'CreateDataSocket'
    'sendCommand' = 'SendCommand'
    'sendCommand2' = 'SendCommand2'
    'readReply' = 'ReadReply'
    'login\(' = 'Login('
    'loginWithoutUser' = 'LoginWithoutUser'
    'setPort' = 'SetPort'
    'getPort' = 'GetPort'
    'setUseStream' = 'SetUseStream'
    'setRemotePath' = 'SetRemotePath'
    'getRemotePath' = 'GetRemotePath'
    'cleanup' = 'Cleanup'
    'close' = 'Close'
    'chdir\(' = 'Chdir('
    'chdirLite' = 'ChdirLite'
    'mkdir\(' = 'Mkdir('
    'rmdir\(' = 'Rmdir('
    'download\(' = 'Download('
    'deleteRemoteFile' = 'DeleteRemoteFile'
    'renameRemoteFile' = 'RenameRemoteFile'
    'getFileSize' = 'GetFileSize'
    'goToPath' = 'GoToPath'
    'goToUpFolder' = 'GoToUpFolder'
    'goToUpFolderForce' = 'GoToUpFolderForce'
    'getFSEntriesListRecursively' = 'GetFSEntriesListRecursively'
    'getFileList' = 'GetFileList'
    'showCertificateInfo' = 'ShowCertificateInfo'
}

# Variable name mappings
$variableMappings = @{
    '\bmes\b' = 'message'
    '\bdebug\b' = 'isDebug'
    '\bPathSelector\.indexZero\b' = 'PathSelector.IndexZero'
    '\bPathSelector\.tokens\b' = 'PathSelector.Tokens'
}

# Czech to English translations
$translations = @{
    'Pokud nejsem připojený' = 'If not connected'
    'Poté se přihlásím příkazem RemoteUser' = 'Then login with RemoteUser command'
    'Hodnota 230 znamená že mohu pokračovat bez hesla\. Pokud ne, pošlu své heslo příkazem PASS' = 'Value 230 means login successful without password. Otherwise send password with PASS command'
    'Byl nastavena cesta ftp na' = 'FTP path set to'
    'Navigating to parent folder' = 'Navigating to parent folder'
}

foreach ($file in $ftpFiles) {
    if (Test-Path $file) {
        Write-Host "Processing $file..." -ForegroundColor Cyan
        $content = Get-Content $file -Raw -Encoding UTF8

        # Remove wrong comments
        $content = $content -replace '// EN: Variable names have been checked and replaced with self-descriptive names\r?\n', ''
        $content = $content -replace '// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy\r?\n', ''

        # Replace method names
        foreach ($old in $methodMappings.Keys) {
            $new = $methodMappings[$old]
            $content = $content -replace $old, $new
        }

        # Replace variable names
        foreach ($old in $variableMappings.Keys) {
            $new = $variableMappings[$old]
            $content = $content -replace $old, $new
        }

        # Replace Czech translations
        foreach ($czech in $translations.Keys) {
            $english = $translations[$czech]
            $content = $content -replace [regex]::Escape($czech), $english
        }

        Set-Content $file $content -Encoding UTF8 -NoNewline
        Write-Host "  ✓ Updated $file" -ForegroundColor Green
    }
}

Write-Host "`nAll FTP client files have been updated!" -ForegroundColor Green
