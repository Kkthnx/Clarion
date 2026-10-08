# Clarion

See it. Set it. Done.

A native Windows app to remove bloat, cut tracking, and tune the system. Every change is explained, graded by evidence, backed up, and reversible.

## Status

Working app. Home with presets, category pages with a tooltip and detail pane on every setting, a pending changes bar with review and apply, a Safety page with history and revert, and Expert mode. The engine covers registry, services, scheduled tasks, app packages and Windows features, with restore points and a change journal.

## Build and run

Needs the .NET 9 SDK and the Windows 11 SDK. The app asks for administrator rights.

```
dotnet test
dotnet build src/Clarion.App -p:Platform=x64
```

The executable is under `src/Clarion.App/bin/x64/Debug/net9.0-windows10.0.19041.0/Clarion.exe`.

## Docs

- [Design and approach](docs/DESIGN.md)
- [Feature matrix](docs/FEATURES.md)
- [Brand and palette](docs/BRAND.md)

## Principles

- Explain every change in plain language, with benefit and risk.
- Restore point before every batch, and a journal that restores the real prior value.
- Never weaken security.
- No telemetry in the app itself.

## License

All rights reserved for now.
