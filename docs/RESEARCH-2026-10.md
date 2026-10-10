# Research round, October 2026

What was looked at, what was found, and what is worth doing, in order. Everything here was measured on a real build or read from a primary source. Nothing in the backlog is built yet. Where a source is a vendor's blog or a forum, it says so.

Tested build: 0.1.0-beta.9 (`cc39d86`) on Windows 11 Pro 25H2, build 26200.9457.

## Status

Done after this was written: finding 1 (app data warning, Sticky Notes rated Medium), finding 2 (the command line is now tested through the real program), finding 3 (Windows tools started by full path), finding 4 (two restore points in a row confirmed on a real PC), the installer (installed, checked and uninstalled for real), high contrast (the theme file is now checked by tests), and Atlas and ReviOS naming (read from their playbooks, see docs/CUSTOM-IMAGES.md). Still open: the update pause undo, seeing the taskbar bar, the licence page of the installer, and high contrast on screen.

## What was checked and found fine

- **The released zip works.** Extracted `Clarion-0.1.0-beta.9-win-x64.zip` into a clean folder: `--version` and `--list-presets` ran correctly. 428 files, 215 MB unpacked, 90 MB zipped. (The installer was not run here; it is built on GitHub only.)
- **Memory does not leak without limit.** 30 passes through all 13 pages (390 page visits): working set 245 MB after 5 passes, 267 MB after 30; private memory 157 to 176 MB; handles flat near 1345. Growth slowed to well under 1 MB per pass. Idle after start is 163 MB.
- **Startup.** Window at 0.5 to 0.6 s, Home content at 0.8 to 0.9 s (three runs). The 4 to 5 s that follows is the background read of every setting and does not block the window.
- **Source links.** `scripts/check-sources.ps1`: 38 links, 37 fine, 1 unsure (a 403 from a forum's bot filter), 0 dead.
- **Windows AI policies.** Compared Microsoft's WindowsAI policy page (updated 2026-09) with the catalog. Covered: Recall (`AllowRecallEnablement`, `DisableAIDataAnalysis`), Paint (`DisableCocreator`, `DisableGenerativeFill`, `DisableImageCreator`), `TurnOffWindowsCopilot` (which Microsoft marks deprecated and not for the new Copilot app), Notepad. Not covered: `DisableClickToDo` (listed as Insider Preview only), `DisableSettingsAgent` and the agent connector policies (Insider only, Enterprise and Education only), `RemoveMicrosoftCopilotApp` (the page contradicts itself on Pro). None of the gaps is a generally available setting, so none is shippable under the catalog's own rule.
- **The size cannot be cut cheaply.** The 215 MB is mostly Windows App SDK parts Clarion does not use (`onnxruntime.dll` 20.7 MB, `DirectML.dll` 17.8 MB, `Microsoft.Windows.SDK.NET.dll` 23.7 MB, 76 language folders). Tested: referencing only the component packages instead of the `Microsoft.WindowsAppSDK` metapackage gave the same size (214 MB) with the same files, because they come from the runtime package. Framework dependent deployment would be small but needs the runtime installed, which breaks "nothing else needs installing". Left as is.
- **Protected packages.** The protected list includes the sign-in pieces (`AAD.BrokerPlugin`, `CloudExperienceHost`, `AccountsControl`, Store, App Installer, Defender UI, shell hosts). Clarion has no setting that removes the Xbox identity provider. Win11Debloat's most discussed account breakage (their issue 537) was closed there as not reproducible.

## Findings worth acting on

### 1. Removing an app deletes its saved data, and Clarion does not say so. (Safety, do first)
Microsoft's app data documentation: "The system removes the contents of these data stores completely and cleanly when your app is uninstalled." So Revert reinstalls an app but cannot bring back what was in it. A Win11Debloat user lost Sticky Notes data this way (their issue 576). Sticky Notes keeps its local copy in one file, `plum.sqlite` in the app's `LocalState` folder (community and Microsoft Q&A sources, not Microsoft documentation), and community reports disagree on whether signed-in notes always sync.
In Clarion today:
- `debloat.app.microsoftstickynotes` says "Notes sync with your Microsoft account" as a fact. That is only true when the person is signed in and syncing, so it can mislead.
- Its risk text says Low and "The app can be put back with Revert", with no mention of the data.
- No app removal setting says that its data goes too.
Suggested: (a) one plain line on every app removal, in the details pane and the review window: "Removing an app also removes the data it saved for your account. Revert reinstalls the app, not that data."; (b) correct the Sticky Notes wording and raise it above Low; (c) consider copying the small data folder of apps that hold the person's own content (Sticky Notes first) to Clarion's backup folder before removing, and say where it is. Restoring it on Revert should not be promised until tested on a real reinstall, because the only source for that is community posts.

### 2. The command line runner has no tests. (Reliability)
Coverage of Core is 91.2% but Engine is 49.4%. `HeadlessRunner` (106 lines, the code behind `--apply`, `--apply-preset`, `--clean`, `--verify`) is at 0%, `WindowsDnsStore` 39%, `WindowsCleanupPlatform` 57%. The runner builds the real runtime inside, so it cannot be unit tested with fakes, but a process level smoke test (start `Clarion.exe --version`, `--list-presets`, `--list-settings`, `--apply-preset standard --preview`, check output and exit code) is cheap and would have caught a broken release.

### 3. Small hardening: start PowerShell by full path. (Low)
`PowerShellHost` runs `powershell.exe` by name. Windows looks in the program's own folder, then the current folder, before System32. For an installed copy that folder is admin only, so the risk is small, but a portable copy can sit in a folder the user can write, and Clarion runs elevated. Use `%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe`. (Also what makes sure it is Windows PowerShell 5.1, which a winutil user got wrong with PowerShell 7, their issue 5142.)

### 4. A "don't surprise me" check for restore points. (Verify before building)
Win11Debloat's maintainer notes that Windows allows one restore point per 24 hours by default, and their script skipped making one. Clarion sets `SystemRestorePointCreationFrequency` to 0 for the moment and puts it back (DESIGN.md section 5). That was reasoned about when written, and unit tested with fakes, but has not been checked on a real machine in this research. Worth one real run: make two batches within an hour and confirm two restore points exist.

### 5. Things users of similar tools hit that Clarion already handles
Per account settings (winutil 4637, Clarion records the account), "already removed" noise (winutil 5142, Clarion shows "Not installed"), policy values not visible in Group Policy (winutil 4532, Clarion writes the documented policy keys and lists the exact values in Expert mode), Start search broken after a debloat (Win11Debloat 361, Clarion's What broke page maps this symptom to the settings behind it).

## Looked at and not recommended

- **Block or wipe the Windows Global Device Identifier (GDID).** A real and recent topic (a VPN vendor's `deGDID` script and the `no-gdid` project). The mitigations rely on registry permission changes, hosts file blocking and clearing sign-in caches, which their own authors say can affect licensing, Microsoft Store, Xbox, Outlook and sync. No Microsoft policy exists for it. The sources are a VPN vendor and third party testers. That fails the catalog rule (vendor documentation, or a read only check on a real machine). If it ever becomes useful, the part that fits Clarion is read only: show whether a GDID is present.
- **A toast from the monthly check.** Unpackaged toast documentation conflicts and an open issue reports toasts that never appear.
- **Installing apps (fresh PC essentials).** Outside what Clarion promises.
- **Shrinking the download.** See above.

## Not yet checked (needs a real test, not a guess)

- **High contrast mode.** The dictionaries exist and a test keeps text colours above 4.5 to 1 in the two normal themes, but no screenshots exist of the high contrast theme. Needs switching a PC to a contrast theme.
- **The installer.** Built and hashed on GitHub, never run in this research. A silent install, launch, uninstall on a spare PC or VM would check the shortcut, the licence page and the uninstaller.
- **The taskbar progress bar.** Interface verified (all calls returned success), the bar not seen.
- **Update pause undo.** Needs a PC where the pause really holds updates. See docs/CUSTOM-IMAGES.md.
- **Atlas and ReviOS naming.** Needs their playbooks read, or a real install.

## Sources

- Microsoft, store and retrieve app data (app data is removed on uninstall): https://learn.microsoft.com/en-us/windows/apps/design/app-settings/store-and-retrieve-app-data
- Microsoft, WindowsAI policy CSP: https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-windowsai
- Win11Debloat issues 576, 537, 361: https://github.com/Raphire/Win11Debloat/issues
- winutil issues 4859, 5142, 4637, 4532: https://github.com/christitustech/winutil/issues
- Sticky Notes data location and recovery (community): https://learn.microsoft.com/en-us/answers/questions/5784300/how-to-restore-the-overwritten-sticky-notes-using
- Windscribe, deGDID (vendor blog): https://windscribe.com/blog/windows-tracks-you-with-a-hidden-id-so-we-built-degdid-to-block-it/
- Windows App SDK 1.8 release notes (metapackage split): https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/release-notes/windows-app-sdk-1-8
