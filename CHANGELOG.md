# Changelog

## Unreleased

- Clean up now shows the files waiting for the next restart, and lets you cancel them. Cancel takes only Clarion's own files off the Windows restart list. A preview now lists the files in use that a real clean would queue. The record of queued files used to be overwritten by each clean, so an earlier run was forgotten. It now adds up.
- Clearing Event Viewer logs is now Expert mode only and marked not suggested. Every log is saved as an .evtx copy first in the Clarion data folder, and a log whose copy cannot be saved is not cleared.
- Setup files from an older or newer Clarion are refused with a message that says which, instead of a generic format error.
- Reverting a DNS change no longer fails when a network adapter has gone away, is switched off, or when its number now belongs to a different adapter. Gone adapters are skipped and the rest are restored.
- When a restore point cannot be made, Clarion now offers to turn on System Protection for the system drive and try again, in the apply dialog, on the Repair page and with `--enable-protection` on the command line. Nothing is changed unless you choose it.
- The progress panel and the summary now say how many settings were made, how many did not finish and how many were left as they were. A failed setting never stops the ones after it.
- Reworded a privacy fact that said Clarion never touches the Windows Update services. The privacy settings do not turn off Windows Update, and the Repair page has a separate Reset Windows Update job.
- Report a problem can now include how each of your settings is holding up, taken from the last Verify scan. It is off by default, shown in full in the preview, and nothing is sent by Clarion. You open it on GitHub yourself, as before.
- Release workflow that builds the setup program and zip on a version tag, with code signing steps that switch on once SignPath is set up. `build-installer.ps1` can now package an existing folder. Signing plan and blockers are in docs/SIGNING.md.
- New What broke page. Pick what stopped working, such as search, notifications, the camera or websites, and Clarion lists the changes it made that could explain it, named ones first and newest first, each with a revert button. A test keeps the symptom list in step with the catalog.
- Expert option for removed apps: also stop Windows installing the app for new accounts. Needs administrator rights, and Revert puts the app back for you only. Verify also notices when such an app is provisioned again.
- New Verify page. It checks everything Clarion applied against Windows as it is now, lists settings that were changed back and removed apps that came back, and puts them back through the usual review step. "Leave it" accepts a change and stops checking that setting. A badge on the menu and a banner on Home show when something needs a look.
- Every setting now records the Windows build it was last checked on, shown in its tooltip. Tests fail when an entry goes stale.
- Service changes are limited to an explicit allowlist, and the protected service list now also covers DPS, DusmSvc and other network and diagnostic services.
- New setting: stop background gameplay recording. Game Bar and manual recording keep working. Added to the gaming preset.

## 0.1.0-beta.3

- Fixed the build checks on GitHub. Tests that need a Windows client now skip on server images, and the DNS reader no longer fails on machines without network adapters.
- New README with logo, badges and screenshots.

## 0.1.0-beta.2

- Installer: a setup program with Start menu entry, optional desktop shortcut and a clean uninstall.
- App icon.
- System page: Windows edition and build, hardware, security state and activation, with a copy report button.
- Windows install check: lists the signs that Windows came from a customized image, with the reason for each. It is a hint, not a verdict.
- DNS is now one dropdown with an encryption switch, since only one provider can be active.
- Faster startup state read, about 5 seconds instead of 20 after the DNS entries were added.
- Scrollbar colors follow the theme and the lists leave room for it.
- Report a problem page: builds a bug report or suggestion with system details and recent activity, with names, addresses and accounts removed. You read it, then open it on GitHub yourself.
- Live activity panel while changes apply: progress bar, one line per setting, problems shown inline, and an Open log button.
- The summary dialog now only appears when something failed or a restart or sign out is needed.
- New tests cover the system report, install check, live progress steps and the report scrubbing.
## 0.1.0-beta.1

First public beta.

- Settings catalog of about 175 entries with tooltips, evidence grades and sources.
- Privacy policy entries verified against the Windows policy definition files.
- Clean up page with scan first, exact folders, running process skip and restart queue.
- Repair page with live output and Stop.
- DNS provider settings with optional encrypted lookups.
- Presets, setup file save and load, and a command line.
- Restore points, change journal, revert and history.
- Light, dark and high contrast themes.

Known limits: unsigned build, x64 only, not yet tested on a clean Windows 10 or Home install.
