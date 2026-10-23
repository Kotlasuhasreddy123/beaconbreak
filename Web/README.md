# SHADOW WARRIORS web edition (formerly Duskbound / Beaconbreak)

Open the game on two separate devices. The first player creates a room and shares the eight-character code. The other player enters that code, joins, and presses **I'M READY**. Both players confirm readiness after victory or defeat. There is no shared-keyboard mode in the web interface.

## Run on this computer

Run `START-GAME.ps1`, or install `requirements.txt` in a Python environment and run `python server.py`. Open `http://localhost:8787`. The second device uses `http://<host Wi-Fi address>:8787` on the same network. The server binds port 8787. Keep the computer awake and the server running. Run `ALLOW-LOCAL-PLAY.ps1` in Administrator PowerShell if Windows blocks the second device. It permits only local-subnet traffic to this server on port 8787. No router port forwarding is needed for devices on the same local network.

Keyboard: A/D move, Space or W jump, J low attack, K high/aerial attack, L guard, U tandem. Face the enemy to guard. Attack from opposite sides to avoid ally hits. Tandem unlocks at level 5; both fighters press U within eight simulation ticks near the boss. T toggles touch controls, M mutes impacts, Escape leaves the room. Touch input supports multiple fingers. Use landscape orientation on phones.

Each player's browser stores its own highest reached mission after a host-confirmed victory through Unity PlayerPrefs. The host chooses shared expedition upgrades; guests retain their own upgrade choices for games they later host. These are local saves, not online accounts. Use the same device/browser to resume; private browsing or clearing site data removes the save. A disconnect ends the room; create a new room to resume the last unlocked mission. Playing with a more advanced host can unlock later missions on the guest's device.

## Free-tier public hosting

For a temporary public playtest link hosted from this computer, run `START-SHARING.ps1` while the server is running. It uses Cloudflare Quick Tunnels, requires no account, and avoids an inbound Windows firewall change. The URL changes when the tunnel restarts and has no uptime guarantee. It is for testing, not a permanent competition submission URL. The local server and tunnel must remain running. `STOP-HOSTING.ps1` stops this project's server and tunnel.

This directory is a deployable server bundle once `public/` has been built. It contains a Dockerfile and a Render blueprint. Upload this directory as a repository root, or select `Web` as the service root directory. Create a Docker web service with the Free plan, port supplied through `PORT`, and health check `/health`. A single process serves the game and relays rooms. HTTPS automatically selects secure WebSockets.

Render supports WebSockets and a free web-service plan, subject to account quotas and idle shutdown. See https://render.com/docs/websocket and https://render.com/docs/free for current limits. Public hosting is not activated merely by including these files; it requires a hosting account and deployment. Do not use a static-only host for this bundle: the room server must also run. Do not scale it to multiple instances without adding shared room routing.

## Scope

Modern browsers with WebGL 2 and WebAssembly are the target. Desktop and touch-layout browser tests are reproducible in `tests/browser.cjs`; they are not physical Android/iOS certification. The host browser owns game state, so this is suitable for cooperative private rooms, not a cheat-resistant competitive service. The relay caps room count and message sizes, accepts two players per room, and cleans up disconnected peers.

## Rebuild

Open the Unity project in `../Game` with Unity 6000.6.5f1 and Web build support. Select **Co-op Fighter → Build Web Game**. It writes `public/`. For the simulation report, select **Co-op Fighter → Run 20 Player Scenarios**. Relay checks: `python tests/test_relay.py`.
