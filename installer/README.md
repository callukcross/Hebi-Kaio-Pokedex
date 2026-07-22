# Windows installer

Run `powershell -ExecutionPolicy Bypass -File installer/build-installer.ps1` from the repository root.

The script produces three self-contained Windows executables:

- `artifacts/windows-installer/Install-HebiKaio-Pokedex.exe`: the distributable installer with the application payload embedded.
- `artifacts/windows-publish/HebiKaioPokedex.exe`: the unpackaged application executable.
- `artifacts/windows-publish/Uninstall-HebiKaio-Pokedex.exe`: the uninstaller included in the installed application directory.

The installer performs a per-user installation in `%LOCALAPPDATA%\Programs\HebiKaio Pokedex`, creates Start Menu and optional Desktop shortcuts, and registers the app in Windows Installed Apps. User profiles and modules remain under `%LOCALAPPDATA%\HebiKaioPokedex` when uninstalling so an accidental uninstall does not destroy user data.
