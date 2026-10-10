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

### 7. Saved setup files and versions (partly already done)
- `ProfileFile` already carried a `format` number and refused any other value. Kept.
- The refusal gave one message for older and newer files. It now says which, and what to do.
  There is no migration, so the advice for an older file is to choose the settings again.
- Catalog ids that no longer exist are already reported as unknown when a setup is queued.

### 8. DNS revert and adapters that come and go (confirmed)
- Restore addressed adapters by index alone. If an adapter had gone, the call failed and revert stopped
  before the encrypted DNS entries were handled. Verification also failed when no adapter was connected
  (an offline laptop), because it compared against the list of connected adapters.
- Research: interface indexes are assigned at runtime, so a stored index can point at nothing, or at
  a different adapter later. Resetting a missing adapter is a no-op, not an error worth failing for.
- Only physical adapters are ever changed (`HardwareInterface`), so a VPN adapter is not touched at
  apply time. The same problem applies to docking station and USB adapters, and to Wi-Fi switched off.
- Built: `IDnsStore.Find(index)` returns an adapter even when not connected. Restore acts only when the
  index still has the same name. Gone adapters are skipped. Verification ignores adapters that are gone.
  Four tests: gone, present but not connected, none connected, index reused by something else.
- The real PowerShell for `Find` was run against this PC: an existing index returns its record and a
  missing one returns an empty list.

### 4. Restart queue for locked files (confirmed, and a bug found)
- Research: the restart list is a registry value of pairs, a file and an empty entry for delete. Order
  matters. Other programs write to it. Cancelling should remove only your own entries. Deleting the
  whole value is what forum advice suggests, and it would break other installers.
- This PC's list has 5 delete operations from an NSIS installer, written with a `*1` prefix. The parser
  handles that prefix and never matches those entries.
- Bug: `RememberQueued` overwrote `pending-delete.json` on every run, so a second cleanup before a
  restart forgot the first one's files. Replaced by `RestartQueue`, which adds up across runs.
- Built: `RestartQueue` (record, view, cancel, settle after boot), `PendingDeletes` (pure parsing and
  removal), `IPendingRenameStore` with a registry implementation. Cleanup page shows a card with the
  exact files and a cancel button, and counts other programs' operations without listing them.
- Preview: it never tried to delete, so it could not know which files were locked. It now asks Windows
  with a delete-access open (`IsLocked`) and lists the files a real clean would queue. Locked files are
  no longer counted as freed in a preview.
- Real tests on this PC: the lock probe against a file held open, and queue then list then cancel with
  the registry value checked before and after (10 entries both times).
- Limit: only `PendingFileRenameOperations` is handled. The `...2` value was not present here, and
  one source mentions it, so it is not covered.

### 5. Event log clearing (agreed in part)
- Research: clearing logs is a documented defense evasion technique (MITRE T1070.001), and Windows
  records each clear as an event (104 in the System log, 1102 for Security). The .NET call that clears a
  log can also write a backup first.
- Tested on a throwaway custom log: a bad backup path throws and leaves the log intact, a good one
  clears the log and writes a readable .evtx. So "no copy, no clear" holds.
- Kept the row, but Expert mode only, not suggested, every log copied first. Copy says plainly that the
  copies take the same space, so nothing is freed until you delete them.
- Not removed outright. If you would rather drop it, it is one catalog entry.

### 6. Customized Windows images (agreed, built a different way)
- Research on what Ghost Spectre, ReviOS and Tiny11 remove was thin. Sources for Ghost Spectre were
  low quality mirror pages. One said Defender and SmartScreen are removed and Windows Update is kept.
  Tiny11's regular mode keeps servicing and Windows Update, its core mode and Nano11 remove more.
  Edge and Store defaults for Ghost Spectre were not confirmed anywhere. Per image rules would be guessing.
- So Clarion adapts to what it can observe on the PC: a missing or disabled service, no Store, no Edge.
  `InstallVerdict` now keeps those inputs and the image name. `ImageAdvice` turns them into notes:
  Edge settings when Edge is gone, Defender sharing settings when the service is gone, Updates settings
  when the update service is missing or off, and app removals when the Store is gone (revert cannot
  reinstall from it). Notes show in the detail pane and Home counts them.
- "Skip what is already gone" was already true: an app removal or service change whose target does not
  exist shows as nothing to change, via the engine's vacuous check.
- Real check on this PC: hardware checks bypassed, no Store, no Edge. 30 settings got notes.
- Not done: warning where an image's own tweaks conflict with a Clarion tweak. That needs a reliable
  list of what each image changes, which the research did not find. Revisit after running Clarion on
  Ghost Spectre and writing down what actually differs.

### Regression caught while checking on the real PC
- My earlier fix for exclusive groups ("not drift if another choice in the group is on now") hid a real
  case: the Ultimate plan was switched to Balanced outside Clarion, and Verify stopped reporting it.
  Found because Verify said 0 changed back where it had said 1.
- Replaced with a rule based on the journal: a choice is replaced inside Clarion only when Clarion applied
  a later choice from the same group that is still on. Three tests, including this exact case.

## Visual polish pass

Method: a small script launches the real app, opens each page and saves a picture of the window
only (`PrintWindow`, so no other window can end up in the picture). Every fix below was checked in a new picture.
The first version of the script used a screen grab and caught part of the browser behind the app.
That picture was deleted unused, and the script was changed.

### Found and fixed
- Nothing showed which setting was selected. Cause: each card painted its own border over the list
  item's selection chrome. Fix: one shared card style (`CardListViewItemStyle`) with hover, pressed and
  selected states. Used by the settings lists, Clean up and Repair. The latter two were border taps
  with no selection at all, and are now real lists.
- The title bar showed a blue square and no version. Now the real icon and `0.1.0-beta.3`. Home has
  the logo and a version badge too.
- Sidebar overflow: 14 entries plus Settings did not fit, so Verify and Safety were not visible at all
  and What broke was cut in half. The framework template ignored the item height override placed in the
  app resources. Real fix was structural: the five Windows pages sit under one expandable entry.
- The window set a size but not a position, so Windows could start it low enough to push the bottom bar off
  screen. It is now centred in the work area.
- Closed notices (account, changed back, image, update) still took layout space, leaving a gap at
  the top of Home and Verify. They are collapsed when closed.
- The "N settings" label was clipped to "settings" because its column was 65 pixels wide.
- Repair job duration chips were clipped. Duration is now its own line.
- Verify showed raw registry paths. Plain wording by default, exact steps in Expert mode.
- What broke was an empty page until a symptom was picked. It now explains itself.
- Debloat's icon was a minus sign. Now a package icon.

### Real finding from the pictures, not a bug
- Verify listed three permission settings (app diagnostics, phone calls, eye tracker) as changed back
  that it had not listed an hour earlier. Registry check: the values are `Allow` again. Windows rewrites
  these when apps use the capability. This is the drift scan doing its job on real data.

## Audit: performance, leaks, bad code (ongoing)

Standing brief from the owner: keep looking for performance problems, leaks, overlooked bad code,
missing research, visual polish, enhancements, organisation and bugs. This section is the running list.

### Fixed so far
- Leak: `HomePage` and `CleanupPage` subscribed lambdas to singletons (`AppServices`, `CleanupService`,
  and one per cleanup row) in their constructors. Pages are made again on every visit, so each visit left a
  dead page attached and every status change ran a refresh on all of them. Now named handlers removed
  on `Unloaded`, as `TweakListPage` and `DnsChooser` already did. The frame back stack is also cleared, as
  there is no back button.
- Journal reads: the Safety page read the whole file once per tracked setting. Now one read.
- Sort bug: Safety sorted rows by the displayed date text. Now by the real time.
- Light theme: chips looked up their brushes in code and could lock in the dark palette. They now use
  visual states with `ThemeResource`, which follow the theme.
- Correctness: the DNS adapter lookup recorded effective servers, which for an adapter on DHCP are the
  router's. Revert wrote them back as static. Now reads only the servers set by hand from the interface
  registry key (`NameServer`). Checked on this PC. Its Wi-Fi has a static override, so the DHCP only
  case is covered by reasoning and the registry layout, not by a live test.
- Startup: the features read (DISM, 17 seconds alone) no longer holds up the Ready status. Measured:
  no gain in time to first Ready (about 17 s either way), because the app and DNS lookups dominate.
  Each PowerShell start costs about 3 s on this PC.

### Measured, not yet fixed
- Startup is about 17 s to Ready on this PC. Each PowerShell lookup is 3 to 9 s here (apps 5 s, DNS 9 s,
  encrypted DNS 5 s, features 17 s, one capability 10 s). A real fix is to read these without
  PowerShell (registry or .NET calls) or keep a persistent session. Bigger change, own task.
- `ThemeBrushes.Get` creates an `AccessibilitySettings` object on every call.
- `WindowsTaskStore` makes a new Task Scheduler COM connection per call and never releases it.
- Capability state is queried with one PowerShell process per capability.

### Also fixed in the audit
- Starting a second copy used to exit silently. It now brings the open window to the front.
- Opening System Restore, the log and the data folder used `Process.Start` with no error handling, and the
  process handles were never disposed. One helper (`Shell.Open`) now handles both.
- Sweep for risky patterns found no empty catch blocks and no blocked tasks. The 16 `async void` methods
  are all event handlers, and the app has a global handler that logs to `crash.log`.

### Still open from the audit
- Startup reads (see above), capability lookups one process each, Task Scheduler COM connections.
- `crash.log` has no size limit. The global handler only logs, so the user sees nothing when it fires.
- Windows features rows show as checking for the first 15 to 30 seconds. A cache of the last result
  would show something sensible at once.

### Release 0.1.0-beta.4: the first run of the release workflow failed
- The tag run failed at `dotnet test`, before any packaging. Cause: `Five_thousand_cache_files_scan_and_clean_quickly`
  built its folder in a scratch place and renamed it into the allowed area. On the GitHub runner the rename
  was refused (access denied), most likely because a scanner was still holding a folder with 5,000 new files.
  The same commit passed in the normal CI run, so it is a flaky test, not a product bug.
- Fix: create the files in the final place. No other test renames folders.
- The tag `v0.1.0-beta.4` was already pushed, so it was not moved. The release files were built on this PC.
  The workflow itself is untested beyond that failing test step and needs one clean run on the next tag.

## Third review pass (lifecycle)

Each item was checked against the code and researched before building.

### 1. "That was me" (premise only partly true)
- "Leave it" already ended tracking for good, because it writes a journal entry. A test covers it, so it did not nag.
  What was missing: the label did not say so, and there was no way back.
- Renamed to "That was me" with a plain tooltip. Added a "Stopped checking" list on Verify with "Watch again"
  (`TweakEngine.Resume`), which records the earlier steps as current again without changing Windows.

### 2. What broke empty state
- Names the usual other causes and, when the install check found something, says so, with the image name when known.
  The wording follows the check's strength: "looks like" for likely, "may be" for possible.
- Links to Verify and System.

### 3. Deprovision without a Store
- Research: restoring a missing Store depends on whether its package files are still on disk. Sources disagree on a method,
  and one gave a package id that was not verified. So the warning does not recommend a method. It says Clarion cannot restore it.
- Only whether the Store is installed is checked. Whether it works is not.

### 4. Command line parity
- `--verify`, `--what-broke`, `--since`, `--list-symptoms`, `--history`. Read only, no administrator rights needed.
  Text comes from `CliReports` in Core, so it is unit tested. Run against this PC: the same four changed-back settings the window shows.

### 5. "It worked until roughly"
- Changes older than the date are left out, since something that worked afterwards cannot have been broken by them.
  The empty state says how many older changes were hidden.
- Found by a test: the date in the report text was converted to local time and could print the day before.

### 6. Which account
- Confirmed gap: a second account would read the first one's HKCU settings as changed back, and a revert would touch the wrong
  account. The journal now stamps per account steps (current user registry values, per account app removals) with the SID and shows
  each account its own entries plus machine wide ones. Entries with no owner stay visible to everyone, so existing journals work.
- The deprovision variant is machine scope but removes the app for the current account only, so ownership is decided per step, not per tweak.

### 7. Monthly source check
- The script scanned 27 of 37 links and failed on any refusal. It now scans all of them, retries with GET when HEAD is refused,
  fails only for 404 and 410, and lists the rest as unsure. Tested for a real 404, an unknown host and a good page on Windows
  PowerShell 5.1. The workflow uses 5.1 explicitly, because PowerShell 7 was not available to test.
- One link is unsure on every run (403 from a forum that blocks automated requests).

### 8. Pending update notice
- Research: nothing reliable shows a staged feature update without asking Microsoft's servers, which Clarion does not do.
  The restart markers (`RebootRequired`, servicing `RebootPending`, `UpdateExeVolatile`) show that an update is waiting for a
  restart, not which kind. So the notice says exactly that.
- The list of files waiting for restart is left out on purpose. Other programs set it all the time, and Clarion's own cleanup does too.

### Verified rather than built
- Missing .NET runtime: the packaged app is self contained (runtime and Windows App Runtime are in the folder), so there is nothing
  to be missing. The installer already refuses Windows older than 10 version 2004. README now says nothing else needs installing.
- Verify progress: was a spinner only. Now a count.

## Fourth review pass (last mile)

Claims were checked against the code and measured before building.

### Claims that did not hold
- "Startup blocks the window." Measured with a polling loop from process start: window exists in about 0.5 s (1.7 s on the first,
  cold launch) and the sidebar is painted in 0.7 to 1.9 s. The long wait is the 17 s state read, which is already asynchronous.
  Restructuring startup would add risk for no measurable gain, so it was not done.
- "Restart as administrator." The manifest says `requireAdministrator`, so Windows asks for elevation at launch and the app cannot normally
  run unelevated. The "not running as administrator" line only shows in unusual launches such as a debugger. No button added.
- "MainWindow sets no window size." `ResizeToFit` already sets 1360 x 900 and, since the polish pass, centres it. The part that was
  missing was a minimum size. Added.
- Opt-in telemetry: not built, for the reason in the second review. Clarion promises none, and the optional results section in
  Report a problem already moves the data without the app sending anything.

### A real bug found while checking: settings were never saved
- Theme and Expert mode lived only in memory and reset on every launch. Added `UserSettings` and `UserSettingsStore`
  (`settings.json` in the data folder, written through a temporary file, a missing or damaged file means defaults).

### Update check
- Research: GitHub allows 60 anonymous requests an hour per IP and returns 403 without a User-Agent. Both confirmed live (403 without,
  200 with). `/releases/latest` returns 404 for this repository because every release is a pre-release, so the first idea would never
  have worked. The list endpoint is used, with semantic version ordering (beta.10 above beta.9, a final release above its betas).
- It contacts github.com, so it sits against "sends nothing anywhere". Decision: manual button, automatic check off by default, wording says
  exactly what is contacted, and the README and Settings text were reworded to stay true. The link from the network is only opened when it
  points at this project's releases. Nothing is downloaded or installed.
- Live check against the real list: offers beta 4 to beta 3, nothing to beta 4.

### First run
- A welcome with the five presets, Standard pre selected, then the existing review dialog. Shown once. Not shown when settings are
  already applied. Tested with a clean data folder (`CLARION_DATA_DIR`, added for this) so the real history was never touched.

### Monthly check
- Research: a monthly day of week trigger is `ScheduleByMonthDayOfWeek` with `Week 2` and `Wednesday`. `StartWhenAvailable` runs a missed
  check at the next start. Microsoft's own example has typos, so the structure was copied and not the text.
- Proven on this PC: Task Scheduler accepted the XML and reported Next Run Time 10/14/2026 9:00 AM (the second Wednesday). Then the whole chain
  through the real window: switch on, task created, Windows ran it, `scheduled-verify.json` saved with the four changed back settings,
  Home showed the notice, it went away after Verify was opened, switch off removed the task.
- Found along the way: the task showed "Last Result 1" because `--verify` exits 1 when it finds changes. Task Scheduler shows that as a
  failure. With `--save` the exit code is now 0.
- The task runs as the person who turned it on (interactive, highest available), so their own settings are checked. If Clarion is moved
  to another folder the task still points at the old path. Turning it off and on fixes it.

### Window
- `PreferredMinimumWidth` exists. Measured: it is in real pixels. Asking for 980 gave 980 wide at 125% scaling, so the limit follows the
  screen's scale (860 x 580 scaled). Shrinking to 400 x 300 now stops at 1075 x 725 here.

### Still open
- Opt-in telemetry stays declined.
- Scheduled check has no notification of its own. It is shown when Clarion is next opened.
- Source link check and release workflow still need their first clean run on GitHub.

## What's new page (visual changelog)

Idea taken from the way phone apps show an update: a clear "an update is available" card, then notes grouped by kind.

### What was built
- Three places: a notice on Home that links into the page, a What's new page in the menu (pinned above Settings, with a dot while there is
  something to read), and a one time "Clarion was updated to X" notice on Home after an upgrade.
- The page shows the installed release and the four before it, with "Show N older releases". When a newer version exists it shows a card
  (from and to versions, count pills) and then every release newer than the installed one, newest open.
- No Download button. Clarion does not download or install anything, so the card has "Open the release page" and "Hide this version".
- The notes come from `CHANGELOG.md`, embedded into the app at build time, so the page and the file cannot disagree. Newer releases use
  the body of the GitHub release, which `release.yml` already fills from the same file. A test fails if the building version has no entry
  or an entry has no changes, or a heading is not one of New, Safer, Fixed, Polish.
- Chip colours by kind: New is blue, Safer green, Fixed amber, Polish cyan.

### Bug found while checking it
- The "updated" notice never fired. `AppServices.Instance` was declared above `Version`, so static initializers ran the constructor while
  `Version` was still null, and the stored last seen version was overwritten with nothing on every start. Found because the settings file
  came back with the value blanked. `Version` is now declared first, with a comment saying why.

### How it was tried
- `CLARION_RELEASES_JSON` points the update check at a local file instead of GitHub, so the update screens can be seen without publishing a
  release. A data folder with an older `LastSeenVersion` gives the "updated" notice. Checked in dark and light themes.

## Research and bug pass after beta 5

### Measured, nothing to fix
- Memory over 16 rounds through every page: private memory 130 MB to 187 MB, with growth falling from about 10 MB a round to about 1 MB a round and handles dropping back when the garbage collector ran. That is warm up and lazy collection, not a leak. Pages that subscribe to shared objects already unsubscribe on unload.
- Screen reader names: a UI Automation sweep of every page found two unnamed controls (the symptom list and the release cards). Both fixed, sweep now clean.
- The catalog already has every Windows AI policy that applies to Pro on a shipped build. Click to Do, the Settings agent and Copilot app removal are Insider or Enterprise only in Microsoft's policy reference, so they were left out.
- Setup file save and load already exist (Settings, Your setup), so that gap from the comparison with O&O ShutUp10++ and WinUtil was not real.

### Found and fixed
- A tool that cannot start (Win32Exception) escaped the repair runner. Proved with a failing test first. In the app that left the repair dialog open with no way to close it and the page stuck "running". The runner now reports it as a failed job, the "always" steps still run, and the page ends the dialog whatever happens.
- Same hole in the restore point service (PowerShell cannot start). Now a reported failure, and the frequency value is still put back.
- Window minimum size is clamped to the work area. 580 scaled pixels at 200% on a 1080p screen is taller than the screen.
- Cleanup scan and clean left their cancellation sources undisposed.

### Added from research
- Notepad AI policy. Microsoft's Notepad management page gives `DisableAIFeatures` = 1 under `HKLM\SOFTWARE\Policies\WindowsNotepad`, for Windows 11 22H2 and later and Notepad 11.2503.16.0 or later. Notepad is not installed on this PC, so only the registry round trip was checked here (write, read as DWORD, delete), not Notepad's behaviour.
- `--list-settings` and `--list-cleanups`, because setup files and `--only` need names and nothing listed them.

### Looked at and left
- Analyzer run with all rules: most output is style. The real ones were the items above. `HttpClient` use is disposed. Dates written for machines (Task Scheduler XML) already use the invariant culture.
- A startup apps page: Windows Settings and Task Manager already do it, and the registry format is only documented by the community, so the value is low.

### Journal shared between processes
- The history file is opened by the window and, separately, by the monthly check (`--verify --save`) and any command line run. `ReadAll` used `File.ReadLines`, which opens with `FileShare.Read`, and `Append` holds the file for writing, so each could fail the other with a sharing violation. Two tests reproduced both failures before the fix. Reads now use `FileShare.ReadWrite | Delete`, and `Append` retries for up to two seconds, because by then the change itself has been made and the record of it must not be lost.
- The other small files (drift state, restart queue, scheduled result, settings) already catch IO errors on read. Settings and the scheduled result are written atomically. The restart queue and drift state are not, and a damaged copy just reads as empty, so they were left alone.

### Restore point for revert only runs
- The review window always offers "Create a restore point first", ticked. The restore point was made inside the apply step only, so a queue that held only reverts skipped it silently. `BatchRunner` now shares one restore point gate between the apply and revert parts of a run, so a run gets exactly one, made before the first change of either kind, and a failure blocks both. Tests cover revert only, revert only with a failing restore point, and apply plus revert making a single one.

### Screen reader names for list rows
- The first sweep only looked at buttons, boxes and pickers. Reading the names of the list rows themselves showed every row on Privacy, Debloat, Tweaks, Power, Network, Windows features, Updates, Clean up and Repair named `Clarion.App.Services.TweakItem` (or `CleanupItem`, `ActionItem`): a WinUI list row takes its name from the item's text, and the items had none. The three row types now say what they are ("Hide search highlights, On"). Checked again through UI Automation in the running app.
