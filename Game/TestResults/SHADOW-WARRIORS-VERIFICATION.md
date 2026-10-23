# Shadow Warriors 2.2 — October 23, 2026

Protocol: shadow-warriors-2.2-cinema-20261023.

Unity BuildVerifiedWeb succeeded, including the existing 360-run behavioral
matrix, all-level sweep, serialized continuation and rollback regressions.

New real-browser integration checks passed locally and through the public HTTPS link:

1. The host copies an invitation containing the active room.
2. Declining leaves the guest disconnected and does not start a match.
3. Accepting connects two separate browser contexts.
4. The first victory in the session automatically plays all three shots.
5. One skip vote cannot interrupt the other player's scene.
6. Two votes release the shared transition.
7. A later victory also plays, and three final match hashes agree.

Screenshots cover the fallen guardian, transmitted memory and incoming opponent.
These are original reusable in-engine scenes, not copied reference footage or
100 individually produced films. Mission identity, palette, skyline, opponent
silhouette and telemetry caption vary. The final mission uses an ending branch.
The final-boss transition was exercised by the editor gate test, but its visual
ending was not exercised by this browser flow, which plays missions 5, 5 and 6
for replay/transition coverage. The editor test additionally passed single-vote,
two-vote, guest-authority, duplicate-result and combat-state-isolation checks.

The menu/chronicle/upgrade/profile flows and 20 viewport checks also passed.
The responsive harness was updated to target the join dialog specifically now
that there are separate share and invitation dialogs.

Both players must still be connected and ready to fight. Nicknames and saves
are local to each browser/site address, not unique online identities. Accounts,
friends lists, online presence and account-based matchmaking were explicitly
deferred by the owner for this submission. Treat invitation links as private;
anyone with the link can request the vacant guest slot.

Tests use Edge/Chromium with touch emulation, not physical phones. Native share
sheet behavior depends on the device; unsupported browsers show a copyable URL.
