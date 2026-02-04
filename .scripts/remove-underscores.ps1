# Script to remove underscore prefixes from parameters and local variables
$rootPath = "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp"

# Get all .cs files except in obj folders
$files = Get-ChildItem -Path $rootPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }

$replacements = @(
    # Parameters with underscore prefix
    @{Old = 'string _UploadPath\b'; New = 'string uploadPath'},
    @{Old = '\b_UploadPath\b'; New = 'uploadPath'},
    @{Old = 'string _FileName\b'; New = 'string fileName'},
    @{Old = '\b_FileName\b'; New = 'fileName'},
    @{Old = 'string _Path\b'; New = 'string path'},
    @{Old = '\b_Path\b'; New = 'path'},

    # Local variables with underscore prefix
    @{Old = 'var _FileInfo\b'; New = 'var fileInfo'},
    @{Old = '\b_FileInfo\.'; New = 'fileInfo.'},
    @{Old = 'Stream _Stream\b'; New = 'Stream ftpStream'},
    @{Old = '\b_Stream\.'; New = 'ftpStream.'},
    @{Old = '\b_Stream\)'; New = 'ftpStream)'},
    @{Old = '\(_Stream'; New = '(ftpStream'},
    @{Old = 'FileStream _FileStream\b'; New = 'FileStream fileStream'},
    @{Old = '\b_FileStream\.'; New = 'fileStream.'},
    @{Old = '\b_FileStream\)'; New = 'fileStream)'},
    @{Old = '\(_FileStream'; New = '(fileStream'},
    @{Old = 'var _FtpWebRequest\b'; New = 'var ftpWebRequest'},
    @{Old = '\b_FtpWebRequest\.'; New = 'ftpWebRequest.'},
    @{Old = 'SslStream _sslStream\b'; New = 'SslStream sslStream'},
    @{Old = '\b_sslStream\.'; New = 'sslStream.'},
    @{Old = '\b_sslStream\)'; New = 'sslStream)'},
    @{Old = '\(_sslStream'; New = '(sslStream'},
    @{Old = '\b_sslStream'; New = 'sslStream'}
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
        Write-Host "Removed underscores: $($file.FullName)"
    }
}

Write-Host "Underscore removal complete!"
