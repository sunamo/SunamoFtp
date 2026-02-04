# Script to fix remaining compilation errors
$rootPath = "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp"

# Get all .cs files except in obj folders
$files = Get-ChildItem -Path $rootPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }

$replacements = @(
    # Fix fz that wasn't caught
    @{Old = '\bvar fz ='; New = 'var firstChar ='},
    @{Old = '\bif \(fz =='; New = 'if (firstChar =='},
    @{Old = '\belse if \(fz =='; New = 'else if (firstChar =='},

    # Fix startup in FTP.cs
    @{Old = 'private new bool startup ='; New = 'private new bool isStartupPhase ='},

    # Fix remoteUser, remotePass, remoteHost, remotePort in FtpNet files that weren't caught
    @{Old = 'NetworkCredential\(remoteUser, remotePass\)'; New = 'NetworkCredential(RemoteUser, RemotePass)'},
    @{Old = ', remoteHost \+'; New = ', RemoteHost +'},
    @{Old = '\bremoteHost \+'; New = 'RemoteHost +'},
    @{Old = '\bremoteHost,'; New = 'RemoteHost,'},
    @{Old = '\bremoteHost\)'; New = 'RemoteHost)'},
    @{Old = '\bremotePort'; New = 'RemotePort'},

    # Fix v4 that wasn't renamed
    @{Old = '\bIPAddress v4 ='; New = 'IPAddress ipv4Address ='},

    # Fix _FileStream and _Stream in finally blocks
    @{Old = '\b_FileStream !='; New = 'fileStream !='},
    @{Old = '\b_Stream !='; New = 'ftpStream !='},

    # Fix td that wasn't caught
    @{Old = '\btd,'; New = 'directoriesToDelete,'},
    @{Old = '\btd\)'; New = 'directoriesToDelete)'}
)

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $modified = $false

    foreach ($replacement in $replacements) {
        $newContent = $content -replace $replacement.Old, $replacement.New
        if ($newContent -ne $content) {
            $content = $newContent
            $modified = $true
        }
    }

    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Fixed errors: $($file.FullName)"
    }
}

Write-Host "Error fixing complete!"
