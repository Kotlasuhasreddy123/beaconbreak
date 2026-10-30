# Shadow Warriors: 20 scenario simulation report

600 simulated fights: 20 behavior/network categories × 10 guardians × 3 seeds. Each fight is capped at 180 simulated seconds. Input scripts use observed game state; these are reproducible bots, not human subjects. A timeout is recorded, not counted as victory.

Network checks replay the same input trace through delayed/reordered/retransmitted delivery. They verify convergence, not subjective latency feel. Live two-process transport is a separate test.

| Scenario | Wins | Losses | Timeouts | Average seconds | Ally hits | Replay / snapshot / rollback |
|---|---:|---:|---:|---:|---:|---|
| 01 First-time duo | 10 | 20 | 0 | 39.2 | 943 | PASS |
| 02 Button mashers | 27 | 3 | 0 | 13.8 | 908 | PASS |
| 03 Balanced duo | 30 | 0 | 0 | 18.7 | 606 | PASS |
| 04 Defensive duo | 27 | 3 | 0 | 56.6 | 1264 | PASS |
| 05 Low-strike specialists | 18 | 12 | 0 | 23.1 | 1190 | PASS |
| 06 Aerial specialists | 27 | 3 | 0 | 29.6 | 622 | PASS |
| 07 Opposite-side flankers | 30 | 0 | 0 | 14.7 | 570 | PASS |
| 08 Tandem coordinators | 30 | 0 | 0 | 13.2 | 531 | PASS |
| 09 Aggressive duo | 24 | 6 | 0 | 21.3 | 1092 | PASS |
| 10 Expert with novice | 30 | 0 | 0 | 26.5 | 830 | PASS |
| 11 Guard and attacker | 24 | 6 | 0 | 33.9 | 1016 | PASS |
| 12 Ground and aerial | 27 | 3 | 0 | 27.4 | 670 | PASS |
| 13 Slow reactions | 21 | 9 | 0 | 26.3 | 1075 | PASS |
| 14 Idle partner | 30 | 0 | 0 | 43.5 | 6 | PASS |
| 15 Both idle | 0 | 30 | 0 | 58.8 | 0 | PASS |
| 16 Ally-targeting player | 29 | 1 | 0 | 25.7 | 1177 | PASS |
| 17 Retreating duo | 0 | 30 | 0 | 46.8 | 0 | PASS |
| 18 100ms one-way delay | 30 | 0 | 0 | 14.7 | 570 | PASS |
| 19 200ms plus jitter and reorder | 30 | 0 | 0 | 27.2 | 799 | PASS |
| 20 Retransmitted lost inputs | 30 | 0 | 0 | 14.7 | 570 | PASS |

Regression checks: all 100 level definitions; fifth weapon reachable; frontal/rear guard; nonlethal ally hits; bounded prediction; version mismatch rejection; adaptive low/air transfer; serialized continuation; skill prerequisite/budget validity. All passed.

Additional all-level sweep: 100/100 wins with the coordinated bot pair. Each level starts with a fresh loadout and a synthetic prior adaptation profile; this is level coverage, not a claim that a human completed the campaign. Every run also passed deterministic replay, JSON restore, and delayed-input convergence.
