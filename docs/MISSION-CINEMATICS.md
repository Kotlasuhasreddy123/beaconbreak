# Mission cinematic system

The previous version replayed a guardian fall, travelling signal and chapter-villain reveal. Version 2.3 instead resolves a mission recipe after the confirmed result: first action, second action, then the actual next opponent. The final level has its own ending branch.

Ten action families: pursuit, vault, slide, wall-run, duel, rescue, chain-break, ambush, collapse and signal. Ten chapter backdrops: gate fortifications, dock cranes, metal orchard, market lanterns, storm columns, basilica arches, foundry gears, citadel towers, broken crossing and Beacon rings. These are original procedural compositions, not imported video or 100 individually animated films. Traversal shown in cinematics is not an interactive parkour mechanic.

Every mission has a unique title and opponent identity. On-screen numbering identifies the completed mission and next mission out of 100. Reveals use actual LevelSpec health, damage and speed; health differs for all 100 encounters, while damage and speed deliberately share bounded values between some levels for balance. No fabricated statistics or artificial adaptation values are displayed.

MissionCinemaValidation validates the complete catalog and exports Game/TestResults/mission-cinematics.csv. The existing cinematic gate and progression tests verify host authority, two skip votes, duplicate-event handling, ending persistence and simulation isolation. Browser tests exercise real two-player transitions; they do not visually validate all 100 scenes or prove the game is enjoyable to humans.

The live game remains a cooperative prototype. It does not replicate Shadow Fight 2 or Vector's assets, characters, choreography, full combat depth or production quality. No competition submission is authorized at present.
