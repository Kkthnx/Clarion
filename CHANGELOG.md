# Changelog

## Unreleased

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
