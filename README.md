# Clarion

See it. Set it. Done.

A native Windows app to remove bloat, cut tracking, clean up, repair and tune the system. Every change is explained, graded by evidence, backed up, and reversible.

## Status

Beta (0.1.0-beta.2). Expect rough edges and report them in Issues. The version stays below 1.0 until the beta is ironed out.

## What it does

- About 175 settings across privacy, debloat, tweaks, power, network, Windows features and updates. Each has a tooltip with facts and a clear recommendation.
- Privacy policies are checked against the policy definition files that ship with Windows.
- Clean up: shader caches, launcher caches, browser and Windows leftovers. Scans first, shows exact sizes and folders, skips files in use, and queues locked files for the next restart.
- Repair: system file check, component repair, network reset, update reset and more. Output streams live and every job can be stopped.
- Presets: Minimal, Standard, Advanced, Gaming and Privacy.
- DNS providers with optional encrypted lookups.
- Restore point before every batch, a change journal that restores the real prior value, and one click revert.
- System page with hardware, security state and a check for customized Windows images.
- Save your choices to a setup file and apply them later or on another PC.

## Command line

```
Clarion.exe --version
Clarion.exe --list-presets
Clarion.exe --apply-preset standard [--preview] [--no-restore-point]
Clarion.exe --apply setup.json [--preview] [--no-restore-point]
Clarion.exe --clean [--preview]
```

Exit codes: 0 success, 1 some steps failed, 2 bad arguments or file, 3 not elevated.

## Build and run

Needs the .NET 9 SDK and the Windows 11 SDK. The app asks for administrator rights.

```
dotnet test
dotnet build src/Clarion.App -p:Platform=x64
```

The executable is under `src/Clarion.App/bin/x64/Debug/net9.0-windows10.0.19041.0/Clarion.exe`.

Releases include a setup program and a zip. They are unsigned, so SmartScreen may warn on first run. Check the published SHA256 files. To build the setup yourself, run `scripts/build-installer.ps1`.

## Docs

- [Design and approach](docs/DESIGN.md)
- [Feature matrix](docs/FEATURES.md)
- [Brand and palette](docs/BRAND.md)
- [Changelog](CHANGELOG.md)

## Principles

- Explain every change in plain language, with benefit and risk.
- Restore point before every batch, and a journal that restores the real prior value.
- Never weaken security.
- No telemetry in the app itself.

## License

All rights reserved for now.
