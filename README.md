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

- Camera positioning and pitch angle (first-person & third-person)
- FOV, near clip plane, and smoothing
- Hotkey bindings
- Real-time adjustment toggle

---

Disclaimer: This is an unofficial mod and is not affiliated with or endorsed by Megagon Industries.
