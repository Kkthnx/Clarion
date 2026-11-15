# Project context

Short, current, for anyone resuming work. The code and tests are the source of truth. Update this at the end of each milestone.

## What it is

.Clarion is a Windows 11 debloat and tuning app. Every setting is explained, graded for evidence, undoable and checked later for drift. It is for people who want a faster, quieter PC without breaking it, including gaming laptops.

Design thesis: a careful workbench for a PC you care about, where nothing changes until you have read exactly what it will do.

## Stack and commands

- WinUI 3 unpackaged on .NET 9. Engine in `Clarion.Core` and `Clarion.Engine`, UI in `Clarion.App`, tests in `tests/Clarion.Tests` (xUnit).
- Tests: `dotnet test`. Build: `dotnet build src/Clarion.App -p:Platform=x64`. The app needs administrator rights, and `CLARION_NO_ELEVATE=1` skips the prompt for testing.
- Developer switches: `CLARION_LEAK_PROBE=1` (pages and listeners in the log), `CLARION_FORCE_LAPTOP=1` (show laptop behaviour on a desktop), `CLARION_FAKE_BATTERY=18` (show the low battery question), `CLARION_REAL_TESTS=1` (tests that read the real system).
- Crash trail: `C:\ProgramData\Clarion\crash.log` for errors, `clarion.log` for activity, `%LOCALAPPDATA%\Clarion\session.txt` while a session runs (removed on a normal close).

## Constraints

- Never name other tools or authors in the repo. Catalog entries need advice, facts, topic and evidence grade, and policy entries are checked against the Windows ADMX files in a test.
- Changes are recorded and undoable. Anything that cannot be recorded is not made.
- A desktop is never affected by laptop rules. A PC that cannot be classified is a desktop.

## Open issues and next steps

- Issue 7, the window closing while changing between Home, Apps and Clean up on an ASUS TUF A15, running a Ghost Spectre image. Not reproduced here at 120 ms, 30 ms or 10 ms between clicks, so the cause is unknown. The next report will carry the page it was on and any error from another thread. Ask for crash.log and the Windows Event Viewer entry for Clarion.exe under Application.
- The state read on that laptop took 17 to 21 s against 1.6 s on the development desktop. Which reader is slow there is not known. A warm up of 500 ms or more is now logged by handler name ("Slow read: ..."), so the next log from that laptop names it. On this desktop the read took 1.5 s when run from the dev build on 2026-11-15.
- Not yet checked on a real laptop: the Home laptop notice and the battery question were only seen with the two developer switches above.
- Visual refresh of Home and rows is in docs/DESIGN-REFRESH.md. Other pages keep their look. Light theme, high contrast and 150 percent scaling were not re-checked.
- The two research notes that named other tools were moved out of the repo on 2026-11-07 to `Documents\Clarion private notes`, and the names were taken out of CHANGELOG.md and one code comment. Older commits, tags and the published release texts still hold them. Rewriting that history was not done.
- Checked on 2026-11-07 on Windows 11 Pro build 26200.9818 (the development desktop): the eight settings added in 1.5.0 and after read their state from the real registry through `--apply setup.json --preview`, and five of them were already on. None was applied and reverted by hand. The Home laptop notice, the battery question and the new rows were never seen on a real laptop.
- Not found: a cause for issue 7. Four launches with 18 page switches each between Apps, Home and Clean up did not close the window or write to crash.log.
- Ideas, each needs a new action type and real testing: per game GPU choice for hybrid laptops (a per user registry value per exe), Windows power mode, a job queue behind a running job, BitLocker awareness.
- Ideas: Windows power mode overlay, per game graphics preference, BitLocker awareness, indexing setting, a drift reapply service, code signing.
- Checked on 2026-11-15 on build 26200: 26 settings added after 1.5.1 (Windows AI policies, Share and sign-in nags, three Edge policies, Game Bar controller, printer, two Explorer options, two virtualization features, Gallery and Home, snap, restart apps, text and image generation per account, setup reminders and sign-in after update). Policy values are checked by a test against the policy files that ship with Windows, Edge values against Microsoft's Edge policy pages. The three Windows AI policies were already set on the development desktop, so reading them back from the real registry matched. Not applied and reverted by hand. Windows Hypervisor Platform and Virtual Machine Platform feature names come from Microsoft's docs and were not read through DISM here.
- Not added, on purpose: RemoveMicrosoftCopilotApp (only acts when three conditions hold, reports conflict on editions), Experimental agentic features switch (registry path found only on third party pages), DisableWidgetsOnLockScreen (its policy file maps Enabled to 0, unclear), EdgeCopilotEnabled (Microsoft lists Windows as not supported).
- The Home page stays a 1100 wide column on the left, so a maximized window on a wide monitor leaves the right half empty. Other pages fill the width. Not changed.
- Checked on 2026-11-15 by toggling the Windows switch and reading the registry, then putting it back: RestartApps (Winlogon) and both snap values (EnableSnapAssistFlyout, SnapAssist). Gallery and Home were already set on the development desktop. RestartApps now reads an explicit 0 where it was absent, which Windows treats the same. The per account text and image generation value could not be toggled, since the all accounts policy locks that page on this PC.
- Open after 1.6.0: a read only BitLocker status notice on Home, a Memory integrity status line (not a toggle, since exploit mitigations are listed as not included), a short "tweaks we skip and why" list in docs/FEATURES.md, Home layout on wide windows, and a check of high contrast, 150 percent scaling and the first-run welcome.
- 2026-11-15 after 1.6.0: added Chrome (6) and Firefox (2) policy settings, five more Windows policies, game recording policy, and five typing settings, 270 settings in all. Chrome values come from the Chromium policy definitions and Firefox values from Mozilla's policy page. The physical keyboard suggestions value (EnableHwkbTextPrediction) is graded Unproven because it was not read back on a current build. The ms-gamebar and ms-gamingoverlay protocol redirect was left out on purpose, see docs/FEATURES.md. Memory integrity is now a read only line on the System page. Still open, BitLocker status, Home on wide windows, a check of high contrast and 150 percent scaling.
