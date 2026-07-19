# Builds the redistributable MarkStudio Editor bundle.
# Output: dist\MarkStudioEditor\ (folder) and dist\MarkStudioEditor-<version>-win-x64.zip
# The bundle is fully self-contained: target PCs need no Visual Studio and no .NET install.

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$outDir = Join-Path $root 'dist\MarkStudioEditor'
$version = '1.0.0'

if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }

dotnet publish (Join-Path $root 'src\MarkdownEditor.App') `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none `
    -o $outDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

# Strip build artifacts that end-users do not need
Remove-Item (Join-Path $outDir '*.xml') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $outDir '*.pdb') -Force -ErrorAction SilentlyContinue

@'
MarkStudio Editor 1.0.0
=======================
A Markdown document viewer and editor with a Word-style toolbox,
live preview, and PDF export.

Developed by Praveen K P.
Built with .NET 8 (WPF/C#), Markdig, AvalonEdit, Microsoft Edge WebView2,
xUnit, and CommunityToolkit.Mvvm. Developed with Claude Code (Anthropic's Claude AI).

HOW TO RUN
----------
1. Copy this whole folder anywhere on the target PC (keep MarkStudioEditor.exe
   and the Assets folder together).
2. Double-click MarkStudioEditor.exe.

Nothing needs to be installed: the .NET runtime is bundled inside the exe.
The first launch may take a few extra seconds while it unpacks itself.

You can also open a file directly:  MarkStudioEditor.exe "C:\path\to\notes.md"
or drag a .md file onto MarkStudioEditor.exe.

REQUIREMENTS
------------
- Windows 10 (64-bit) or Windows 11.
- Microsoft Edge WebView2 Runtime - already present on Windows 11 and on any
  Windows 10 machine with Microsoft Edge. If the preview pane reports it is
  missing, install it free from:
  https://developer.microsoft.com/microsoft-edge/webview2/

NOTES
-----
- Settings are stored per user in %APPDATA%\MarkdownEditor\.
- If Windows SmartScreen warns about an unrecognized app (the exe is not
  code-signed), choose "More info" -> "Run anyway".
'@ | Out-File (Join-Path $outDir 'README.txt') -Encoding utf8

$zipPath = Join-Path $root "dist\MarkStudioEditor-$version-win-x64.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path $outDir -DestinationPath $zipPath

"Bundle folder: $outDir"
"Zip:           $zipPath ($([Math]::Round((Get-Item $zipPath).Length/1MB,1)) MB)"
