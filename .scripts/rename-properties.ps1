# Script to rename FTP properties to PascalCase throughout the codebase
$rootPath = "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp"

# Get all .cs files except in obj folders
$files = Get-ChildItem -Path $rootPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }

$replacements = @(
    @{Old = '\bremoteHost\b'; New = 'RemoteHost'},
    @{Old = '\bremoteUser\b'; New = 'RemoteUser'},
    @{Old = '\bremotePass\b'; New = 'RemotePass'},
    @{Old = '\bremotePort\b'; New = 'RemotePort'},
    @{Old = '\blogined\b'; New = 'IsLoggedIn'},
    @{Old = '\breallyUpload\b'; New = 'ReallyUpload'},
    @{Old = '\bfolderSizeRec\b'; New = 'FolderSizeRecursive'}
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
        Write-Host "Updated: $($file.FullName)"
    }
}

Write-Host "Property renaming complete!"
