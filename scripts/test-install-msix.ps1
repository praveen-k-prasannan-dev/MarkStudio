# Locally sideloads dist\MarkStudioEditor_1.0.0.0_x64.msix for testing BEFORE uploading to
# Partner Center. The Store re-signs the package itself on submission - this local signing
# is only so Windows will let you install and run it on this dev machine to verify it works.
# Must be run as Administrator.

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$msixPath = Join-Path $root 'dist\MarkStudioEditor_1.0.0.0_x64.msix'
$pfxPath = Join-Path $root 'dist\test-signing.pfx'
$publisher = 'CN=558070D9-0C16-4D08-B8C5-4FF10D766ACB'  # must match Package/Identity/Publisher exactly

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Run this script from an elevated (Administrator) PowerShell window."
}
if (-not (Test-Path $msixPath)) { throw "Package not found: $msixPath - run scripts\package-msix.ps1 first." }

function Find-SdkTool([string]$name) {
    $roots = Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\bin" -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending
    foreach ($r in $roots) {
        $candidate = Join-Path $r.FullName "x64\$name"
        if (Test-Path $candidate) { return $candidate }
    }
    throw "$name not found under Windows Kits 10 bin."
}
$signtool = Find-SdkTool 'signtool.exe'

# 1. Create (or reuse) a local test-signing certificate whose subject matches the Publisher.
$existing = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $publisher }
if ($existing) {
    $cert = $existing[0]
    "Reusing existing test certificate: $($cert.Thumbprint)"
} else {
    $cert = New-SelfSignedCertificate -Type Custom -Subject $publisher `
        -KeyUsage DigitalSignature -FriendlyName "MarkStudio Editor test signing" `
        -CertStoreLocation Cert:\CurrentUser\My `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
    "Created test certificate: $($cert.Thumbprint)"
}

# 2. Export it and trust it locally (Trusted People) so Windows accepts packages signed with it.
$password = ConvertTo-SecureString -String "MarkStudioTest!1" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $password | Out-Null
Import-PfxCertificate -FilePath $pfxPath -CertStoreLocation Cert:\LocalMachine\TrustedPeople -Password $password | Out-Null

# 3. Sign the package with it (local test signature only - NOT what ships to the Store).
& $signtool sign /fd SHA256 /a /f $pfxPath /p "MarkStudioTest!1" $msixPath
if ($LASTEXITCODE -ne 0) { throw "signtool failed" }

# 4. Install it.
Add-AppxPackage -Path $msixPath -ForceApplicationShutdown
"Installed. Launch 'MarkStudio Editor' from the Start menu to verify it, then:"
"  Remove-AppxPackage -Package (Get-AppxPackage *MarkStudioEditor*).PackageFullName"
"to uninstall the test copy when you're done."
