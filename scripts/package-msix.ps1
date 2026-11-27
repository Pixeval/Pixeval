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

function Get-WindowsSdkToolPath([string] $Name)
{
    $command = Get-Command $Name -CommandType Application -ErrorAction SilentlyContinue
    if ($null -ne $command)
    {
        return $command.Source
    }

    $windowsKitRoot = Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin"
    $tool = Get-ChildItem -LiteralPath $windowsKitRoot -Filter $Name -Recurse -File -ErrorAction SilentlyContinue
        | Where-Object { $_.Directory.Name -eq "x64" }
        | Sort-Object FullName -Descending
        | Select-Object -First 1
    if ($null -eq $tool)
    {
        throw "$Name was not found. Install the Windows SDK on the build runner."
    }

    return $tool.FullName
}

function Write-MsixIcon(
    [System.Windows.Media.Imaging.BitmapSource] $Source,
    [int] $Size,
    [string] $Path,
    [int] $Inset = 0)
{
    $contentSize = $Size - 2 * $Inset
    $visual = [System.Windows.Media.DrawingVisual]::new()
    [System.Windows.Media.RenderOptions]::SetBitmapScalingMode(
        $visual, [System.Windows.Media.BitmapScalingMode]::HighQuality)
    $drawing = $visual.RenderOpen()
    try
    {
        $drawing.DrawImage($Source, [System.Windows.Rect]::new($Inset, $Inset, $contentSize, $contentSize))
    }
    finally
    {
        $drawing.Close()
    }

    $bitmap = [System.Windows.Media.Imaging.RenderTargetBitmap]::new(
        $Size, $Size, 96, 96, [System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream = [IO.File]::Create($Path)
    try
    {
        $encoder.Save($stream)
    }
    finally
    {
        $stream.Dispose()
    }
}

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$publishRoot = [IO.Path]::GetFullPath((Resolve-Path -LiteralPath $PublishDirectory))
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
$temporaryBase = if ($env:RUNNER_TEMP) { $env:RUNNER_TEMP } else { [IO.Path]::GetTempPath() }
$temporaryRoot = Join-Path $temporaryBase "pixeval-msix-$Architecture-$([Guid]::NewGuid().ToString('N'))"
$packageRoot = Join-Path $temporaryRoot "package"
$resourceRoot = Join-Path $temporaryRoot "resources"

$makeAppxPath = Get-WindowsSdkToolPath "MakeAppx.exe"
$makePriPath = Get-WindowsSdkToolPath "MakePri.exe"
Add-Type -AssemblyName PresentationCore

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
    $assetDestination = Join-Path $resourceRoot "Assets"
    $null = New-Item -ItemType Directory -Force -Path $assetDestination

    $logoPath = Join-Path $repositoryRoot "src\Pixeval\Assets\logo.ico"
    $logo = [System.Windows.Media.Imaging.BitmapDecoder]::Create(
        [Uri]::new($logoPath),
        [System.Windows.Media.Imaging.BitmapCreateOptions]::PreservePixelFormat,
        [System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad)
    $logoFrames = @{}
    foreach ($frame in $logo.Frames)
    {
        $logoFrames[$frame.PixelWidth] = $frame
    }
    $masterLogo = $logo.Frames | Sort-Object PixelWidth -Descending | Select-Object -First 1

    $baseSizes = @{ StoreLogo = 50; Square44x44Logo = 44; Square150x150Logo = 150 }
    foreach ($name in $baseSizes.Keys)
    {
        $baseIcon = Join-Path $assetSource "$name.png"
        Copy-Item -LiteralPath $baseIcon -Destination $assetDestination
        Copy-Item -LiteralPath $baseIcon -Destination (Join-Path $assetDestination "$name.scale-100.png")
        foreach ($scale in 125, 150, 200, 400)
        {
            $size = [int] [Math]::Ceiling($baseSizes[$name] * $scale / 100)
            Write-MsixIcon $masterLogo $size (Join-Path $assetDestination "$name.scale-$scale.png")
        }
    }

    foreach ($size in 16, 20, 24, 30, 32, 36, 40, 48, 60, 64, 72, 80, 96, 256)
    {
        $name = "Square44x44Logo.targetsize-$size"
        $inset = [int] [Math]::Floor($size / 8)
        Write-MsixIcon $masterLogo $size (Join-Path $assetDestination "$name.png") -Inset $inset

        # Use full-size artwork when Windows draws the logo without a backplate.
        $unplatedSource = $logoFrames[$size] ?? $masterLogo
        $unplatedPath = Join-Path $assetDestination "${name}_altform-unplated.png"
        Write-MsixIcon $unplatedSource $size $unplatedPath
        Copy-Item -LiteralPath $unplatedPath -Destination (Join-Path $assetDestination "${name}_altform-lightunplated.png")
    }

    $manifestTemplate = [IO.File]::ReadAllText(
        (Join-Path $assetSource "Package.appxmanifest.template"))
    $manifest = $manifestTemplate.Replace(
        "__VERSION__",
        $PackageVersion,
        [StringComparison]::Ordinal).Replace(
        "__ARCHITECTURE__",
        $Architecture,
        [StringComparison]::Ordinal)
    $manifestPath = Join-Path $temporaryRoot "AppxManifest.xml"
    [IO.File]::WriteAllText($manifestPath, $manifest, [Text.UTF8Encoding]::new($false))

    # Index the MSIX assets without indexing the published application files.
    $priConfigPath = Join-Path $temporaryRoot "priconfig.xml"
    & $makePriPath createconfig /cf $priConfigPath /dq zh-Hans /o
    if ($LASTEXITCODE -ne 0)
    {
        throw "MakePri.exe createconfig failed with exit code $LASTEXITCODE."
    }

    # Keep every scale in the main PRI because this script builds a single MSIX per architecture.
    [xml] $priConfig = [IO.File]::ReadAllText($priConfigPath)
    $packaging = $priConfig.SelectSingleNode("/resources/packaging")
    if ($null -ne $packaging)
    {
        $null = $packaging.ParentNode.RemoveChild($packaging)
        $priConfig.Save($priConfigPath)
    }

    $priPath = Join-Path $resourceRoot "resources.pri"
    & $makePriPath new /pr $resourceRoot /cf $priConfigPath /mn $manifestPath /of $priPath /o
    if ($LASTEXITCODE -ne 0)
    {
        throw "MakePri.exe new failed with exit code $LASTEXITCODE."
    }

    Copy-Item -LiteralPath $assetDestination -Destination $packageRoot -Recurse -Force
    Copy-Item -LiteralPath $manifestPath, $priPath -Destination $packageRoot -Force

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
