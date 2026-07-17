param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Container })]
    [string] $PublishDirectory,

    [Parameter(Mandatory)]
    [ValidateSet("x86", "x64", "arm64")]
    [string] $Architecture,

    [Parameter(Mandatory)]
    [ValidatePattern("^\d+\.\d+\.\d+$")]
    [string] $ReleaseVersion,

    [Parameter(Mandatory)]
    [ValidatePattern("^\d+\.\d+\.\d+\.\d+$")]
    [string] $PackageVersion,

    [Parameter(Mandatory)]
    [string] $OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$publishRoot = [IO.Path]::GetFullPath((Resolve-Path -LiteralPath $PublishDirectory))
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
$temporaryBase = if ($env:RUNNER_TEMP) { $env:RUNNER_TEMP } else { [IO.Path]::GetTempPath() }
$temporaryRoot = Join-Path $temporaryBase "pixeval-msix-$Architecture-$([Guid]::NewGuid().ToString('N'))"
$packageRoot = Join-Path $temporaryRoot "package"

try
{
    $null = New-Item -ItemType Directory -Force -Path $packageRoot, $outputRoot

    foreach ($file in Get-ChildItem -LiteralPath $publishRoot -Recurse -File)
    {
        if ($file.Extension -in ".pdb", ".dbg")
        {
            continue
        }

        $relativePath = [IO.Path]::GetRelativePath($publishRoot, $file.FullName)
        $destination = Join-Path $packageRoot $relativePath
        $null = New-Item -ItemType Directory -Force -Path ([IO.Path]::GetDirectoryName($destination))
        Copy-Item -LiteralPath $file.FullName -Destination $destination
    }

    $assetSource = Join-Path $repositoryRoot "src\Pixeval.Desktop\Assets\Windows\MSIX"
    $assetDestination = Join-Path $packageRoot "Assets"
    $null = New-Item -ItemType Directory -Force -Path $assetDestination
    Copy-Item -LiteralPath (Join-Path $assetSource "StoreLogo.png") -Destination $assetDestination
    Copy-Item -LiteralPath (Join-Path $assetSource "Square44x44Logo.png") -Destination $assetDestination
    Copy-Item -LiteralPath (Join-Path $assetSource "Square150x150Logo.png") -Destination $assetDestination

    $manifestTemplate = [IO.File]::ReadAllText(
        (Join-Path $assetSource "Package.appxmanifest.template"))
    $manifest = $manifestTemplate.Replace(
        "__VERSION__",
        $PackageVersion,
        [StringComparison]::Ordinal).Replace(
        "__ARCHITECTURE__",
        $Architecture,
        [StringComparison]::Ordinal)
    [IO.File]::WriteAllText(
        (Join-Path $packageRoot "AppxManifest.xml"),
        $manifest,
        [Text.UTF8Encoding]::new($false))

    $makeAppx = Get-Command MakeAppx.exe -ErrorAction SilentlyContinue
    if ($null -eq $makeAppx)
    {
        $windowsKitRoot = Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin"
        $makeAppx = Get-ChildItem -LiteralPath $windowsKitRoot -Filter MakeAppx.exe -Recurse
            | Where-Object FullName -Match "\\x64\\MakeAppx\.exe$"
            | Sort-Object FullName -Descending
            | Select-Object -First 1
    }

    if ($null -eq $makeAppx)
    {
        throw "MakeAppx.exe was not found. Install the Windows SDK on the build runner."
    }

    $makeAppxPath = if ($makeAppx -is [System.Management.Automation.CommandInfo])
    {
        $makeAppx.Source
    }
    else
    {
        $makeAppx.FullName
    }

    $packagePath = Join-Path $outputRoot "Pixeval-$ReleaseVersion-windows-$Architecture-unsigned.msix"
    & $makeAppxPath pack /d $packageRoot /p $packagePath /o
    if ($LASTEXITCODE -ne 0)
    {
        throw "MakeAppx.exe failed with exit code $LASTEXITCODE."
    }

    Write-Output $packagePath
}
finally
{
    $resolvedTemporaryBase = [IO.Path]::GetFullPath($temporaryBase)
    $resolvedTemporaryRoot = [IO.Path]::GetFullPath($temporaryRoot)
    $isInsideTemporaryBase = $resolvedTemporaryRoot.StartsWith(
        $resolvedTemporaryBase,
        [StringComparison]::OrdinalIgnoreCase)
    if ($isInsideTemporaryBase -and (Test-Path -LiteralPath $resolvedTemporaryRoot))
    {
        Remove-Item -LiteralPath $resolvedTemporaryRoot -Recurse -Force
    }
}
