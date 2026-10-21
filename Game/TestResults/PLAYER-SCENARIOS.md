# Beaconbreak: 20 scenario simulation report

360 simulated fights: 20 behavior/network categories × 6 levels × 3 seeds. Each fight is capped at 180 simulated seconds. Input scripts use observed game state; these are reproducible bots, not human subjects. A timeout is recorded, not counted as victory.

Network checks replay the same input trace through delayed/reordered/retransmitted delivery. They verify convergence, not subjective latency feel. Live two-process transport is a separate test.

| Scenario | Wins | Losses | Timeouts | Average seconds | Ally hits | Replay / snapshot / rollback |
|---|---:|---:|---:|---:|---:|---|
| 01 First-time duo | 3 | 15 | 0 | 20.4 | 166 | PASS |
| 02 Button mashers | 9 | 9 | 0 | 12.6 | 411 | PASS |
| 03 Balanced duo | 18 | 0 | 0 | 22.3 | 491 | PASS |
| 04 Defensive duo | 9 | 9 | 0 | 53.1 | 643 | PASS |
| 05 Low-strike specialists | 3 | 15 | 0 | 15.6 | 296 | PASS |
| 06 Aerial specialists | 6 | 12 | 0 | 18.1 | 217 | PASS |
| 07 Opposite-side flankers | 18 | 0 | 0 | 19.3 | 377 | PASS |
| 08 Tandem coordinators | 18 | 0 | 0 | 17.7 | 375 | PASS |
| 09 Aggressive duo | 3 | 15 | 0 | 14.2 | 353 | PASS |
| 10 Expert with novice | 18 | 0 | 0 | 39.1 | 197 | PASS |
| 11 Guard and attacker | 6 | 12 | 0 | 49.7 | 308 | PASS |
| 12 Ground and aerial | 6 | 12 | 0 | 20.7 | 209 | PASS |
| 13 Slow reactions | 3 | 15 | 0 | 16.0 | 253 | PASS |
| 14 Idle partner | 6 | 12 | 0 | 58.0 | 0 | PASS |
| 15 Both idle | 0 | 18 | 0 | 23.6 | 0 | PASS |
| 16 Ally-targeting player | 6 | 12 | 0 | 27.4 | 481 | PASS |
| 17 Retreating duo | 0 | 18 | 0 | 22.0 | 0 | PASS |
| 18 100ms one-way delay | 18 | 0 | 0 | 19.3 | 377 | PASS |
| 19 200ms plus jitter and reorder | 6 | 12 | 0 | 33.8 | 316 | PASS |
| 20 Retransmitted lost inputs | 18 | 0 | 0 | 19.3 | 377 | PASS |

Regression checks: all 100 level definitions; fifth weapon reachable; frontal/rear guard; nonlethal ally hits; bounded prediction; version mismatch rejection; adaptive low/air transfer; serialized continuation; skill prerequisite/budget validity. All passed.

Additional all-level sweep: 100/100 wins with the coordinated bot pair. Each level starts with a fresh loadout and a synthetic prior adaptation profile; this is level coverage, not a claim that a human completed the campaign. Every run also passed deterministic replay, JSON restore, and delayed-input convergence.
