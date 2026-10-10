# Changelog

## Unreleased

New
- The System page has a "Windows Update right now" card. It reads the update services, the policies that can block updates, the background scan task, any stored pause, and the dates Windows last checked and last installed, then says in plain words whether updates are working, limited or held back, with the reason for each finding. A pause stored far past the 35 days Windows allows is reported as a note when Windows itself says it has ended, because updates still install in that case. Home mentions it when updates are limited or held.

Polish
- Topic pills wrap onto a second row instead of running off the edge, and wait until a list is long enough to need them. On Network they counted ten DNS providers as ten settings, and now count the one row they appear as.
- The details pane shows a centred hint with an icon when nothing is selected, and Verify shows a clear card when there is nothing to check yet, or when everything is in place.
- On Home the Standard preset is the one filled button, marked Recommended. The other four are plain buttons, so the choice is easier to see. The list of what is in a preset lines up in two columns.
- The Windows group in the sidebar closes when you go to another page, so Verify, Safety and the rest stay on screen.
- A setting name that was just wide enough to wrap could leave a blank line under it until the row was measured again. The state text beside the switch now has a fixed width, so the name wraps the same way every time.
- Right margins match on every page. Rows on Clean up no longer keep an empty line when there is no status to show.
- The Customized images are named from evidence. Ghost Spectre is recognized by its Ghost Toolbox folder or shortcut, which the old check missed because the Windows name on those PCs is plain. tiny11 is recognized only from its full pattern (all five hardware check skips plus the network setup skip, or Windows Update off and pointed at the PC itself for tiny11 Core), and that name is worded as a guess. Three of the five skips, as on Ghost Spectre, is not read as tiny11.

## 0.1.0-beta.7

New
- A Compact switch on the settings lists. It shows one line per setting, so about three times as many fit on the screen, and keeps the details in the pane on the right. It is remembered.
- Topic pills with counts (such as "App permissions · 19") replace the topic drop-down on the settings lists, so a long list can be narrowed in one click.
- Home shows this PC at a glance: Windows, processor, memory, graphics, storage, and how many settings are in effect.
- Each preset on Home has a "What is in it" list that names its settings and marks the ones already on.
- Safety lists the newest restore points Windows has, and can make one on demand.
- A "Test the speed of each provider" button on the Network page. It sends three small lookups to each DNS provider, times the answers, and shows which is nearest. It changes nothing, and only runs when you press it.
- `--catalog-doc` prints the full settings reference, and `docs/CATALOG.md` is that page, kept current by a test.
- A Stop button while changes are being made. It stops before the next setting, never in the middle of one, so nothing is left half done. The settings not reached stay queued.
- Safety can go back to a day: pick a date and Clarion queues a revert for every setting last applied after the end of that day. You still review it before anything changes.
- Home tells you when the monthly check starts a different copy of Clarion, for example after Clarion was moved to another folder, and "Fix it" sets it up again from this copy.
- An About page, under What's new, with why Clarion exists, who makes it, links, and ways to support it.
- A LICENSE file. Clarion is all rights reserved with public source: anyone can read the code and check what it does, and run the releases free, but the code, catalog and logo may not be reused or redistributed. The installer shows it.
- The read only command line commands (`--verify`, `--history`, `--what-broke`, and the list commands) now start without administrator rights, as the help text always said. The window asks for the rights when it opens, with the usual Windows prompt. If you say no, it opens in a look and verify mode with a "Restart as administrator" button, and asks for the rights before it makes any change.

Safer
- The record of each step is written before the step is made. If Windows or the PC stopped in between, the history would hold a change that may not have happened, which is harmless to revert, and never a change that nothing remembers.
- When a setting fails part way and going back also fails for some steps, the message now says the setting is partly applied and which steps could not be put back, and the history keeps listing it so Revert can try again. Before, those failures were dropped without a word.

Fixed
- PowerShell errors were shown as "#< CLIXML", the first line of an XML document. They now show the actual error text.
- Without administrator rights, reading the apps and the Windows features failed again and again, one setting at a time, and kept Clarion busy for minutes. A failed read is now remembered until the next refresh, and the read finishes in a few seconds.
- A warm up that failed was ignored without a trace. It is now written to the log.

## 0.1.0-beta.6

New
- A setting to turn off Notepad's AI features, using the policy Microsoft documents for it. Needs Notepad 11.2503.16.0 or later.
- `--list-settings` and `--list-cleanups` print every setting and cleanup row with the name that setup files, `--only` and `--include` use.

Polish
- A redrawn logo and app icon. The mark is drawn from shapes at each size instead of scaled from one picture, so the edges are sharp, and the small sizes (16 to 32 pixels) use a heavier C so it still reads in the taskbar and title bar. The icon file now has nine sizes up to 256 pixels, the logo is 512 pixels, and the README has a new banner.
- A preset whose settings are all already on now says "Already on" on its button. Before, pressing it did nothing and looked broken.
- Text is easier to read. Chip labels in the light theme were between 3.4 and 4.2 to 1 against their background, and muted text in both themes fell below 4.5 to 1 on some surfaces, which is the usual minimum for small text. Status text now uses its own darker (light theme) or lighter (dark theme) colour, and a test keeps every text colour above the line.
- Windows features finish loading in about half the time. Clarion asked Windows about every optional feature on the PC, about 140 of them, when it only needs the dozen it has settings for. On the test PC that took the full read at start up from about 20 seconds to about 10.

Fixed
- Pages were cut off on the right at the smallest window size, and on screens scaled to 150%, where the window opened narrower than the pages need. The filter row, the settings count, the detail pane and the Clean up status text could not be seen. The sidebar now becomes a column of icons on narrower windows, the list and detail columns shrink to fit, the Clean up status line sits under the buttons, and the window opens at the same size in scaled pixels on every screen.
- A repair job whose tool could not be started, for example because it is missing or blocked, left its window open with no way to close it. It now ends as a failed job with the reason, and the steps that put services back still run.
- Making a restore point failed with a crash instead of the usual "No restore point" choices when PowerShell could not be started.
- On a small screen with large text scaling the window could not be made short enough to keep the bottom bar on screen.
- Screen readers read every row in the settings, Clean up and Repair lists as an internal name, "Clarion.App.Services.TweakItem". They now read the setting's name and whether it is on. The symptom list on What broke and the release cards on What's new had no name at all and now have one.
- Each scan or clean on the Clean up page left its cancellation object behind.
- The history file could not be read while another Clarion process was adding to it, and an addition could fail while another process was reading. The monthly check and the command line run as their own processes, so they could hit this. Reads now share the file and a write waits up to two seconds for it.
- The review window offers a restore point for the whole run, but a run that only reverted settings made none. It now makes one, and stops with nothing changed if it cannot, the same as for applying.
- The Clean up page said "3 selected, 0 bytes" before the first scan. It now says the rows have not been scanned yet.

## 0.1.0-beta.5

New
- A welcome the first time Clarion opens on a PC where nothing has been applied: pick a starting point, see the list of what it would change, then apply it. Someone who already has settings applied is not welcomed.
- Optional monthly check. A Windows scheduled task runs the read only check on the second Wednesday of each month, the day after Windows' monthly updates, and runs at the next start if the PC was off. The next time you open Clarion, Home says what had changed back. It changes nothing and sends nothing. Turn it on or off in Settings.
- Update check. "Check for updates now" in Settings asks GitHub whether a newer version exists and links to its page. An automatic check at start up is available but off unless you turn it on. Clarion never downloads or installs anything itself.
- A What's new page. Each release is a card on a timeline that opens to show what changed, grouped as New, Safer, Fixed and Polish with a colour for each. When a newer version exists the page says so at the top, shows the versions you would go from and to, and lists every release you missed. The release notes are read from the same file as this one, so the page and the notes cannot drift apart.
- After Clarion is updated, Home says "Clarion was updated to" the new version once and links to What's new. A dot on the What's new menu item shows while there is something to read.
- Command line commands that only look, and need no administrator rights: `--verify`, `--what-broke <symptom>` with `--since <date>`, `--list-symptoms` and `--history`. `--save` with `--verify` keeps the result for the next time Clarion is opened and exits with 0, since finding changes is not a failure.
- What broke can be limited to changes made after a date ("it worked until roughly"), and when nothing explains the problem it names the usual other causes, including a customized Windows image on this PC.
- Verify shows a progress bar and "Checking 12 of 50" while it works, and notes when Windows is waiting for a restart to finish an update. Home shows the same notice.
- Verify has "That was me" for changes you made on purpose, and a "Stopped checking" list with "Watch again" so that choice can be undone.
- The window has a minimum size, so the layout cannot be squeezed until it breaks.
- A monthly check of the catalog's source links, which fails only for dead pages (404 or 410).

Safer
- The history now records which Windows account a per-account change belongs to. A second account no longer sees the first one's settings as changed back, and a revert cannot touch the wrong account. Older entries without an owner behave as before.
- The Expert option to stop new accounts getting a removed app warns more strongly when the Microsoft Store is not installed, because then there is no way back for new accounts until the Store itself is restored.

Fixed
- The theme and Expert mode were forgotten every time Clarion was closed. They are now remembered.
- The notice that Clarion was updated could not fire, because the version number was not yet known when the saved settings were first read.
- The date in the What broke command line text could show the day before the one you typed, depending on your time zone.
- The source link check scanned only part of the catalog and treated a refused request as a failure.
- A large test no longer fails on slow build machines.

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
