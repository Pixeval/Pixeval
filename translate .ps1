function Load-Json($filePath) {  
    return Get-Content -Raw -Path $filePath -Encoding UTF8 | ConvertFrom-Json  
}  
  
function Save-Json($data, $filePath) {  
    $data | ConvertTo-Json -Depth 100 | Out-File -FilePath $filePath -Encoding UTF8  
}  
  
function Compare-AndUpdateResJsons($folder1, $folder2) {  
    $updatedFolders = @()
    Get-ChildItem -Path $folder1 -Recurse -Filter "*.resjson" | ForEach-Object {  
        $file1Path = $_.FullName  
        $relativePath = $file1Path.Substring($folder1.Length + 1) -replace '^[\\\/]+', ''  # remove leading slashes
        $file2Path = Join-Path -Path $folder2 -ChildPath $relativePath
  
        if (Test-Path -Path $file2Path) {  
            $data1 = Load-Json -filePath $file1Path  
            $data2 = Load-Json -filePath $file2Path  
  
            $updated = $false  
            foreach ($key in $data1.PSObject.Properties.Name) {  
                if (!($data2.PSObject.Properties.Name -contains $key)) {  
                    $data2 | Add-Member -NotePropertyName $key -NotePropertyValue $data1.$key  
                    $updated = $true  
                }  
            }  
  
            if ($updated) {  
                Save-Json -data $data2 -filePath $file2Path  
                Write-Host "Updated $file2Path with new keys from $file1Path"
                $updatedFolders += (Split-Path -Parent $file2Path)
            }  
        } else {  
            $targetDir = Split-Path -Parent $file2Path  
            if (!(Test-Path -Path $targetDir)) {  
                New-Item -ItemType Directory -Path $targetDir -Force | Out-Null  
            }  
            Copy-Item -Path $file1Path -Destination $file2Path -Force  
            Write-Host "Copied $file1Path to $file2Path"  
            $updatedFolders += $targetDir
        }  
    }
    return $updatedFolders
}

# Main script
$languageCode = Read-Host "Please enter your language code (e.g., 'ru-ru', 'zh-cn')"

$basePaths = @('G:\My Projects\Pixeval\src\Pixeval\Strings', 'G:\My Projects\Pixeval\src\Pixeval.Controls\Strings')
$allUpdatedFolders = @()

$firstFolderExists = Test-Path -Path (Join-Path -Path $basePaths[0] -ChildPath $languageCode)

if (!($firstFolderExists)) {
    $createFolder = Read-Host "The folder for the language code '$languageCode' does not exist. Do you want to create it? (yes/no)"
    if ($createFolder.ToLower() -eq "yes") {
        foreach ($basePath in $basePaths) {
            $folder2 = Join-Path -Path $basePath -ChildPath $languageCode
            New-Item -ItemType Directory -Path $folder2 -Force | Out-Null
            Write-Host "Folder '$folder2' created successfully."
        }
    } else {
        Write-Host "Exiting without creating any folders."
        exit
    }
}

foreach ($basePath in $basePaths) {
    $folder1 = Join-Path -Path $basePath -ChildPath 'en-us'
    $folder2 = Join-Path -Path $basePath -ChildPath $languageCode

    $updatedFolders = Compare-AndUpdateResJsons -folder1 $folder1 -folder2 $folder2
    $allUpdatedFolders += $updatedFolders
}

$allUpdatedFolders | Sort-Object -Unique | ForEach-Object {
    Start-Process explorer.exe -ArgumentList $_
}
