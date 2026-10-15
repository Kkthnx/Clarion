# Clarion

[![Release](https://img.shields.io/github/v/release/Kkthnx/Clarion?include_prereleases&label=release&color=5C8BCF)](https://github.com/Kkthnx/Clarion/releases)
![Platform](https://img.shields.io/badge/Windows-10%20%7C%2011-5C8BCF)

A native Windows app to remove bloat, cut tracking, clean up, repair and tune your PC. Every change is explained, backed up and reversible.

## What it does

- 217 settings across privacy, app removal, tweaks, power, network, Windows features and updates. Each one explains what it does, what you gain, and what could go wrong, with an evidence grade.
- Installs 135 apps as exact winget package IDs. The review shows publisher, license, installer host and checksum before anything installs, and revert uninstalls.
- Cleanup scans shader, launcher, browser and Windows caches first, shows exact folders and sizes, skips files in use, and queues locked files for the next restart.
- Repair runs system file checks, component repair, and network and update resets, with live output and a stop button.
- Safety by design: a restore point before every batch, a journal that records the real previous value, one-click revert, and presets (Minimal, Standard, Advanced, Gaming, Privacy). No telemetry in the app, and the update check is off unless you ask for it.

## Get it

Download `Clarion-<version>-setup.exe` (or the portable zip) from the [latest release](https://github.com/Kkthnx/Clarion/releases/latest). Not code signed yet, so SmartScreen shows its warning on first run; every download has a SHA256 file next to it so you can verify it. Needs Windows 10 version 2004 or later, 64-bit.

## Quick start

1. Run the installer and open Clarion. It asks for administrator rights, because most settings are machine wide.
2. Pick a preset, or open the page for what you want to change.
3. Review the queued changes. A restore point is made before the batch applies.
4. Revert anything you do not like, one setting or everything, from the Safety page.

## Support the work

Free forever. If it saved you time, chip in:

- [Ko-fi](https://ko-fi.com/kkthnx)
- [Buy Me a Coffee](https://buymeacoffee.com/kkthnx)
- [PayPal](https://www.paypal.com/paypalme/kkthnxtv)

Every bit goes back into building more.

## Community

- Glitch Haven Discord: https://discord.gg/FaVGhTYmck
- Website: https://kkthnx.com

## License

Copyright (c) 2026 Kkthnx. All rights reserved. The source is public so you can read it and check what Clarion does, but it is not open source and you may not reuse or redistribute it. See [LICENSE](LICENSE).
