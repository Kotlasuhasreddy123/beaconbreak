# Duskbound 2.1 verification — October 23, 2026

Build protocol: duskbound-2.1-story-20261023. Unity 6000.6.5f1 Web build succeeded.

Executed in this revision:

- 360 simulation runs over 20 player-behavior categories: determinism, snapshot
  continuation and delayed-input rollback checks passed. Wins and losses are
  reported separately; this is not a claim every player category wins.
- All 100 missions swept with the coordinated bot pair; 100 wins, with replay,
  JSON restore and delayed-input checks. Not a human campaign completion.
- 20 browser viewport checks passed: fitting, proportions, rotation prompt and
  join-dialog bounds. These use Chromium/Edge, not physical mobile devices.
- Seven control/integration checks passed with laptop and touch-phone contexts,
  including a run through the new public HTTPS tunnel.
- Three successive encounters completed both locally and through the public
  HTTPS tunnel with identical final states on both clients, including replay
  and next-mission transition.
- Ten relay checks passed, including distinct missing/full-room errors.
- Five campaign UI flows passed: branded load, guide, chronicle, bounded upgrade
  paging, and profile-name save/reload. The restored name was visually checked.
- Editor progression validation passed: fighting state gives no reward,
  confirmed victory advances progress, duplicate results are idempotent,
  replay cannot downgrade progress, automated bots do not alter saves, and the
  final mission sets the completion flag. Editor preferences were restored.

Visual inspection: camp, chronicle, upgrade cards and restored profile. The
gold primary button has dark text; secondary buttons use light text on teal.
Locked upgrade buttons are disabled. The existing silhouette arena remains.

Reproduction: use the scripts in Web/tests and the Unity validation methods in
Assets/Editor. ProgressionValidation.Run exercises the real save method.

Still unverified: physical Android/iOS, Safari, weak-device frame rate, hostile
clients, hosting uptime and event-specific submission compliance. No cloud
account system exists. See the root SUBMISSION-STATUS.md before submitting.
