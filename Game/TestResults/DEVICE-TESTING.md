# Device-layout and two-player checks — 2026-10-21

This run changed the web shell, not the deterministic combat rules.

Passed 20 rendered viewport checks:
320x568, 360x640, 375x667, 390x844, 412x915, 430x932,
568x320, 640x360, 667x375, 844x390, 915x412, 932x430,
768x1024, 1024x768, 820x1180, 1180x820,
1280x720, 1366x768, 1920x1080, 2560x1080.

Each verified: positive canvas dimensions, preserved 16:9 ratio, canvas inside
viewport, no horizontal overflow, correct portrait rotation prompt, usable
join dialog. Screenshots were inspected for the wide-phone menu and combat.
Screen resizing covers orientation changes without restarting Unity.

Passed 7 real-input integration checks over the temporary public HTTPS link:
room creation, joining/readiness, laptop keyboard movement and attack,
phone-emulated touch movement and attack, replication of touch attack to host,
and leaving/disconnect cleanup. The report groups these into seven assertions.
Contexts: 1366x768 laptop and 844x390 landscape phone, device scale factor 3.

Passed 10 relay tests, including two-player pairing, isolation, message ordering,
duplicate host/third player rejection, disconnect cleanup and oversize rejection.

Reproduce:

- python Web/tests/test_relay.py
- node Web/tests/responsive.cjs <installed-playwright-module-path> <game-url>
- node Web/tests/controls.cjs <installed-playwright-module-path> <game-url>

Install Playwright separately for browser checks; scripts use installed Edge.
Start the server before browser tests. JSON reports are stored in this folder.

Limits: Chromium/Edge on Windows with touch emulation, not actual Android/iOS,
Safari, Firefox, a screen notch, an on-screen mobile keyboard, or low-memory
hardware. Safe-area CSS is included but real cutout/browser-bar behavior still
needs device validation. Portrait is a rotation-prompt mode, not a usable combat
layout. A 320px-high landscape device renders smaller touch targets than a tablet.
No universal-device or frame-rate guarantee is made.

Before competition: test two physical devices on separate networks, phone
rotation, software keyboard, multitouch, mute/fullscreen, interruptions, and a
full session with the host PC staying awake. A permanent hosted URL has not
been deployed by these tests.
