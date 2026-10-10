# Customized Windows images: what they leave behind

Notes for how Clarion recognizes a trimmed or rebranded Windows install (Ghost Spectre, tiny11, Atlas, ReviOS and similar), and how it should deal with the way they handle Windows Update. Written 10 October 2026. Facts marked "seen" were read from a real Ghost Spectre PC. Facts marked "script" come from the builder's own source. Everything else is marked as a claim.

## What Clarion checks today

`InstallCheck` weighs signs: the system caption naming a known image, setup hardware checks skipped (`HKLM\SYSTEM\Setup\LabConfig`), missing Update, Defender or other core services, no Store, no Edge, a volume licence on a PC that is not on a domain. It names an image only when the Windows caption contains the name.

That never fires. Seen on a Ghost Spectre PC: the caption is a plain "Windows 11 Pro", and the registry has no owner, organization or OEM text. The result is "may have been customized" with no name.

## Seen on a Ghost Spectre PC (Windows 11 25H2, build 26200.9457)

- Folder `C:\Ghost Toolbox` holding `toolbox.updater.x64.exe` and downloaders (wget, aria2c, 7-Zip), and a desktop shortcut `Ghost Toolbox.lnk` that points at that program. This is the clearest marker and is not in Clarion's checks.
- `HKLM\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings`: `PauseUpdatesExpiryTime`, `PauseFeatureUpdatesEndTime` and `PauseQualityUpdatesEndTime` all `2077-01-01T10:38:56Z`. The start times are dated 2019, long before this Windows build existed, so the values were baked into the image. A stock Windows allows a pause of at most 35 days (Microsoft Learn, "Configure Windows Update client policies").
- `HKLM\SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState`: `QualityUpdatesPaused=1`, `FeatureUpdatesPaused=1`, pause periods of 20977 days.
- Yet `UpdatePolicy\Settings`: `PausedFeatureStatus=2` and `PausedQualityStatus=2`. Microsoft documents 2 as "auto-resumed after being paused".
- A real Windows Update search worked (28 seconds, result code 2, six driver updates offered), the last install was the same day, and the build is current. So the stored pause does not block updates on this PC. The registry values disagree with each other, so no single value tells you whether updates are really held.
- Services: `wuauserv` Manual, `UsoSvc` Auto, `WaaSMedicSvc` Manual. All present. `WinDefend` Auto. Several others disabled (DiagTrack, Spooler, RemoteRegistry, WinRM, SENS, Sense and more, 23 in all).
- Tasks `Schedule Work`, `Schedule Maintenance Work` and `Schedule Wake To Work` under `UpdateOrchestrator` are disabled. `Schedule Scan` is ready.
- `HKLM\SYSTEM\Setup\LabConfig`: `BypassTPMCheck`, `BypassSecureBootCheck`, `BypassCPUCheck` set, plus `MoSetup\AllowUpgradesWithUnsupportedTPMOrCPU=1`. Only three of the five bypass values, which differs from tiny11.
- No Store, no Edge. Registry `ProductName` says "Windows 10 Pro" on this Windows 11 build, but stock Windows 11 does that too, so it is not a sign.
- Claims from the build's download page (not verified): paused "until 2077", Defender, SmartScreen and the Store removable, UAC set to Never Notify, a Compact/Superlite/SE edition family each with or without Defender, uploader and creator named Ghost Spectre.

## tiny11 (from its own scripts, github.com/ntdevlabs/tiny11builder)

Regular `tiny11maker.ps1`:
- Hardware bypass: `LabConfig` `BypassCPUCheck`, `BypassRAMCheck`, `BypassSecureBootCheck`, `BypassStorageCheck`, `BypassTPMCheck` all 1, plus `MoSetup\AllowUpgradesWithUnsupportedTPMOrCPU=1`.
- `OOBE\BypassNRO=1`, an `autounattend.xml` left in `Windows\System32\Sysprep`.
- Edge folders removed (Edge, EdgeUpdate, EdgeCore, WebView folder). Store is kept. Defender is not touched.
- Policies written: `AllowTelemetry=0`, `DisablePushToInstall`, the three CloudContent consumer values, `TurnOffWindowsCopilot`, `DisableFileSyncNGSC`, Teams `DisableInstallation`, Mail `PreventRun`, `PreventDeviceEncryption=1`, `MRT\DontOfferThroughWUAU=1`.
- It writes no name, owner or other marker. Recognizing it is a pattern match, never certain.

Core `tiny11Coremaker.ps1` (the maintainer calls it not for production):
- `wuauserv` Start=4, the `WaaSMedicSVC` and `UsoSvc` service keys deleted.
- Policies `DisableWindowsUpdateAccess=1`, `DoNotConnectToWindowsUpdateInternetLocations=1`, `WUServer`, `WUStatusServer` and `UpdateServiceUrlAlternate` set to `localhost`, `UseWUServer=1`, `NoAutoUpdate=1`, `OOBE\DisableOnline=1`.
- `SettingsPageVisibility` set to `hide:virus;windowsupdate`.
- Defender services (`WinDefend`, `WdNisSvc`, `WdNisDrv`, `WdFilter`, `Sense`) Start=4 and the Defender package removed.
- WinSxS rebuilt from a short list and `ResetBase` run, so the component store cannot repair itself.

## AtlasOS and ReviOS (what is documented)

- Atlas: its install guide has the user pause updates for a week, run Windows Update fully, then resume. The README lists Windows Update as an optional, togglable item. Folders named by its docs: `C:\Windows\AtlasDesktop` (documented) and, per a third party overview, `AtlasModules` (not confirmed). The playbook version shows in System > About and `winver` (documented). A community post says it disables automatic updates and uses a scheduled task, not verified.
- ReviOS: its comparison page lists Windows Update as "Fully compatible, Paused". It removes components at the WinSxS level using an empty update package, which keeps the original files so uninstalling that package restores them. The Revision Tool is its settings app, GPL v3, made only for ReviOS.
- Not found for either: the exact registry values or services their playbooks change. Do not write detection for them from guesses. Read their playbooks first, or check a real install.

## What Clarion does now

Built after this research: items 1 and 2 below. The System page names Ghost Spectre from the Toolbox marker and words tiny11 as a guess. A "Windows Update right now" card reports working, limited or held, from the services, policies, scan task, stored pause, Windows' own pause status and the last check and install dates. Items 3 and 4 are not built. See "Why the undo is not built" at the end.

## What this means for Clarion

1. Name the image from evidence, not the caption. Ghost Spectre has a solid marker (the Toolbox folder and shortcut). tiny11 has only a pattern (all five bypass values with `BypassNRO`; Core adds the localhost update server and removed services). Atlas and ReviOS need a read of their playbooks first. Every sign stays listed with its reason, and the result stays a hint.
2. Report the real state of Windows Update, separate from the image name, because it is what the user acts on. Gather: pause dates and whether they run past the 35 day limit, `PausedFeatureStatus` and `PausedQualityStatus`, the policy keys above (`NoAutoUpdate`, `WUServer`, `DisableWindowsUpdateAccess`), service start types, the orchestrator tasks, and the Settings page hiding. Then say in plain words: held, not held, or unclear. The Ghost Spectre PC shows why: a pause to 2077 is stored but updates still run.
3. Offer to undo it, as ordinary Clarion settings with a recorded previous value so Revert puts the image's choice back: clear the stored pause, remove the policy overrides, set the services and tasks to normal. This matches the rule that Clarion never weakens security; it only strengthens.
4. Do not touch the image builder's own tools (the Toolbox), and expect them to re-apply their choices.

## Why the undo is not built

No source documents what the Settings "Resume updates" button changes. Community guides say to delete the six pause values under `UXSettings` and some also reset `PausedFeatureStatus` and `PausedQualityStatus`, and one forum thread says deleting them unpauses but nothing downloads until Resume is pressed. On the Ghost Spectre PC the stored pause is already ignored by Windows, so clearing it would change nothing observable here. A setting that edits update state should be shipped only after a before and after export of both keys around a real Resume on a PC where the pause is in effect. Until then Clarion reports the state and leaves it alone.

## Sources

- Microsoft Learn, Configure Windows Update client policies: https://learn.microsoft.com/en-us/windows/deployment/update/waas-configure-wufb
- tiny11builder scripts: https://github.com/ntdevlabs/tiny11builder
- Atlas install guide: https://docs.atlasos.net/getting-started/install/install-playbook/
- Atlas folder and version notes: https://docs.atlasos.net/faq-and-troubleshooting/common-questions/atlas-folder-missing/
- ReviOS comparison: https://revi.cc/docs/comparison
- Revision Tool: https://github.com/meetrevision/revision-tool
- Ghost Spectre build page (claims): https://archive.org/details/win-11.-io-t-ent.-25-h-2.-u-3-2.-x-64.-wpe
