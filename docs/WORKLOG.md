# Work log

Running notes on what was done, what was found, and what is still open. Newest first.

## Verify scan and returned apps

Goal: one "Verify my settings" scan that lists everything Windows (or the user) changed back
since Clarion applied it, with apps that came back called out and a one click fix.

### What the code already had
- `TweakEngine.Detect` reads the live state of any tweak. `ChangeJournal.TweakIdsWithOutstanding`
  lists tweaks Clarion applied that nobody reverted. Drift is the gap between the two.
- `AppServices.Apply` already marked a row `IsDrifted` ("Changed back by Windows") on the tweak
  pages. It lived in the app layer, had no tests, and had no summary view or fix action.
- Presets, the batch runner and the pending queue already handle restore points and journaling,
  so a fix should go through them and not around them.

### Decisions
- The scan logic lives in Core (`Drift/DriftScanner`) so it can be tested with the existing fakes.
- Returned apps are the same drift, filtered to tweaks made of app removal steps. No second engine.
- The last scanned Windows build is stored in `drift.json` beside the journal. If the build
  changed since the last scan, the report says a feature update happened in between.
- "Stop tracking" ends tracking for one tweak without touching Windows. It is a new journal action.

### Built so far (core)
- `Drift/DriftScanner`, `DriftReport`, `DriftItem`, `DriftState` in Clarion.Core.
  The scan checks only tweaks the journal says are still on, skips editions the tweak does not
  support, lists unreadable ones instead of failing, and always invalidates cached reads first
  (a stale app list would hide a returned app).
- `TweakEngine.FindDrifted` names the exact steps that no longer hold. `TweakEngine.Release` stops tracking.
- `JournalAction.Release` added. `OutstandingFor` and `TweakIdsWithOutstanding` now treat any
  non Apply entry as the end of tracking.
- `ClarionRuntime.Drift` exposes the scanner. Scan state is stored in `drift.json` in the data folder.
- 13 tests in `DriftTests.cs`. Suite is at 225 passing.

### Things learned
- The Appx handler only tests the current account unless the tweak sets AllUsers, so a removed
  app that returns for another account is not seen. Matches what Clarion removed, so left alone.
- Old versions reading a journal with a `Release` line skip that line and treat the tweak as still on.
  Harmless, noted for the changelog.
- A shell heredoc containing a quote in the test file broke the command. Use the file tool for
  large files.

### Built so far (app)
- Verify page (`Pages/VerifyPage`), menu badge with the count, Home banner. The scan runs after
  every state refresh, so the badge stays current without opening the page.
- "Put back" and "Remove again" queue the settings and open the normal review dialog, so the
  restore point choice and the journal behave as for any other change. "Leave it" calls `Release`.
- `AppServices` keeps the last report and keeps the "feature update happened" note for the session,
  because each scan saves the current build.

### Found while testing on this PC
- Real drift on first run: Ultimate Performance power plan was applied, nothing reverted it, and
  Balanced is active. Correctly reported.
- Real bug in my first version: choosing another member of an exclusive group (power plans, DNS
  providers) leaves the old one outstanding in the journal, so it would be reported forever.
  Fixed in the scanner: a tweak is not drift when another tweak in its group is currently on. Two tests.
- Driving the app with UI Automation from a shell works for smoke checks: start the exe, select the
  menu item with SelectionItemPattern, read element names.

### Still open
- Deprovision option for removed apps (so new accounts and feature updates do not bring them back).
  Needs an Expert toggle and a note that revert then needs the Store.
- Verify only checks the current account for app removal unless the tweak removes for all accounts.
- No scheduled or background scan. It runs when the app opens and after each change.
- No screenshot check of the new page layout yet, only a structure check through UI Automation.
- Troubleshooter (#3) can reuse the scan: same journal, same `FindDrifted` step names.
