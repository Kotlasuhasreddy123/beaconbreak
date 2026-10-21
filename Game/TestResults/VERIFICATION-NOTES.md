# Verification scope — 21 October 2026

## Automated gameplay

The 20 categories in PLAYER-SCENARIOS.md cover novice, button-mashing, balanced, defensive, low-attack, aerial, flanking, tandem, aggressive, mixed-skill, support/attacker, ground/air, slow-reaction, idle-partner, fully idle, ally-targeting, retreating, delayed, jittered/reordered, and retransmitted-input players.

Each category runs on levels 1, 10, 25, 50, 75, and 100 with three seeds: 360 fights. Every trace checks deterministic replay, serialization/continued simulation, state bounds, and rollback convergence. A separate sweep tests all 100 levels using coordinated bots and injected delay/jitter; all 100 were cleared. These are scripted policies, not recruited human players.

Observed defects fixed during this iteration: null/default adaptation serialization mismatch; unreachable fifth weapon; guard protecting the back; untelegraphed duelist attack; boss range deadlocks; boss instant turning during active attacks; boss permanent stagger; excessive early difficulty; ally finishing blows; unbounded rollback history; missing input acknowledgments after snapshot catch-up; same-level retry identity; stale/same-tick snapshot handling; main-thread socket writes; mirrored animation transforms; mobile text entry.

## Real transport and browser checks

- Ten relay checks passed: health, invalid/missing/full rooms, duplicate host, pairing, third-player rejection, 200 ordered duplex messages, room isolation, disconnect cleanup, oversized input rejection. See relay-tests.json.
- The local WebGL game ran in two isolated Chromium contexts. Three encounters completed with identical terminal ticks and hashes on both peers. This included a same-level restart and next-level transition. Closing the guest returned the host to the menu. See browser-tests.json.
- Seven normal-interface/input checks passed through the public HTTPS tunnel: create/join/ready, keyboard movement, keyboard attack, touch movement, touch attack, host receipt of remote input, and leaving. See control-tests.json.
- The same three-encounter test also passed through the public HTTPS/WebSocket tunnel, with matching terminal ticks and hashes. See browser-tests-public.json.

## Practical limits

Touch was tested using browser device emulation and dispatched touch events, not physical iPhones or Android phones. The simulations validate game rules and synchronization, not human enjoyment or animation quality. The 100 levels are generated from four boss styles and use one arena. The cutscene is an animated telemetry report. The temporary public link depends on this computer staying awake; a stable hosted URL and the competition's own rules still need to be addressed before submission. The relay has not been load-tested for a public audience.

The Windows firewall rule could not be added because the process lacked administrator rights. The public tunnel was tested as an alternative and needs no inbound firewall rule. ALLOW-LOCAL-PLAY.ps1 is available for optional direct Wi-Fi play.
