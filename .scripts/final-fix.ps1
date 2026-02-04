# Final script to fix all remaining property references
$files = @(
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FtpNet.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FtpNet1.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FTP1.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\Base\FtpBase.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw

        # Replace any remaining lowercase property names with PascalCase
        $content = $content -replace '([^\w])remoteUser([^\w])', '${1}RemoteUser${2}'
        $content = $content -replace '([^\w])remotePass([^\w])', '${1}RemotePass${2}'
        $content = $content -replace '([^\w])remoteHost([^\w])', '${1}RemoteHost${2}'
        $content = $content -replace '([^\w])remotePort([^\w])', '${1}RemotePort${2}'
        $content = $content -replace '([^\w])v4([^\w])', '${1}ipv4Address${2}'

        # Fix _FileStream and _Stream in FtpBase.cs
        $content = $content -replace '\b_FileStream\b', 'fileStream'
        $content = $content -replace '\b_Stream\b', 'ftpStream'

        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "Fixed: $file"
    }
}

Write-Host "Final fix complete!"
