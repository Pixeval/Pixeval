$ErrorActionPreference = "silentlycontinue"

function IsDotNetCoreInstalled {
    $installed = $null
    $folders = Get-ChildItem -Path "$($Env:ProgramFiles)\dotnet\shared\Microsoft.NETCore.App"
    $name = $folders | select Name | select-string "3.\d+.\d+"
    if ($name.Matches.Length -eq 1) {
        return $true
    }
    return $false
}

function GetDownloadLink {
    $SiteAddr = "https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.0-windows-x64-installer"
    $HttpContent = Invoke-WebRequest -Uri $SiteAddr
    $DirectLink = $HttpContent.InputFields | Where-Object {$_.id -eq "directLink"} | select value
    return $DirectLink[0]
}

function DownloadFile($Link, $Location) {
    Invoke-WebRequest -Uri $Link -OutFile $Location
}

$installed = IsDotNetCoreInstalled
$DownloadFolder = Get-ItemPropertyValue 'HKCU:\software\microsoft\windows\currentversion\explorer\shell folders\' -Name '{374DE290-123F-4565-9164-39C4925E467B}'

if ($Installed -eq $false) {
    $IsInstall = Read-Host -Prompt "您尚未安装.NET Core 3.0或3.0以上的运行环境，请问是否要现在安装？(是请输入yes，否请按任意键退出)"
    if ($IsInstall -eq "Yes") {
        $Location = $DownloadFolder + "\runtime.exe"
        $FileExists = [System.IO.File]::Exists($Location)
        if ($FileExists -eq $true) {
            Write-Output "$($Location)所在位置已经存在文件，请检查后重新执行本程序"
            pause
            exit
        }
        $Page = GetDownloadLink
        DownloadFile $Page.value $Location
        Start-Process -FilePath $Location
    }
} else {
    Write-Output "您已经安装.NET Core 3.0或3.0以上的运行环境，无需再次安装！"
}