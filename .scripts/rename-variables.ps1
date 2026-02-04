# Script to rename Czech variables and single-letter names to self-descriptive English names
$rootPath = "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp"

# Get all .cs files except in obj folders
$files = Get-ChildItem -Path $rootPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }

$replacements = @(
    # Czech variable names
    @{Old = '\bnalezenAdresar\b'; New = 'directoryFound'},
    @{Old = '\bvseMa8\b'; New = 'allHaveEightTokens'},
    @{Old = '\bsmazaneAdresare\b'; New = 'deletedDirectories'},

    # Single-letter variables (not in for loops)
    @{Old = '(var|FileSystemType) fz\b'; New = '$1 firstChar'},
    @{Old = 'out string fn\b'; New = 'out string fileName'},
    @{Old = ', string fn\)'; New = ', string fileName)'},
    @{Old = '\(string fn\)'; New = '(string fileName)'},
    @{Old = 'List<DirectoriesToDeleteFtp> td\)'; New = 'List<DirectoriesToDeleteFtp> directoriesToDelete)'},
    @{Old = 'int i, List<DirectoriesToDeleteFtp> td\)'; New = 'int depth, List<DirectoriesToDeleteFtp> directoriesToDelete)'},
    @{Old = '\btd\.'; New = 'directoriesToDelete.'},
    @{Old = '\btd\['; New = 'directoriesToDelete['},
    @{Old = '\(td\)'; New = '(directoriesToDelete)'},
    @{Old = ', td\)'; New = ', directoriesToDelete)'},
    @{Old = 'Dictionary<string, List<string>> ds\b'; New = 'Dictionary<string, List<string>> directoryMap'},
    @{Old = '\bds\.'; New = 'directoryMap.'},
    @{Old = '\bds\b'; New = 'directoryMap'},
    @{Old = 'string sa\b'; New = 'string deletedDirectoryPath'},
    @{Old = '\bsa\b'; New = 'deletedDirectoryPath'},
    @{Old = 'var yValue\b'; New = 'var depthIndex'},
    @{Old = 'yValue--'; New = 'depthIndex--'},
    @{Old = 'yValue >='; New = 'depthIndex >='},
    @{Old = '\byValue\]'; New = 'depthIndex]'},
    @{Old = 'IPAddress v4\b'; New = 'IPAddress ipv4Address'},
    @{Old = '\bv4\.'; New = 'ipv4Address.'},
    @{Old = '\bv4,'; New = 'ipv4Address,'},
    @{Old = '\bv4\)'; New = 'ipv4Address)'},

    # Inconsistent naming
    @{Old = 'Socket Csocket\b'; New = 'Socket clientSocket'},
    @{Old = '\bCsocket\b'; New = 'clientSocket'},
    @{Old = 'var cSocket\b'; New = 'var dataSocket'},
    @{Old = '\bcSocket\.'; New = 'dataSocket.'},
    @{Old = '\bcSocket\)'; New = 'dataSocket)'},
    @{Old = '\(cSocket'; New = '(dataSocket'},
    @{Old = 'var buff\b'; New = 'var buffer'},
    @{Old = '\bbuff\['; New = 'buffer['},
    @{Old = '\bbuff,'; New = 'buffer,'},
    @{Old = 'byte\[\] buff\b'; New = 'byte[] buffer'}
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
        Write-Host "Renamed variables: $($file.FullName)"
    }
}

Write-Host "Variable renaming complete!"
