# LMD-Camera

A camera mod for Lonely Mountains: Downhill that adds customizable first-person and third-person camera modes.

## Features

- First-Person Camera
- Default Camera
- Third-Person Camera
- Persistent Settings
- Customizable Hotkeys
- Real-Time Adjustments

## Installation

1. Set up [MelonLoader](https://github.com/LavaGang/MelonLoader) for Lonely Mountains: Downhill

> [!NOTE]
>
> If the installer fails, download the latest `MelonLoader.x64.zip` from the [MelonLoader releases](https://github.com/LavaGang/MelonLoader/releases) and extract it directly into the game directory.

2. Download the latest release from the Releases page
3. Place the `.dll` file in your `Mods` folder
4. Launch the game

## Usage

### Switching Camera Modes

| Action                              | Key |
| ----------------------------------- | --- |
| First-Person View                   | `1` |
| Default View (original game camera) | `2` |
| Third-Person View                   | `3` |

Camera mode is saved automatically and persists between sessions.

### Real-Time Adjustments

> [!NOTE]
>
> Disabled by default. Set `AllowKeyAdjustments = true` in the config file to enable.

When enabled, use the following keys while in First-Person or Third-Person mode:

| Action                           | Key           |
| -------------------------------- | ------------- |
| Adjust vertical position         | `↑/↓ Arrow`   |
| Adjust forward/backward position | `←/→ Arrow`   |
| Tilt camera down                 | `Right Shift` |
| Tilt camera up                   | `Right Ctrl`  |

Adjustments are saved automatically and persist between sessions.

## Configuration

Config file location: `UserData/LMD-Camera.cfg`

Created on first launch with default values.

Configurable settings:

- Camera positioning and pitch angle
- FOV, near clip plane, and smoothing
- Hotkey bindings
- Real-time adjustment toggle

## Building

Requires the .NET SDK and a MelonLoader install that has been launched once, so the Il2Cpp assemblies exist.

```sh
dotnet build
```

The game is expected at `..\..\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`. Override it with the `LMD_GAME_DIR` environment variable or `-p:GameDir=<path>`. The built DLL is copied into the game's `Mods` folder.

---

Disclaimer: This is an unofficial mod and is not affiliated with or endorsed by Megagon Industries.
