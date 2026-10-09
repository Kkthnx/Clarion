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
- Verify only checks the current account for app removal unless the tweak removes for all accounts.
- No scheduled or background scan. It runs when the app opens and after each change.
- No screenshot check of the new page layout yet, only a structure check through UI Automation.
- Troubleshooter (#3) can reuse the scan: same journal, same `FindDrifted` step names.

## Deprovision option (#1, second half)

### Research
- Microsoft documents `Remove-AppxProvisionedPackage` as: the app will not be installed for new
  accounts, and it is not removed from existing accounts. So it is a different effect from removing
  the app for the current account, and Clarion needs both.
- Whether feature updates re-provision removed inbox apps is documented for Windows 10 (the 1607
  guidance was to clean up after the upgrade). Nothing found for current Windows 11 builds. That is
  why Verify checks for it instead of Clarion claiming it prevents it.
- Windows 11 25H2 added a "Remove Default Microsoft Store Packages" policy. Two sources disagree on
  whether it also removes the apps for existing users. Not used. Revisit when Microsoft documents it.

### Decisions
- The option is a variant of the catalog entry (`Appx/DeprovisionVariant`), not 25 more catalog
  entries. Expert mode only, needs administrator rights, shown in the detail pane.
- It is locked once the journal shows it was applied, because Revert cannot undo it.
- Drift now checks the steps the journal recorded, not the catalog steps. Otherwise an app that is
  gone but provisioned again looks fine, since the catalog entry says deprovision false.
- Handlers can explain a broken step (`WhyBroken`) so the page says "installed again" only when the
  app really is installed, and "will be installed for new accounts again" when only the package returned.

### Check done
- UI Automation smoke test: Expert mode on, Debloat, select an app, checkbox and note present, app stays up.
  Did not click it, since that would queue a real change on this PC.
- Side finding: Settings says "Clarion has no telemetry and sends nothing anywhere." Any telemetry
  work (#7) has to change that promise on purpose.

## Troubleshooter (#3)

### Design
- `Catalog/Symptoms/symptoms.json`: 22 symptoms in a person's words. Each names catalog topics and/or
  specific setting ids, plus a tip that only claims what those settings do.
- `Troubleshoot/Troubleshooter` filters to settings the journal says are still on, puts named ones
  before same-topic ones, then newest first. Newest first because a change made just before the
  problem is the most likely cause.
- Reverts go through the normal review dialog (`AppServices.QueueRevert`).

### Guard tests (so the list cannot rot)
- Every topic and setting id in the symptom file must exist in the catalog.
- Every setting must be reachable from some symptom, except silent topics (Diagnostics, Suggestions
  and ads, Defender sharing) and `system.long-paths`. Adding a catalog entry with no symptom now
  fails the build, which is the point.
- First run of the coverage test found 4 gaps (permissions, sensors, lock screen, recent items) and a
  typo'd id (`perm.user-text`). Fixed.

### Limits
- Symptoms are curated by hand, not measured. Real reports (#7) are the way to improve the mapping.
- A setting Windows already changed back is listed but its Revert is disabled, with a reason.

## Signing (#5)

### Research
- SignPath Foundation signs open source projects for free. Needs an OSI approved license with no
  commercial dual licensing, no proprietary components, an automated build, a released version, and
  active maintenance. The publisher shown to users is SignPath Foundation.
- Azure Artifact Signing is the paid route. Individuals must be in the US or Canada for public trust.
  Microsoft's own pages disagree on the organization country list.
- SignPath GitHub action: `signpath/github-action-submit-signing-request@v3` after an
  `actions/upload-artifact` step, inputs documented at docs.signpath.io.

### Blocker found
- README says "All rights reserved for now" and there is no LICENSE file. The free route needs an
  open source license. This is the owner's decision. Not changed.

### Built
- `scripts/build-installer.ps1 -PublishDir` packages an existing folder.
- `.github/workflows/release.yml`: build, test, package. Two signing requests (app files, then setup)
  that run only when `SIGNPATH_ORGANIZATION_ID` is set. Passes `actionlint` (installed with winget).
  Has not run on GitHub yet.
- `docs/SIGNING.md` with options, the blocker, setup steps and what is not known.
- README beta note now ties 1.0 to signing.

## Opt-in results (#7)

### Research
- Common ground across Homebrew, VS Code, Fedora, Ubuntu and open source discussion: default off,
  show the exact payload, do not degrade the tool when declined, name every field, scrub paths and
  free text, one easy way out. Opt-in gives smaller and skewed samples, which is the cost.

### Decision
- No network telemetry. Clarion promises in its README and Settings page that it has none, and
  that promise is worth more than the data. Breaking it would also need an endpoint, a privacy
  policy and retention rules, none of which exist.
- Instead: an optional "How settings are holding up" section in Report a problem. Setting ids,
  holds or changed back, and the Windows build. Off by default, shown in the preview, passed through
  the same sanitizer, and the person opens it on GitHub themselves.
- Revisit real telemetry only with an explicit decision to change the promise.

### Open
- Someone has to read the reports and update `lastVerifiedBuild` by hand. No tooling for that yet.

## Second review: engine and safety layers

Each claim was checked against the code and researched before changing anything.

### 1. Restore point when System Protection is off (confirmed)
- `EnableProtection` was only declared and implemented, never called. Confirmed by search.
- Research: `Enable-ComputerRestore` is the documented fix for protection being off on the system
  drive. A group policy (`DisableSR`, `DisableConfig`) can also block it, so a failed enable must
  show its error. Sources did not give a reliable default storage size, so no figure is stated.
- Built: `RestorePointExtensions.CreateEnsuring` (create, and if allowed enable protection and retry
  once, reporting both errors if both fail). `BatchOptions.TurnOnSystemProtection`. Apply dialog
  and Repair page ask: turn on and retry, continue without one, or cancel. CLI: `--enable-protection`.
- The Repair page used to log "Restore point failed. Continuing." and carry on. It now asks first.
- Nothing is turned on without the person choosing it.

### 2. Update services wording (confirmed)
- The telemetry entry said Clarion never touches the update services, but Reset Windows Update stops
  and restarts them. Reworded to "None of the privacy settings turn off Windows Update".
  DESIGN.md says update patches are never turned off, which is still true, so it was left.

### 3. Partial batch failures (claim was wrong)
- `BatchRunner` has no early exit. A failure at step 12 does not stop 13 to 30. A test now proves it.
- The real gap was the wording. The panel said "N made, M did not finish" and left out settings that
  were skipped. It now adds "K left as they were". The summary also says the others ran as normal.
