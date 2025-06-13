$scriptDir = $PSScriptRoot
$assetsPath = Join-Path $scriptDir 'Assets'
$outputFile = Join-Path $assetsPath 'checks.dat'

$files = Get-ChildItem -Path $assetsPath -Recurse -File |
    Where-Object { $_.FullName -notmatch [regex]::Escape((Join-Path $assetsPath 'images')) } |
    Sort-Object FullName

$hashes = foreach ($file in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $md5 = [System.Security.Cryptography.MD5]::Create()
    $hashBytes = $md5.ComputeHash($bytes)
    $md5.Dispose()

    $hashString = [BitConverter]::ToString($hashBytes) -replace '-', ''

    $relativePath = $file.FullName.Substring($assetsPath.Length + 1) -replace '\\', '/'

    [PSCustomObject]@{
        Path = $relativePath
        Hash = $hashString
    }
}

$jsonString = $hashes | ConvertTo-Json -Depth 10

$bytes = [System.Text.Encoding]::UTF8.GetBytes($jsonString)
$base64String = [Convert]::ToBase64String($bytes)

Set-Content -Path $outputFile -Value $base64String -Encoding ASCII
