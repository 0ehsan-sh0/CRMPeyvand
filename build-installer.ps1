<#
.SYNOPSIS
    Publishes CRMPeyvand and packages it into a Windows Installer MSI.

.DESCRIPTION
    Produces artifacts\CRMPeyvand.msi from a clean framework-dependent publish.

    The install is framework-dependent, so the target machine needs the
    .NET 10 Desktop Runtime. The MSI checks for it at launch and refuses to
    install with a clear message if it is missing, rather than failing later
    inside the app.

    No database prerequisite: the app defaults to SQLite, whose native
    e_sqlite3.dll ships inside the publish output, and SQL Server is opt-in from
    the settings screen.

.PARAMETER Version
    Product version recorded in the MSI. Must be bumped on every release so
    MajorUpgrade chains correctly.

.PARAMETER SignPfx
    Optional path to a .pfx code-signing certificate. Without it the MSI is
    unsigned and SmartScreen will warn end users.

.EXAMPLE
    .\build-installer.ps1 -Version 1.0.0

.EXAMPLE
    .\build-installer.ps1 -Version 1.1.0 -SignPfx certs\peyvand.pfx -SignPfxPassword $env:PFX_PASSWORD
#>
[CmdletBinding()]
param(
    [string] $Version = '1.0.0',
    [ValidateSet('x64')]
    [string] $Runtime = 'win-x64',
    [string] $Configuration = 'Release',
    [string] $SignPfx,
    [string] $SignPfxPassword,
    [string] $SignTimestampUrl = 'http://timestamp.digicert.com'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = $PSScriptRoot
$artifacts = Join-Path $repoRoot 'artifacts'
$publishDir = Join-Path $artifacts 'publish'
$appProject = Join-Path $repoRoot 'CRMPeyvand\CRMPeyvand.csproj'
$installerProject = Join-Path $repoRoot 'installer\CRMPeyvand.Installer.wixproj'
$msiPath = Join-Path $artifacts 'CRMPeyvand.msi'

# Stale publish output would otherwise be harvested into the MSI: a file removed
# from the project would linger in the package indefinitely.
if (Test-Path -LiteralPath $publishDir) {
    Remove-Item -Recurse -Force -LiteralPath $publishDir
}
if (Test-Path -LiteralPath $msiPath) {
    Remove-Item -Force -LiteralPath $msiPath
}

Write-Host '==> Restoring' -ForegroundColor Cyan
dotnet restore $installerProject --nologo
if ($LASTEXITCODE -ne 0) { throw "restore failed ($LASTEXITCODE)" }

Write-Host '==> Publishing application' -ForegroundColor Cyan
dotnet publish $appProject `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -o $publishDir `
    --nologo
if ($LASTEXITCODE -ne 0) { throw "publish failed ($LASTEXITCODE)" }

$published = (Get-ChildItem -LiteralPath $publishDir -Recurse -File).Count
Write-Host "    $published files published"

Write-Host "==> Building MSI (version $Version)" -ForegroundColor Cyan
# -t:Rebuild is not optional. WiX does not treat DefineConstants as an input to
# its compile step, so a warm installer\obj makes the target up to date and the
# link step re-copies the *previous* MSI - version included. A plain build
# therefore silently emits whatever ProductVersion was compiled last, which is
# the wixproj default rather than $Version. A fresh CI runner hides this because
# its obj is always empty; a developer's second build does not.
$wixOutput = dotnet build $installerProject `
    -c $Configuration `
    -t:Rebuild `
    -p:Platform=x64 `
    -p:ProductVersion=$Version `
    -p:PublishDirOverride=$publishDir `
    --nologo 2>&1
$wixExit = $LASTEXITCODE
$wixOutput | ForEach-Object { Write-Host $_ }
if ($wixExit -ne 0) { throw "installer build failed ($wixExit)" }

# WIX8600 is only a warning, and an MSI with no files in it is a valid package
# that installs nothing - the worst possible release artefact. Fail loudly.
if ($wixOutput -match 'WIX8600') {
    throw 'the installer harvested zero files - the MSI would be empty (check PublishDir)'
}

if (-not (Test-Path -LiteralPath $msiPath)) {
    throw "expected MSI not found at $msiPath"
}

# The version drives MajorUpgrade, so a package carrying the wrong one cannot be
# upgraded over the right one. Read it back out of the built MSI rather than
# trusting the -p: value: the whole point is that the value can be dropped on the
# floor between the command line and the package, and it has been.
$installerShell = New-Object -ComObject WindowsInstaller.Installer
$msiDb = $installerShell.OpenDatabase($msiPath, 0)
$msiView = $msiDb.OpenView("SELECT ``Value`` FROM ``Property`` WHERE ``Property``='ProductVersion'")
$msiView.Execute()
$builtVersion = $msiView.Fetch().StringData(1)
if ($builtVersion -ne $Version) {
    throw "MSI reports ProductVersion '$builtVersion' but '$Version' was asked for - the version did not reach the package"
}
Write-Host "    MSI ProductVersion $builtVersion" -ForegroundColor DarkGray

if ($SignPfx) {
    if (-not (Test-Path -LiteralPath $SignPfx)) {
        throw "signing certificate not found: $SignPfx"
    }

    $signtool = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\' } |
        Sort-Object FullName -Descending |
        Select-Object -First 1 -ExpandProperty FullName

    if (-not $signtool) {
        throw 'signtool.exe not found - install the Windows SDK'
    }

    Write-Host '==> Signing MSI' -ForegroundColor Cyan
    $signArgs = @('sign', '/fd', 'sha256', '/tr', $SignTimestampUrl, '/td', 'sha256', '/f', $SignPfx)
    if ($SignPfxPassword) { $signArgs += @('/p', $SignPfxPassword) }
    $signArgs += $msiPath

    & $signtool @signArgs
    if ($LASTEXITCODE -ne 0) { throw "signing failed ($LASTEXITCODE)" }
}
else {
    Write-Warning 'MSI is UNSIGNED - SmartScreen will warn end users. Pass -SignPfx to sign it.'
}

$size = [Math]::Round((Get-Item -LiteralPath $msiPath).Length / 1MB, 1)
Write-Host ''
Write-Host "==> artifacts\CRMPeyvand.msi  ($size MB)" -ForegroundColor Green
