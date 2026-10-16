# .Clarion guides

Short, task by task. Each one says where to click, what happens, and how to undo it. For what every setting does, see [CATALOG.md](CATALOG.md), or the same list on the [documentation site](https://kkthnx.github.io/Clarion/).

- [Install and check the download](#install-and-check-the-download)
- [Change settings](#change-settings)
- [Put a change back](#put-a-change-back)
- [Check that Windows kept your settings](#check-that-windows-kept-your-settings)
- [Use the same setup on another PC](#use-the-same-setup-on-another-pc)
- [Install and remove apps](#install-and-remove-apps)
- [Fix a problem](#fix-a-problem)
- [Ask for a setting](#ask-for-a-setting)

## Install and check the download

1. Download `Clarion-<version>-setup.exe`, or the portable zip, from the [latest release](https://github.com/Kkthnx/Clarion/releases/latest). It needs Windows 10 version 2004 or later, 64-bit.
2. Windows SmartScreen warns on first run, because the program is not code signed yet. Every download has a `.sha256` file next to it. Compare it with the file you have:

   ```
   Get-FileHash .\Clarion-<version>-setup.exe -Algorithm SHA256
   ```

3. For a stronger check, GitHub signs a statement that its runner built these exact files from this commit. With the GitHub CLI installed:

   ```
   gh attestation verify .\Clarion-<version>-setup.exe --repo Kkthnx/Clarion
   ```

   It prints nothing and exits with 0 when the file matches. Windows does not read this statement, so SmartScreen still warns.
4. Open .Clarion. It asks for administrator rights, because most settings are machine wide, and it will not change anything it cannot record in its history.

## Change settings

1. Open a page, or pick a preset on Home. Turn a setting on. Nothing changes yet. It joins the queue.
2. Click the circled **i** beside a setting to read what it does, what you gain and what could go wrong. Turn on **Expert mode** in Settings to see the exact registry values, services and commands, and the source.
3. Click **Review and apply**. The review lists every queued item and, in Expert mode, the exact changes. Click **Leave out** beside an item to take just that one out of the queue.
4. Leave **Create a restore point first** ticked unless the queue is only apps. Click **Apply**.
5. Watch the panel at the bottom. If a setting needs a restart or signing out, the app says so when it finishes.

**Clear queue** empties the queue and changes nothing on the PC.

## Put a change back

1. Open **Safety**.
2. Click **Queue revert** beside one change, or **Queue revert for all**. To go back to how things were on a day, pick it under **Go back to how it was on** and click **Queue revert for what came after**. That queues a revert for every setting last applied after the end of that day.
3. Click **Review and apply**, the same as for any change.

Revert puts back the value Windows had before, which .Clarion recorded just before it made the change. If there was no value, it removes the one that was added. If a revert stops part way, run it again. It finishes the steps that did not run.

For something bigger, **Make a restore point now** on the same page, or open **System Restore** from there.

## Check that Windows kept your settings

Windows updates and other tools sometimes undo a change or bring a removed app back.

1. Open **Verify** and click **Scan now**. It is read only.
2. **Put them all back** applies the settings again. **Remove them all again** removes apps that returned. Both go through the same review.
3. If a change is one you made on purpose with another tool, click **That was me**. .Clarion stops checking that one.

To have the check run for you, turn on **Check my settings every month** in Settings. It runs `Clarion.exe --verify --save` as a scheduled task, makes no change, and shows the result next time you open the app.

## Use the same setup on another PC

In **Settings**:

1. On the PC you set up, click **Save my setup**. The file is a small text file that lists the settings that are on.
2. On the other PC, click **Load a setup**. The settings are queued, not applied. Review them as usual.

Settings that do not exist on that PC or version are reported and skipped. A file from an older or newer file format is refused with the reason, since settings may have changed.

### Without the window

```
Clarion.exe --apply setup.json --preview
Clarion.exe --apply setup.json
Clarion.exe --apply-preset gaming --preview
```

`--preview` shows what would change and changes nothing. Without it, the batch has the same review rules as the window: a restore point first (skip it with `--no-restore-point`, or turn System Protection on with `--enable-protection`), and the journal records every change. Apps in a setup are skipped unless you add `--accept-package-agreements`, which means you have read the publishers' licence terms. Exit codes are 0 for done, 1 for some items not finished, 2 for bad input and 3 when administrator rights are needed. `Clarion.exe --help` lists everything, and `--list-settings` shows the name a setup file uses for each setting.

## Install and remove apps

1. Open **Apps** and turn on the apps you want. Each is one exact winget package ID, never a search.
2. **Review and apply** asks winget for the version, publisher, licence, installer host and checksum of each one. Read them. The tick box that accepts the licence terms stays off until winget has answered, and it covers this run only.
3. Winget downloads the installer from the publisher and checks its checksum before it runs.

Turning an app off runs the program's own uninstaller through winget. If you did not install it with .Clarion, this removes the copy you already had.

## Fix a problem

- **Something stopped working after a change.** Open **What broke**, pick the symptom, and optionally the date it last worked. You get the changes .Clarion made that could explain it, newest first, each with a **Queue revert** button.
- **A setting looks undone.** Use **Verify**, above.
- **Windows itself misbehaves.** Open **Repair**. Each job says what it does and what it risks, shows its output while it runs, and can be stopped. Jobs that matter make a restore point first. The **Windows tools** section opens the classic Control Panel items and consoles.
- **The screen is black or garbled.** **Restart the graphics driver** on the Repair page asks Windows to stop and start each running graphics adapter. The screen goes black for a second or two, and a running game can close, so save your work first.
- **Slow or odd lookups.** On **Windows**, **Network**, **Test the speed of each provider** measures which DNS provider answers fastest from your PC, and **Flush the DNS cache** empties the PC's remembered addresses.
- **You need to send someone a report.** **System** has **Copy report** and **Save report**. Both remove the computer name, user name, network addresses and account IDs first. **Report a problem** builds a fuller report, which you read before you send it.

The log is in **Settings**, **Open log**. The history, settings and log are kept in the Clarion folder under ProgramData. Set the `CLARION_DATA_DIR` environment variable to point .Clarion at another folder, which is a clean way to try it without touching your own history.

## Ask for a setting

Open an issue with the [setting request](https://github.com/Kkthnx/Clarion/issues/new?template=setting_request.md) template. Say what it changes, what you saw, the source, what could go wrong, and how to put it back. [CONTRIBUTING.md](../CONTRIBUTING.md) explains how a setting is added and what has to be true before it goes in.
