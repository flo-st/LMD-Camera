# LMD-Camera

Camera mod for Lonely Mountains: Downhill that adds first- and third-person views.

## Features

- First-person view
- Third-person view
- Customizable settings

_Optional, enable in config:_

- Immersive ragdoll mode
- Hide rider
- In-game camera adjustments

## Installation

1. Set up [MelonLoader](https://github.com/LavaGang/MelonLoader) for Lonely Mountains: Downhill

> [!NOTE]
>
> If installer fails, download latest `MelonLoader.x64.zip` from [MelonLoader releases](https://github.com/LavaGang/MelonLoader/releases) and extract it into your game folder.

2. Download latest release from [Releases](https://github.com/flo-st/LMD-Camera/releases)
3. Place `.dll` in your game's `Mods` folder
4. Launch game

## Usage

### Switching Camera Modes

| Action                         | Default Key |
| ------------------------------ | ----------- |
| First-Person View              | `1`         |
| Default View (original camera) | `2`         |
| Third-Person View              | `3`         |

Last used view is restored on next launch. Switching is disabled during crashes.

### In-Game Camera Adjustments

> [!NOTE]
>
> Disabled by default. Set `AllowKeyAdjustments = true` in config to enable.

When enabled, use these keys in first- or third-person view:

| Action                   | Default Key   |
| ------------------------ | ------------- |
| Move camera up/down      | `↑/↓ Arrow`   |
| Move camera forward/back | `←/→ Arrow`   |
| Tilt camera down         | `Right Shift` |
| Tilt camera up           | `Right Ctrl`  |

Adjustments are saved automatically.

## Configuration

- Settings are in `UserData/LMD-Camera.cfg` and apply after restart
- Higher smoothing values follow faster, `0` disables smoothing completely
- Hotkeys use Unity [KeyCode](https://docs.unity3d.com/ScriptReference/KeyCode.html) names

## Building

1. Install [.NET SDK](https://dotnet.microsoft.com/download) (6.0 or newer)
2. Set up MelonLoader and launch game once to generate its assemblies
3. Run `dotnet build` in project folder

Built `.dll` is copied to your game's `Mods` folder automatically.

> [!NOTE]
>
> Expected game location: `..\..\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`\
> Installed elsewhere? Build with `dotnet build -p:GameDir="<game folder path>"`

---

Unofficial mod, not affiliated with or endorsed by Megagon Industries.
