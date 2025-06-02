param (
    [string]$filepath,
	[string]$output
)

function Load-RecursiveAsync {
    param (
        [string]$filepath
    )

    $result = @{}

    $filesList = Get-ChildItem -Path $filepath

    foreach ($file in $filesList) {
        $curPath = $file.FullName
        if ($file.PSIsContainer) {
            $result[$file.BaseName] = Load-RecursiveAsync "$filepath\$($file.Name)"
        } elseif ($file.Extension -eq ".json") {
            $result[$file.BaseName] = Generate-HashForData (Get-Content -Raw -Path "$filepath\$($file.Name)")
        }
    }

    return $result
}

function Generate-HashForData {
    param (
        [string]$data
    )

    $sha1 = [System.Security.Cryptography.SHA1]::Create()
    $hashBytes = $sha1.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($data))
    $hashHex = -join ($hashBytes | ForEach-Object { $_.ToString("x2") })  # Convert bytes to hex

    return $hashHex
}

function Encode-Base64 {
    param (
        [Parameter(ValueFromPipeline=$true)]
        [string]$inputString
    )

    process {
        if ($inputString -and $inputString -ne "") {
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($inputString)
            $base64String = [Convert]::ToBase64String($bytes)
            return $base64String
        } else {
            Write-Output "Error: No valid input received!"
        }
    }
}

$results = Load-RecursiveAsync $filepath
$results | ConvertTo-Json -Depth 10 -Compress | Encode-Base64 | Out-File $output