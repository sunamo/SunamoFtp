# Fix FTP2.cs and FtpNet2.cs
$files = @(
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FTP2.cs",
    "E:\vs\Projects\PlatformIndependentNuGetPackages\SunamoFtp\SunamoFtp\FtpClients\FtpNet2.cs"
)

foreach ($file in $files) {
    $content = Get-Content $file -Raw
    $content = $content -replace '([^\w])remoteHost([^\w])', '${1}RemoteHost${2}'
    $content = $content -replace '([^\w])remoteUser([^\w])', '${1}RemoteUser${2}'
    $content = $content -replace '([^\w])remotePass([^\w])', '${1}RemotePass${2}'
    $content = $content -replace '([^\w])remotePort([^\w])', '${1}RemotePort${2}'
    Set-Content -Path $file -Value $content -NoNewline
    Write-Host "Fixed: $file"
}
