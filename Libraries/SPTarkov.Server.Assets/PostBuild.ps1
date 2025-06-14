$scriptDir = $PSScriptRoot
$sptDataPath = Join-Path $scriptDir 'SPT_Data'
$outputFile = Join-Path $sptDataPath 'checks.dat'

$files = Get-ChildItem -Path $sptDataPath -Recurse -File |
    Where-Object { $_.FullName -notmatch [regex]::Escape((Join-Path $sptDataPath 'images')) } |
    Sort-Object FullName

$hashes = foreach ($file in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $md5 = [System.Security.Cryptography.MD5]::Create()
    $hashBytes = $md5.ComputeHash($bytes)
    $md5.Dispose()

    $hashString = [BitConverter]::ToString($hashBytes) -replace '-', ''

    $relativePath = $file.FullName.Substring($sptDataPath.Length + 1) -replace '\\', '/'

    [PSCustomObject]@{
        Path = $relativePath
        Hash = $hashString
    }
}

$jsonString = $hashes | ConvertTo-Json -Depth 10

$bytes = [System.Text.Encoding]::UTF8.GetBytes($jsonString)
$base64String = [Convert]::ToBase64String($bytes)

Set-Content -Path $outputFile -Value $base64String -Encoding ASCII
