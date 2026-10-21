# Beaconbreak — web co-op edition

An original Unity 6000.6.5f1 martial-arts boss campaign for two players on separate devices. The primary release is now the WebGL game in `../Web/public`, served by `../Web/server.py`. Older Windows ZIPs in this workspace are historical builds and do not include this upgrade.

## Play

Run `../Web/START-GAME.ps1` and open `http://localhost:8787`. Player 1 creates a room. Player 2 opens the same hosted game on their own device, enters the eight-character code, and confirms readiness. Phones use landscape orientation and multi-touch controls; computers can use the keyboard. See `../Web/README.md` for local-network and free-tier deployment details.

Controls: A/D move, W or Space jump, J low strike, K high/aerial strike, L directional guard, U tandem, T touch overlay, M mute, Escape leave. Both players use the same controls on their respective devices. There is no shared-keyboard option in the released interface.

## Implemented

- 100 generated encounters in ten named acts, four boss policies, five weapon tiers, combo progression, and a 19-node team skill tree.
- Two fighters versus one boss. Friendly hits cause reduced damage, brief stun, and knockback, but cannot deliver the final blow to a teammate.
- Visible boss wind-ups, committed attack direction, directional guard, aerial/low attacks, bounded boss stagger, synchronized tandem strikes, and a three-minute round limit.
- Original painted arena, articulated silhouette fighters, procedural combat animation, hit effects, synthesized impact/ally-warning audio, and an animated telemetry-transfer report.
- Boss habit recording and bounded counter-adaptation. The host commits progression only after the terminal tick is confirmed; both players ready up for the next encounter.
- Browser room codes over WebSockets, version handshake, encounter IDs, bounded prediction and rollback, authoritative snapshots, disconnect recovery to the menu, keyboard and multi-touch controls.
- Local browser save for the room host. A new room resumes the last unlocked level; there is no mid-fight reconnect or cloud account save.

## Verification

`TestResults/PLAYER-SCENARIOS.md` reports 360 simulated fights across 20 player/network categories, six sampled levels and three seeds, plus a separate 100-level sweep. Coordinated bots cleared all 100 levels in that sweep. Every run checks deterministic replay, Unity JSON restoration, and delayed-input convergence. Wins, losses, and timeouts are reported separately.

`TestResults/relay-tests.json` records ten real loopback WebSocket checks. `TestResults/browser-tests.json` records independent browser contexts completing three encounters with matching final state hashes, including same-level restart, next-level progression, and disconnection. `TestResults/control-tests.json`, when present, records direct keyboard and emulated-touch checks. Screenshots are in `../output/playwright/`.

These results establish a tested playable demo, not parity with a commercially produced fighting game. The 100 levels reuse one arena and four boss archetypes. Story transitions are animated text/data reports rather than directed character cinematics. Physical Android/iOS devices, controllers, broad accessibility, public load testing, and competition-specific acceptance checks remain outside this verification.

## Build and test

Open `Assets/Scenes/Main.unity` in Unity. Use **Co-op Fighter → Build Web Game**. Web build support must be installed. The custom template is in `Assets/WebGLTemplates/Beaconbreak`. **Run 20 Player Scenarios** writes the simulation reports. **Build Windows Game** remains available for native direct-IP builds, but web is the tested release target for this iteration.

Engine-independent simulation is in `Assets/Scripts/Core`. Rendering, sound, sessions, and transports are in `Assets/Scripts/Unity`. WebSocket interop is in `Assets/Plugins/WebGL`. The server only forwards room traffic; the host browser owns simulation. It is a single-process cooperative demo service, not a competitive anti-cheat backend.

Asset provenance and the generation prompt are in `ASSETS.md`.
