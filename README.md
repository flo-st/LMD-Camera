# LMD-Camera

A camera mod for Lonely Mountains: Downhill that adds customizable first-person and third-person camera modes.

## Features

- First-Person Camera
- Default Camera
- Third-Person Camera
- Persistent Settings
- Customizable Hotkeys
- Real-Time Adjustments
- Hide Rider in First Person
- Immersive Ragdoll Mode

## Installation

1. Set up [MelonLoader](https://github.com/LavaGang/MelonLoader) for Lonely Mountains: Downhill

> [!NOTE]
>
> If the installer fails, download the latest `MelonLoader.x64.zip` from the [MelonLoader releases](https://github.com/LavaGang/MelonLoader/releases) and extract it directly into the game directory.

2. Download the latest release from the [Releases page](https://github.com/flo-st/LMD-Camera/releases)
3. Place the `.dll` file in the `Mods` folder inside the game folder
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

| Action                   | Key           |
| ------------------------ | ------------- |
| Move camera up/down      | `↑/↓ Arrow`   |
| Move camera forward/back | `←/→ Arrow`   |
| Tilt camera down         | `Right Shift` |
| Tilt camera up           | `Right Ctrl`  |

Adjustments are saved automatically and persist between sessions.

## Configuration

Config file location: `UserData/LMD-Camera.cfg` inside the game folder

Created on first launch with default values. Changes apply after restarting the game.

Configurable settings:

- Camera positioning and pitch angle
- FOV, near clip plane, and smoothing
- Hotkey bindings
- Real-time adjustment toggle
- Rider hiding and immersive mode toggles

## Building

1. Install the [.NET SDK](https://dotnet.microsoft.com/download) (6.0 or newer)
2. Set up MelonLoader for the game and launch it once, so it generates the game assemblies
3. Run `dotnet build` in the project folder

The built `.dll` is copied into the game's `Mods` folder automatically.

> [!NOTE]
>
> Expected game location: `..\..\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`\
> Installed elsewhere? Build with `dotnet build -p:GameDir="<game folder path>"`

---

Disclaimer: This is an unofficial mod and is not affiliated with or endorsed by Megagon Industries.
