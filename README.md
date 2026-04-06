# Alt Command

Alt Command is a small Windows 11 tray app that lets `Left Alt` behave like a macOS-style `Command` key for the shortcuts you choose.

The core idea is simple:

- `Alt+C` sends `Ctrl+C`
- `Alt+V` sends `Ctrl+V`
- `Alt+W` sends `Ctrl+W`
- `Alt+Tab` stays `Alt+Tab`
- any unmapped `Left Alt + key` combination falls back to native Windows `Alt`

This keeps the finger motion close to macOS while preserving Windows behavior such as task switching.

## Why this stack

- `C# + .NET 8 + WinForms` is the fastest reliable path for a small global keyboard utility on Windows.
- The app runs as a tray process with a low-level keyboard hook and uses `SendInput` to inject the mapped shortcuts.
- GitHub Actions builds self-contained `.exe` bundles for `win-x64` and `win-arm64`, so end users do not need to install .NET.

## Features

- Full tray app with no main window
- Global `Left Alt` remapping
- Native `Alt` fallback for everything you do not map
- Multi-step shortcut sequences for macOS-style editing actions
- JSON config stored in `%LocalAppData%\AltCommand\hotkeys.json`
- Per-app exclusion list
- Launch-at-startup toggle
- One-click reload from the tray menu

## Default shortcuts

The first run writes a default config with common `Ctrl`-style shortcuts:

- `Alt+C`, `Alt+V`, `Alt+X`
- `Alt+A`, `Alt+S`, `Alt+Z`, `Alt+Y`
- `Alt+F`, `Alt+N`, `Alt+O`, `Alt+P`
- `Alt+W`, `Alt+T`, `Alt+R`, `Alt+L`, `Alt+Q`
- `Alt+Shift+T`
- `Alt+1` through `Alt+9`
- `Alt+Left`, `Alt+Right`, `Alt+Shift+Left`, `Alt+Shift+Right`
- `Alt+Up`, `Alt+Down`, `Alt+Shift+Up`, `Alt+Shift+Down`
- `Alt+Backspace`, `Alt+Delete`

## Config format

Example config:

```json
{
  "disabledProcesses": ["Code", "WindowsTerminal"],
  "remaps": [
    { "from": "Alt+C", "to": "Ctrl+C", "description": "Copy" },
    { "from": "Alt+V", "to": "Ctrl+V", "description": "Paste" },
    { "from": "Alt+Left", "to": "Home", "description": "Move to line start" },
    {
      "from": "Alt+Backspace",
      "toSequence": ["Shift+Home", "Backspace"],
      "description": "Delete to line start"
    },
    { "from": "Alt+Q", "to": "Alt+F4", "description": "Quit active window" }
  ]
}
```

Notes:

- process names can be written as `Code` or `Code.exe`
- each remap can use either `to` for one shortcut or `toSequence` for several shortcut steps
- `Right Alt` is untouched to avoid breaking `AltGr`
- `Alt+Left/Right` now behaves like macOS `Command+Left/Right`, so browser back/forward can be restored by removing those mappings or excluding the browser process

## Build locally

Install the .NET 8 SDK on Windows, then run:

```powershell
dotnet restore AltCommand.sln
dotnet build AltCommand.sln -c Release
dotnet publish src/AltCommand.Windows/AltCommand.Windows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Release on GitHub

The included workflow:

- builds on every push and pull request
- publishes `win-x64` and `win-arm64` zip artifacts
- creates a GitHub Release automatically when you push a tag like `v0.1.0`

## Known limitations

- the app is designed for Windows and should be built and tested on Windows 11
- some apps with aggressive keyboard handling may need to be added to `disabledProcesses`
- text-navigation mappings are opinionated defaults, so some users may want app-specific exclusions

## License

MIT
