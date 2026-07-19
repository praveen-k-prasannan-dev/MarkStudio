# Builds MarkStudioEditor_<version>_x64.msix - the package to upload in Partner Center -> Packages.
# Requires only the Windows SDK (MakeAppx.exe) - no Visual Studio needed.

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$version = '1.0.0.0'
$layoutDir = Join-Path $root 'dist\msix-layout'
$msixDir = Join-Path $root 'dist'
$msixPath = Join-Path $msixDir "MarkStudioEditor_${version}_x64.msix"

function Find-SdkTool([string]$name) {
    $roots = Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\bin" -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending
    foreach ($r in $roots) {
        $candidate = Join-Path $r.FullName "x64\$name"
        if (Test-Path $candidate) { return $candidate }
    }
    throw "$name not found under Windows Kits 10 bin - install the Windows SDK."
}

$makeAppx = Find-SdkTool 'makeappx.exe'
"Using: $makeAppx"

# 1. Publish a normal (non-single-file) self-contained layout - MSIX unpacks its own folder,
#    so there is no benefit to single-file bundling and it avoids self-extraction quirks.
if (Test-Path $layoutDir) { Remove-Item $layoutDir -Recurse -Force }
dotnet publish (Join-Path $root 'src\MarkdownEditor.App') `
    -c Release -r win-x64 --self-contained true -p:DebugType=none `
    -o $layoutDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }
Remove-Item (Join-Path $layoutDir '*.xml') -Force -ErrorAction SilentlyContinue

# 2. Drop the manifest and Store tile assets into the layout.
#    The app's own publish output already has an Assets\ folder (CSS themes, icon), so copy
#    the Store tile files individually rather than the whole folder (which would nest them).
$pkgSrc = Join-Path $root 'src\MarkdownEditor.App\Package'
Copy-Item (Join-Path $pkgSrc 'Package.appxmanifest') (Join-Path $layoutDir 'AppxManifest.xml') -Force
Copy-Item (Join-Path $pkgSrc 'Assets\*.png') (Join-Path $layoutDir 'Assets') -Force

# 3. Pack the MSIX
if (Test-Path $msixPath) { Remove-Item $msixPath -Force }
& $makeAppx pack /d $layoutDir /p $msixPath /o
if ($LASTEXITCODE -ne 0) { throw "makeappx pack failed" }

"Package built: $msixPath ($([Math]::Round((Get-Item $msixPath).Length/1MB,1)) MB)"
"Next: run scripts\test-install-msix.ps1 to sideload and test it locally before uploading to Partner Center."
