# Fix unused variables and fields
$rootPath = "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp"

# Get all .cs files except in obj folders
$files = Get-ChildItem -Path $rootPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $modified = $false

    # Remove unused 'ex' variable declarations - replace with underscore discard
    $newContent = $content -replace 'catch \(Exception ex\)\s*\{\s*\}', 'catch (Exception)'
    if ($newContent -ne $content) {
        $content = $newContent
        $modified = $true
    }

    # Fix: catch (Exception ex) when ex is not used
    $newContent = $content -replace 'catch \(Exception ex\)\s*\r?\n\s*\{\s*\r?\n\s*offset = 0;\s*\r?\n\s*\}', 'catch (Exception)
            {
                offset = 0;
            }'
    if ($newContent -ne $content) {
        $content = $newContent
        $modified = $true
    }

    # Remove unused isStartupPhase field - it's assigned but never used
    $newContent = $content -replace 'private new bool isStartupPhase = true;', '// Removed unused field: isStartupPhase'
    if ($newContent -ne $content) {
        $content = $newContent
        $modified = $true
    }

    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Fixed unused code: $($file.FullName)"
    }
}

Write-Host "Unused variable/field fixing complete!"
