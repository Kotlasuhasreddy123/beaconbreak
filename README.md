# SHADOW WARRIORS

Formerly Duskbound / Beaconbreak. An original Unity/C# two-player online co-op fighting game build. Each player uses
their own browser/device; both fight one adaptive boss. Includes friendly-fire
coordination, a ten-guardian authored story campaign, progression, and boss telemetry.

This is an independently developed playable submission build, not a finished
commercial-scale game or a Nekki product. See SUBMISSION-STATUS.md for limitations.

![Shadow Warriors camp](docs/screenshots/camp.png)

## The story

Two former guardians desert a machine that learns from every fight and erases
those it cannot predict. Across ten chapters, confront named wardens with their
own reasons to protect the Beacon. Every victory sends your habits ahead;
cooperation is the one pattern its architect struggles to understand.

The in-game chronicle reveals motives and chapter conclusions as you progress.
The story and interface are original; this is not a copy of Shadow Fight's IP.

## Play together

1. Open https://shadow-warriors.onrender.com on both devices.
2. Allow the free host time to wake if it has been idle.
3. Keep both game tabs open during play. Your PC does not host the public site.
4. Player 1 selects Create room, then Copy invite link or Share invitation.
5. Player 2 opens the invitation, explicitly accepts, then presses I'M READY.
   The eight-character code and Join by code remain available as a fallback.
6. Turn phones sideways. Touch buttons control the phone player; laptops use
   A/D, W or Space, J, K, L and U. Both players confirm between encounters.

Every confirmed victory plays a 32-second original three-shot silhouette scene:
the fallen guardian, its memory transmission, and the next opponent's reveal.
The final boss instead leads into the ending. Both players must vote to skip;
one vote alone cannot interrupt the other player's scene. The host authorizes
the transition, and scenes do not change combat or award extra progression.

Invitations are private links, not online accounts. Nicknames are local and
not globally unique or identity-verified. Friends lists, online presence and
account matchmaking are deferred for this submission at the owner's request.

The public service runs the game and WebSocket relay together on Render Free.
Local scripts remain available for development and temporary tunnel testing.
No paid account features are needed. See the current release audit for tested
coverage and remaining device checks.

## Ten guardians

Veyra teaches flanking; Soren counters repetition; Brann disrupts spacing;
Nima targets the damage leader; Oris alternates low and aerial hazards;
Sable rewards surrounding her; Ivo commits to a charge; Rhea changes timing;
Cael attacks both sides; Aster combines three phases. The defeat scenes carry
one connected plot: expose the prison network, accept the heroes' complicity,
and preserve its human memories while destroying its controller.

Legacy internal encounter IDs (10, 20, … 100) preserve existing save and skill
identities. The player-facing story has ten bosses, not 100 bespoke levels.

## Source and builds

- Game/: Unity 6000.6.5f1 project; install Web Build Support.
- Web/public/: current browser build, included so hosting needs no Unity license
  or editor on the server.
- Web/server.py: static hosting and two-player room relay.
- Game/TestResults/: simulation and browser test reports.
- Web/tests/: reproducible relay, multiplayer, controls and layout tests.

Open Game in Unity. Co-op Fighter > Build Web Game regenerates Web/public.
See Game/README.md and Web/README.md for architecture and hosting instructions.
Temporary logs, local environments, editor caches and old release archives are
excluded from version control.

## Compatibility and tests

Target: modern WebGL 2/WebAssembly browsers, keyboard laptops/desktops and
landscape touch devices. The arena preserves 16:9 and letterboxes wider screens;
portrait shows a rotation prompt. No promise is made for every browser/device.
Automated touch emulation is not physical iPhone/Android certification.

See Game/TestResults for measured outcomes. The host browser is authoritative;
this is private cooperative play, not a cheat-resistant competitive backend.
Progress is stored independently in each player's browser after a confirmed
victory, not a cloud account. Expedition upgrades are selected by the host and
apply to both fighters. Personal records and names are local to the device.

## Rights

All rights reserved for protectable original contributions; see LICENSE.
This is not an open-source license. Third-party rights and platform terms remain
in force. AI-assisted provenance is documented in Game/ASSETS.md.
