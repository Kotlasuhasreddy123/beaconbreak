# Play Duskbound

The current temporary testing link is:

https://designing-clock-might-licenses.trycloudflare.com/

Local address on this computer: http://localhost:8787

1. Open the same link on two separate devices.
2. Player 1 selects **CREATE ROOM** and shares the eight-character room code.
3. Player 2 enters the code and selects **Join room**, or uses **Join by code** at the bottom of the page.
4. Player 2 selects **I'M READY**. After a fight, both players confirm readiness.
5. Use landscape orientation on phones. On a keyboard: A/D move, Space jump, J low, K high, L guard, U tandem. T toggles the touch controls.

This link is hosted from this computer. Keep the computer awake and online. The link stops working when its tunnel closes and changes after a restart. It is a playtest link, not a permanent submission URL. Use the deployment instructions in README.md for stable hosting.

To stop sharing, run `STOP-HOSTING.ps1`. To restart, run `START-GAME.ps1`, then `START-SHARING.ps1`; use the new HTTPS link printed by the tunnel.
