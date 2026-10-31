# Ten Guardians release audit — October 30, 2026

## Scope
Ten story guardians replace the procedural story route. Each has a distinct
combat rule, an authored aftermath and a staged silhouette animation. Persistent
legacy IDs are 10 through 100 in steps of ten; the UI calls them guardians 1–10.
Existing progress and skill storage identities are retained.

## Why the old build needed work
100 encounter IDs disguised a small number of repeated experiences. Scene
permutations were presented as content breadth. Four boss movement styles did
not give players enough reasons to change tactics. Functional tests could pass
while beginners struggled and coordinated bots cleared everything quickly.

## Verification
- 600 deterministic fights: 20 scripted player/network categories, all ten
  guardians, three seeds. Every run passed replay, snapshot and delayed-input
  convergence. Wins and losses are recorded; PASS does not mean every bot won.
- Additional 100-ID compatibility sweep retains coverage of legacy definitions.
- Focused checks cover all ten guardian rules, save migration, nine transfers,
  low/high habit counters, shared skip votes, duplicate results and the ending.
- Each guardian's victory and three story shots were exercised with two separate
  browser contexts. Host/guest final hashes matched for all ten; no page errors.
- Human-style keyboard/touch control tests and camp/profile/upgrade UI checks
  passed locally. Phone tests use browser emulation, not a physical phone.
- Final release rechecks passed for Soren, Sable and Aster, including the
  high-strike counter, changed armor rule, three-shot story and ending.
- Final keyboard/touch controls, 20 viewport sizes and seven invitation/cinema
  checks passed. This includes single-vote refusal and two-vote skip consent.

## Interpretation
The game is a playable indie submission candidate, not a commercial-quality
finished production or a verified competition winner. These tests establish
functionality and deterministic networking, not enjoyment. Story scenes share
rigs and rendering primitives; they are not pre-rendered films. Expert bots can
finish fights quickly. Beginner fixtures still lose later encounters. Physical
phone performance, Safari and subjective pacing remain owner acceptance checks.

## Submission boundary
The owner asked to test before submission. Nothing has been submitted. No paid
hosting plan, billing feature or account service was added. Keep using the
existing Render Free service; its sleep/restart limits remain applicable.

Public-site recheck October 31: seven keyboard/touch/room control checks passed; Soren victory, all three story shots, host/guest hash agreement and advancement to guardian three passed. A prior three-second touch assertion timeout did not reproduce on retry. Physical phone testing remains outstanding.
