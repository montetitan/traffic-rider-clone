# Changelog

All notable changes to this project will be documented in this file.

## [0.3.2] - 2026-03-02
### Added
- Bike menu left/right scroller with large 3D preview of highlighted bike.
- Locked-bike preview opacity support (`20%` alpha) in garage preview renderer.
- Distinct placeholder 3D bike variants per bike id.
- Lane-change indicator blinking toward lane-change direction on obstacle vehicles.

### Changed
- New Game flow now selects traffic direction (`Two-Way` / `One-Way`) and no longer selects mission mode there.
- Settings menu no longer exposes one-way/two-way selector.
- Center road marking now switches by direction mode:
  - two-way: small solid double center lines
  - one-way: center dashed line
- Traffic lane direction assignment updated for two-way mode (left side oncoming, right side same-direction).
- Vehicle speed multipliers updated:
  - auto `0.2x–0.5x`
  - bus `0.3x–0.5x`
  - truck `0.3x–0.5x`
  - ambulance `0.35x–0.75x`
- Same-direction vehicle speeds further reduced/capped so player can overtake consistently.
- Obstacle movement model changed from intra-lane wobble to lane-change-based swerving.
- Draw distance/perceived smoothness tuning:
  - increased road/background horizon
  - extended vehicle spawn distance
  - reduced heavy lane-dash density per segment
- Multiple HUD visibility/reliability fixes for top-left gameplay stats.

### Fixed
- Mission menu back action reliability and fallback handling.
- Bike menu and mission menu list rendering/empty-state issues in runtime.
- Guard rail and obstacle collision fallback handling on mobile/fast movement.

## [0.3.1] - 2026-03-01
### Added
- Main-menu-only quality selector in Settings: `auto`, `low`, `mid`, `high`.
- Placeholder rider model on the player bike visual.

### Changed
- Settings/Bike/Mission menu `Home` actions renamed to `Back` and wired to previous-menu behavior.
- In-game HUD cleaned up to avoid overlap by removing duplicate `Coins` line (kept `Run Coins`).
- Mobile controls tuned: slow-brake now triggers from bottom touch area instead of any touch.
- Bike rigidbody stabilized (fixed Y position/no gravity) to reduce occasional jump behavior.
- Menu scene bootstrap now handles `MainMenu`, `BikeMenu`, and `MissionMenu` separately for reliable menu loading.
- Bike and Mission menu fallback loaders now force immediate local config attempt to avoid empty lists.

## [0.3.0] - 2026-03-01
### Added
- Dedicated Mission Menu scene creator and Mission menu UI flow.
- Dedicated Bike Menu scene creator and Bike menu UI flow.
- In-game pause modal with centered actions: `Resume`, `Settings`, `Home`, `Quit`.
- Quit confirmation dialogs in-game and on main menu.
- `About Me` panel in main menu.
- Wheelie action button with cooldown-based temporary boost.
- Coin collection missions for a single run: `250`, `500`, `1000`.
- Mission-complete end panel for Missions mode.

### Changed
- Main menu settings now behaves as centered foreground modal with blocked underlay interaction.
- Mission selection moved under New Game flow; selected mission only is tracked in Missions mode.
- Run coin economy updated: run starts from `0`, and run coins are committed to total at run end.
- Obstacle speed model updated to fully relative multipliers by vehicle type (no fixed base m/s range).
- Obstacle collision and pickup trigger reliability improved for mobile/physics consistency.
- Scene creation/menu references now use `MainMenu` and `NewGame` naming.

## [0.2.1] - 2026-03-01
### Changed
- Bike Selection now opens a dedicated Bike Menu panel instead of the Settings screen.
- Renamed scenes: `Menu` -> `MainMenu`, `Main` -> `NewGame`.

## [0.2.0] - 2026-02-27
### Added
- Dedicated 3D Menu scene with Start Menu UI (New Game, Settings, Bike Selection, Buy Coins, Quit).
- Background theme selection (city/desert/greenery/wasteland).
- Orientation selector (auto/landscape/portrait).
- Mission selection + completion ticks inside Settings.
- Additional bikes and missions.

### Changed
- Missions only track when Missions mode is active.
- Game now loads Main scene from Menu scene.

## [0.1.0] - 2026-02-27
### Added
- Unity 2022.3 LTS scaffold with procedural 3D placeholder assets.
- Endless highway gameplay loop with distance, overtakes, and coins.
- Missions system with rewards and progress tracking.
- Bike selection and upgrade systems with save/load support.
- Garage UI with bike list, upgrades list, and live 3D preview render.
- Traffic spawner with lane changes, density scaling, and lane speed variance.
- Crash handling with particle burst, audio beep, camera shake, and slow‑motion.
- Game over panel with run stats and restart.
- Auto‑bootstrapper for empty scenes and editor menu for scene creation.
