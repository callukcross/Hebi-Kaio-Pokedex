# Windows installer

Run `powershell -ExecutionPolicy Bypass -File installer/build-installer.ps1` from the repository root.

The script produces three self-contained Windows executables:

- `artifacts/windows-installer/Install-HebiKaio-Pokedex.exe`: the distributable installer with the application payload embedded.
- `artifacts/windows-publish/HebiKaioPokedex.exe`: the unpackaged application executable.
- `artifacts/windows-publish/Uninstall-HebiKaio-Pokedex.exe`: the uninstaller included in the installed application directory.

The installer provides a per-user setup wizard with a selectable writable destination, shortcut options, progress reporting, and a completion page. It defaults to `%LOCALAPPDATA%\Programs\HebiKaio Pokedex`, never requests elevation, and rejects protected destinations that would require administrator privileges. It also registers the app in Windows Installed Apps. User profiles and modules remain under `%LOCALAPPDATA%\HebiKaioPokedex` when uninstalling so an accidental uninstall does not destroy user data.
