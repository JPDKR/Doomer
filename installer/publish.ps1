param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "Doomer\Doomer.csproj"
$publishDir = Join-Path $repoRoot "publish"

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

Write-Host "Publishing $project ($Configuration, $Runtime, self-contained)..."

dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:Version=$Version `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "Published to $publishDir"

$innoCompiler = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
$issFile = Join-Path $PSScriptRoot "Doomer.iss"

if (Test-Path $innoCompiler) {
    Write-Host "Building installer with Inno Setup..."
    & $innoCompiler "/DMyAppVersion=$Version" $issFile
    if ($LASTEXITCODE -ne 0) {
        throw "ISCC.exe failed with exit code $LASTEXITCODE"
    }
    Write-Host "Installer built in $(Join-Path $PSScriptRoot 'Output')"
}
else {
    Write-Warning "Inno Setup compiler not found at '$innoCompiler'."
    Write-Warning "Install Inno Setup (https://jrsoftware.org/isinfo.php) or compile '$issFile' manually from its GUI."
}
