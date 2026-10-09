# Changelog

## 0.1.0-beta.4

New
- Verify page. It checks everything Clarion applied against Windows as it is now, lists settings that were changed back and removed apps that came back, and puts them back through the usual review step. "Leave it" accepts a change and stops checking that setting. A badge on the menu and a banner on Home show when something needs a look.
- What broke page. Pick what stopped working, such as search, notifications, the camera or websites, and Clarion lists the changes it made that could explain it, named ones first and newest first, each with a revert button.
- Expert option for removed apps: also stop Windows installing the app for new accounts. Needs administrator rights, and Revert puts the app back for you only. Verify also notices when such an app is provisioned again.
- Clean up shows the files waiting for the next restart and lets you cancel them. Cancel takes only Clarion's own files off the Windows restart list. A preview now lists the files in use that a real clean would queue.
- Clarion adapts to customized Windows images. When the Store, Edge, the Defender service or the Windows Update service is missing or off, the settings that depend on them carry a note in the detail pane, and Home says how many.
- When a restore point cannot be made, Clarion offers to turn on System Protection for the system drive and try again. This works in the apply dialog, on the Repair page and with `--enable-protection` on the command line. Nothing is changed unless you choose it.
- Report a problem can include how each of your settings is holding up, taken from the last Verify scan. It is off by default, shown in full in the preview, and nothing is sent by Clarion.
- New setting: stop background gameplay recording. Game Bar and manual recording keep working. It is in the gaming preset.
- Every setting records the Windows build it was last checked on, shown in its tooltip. Tests fail when an entry goes stale.
- Starting Clarion a second time brings the window that is already open to the front.
- Release workflow that builds the setup program and zip on a version tag, with code signing steps that switch on once SignPath is set up. Signing plan and blockers are in docs/SIGNING.md.

Safer
- Clearing Event Viewer logs is Expert mode only and marked not suggested. Every log is saved as an .evtx copy first, and a log whose copy cannot be saved is not cleared.
- Service changes are limited to an explicit allowlist, and the protected service list now also covers DPS, DusmSvc and other network and diagnostic services.
- Setup files from an older or newer Clarion are refused with a message that says which.
- The progress panel and the summary say how many settings were made, how many did not finish and how many were left as they were. A failed setting never stops the ones after it.
- Reworded a privacy fact that said Clarion never touches the Windows Update services. The privacy settings do not turn off Windows Update, and the Repair page has a separate Reset Windows Update job.

Fixed
- Reverting a DNS change on an adapter that gets its DNS automatically used to write the router's servers back as typed in ones, so the adapter stayed fixed. Only servers set by hand are recorded now.
- Reverting a DNS change no longer fails when an adapter has gone away, is switched off, or when its number now belongs to a different adapter.
- A power plan or DNS choice switched outside Clarion was not reported by Verify, because another choice in the same group was active. Verify now treats a choice as replaced only when Clarion itself applied the later one.
- Home and Clean up kept a handler on app-wide objects for every visit, so they leaked and ran extra refreshes the longer the app stayed open.
- The Safety page sorted changes by the text of the date instead of the date, and read the whole history file once per setting.
- Opening System Restore, the log or the data folder no longer fails with an error if Windows cannot start the program.
- The apply summary no longer counts a setting twice when it was skipped for needing administrator rights.

Polish
- A setting, cleanup row or repair job you click stays highlighted, with a tinted card, accent border and a bar on its left edge, and it reacts to hover and press.
- The real Clarion icon and the version show in the title bar and on Home.
- The sidebar groups Tweaks, Power, Network, Windows features and Updates under one Windows entry, so every page fits on a small screen. Verify and Safety used to be pushed off the bottom.
- The window opens centred in the work area, so the bottom bar cannot end up under the taskbar.
- Status chips follow the light and dark theme. Closed notices no longer leave a gap at the top of Home and Verify. The settings count and repair durations are no longer cut off.
- Verify uses plain wording unless Expert mode is on. What broke explains itself before you pick something.

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
