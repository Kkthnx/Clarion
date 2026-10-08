# Clarion Design and Approach

## 1. Goals

1. Every change is explained, reversible, and backed by a verifiable source.
2. A novice can pick a preset and finish in under two minutes.
3. A power user can inspect the exact registry value, service, task or command behind every toggle.
4. The app reads the real system state on launch. It never assumes.

## 2. Principles

- **Evidence over folklore.** Each tweak carries an evidence grade (below). Unproven ideas are hidden by default.
- **Policy over hacks.** Prefer documented policy keys and supported settings to binary patching or file deletion.
- **Never weaken security.** Defender, SmartScreen, Windows Update security patches, and exploit mitigations are never turned off.
- **Reversible by construction.** No tweak ships without a recorded undo.
- **Honest scope.** Each tweak declares the Windows builds and editions where it works. Where a setting is ignored by an edition, the app says so and hides it.

## 3. Tweak metadata

Every entry in the catalog has these fields.

| Field | Meaning |
|---|---|
| `id` | Stable identifier |
| `name`, `summary` | One line each, plain language |
| `what`, `benefit`, `risk` | Short paragraphs shown in the detail pane |
| `evidence` | `Proven`, `Situational`, `Cosmetic`, `Unproven` |
| `risk_level` | `Safe`, `Low`, `Medium`, `High` |
| `scope` | `Machine` or `User` |
| `requires` | Min build, editions, architecture |
| `detect` | How to read current state |
| `apply`, `undo` | Ordered operations |
| `needs_reboot`, `needs_signout` | Flags |
| `sources` | Links to the documentation that backs the change |

Evidence grades:

- **Proven.** Documented behavior with a measurable or visible effect.
- **Situational.** Helps on some hardware or workloads. The app says which.
- **Cosmetic.** Changes look or behavior with no performance claim.
- **Unproven.** Folk tweaks with no reliable measurement. Hidden unless the user enables Expert mode, and labeled as such.

## 4. UI and UX layout

### Shell

A WinUI 3 window with Mica backdrop, a left navigation rail, a content area, and a bottom status strip.

```
+--------+---------------------------------------------+
| Rail   |  Header: page title, search, Preset picker  |
|        +---------------------------------------------+
| Home   |  Tweak list (cards)      | Detail pane      |
| Debloat|  toggle + summary        | What it does     |
| Privacy|  risk badge, state dot   | Benefit, Risk    |
| Tweaks |                          | Exact changes    |
| Network|                          | Revert           |
| Power  |                          |                  |
| Updates+---------------------------------------------+
| Apps   |  Status: 4 pending, restore point ready     |
| Repair |                                             |
| Safety |                                             |
+--------+---------------------------------------------+
```

### Navigation

- **Home.** System summary (build, edition, disk, startup count), health score, and three preset cards: Recommended, Gaming, Privacy Max. A fourth card lets the user build a custom set.
- **Category pages.** Debloat, Privacy, Tweaks, Network, Power, Updates, Apps, Repair.
- **Safety.** Restore points, backups, change history, import and export of profiles.
- **Search.** Global, matches names, descriptions, registry paths and service names.

### Two modes

- **Simple.** Plain wording, grouped by outcome ("Stop sending usage data"), only Safe and Low risk items shown, one Apply button.
- **Expert.** Shows exact operations, Medium and High risk items, Unproven items behind a second switch, a dry run view, and a log pane.

### Pending change model

Toggles do not act immediately. They queue into a Pending tray. The user reviews a list of exact operations, then presses Apply. This avoids accidental changes and lets the app build one restore point per batch.

### Status indicators

| Indicator | Meaning |
|---|---|
| Cyan dot | Currently applied |
| Hollow dot | Not applied |
| Blue ring | Queued, not yet applied |
| Amber dot | Drifted (applied before, now changed by Windows or an update) |
| Lock glyph | Not available on this edition or build |
| Risk badge | Safe, Low, Medium, High with matching text |

Drift detection runs on launch and compares live state to the change journal. Windows feature updates often reset settings, so the app offers a one click Reapply.

### Accessibility

Full keyboard navigation, Narrator labels on every control, 4.5:1 text contrast, visible focus ring, reduced motion respected, scalable text.

## 5. Failsafes and safety

1. **Pre-flight checks.** Confirm administrator rights, supported build, no pending reboot, free disk space, and System Protection state.
2. **System Restore point.** Created before every Apply batch through the Checkpoint-Computer cmdlet. Windows limits automatic creation to one per 24 hours by default, so the app temporarily sets the `SystemRestorePointCreationFrequency` value under `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore` to 0, creates the point, then restores the previous value. If System Protection is off, the app offers to turn it on for the system drive. Apply is blocked if the user declines and has not explicitly accepted the risk.
3. **Change journal.** A local append-only file records every operation with the prior value, the new value, time, and tweak id. Undo reads from it, so a revert restores what was actually there, not a guessed default.
4. **Registry exports.** Before touching a key, the app exports it to a `.reg` file in the backup folder.
5. **Per tweak Revert.** Every card has Revert. Home has Revert All.
6. **Profile export and import.** Selections save as a small JSON file the user can share.
7. **Dry run.** Shows the exact operations without running them.
8. **Protected list.** Packages and services the app refuses to touch: Microsoft Store, Windows Security, shell and Start experience hosts, Desktop App Installer, runtime frameworks, input and language components, and the WebView2 runtime.
9. **Verification.** After each operation the app reads the value back. A mismatch marks the step failed and halts the batch.
10. **Safe mode recovery note.** The Safety page shows how to open System Restore from recovery if the shell fails to load.
11. **Dependency warnings.** Removing an app shows what depends on it.
12. **No silent network use.** The only outbound calls are to the package manager and the update check, both visible in the log.

## 6. Code architecture

### Stack

- C# on current .NET LTS, WinUI 3 with the Windows App SDK, MVVM with the CommunityToolkit.Mvvm package.
- Unpackaged, self contained, with an optional MSIX build. Targets x64 and ARM64.
- Requires administrator rights through the application manifest.

### Layers

```
Clarion.App          WinUI 3 views, view models, navigation
Clarion.Core         Catalog model, planner, journal, state, no UI types
Clarion.Engine       Executes operations with typed handlers
Clarion.Engine.PS    PowerShell host and script runner
Clarion.Catalog      Tweak definitions as data (JSON) plus embedded scripts
Clarion.Tests        Unit and integration tests
```

### Execution model: native first, scripts when needed

The engine routes each operation to the narrowest tool.

| Operation type | Handler | Reason |
|---|---|---|
| Registry value or key | Native C# (`Microsoft.Win32.Registry`) | Typed, fast, easy to journal |
| Service start type and state | Native C# service controller | Typed, reversible |
| Scheduled task enable or disable | Task Scheduler COM API | Typed |
| Firewall, DNS, adapter settings | CIM / WMI through managed code | Typed |
| Appx removal and provisioned packages | PowerShell | Cmdlets are the supported path |
| DISM features and capabilities | PowerShell | Cmdlets are the supported path |
| Restore points | PowerShell | Cmdlet is the supported path |
| File or folder cleanup | Native C# with allow list | Safe path checks |
| Package install | Package manager CLI | Supported interface |

Most tweaks are data only. Scripts exist only for operations with no clean native route.

### PowerShell integration

- Scripts run in a child process, never inside the UI process, so a crash or hang cannot take the app down.
- Use the built in Windows PowerShell for guaranteed availability on every supported build.
- Every script ships embedded and is checked against a SHA-256 hash from the catalog before it runs. Nothing is fetched from the web and executed.
- Scripts emit structured JSON lines (progress, result, error). The host parses them into events for the UI log.
- Each script takes parameters, supports a `-WhatIf` style dry run, and returns the prior state so the journal can undo.
- Timeouts and cancellation tokens on every call.

### Data flow

1. Catalog loads and is filtered by the machine profile (build, edition, architecture).
2. Detector reads live state for every visible tweak in parallel.
3. User toggles produce a plan (ordered operations).
4. Planner resolves conflicts and ordering, and shows the Pending tray.
5. Apply: pre-flight, restore point, registry exports, execute, verify, journal.
6. UI updates from engine events.

### Quality

- Catalog schema validated at build time. A bad entry fails the build.
- Every tweak has a unit test for detect, apply and undo against a mock system layer.
- Integration tests run in disposable virtual machines on Windows 11 Home, Pro, and Windows 10 22H2.
- Static analysis and a script linter run in CI.
- Release binaries are code signed, and a SHA-256 list is published with each release.

### Distribution

Zip portable build, installer, and a winget manifest. Update check compares a signed version file.

### Telemetry in Clarion itself

None. No analytics, no crash upload. Logs stay on disk and the user can export them.

## 7. Research and verification process

- Each catalog entry lists its sources: vendor documentation for policies, plus reproducible measurement notes for performance claims.
- Performance claims require a before and after measurement on at least two machines, stored in `docs/evidence/`.
- A tweak without a documented source or a measurement is graded Unproven.
- Entries are re-tested against each new Windows feature update before the build range in `requires` is extended.
- A public changelog records every catalog change.

## 8. Roadmap

1. **M1.** Engine, journal, restore point flow, 20 reference tweaks, Simple mode.
2. **M2.** Full privacy and tweak catalog, Expert mode, drift detection.
3. **M3.** Debloat with protected list, Edge containment, OneDrive unhook.
4. **M4.** Apps page, Updates controls, Repair tools.
5. **M5.** Profiles, localization, signing, winget release.

## 9. Tooltips and detail cards

Every setting must explain itself so nobody has to research it. The catalog enforces this at build time.

Each tweak carries:

- **Recommendation.** Recommended, Optional, Only if it fits you, or Not suggested for most people.
- **Advice.** One or two plain sentences saying who should turn it on and who should skip it.
- **Facts.** Two to six short, checkable statements, each 140 characters or fewer.
- **Benefit and risk,** plus a risk level, an evidence label, and the sources.

The tooltip is built from these fields and always adds notes for scope, Windows version, editions, restarts and sign out. A plain text version of the same content feeds the detail pane, search, and exports. A tweak with no advice or fewer than two facts fails validation, and a Medium or High risk item can never be marked Recommended.

## 10. Verification gate

- A tweak whose registry value, key or behavior cannot be confirmed from vendor documentation or a read-only check on a real machine is not shipped. It waits in the backlog until it is confirmed.
- `scripts/check-sources.ps1` checks that every source link in the catalog still resolves. Run it before each release.
- Names of features, capabilities and services are read from a real machine before they go into the catalog.
