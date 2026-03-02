# Traffic Rider (Unity 3D Scaffold)

A minimal Unity 2022.3 LTS 3D project scaffold for a Traffic Rider–style Android game. It includes:
- Endless highway driving
- Mission tracking and rewards
- Bike selection + upgrades
- Dedicated 3D main menu scene
- Crash VFX, slow‑mo, and game‑over screen
- Procedural placeholder assets

## Requirements
- Unity **2022.3.20f1** (Built‑in render pipeline)
- Android Build Support (for mobile builds)

## Quick Start
1. Open Unity Hub.
2. Add the project at `/traffic-rider`.
3. Open with **Unity 2022.3.20f1**.
4. Use `TrafficRider > Create Main Menu Scene`.
5. Press Play.

The game now uses a **dedicated Main Menu scene** and loads gameplay from there.

## Scenes
- **Main Menu**: `/traffic-rider/Assets/Scenes/MainMenu.unity`
- **New Game**: `/traffic-rider/Assets/Scenes/NewGame.unity`
- **Bike Menu**: `/traffic-rider/Assets/Scenes/BikeMenu.unity`
- **Mission Menu**: `/traffic-rider/Assets/Scenes/MissionMenu.unity`

Use the editor menu:
- `TrafficRider > Create Main Menu Scene`
- `TrafficRider > Create New Game Scene`
- `TrafficRider > Create Bike Selection Menu Scene`
- `TrafficRider > Create Mission Selection Menu Scene`

## Android Build Notes
1. Switch Build Target to Android.
2. Set `Company Name` and `Product Name` in Player Settings.
3. Build and run on device.

Mobile input uses accelerometer tilt + screen touch for throttle.

## Controls
- Desktop: `W/S` throttle/brake, `A/D` steer
- Mobile: tilt to steer, bike auto-accelerates, bottom-screen touch applies slow brake
- In-game: `Menu` button opens centered pause modal (`Resume`, `Settings`, `Home`, `Quit`)
- In-game: `Wheelie` button gives a temporary speed boost with cooldown

## Gameplay Systems
### Modes
- Endless: free run with distance + overtakes.
- Missions: select a mission from Missions menu; only selected mission is tracked.
- Mission success: when selected mission completes, run ends with a dedicated `Mission Complete` panel.

### Economy
- Run coins start at `0` for each run.
- Run coins are committed to total coins on game over / mission complete.
- Coins per meter and per overtake are defined in config.
- Completed missions are one-time and do not reward again.

### Bikes + Upgrades
- Bikes have base stats (speed, acceleration, handling, brake).
- Upgrades add stat modifiers on top of bike base stats.
- Player bike includes a placeholder dummy rider mesh.
- Each bike id now has a distinct placeholder 3D variant model (procedural kit differences).
- Bike selection uses left/right scrolling with a large 3D preview.
- Locked bikes are shown in preview at `20%` opacity.

### Traffic
- Vehicles spawn ahead of the player.
- Density scales with player speed and distance.
- Obstacles use lane changes (no intra-lane wobble drift) and can overtake slower same-direction traffic.
- Lane-change indicator on each obstacle blinks toward lane-change direction.
- Two‑way or one‑way traffic is chosen in `New Game` before run start.

### Crashes
- Colliding with traffic or guard rails triggers a crash.
- Crash plays a particle burst + audio beep.
- Slow‑motion and camera shake occur before the game‑over UI.

## Main Menu Scene
The main menu scene is 3D and uses:
- A static road slice + bike prop
- City backdrop blocks
- Menu UI overlay

Menu options by level:
- Main Menu: `New Game`, `Settings`, `Bike Selection`, `Buy Coins`, `Missions`, `Donate`, `About Me`, `Quit`
- New Game: `Endless`, `Traffic Type (Two-Way / One-Way)`, `Start`, `Cancel`
- Settings (main menu): `Coins`, `Top Score`, `Sync Status`, `Orientation (Auto / Land / Port)`, `Quality (Auto / Low / Mid / High)`, `Background (City / Desert / Green / Waste)`, `Missions` list, `Back`
- Mission Menu: mission list + `Back`
- Bike Menu: bike list + `Back`
  - Bike menu now has `Prev`/`Next` bike scroller + preview + `Unlock/Select` action.

## Settings
Settings include:
- Orientation (auto / landscape / portrait)
- Background theme (city / desert / greenery / wasteland)
- Quality preset in **main menu settings only** (`auto`, `low`, `mid`, `high`)
- Missions list (tick when completed)
- Bike selection on Game Over (bike list lives in the Bike Menu and Game Over)
- Main-menu settings opens as a centered modal overlay and blocks background interaction.
- `Back` button behavior in settings/bike/mission menus returns to the previous menu.

## Content
### Bikes
- Starter 125 — 0 coins
- Sport 600 — 1200 coins
- Super 1000 — 3000 coins
- Racer 01 — 4500 coins
- Racer 02 — 5300 coins
- Racer 03 — 6100 coins
- Racer 04 — 6900 coins
- Racer 05 — 7700 coins
- Racer 06 — 8500 coins
- Racer 07 — 9300 coins
- Racer 08 — 10100 coins
- Racer 09 — 10900 coins
- Racer 10 — 11700 coins

### Missions
Distance:
- Ride 1 km (300)
- Ride 2 km (600)
- Ride 5 km (1200)
- Ride 10 km (2500)

Overtake:
- Overtake 10 (400)
- Overtake 25 (900)
- Overtake 50 (1300)
- Overtake 100 (2000)

Coins (single run):
- Collect 250 coins (250)
- Collect 500 coins (600)
- Collect 1000 coins (1200)

## Configuration
Edit:
- `/traffic-rider/Assets/StreamingAssets/game_config.json`

This file controls:
- Bike list and prices
- Upgrade definitions
- Mission definitions
- Economy values

## Key Scripts
### Core
- `/traffic-rider/Assets/Scripts/Core/AutoBootstrap.cs`
- `/traffic-rider/Assets/Scripts/Core/GameManager.cs`

### Gameplay
- `/traffic-rider/Assets/Scripts/Gameplay/PlayerBikeController.cs`
- `/traffic-rider/Assets/Scripts/Gameplay/RoadSpawner.cs`
- `/traffic-rider/Assets/Scripts/Gameplay/VehicleSpawner.cs`
- `/traffic-rider/Assets/Scripts/Gameplay/TrafficVehicle.cs`
- `/traffic-rider/Assets/Scripts/Gameplay/CrashEffects.cs`
- `/traffic-rider/Assets/Scripts/Gameplay/CameraEffects.cs`
- `/traffic-rider/Assets/Scripts/Gameplay/BackgroundScroller.cs`

### UI
- `/traffic-rider/Assets/Scripts/UI/UIController.cs`
- `/traffic-rider/Assets/Scripts/UI/StartMenuUIController.cs`
- `/traffic-rider/Assets/Scripts/UI/BikeMenuUIController.cs`
- `/traffic-rider/Assets/Scripts/UI/SettingsUIController.cs`
- `/traffic-rider/Assets/Scripts/UI/MenuBackdrop.cs`

### Systems
- `/traffic-rider/Assets/Scripts/Systems/SaveSystem.cs`
- `/traffic-rider/Assets/Scripts/Systems/UpgradeSystem.cs`
- `/traffic-rider/Assets/Scripts/Systems/MissionSystem.cs`
- `/traffic-rider/Assets/Scripts/Systems/BikeSelectionSystem.cs`

## Notes
- Placeholder meshes use Unity primitives and runtime materials.
- UI is runtime‑generated for fast iteration.

## Traffic Tuning
### Spawn timing + density
- Spawn interval base: `0.8–2.4s` (scaled by speed + distance).
- Max vehicles: `6–14` (scaled by speed + distance).
- Density scaling range: `30–140 km/h`.
- Distance density scale: `+8% per km`.

### Lanes
- Lane positions: `[-7.5, -4.5, -1.5, 1.5, 4.5, 7.5]`.
- Same direction lanes: `0,1,2`.
- Oncoming lanes: `3,4,5`.
- Lane speed multipliers: `[0.75, 0.85, 0.95, 1.0, 1.1, 1.2]`.

### Vehicle types + weights
- Car: 55%
- Auto: 15%
- Bus: 12%
- Truck: 10%
- Ambulance: 8%

### Speed rules
- No fixed m/s or km/h obstacle speeds.
- Speeds are fully relative to player max speed by type:
  - Auto: `0.2x` to `0.5x`
  - Car: `0.5x` to `0.7x`
  - Bus: `0.3x` to `0.5x`
  - Truck: `0.3x` to `0.5x`
  - Ambulance: `0.35x` to `0.75x`
- Oncoming:
  - Spawn chance: `0.7`.
  - Speed multiplier: `× 1.56`.
- Same-direction vehicles are additionally capped below player cap to remain overtakeable.

### Swerving / Lane Change
- Intra-lane wobble is disabled.
- Swerving occurs only during lane changes.
- Same-direction traffic changes lane to pass slower blockers and may perform occasional random lane changes.
