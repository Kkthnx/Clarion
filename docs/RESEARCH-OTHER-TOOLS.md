# Other Windows tweak tools, compared

What two widely used tools do, what .Clarion does not do yet, and what is worth doing, with the source for each claim. Read on 2026-10-11 from the tools' own repositories and from Microsoft's and Brave's own documentation. Where a source is a forum or a community answer, it says so.

The two tools are winutil (github.com/ChrisTitusTech/winutil) and Sparkle (github.com/thedogecraft/sparkle).

## Licences, and how they are used here

- winutil is MIT. Sparkle is GPL-3.0. .Clarion is all rights reserved with public source, so nothing from Sparkle, not code and not text, may be copied in. winutil's MIT licence would need its notice kept.
- Neither tool's definitions are copied. They are read as leads. Every setting that is added is written from the vendor's own documentation, and checked against it, as the catalog rules already say.

## What was read

| | winutil | Sparkle |
|---|---|---|
| Tweaks | 67 entries in `config/tweaks.json`, 50 of them change the registry (122 values) | 43 folders under `tweaks/` |
| Undo | 58 of 67 have an undo script or recorded original values, but the original values are fixed text such as `<RemoveEntry>`, not what was on the PC | 39 of 43 have an `unapply.ps1` |
| How a tweak is defined | Registry rows, service rows, or a PowerShell script (26 entries have one) | A PowerShell script per tweak, plus `meta.json` |
| Beyond tweaks | Install apps (236 entries, winget or Chocolatey), Windows features (7, and .Clarion has all seven; the other entries in `config/feature.json` are fixes and shortcut buttons), app removal (34), update modes, repair fixes, presets (Minimal 4, Standard 12, Advanced 17), configuration import and export, a command line run mode, a Windows image builder | Install apps (162 entries, winget or Chocolatey), Debloat that lists every installed program and uninstalls the ones picked, Utilities (15 quick fixes), Backup (create and delete restore points), DNS, Clean, a self-updater, per tweak documentation pages, a recommended mark and a risk level per tweak, an NVIDIA profile import |
| How it is run | `irm https://christitus.com/win \| iex`, a remote script fetched and run each time, from its own README | An installed Electron app |

The Sparkle numbers come from the `v2` branch. Its loader (`src/main/tweakHandler.ts`) reads tweaks only from the folder bundled inside the app. No loader for tweaks a user adds on their own PC was found in `src/` or `tweaks/`, so its "create your own tweaks" page is a guide for contributing a tweak to the project, with a tweak request issue template, not a feature for end users. That is read from file names and the loader, not from running it.

## How much of winutil .Clarion already covers

By registry value (path and name), .Clarion sets 62 of the 122 values winutil sets in its 50 registry tweaks, 51 percent, and 21 of those tweaks are covered completely. The count was 40 (33 percent) before the browser settings and 56 (46 percent) before the settings of 2026-10-11 listed below. winutil's current file has only 5 service entries, so the older figure of a long service list no longer applies to it.

.Clarion is broader in other places: 41 app permission settings, 25 app removals, 10 DNS entries, power plans, and Verify, History and What broke, which neither tool has.

## winutil and Sparkle settings that .Clarion does not have

Sorted by what the evidence allows.

### Added with this document

- **Brave.** winutil's "Brave debloat" sets 12 policy values. Nine are Brave's own policies. All nine exist in Brave's source (`brave/brave-core`, `components/policy/resources/templates/policy_definitions/BraveSoftware`), with the same direction (`Disabled` is 1, `Enabled` is 0). Six .Clarion settings now cover them: Rewards, Wallet, VPN, AI Chat, News and Talk together, and the two usage pings. Brave's own text says a set policy cannot be changed by the user. The registry key is `SOFTWARE\Policies\BraveSoftware\Brave`, from Brave's own source (`chromium_src/chrome/install_static/user_data_dir.cc`).
- **Left out of Brave:** `TorDisabled` removes a privacy feature, so it is not a debloat. `MetricsReportingEnabled`, `UrlKeyedAnonymizedDataCollectionEnabled` and `SafeBrowsingExtendedReportingEnabled` are Chromium policies. Brave's own list does not include them, and Chromium's text for the first describes reporting to Google, so what they do inside Brave was not confirmed.
- **Edge.** winutil sets 17 Edge values. .Clarion had 5 Edge settings and now has 12. Each new one is checked against Microsoft's policy page: feature tips (`ShowRecommendationsEnabled`), Rewards (`ShowMicrosoftRewards`), welcome screens (`HideFirstRunExperience`), feedback (`UserFeedbackAllowed`), the Insider promotion (`MicrosoftEdgeInsiderPromotionEnabled`), the default browser prompt (`DefaultBrowserSettingsCampaignEnabled`) and diagnostic data (`DiagnosticData`). For diagnostic data winutil sets 0, which Microsoft calls "Off (Not recommended)", so .Clarion sets 1, required data only.
- **winutil sets policies Microsoft marks dead.** Microsoft's Edge policy list marks `EdgeCollectionsEnabled` as obsolete, and `WebWidgetAllowed` and `WalletDonationEnabled` as deprecated. winutil still sets all three. Setting them does nothing.

### Added on 2026-10-11, each applied and reverted on a real PC (Windows 11 Professional, build 26200)

- **Prefer IPv4 over IPv6.** Microsoft's "Configure IPv6 for advanced users" does not recommend disabling IPv6 or unbinding it, and gives `DisabledComponents` = 32 (0x20) as the supported way to prefer IPv4. winutil offers both a disable and a prefer IPv4 tweak. Only the second matches Microsoft's guidance, and only that one is built.
- **Stop other computers printing through this PC.** Microsoft's advisory for CVE-2021-34527 (PrintNightmare) gives two workarounds: disable the Print Spooler service, which stops all printing, or disable the policy Allow Print Spooler to accept client connections, which keeps local printing. The policy is documented in the ADMX_Printing2 policy reference (Pro, Enterprise and Education), and its disabled value of 2 is in `Printing2.admx` on the test PC. Both are built: the policy as a Low risk setting, the service as a Medium risk one for people who never print.
- **Skip the lock screen** (policy Do not display the lock screen), **show a clear sign-in background** (Show clear logon background), **turn off the Program Compatibility Assistant** (policy `DisablePCA`) and **stop Edge suggesting pages for failed addresses** (Edge policy `AlternateErrorPagesEnabled`). Each is on a Microsoft policy reference page, and the first three are checked against the ADMX files on the test PC by the policy check in the tests.
- **Seven service settings**, from Microsoft's own table of default start types and recommendations for Windows IoT Enterprise (see "Services" below).

### Worth doing, documented by Microsoft, not built yet

- **Reserved storage off.** Microsoft documents `DISM /Online /Set-ReservedStorageState /State:Disabled` and warns it is refused while reserved storage is in use. winutil has it. It needs a typed operation in the engine, and a read of the current state.

### Not confirmed, so not added

- `DisableWpbtExecution` (WPBT): a search of Microsoft's documentation found the Intune firmware setting that disables WPBT, but not this registry value.
- Num Lock at startup (`InitialKeyboardIndicators`), and scrollbars always visible: only community answers were found, no Microsoft page.
- Detailed blue screen (`DisplayParameters`): a Microsoft support article exists but covers Windows 8 and Windows Server 2012. It does not say it holds on Windows 11.
- Window snapping, Settings home page, logon blur, verbose logon, notification centre and the sleep, display and graphics tweaks: not checked yet.

### Declined, with the reason

- **Disable Windows Update, or "security only".** .Clarion's rule is never to turn security updates off. The System page already reports whether updates are working.
- **Defender real-time protection off, BitLocker off, core isolation off** (Sparkle and winutil). They weaken security.
- **Disable IPv6.** Microsoft advises against it, as above.
- **Remove Edge, remove OneDrive, remove Widgets** by script. Scripted removals with no put-back. .Clarion has a Widgets policy and OneDrive sync policy, both reversible.
- **Set most services to manual, as one tweak.** The measurements and Microsoft's own table below show it would mostly change nothing, so each service is offered on its own, with what stops working. See "Services".
- **Hardware clock as UTC** (`RealTimeIsUniversal`, for dual boot with Linux). winutil and Sparkle both set it. Microsoft's own support answers call it an undocumented and unsupported registry key, and a Windows 7 support article describes a hang at the daylight saving changeover with it set. Not built.
- **Hosts file blocklists from a third party.** winutil's Adobe block downloads a list from another GitHub repository and applies it, which is a runtime download of data .Clarion cannot review.
- **Wi-Fi Sense** (Sparkle). Microsoft removed the contact sharing part during Windows 10 development. What remains, connecting to open hotspots, has a Group Policy.
- **Network tuning, `Win32PrioritySeparation`, dynamic tick and the NVIDIA profile.** Looked at again on 2026-10-12, with measurements. See "Network and graphics tuning" below for what was built and why the rest was not.

## Network and graphics tuning

Looked at on 2026-10-12 because Sparkle has "Optimize network settings" and "Optimize NVIDIA settings" and .Clarion had neither. Measured on Windows 11 Pro 25H2 build 26200 with a GeForce RTX 2060 (driver 32.0.16.1742). Sparkle was read as a lead only, and nothing from it was copied.

### TCP settings: what Sparkle sets and what Windows does

Sparkle's script runs eight netsh commands. To learn which of them change anything, `netsh int tcp show global` and `show supplemental` were saved, `netsh int tcp reset` was run (Microsoft's netsh reference describes it as removing user configured settings and resetting all TCP parameters to their default values), the two were compared, and the saved values were put back so the PC ended exactly as it began.

| Setting | Sparkle sets | This PC before | Windows default after the reset |
|---|---|---|---|
| Congestion provider | ctcp | ctcp | cubic |
| ECN capability | enabled | enabled | disabled |
| Timestamps | disabled | disabled | allowed |
| Scaling heuristics | disabled | disabled | disabled |
| Receive side scaling | enabled | enabled | enabled |
| Fast Open, and its fallback | enabled | enabled | enabled |
| Initial congestion window | 10 | 10 | 10 |

Three of the settings differ from Windows' own defaults, and this PC had all three, so the tool had been used on it. Its congestion provider choice replaces CUBIC, the default here, with the older CTCP. Its undo writes an initial congestion window of 4, where Windows uses 10, so undoing it does not give the defaults back. No test was made of whether any of this is faster or slower, so .Clarion does not claim either.

Microsoft's Set-NetTCPSetting page says client computers use NewReno by default. The measured default here is CUBIC, so that page is out of date for this build and was not used as a source for the default.

Built from this: a repair job, Put TCP settings back to Windows defaults, which runs `netsh int tcp reset`, and one that releases and renews the IP address. No setting changes TCP parameters.

### Other network tweaks that were looked up and not built

- **`NetworkThrottlingIndex`.** `mmcss.sys` on this build contains the value name, but Microsoft's Multimedia Class Scheduler Service page does not document it. Not built.
- **`SystemResponsiveness`.** Documented on that page. As retrieved, the page says a value below 10 is treated as 20, so the often repeated advice to set it to 0 does not do what its authors say.
- **`TcpAckFrequency` and `TCPNoDelay` (Nagle).** Microsoft's article for TcpAckFrequency describes Windows XP and Server 2003 and says Microsoft does not recommend changing the default of 2 without careful study. The registry value TcpNoDelay is documented only for Message Queuing. A program that wants Nagle off sets TCP_NODELAY on its own socket.
- **Letting Windows switch a network adapter off to save power.** Microsoft's NetAdapter module has Set-NetAdapterPowerManagement, but on build 26200 its parameter list (checked with Get-Command) has no parameter for the option, so a script that passes one fails. The only registry route found in Microsoft's pages is the Windows 7 one with PnPCapabilities. Not built.
- **`Win32PrioritySeparation`.** Microsoft's only page is a Windows 2000 era TechNet page. The value here is 2. No current documentation for what other values do. Not built.
- **`bcdedit /set disabledynamictick yes`.** Microsoft's BCDEdit reference says to use it only for debugging, and the timeBeginPeriod page says that since Windows 10 version 2004 a program's timer request no longer sets the global resolution. Not built.
- **Teredo.** Built, as Microsoft's policy. The default state is Default, and netsh on this PC reported it disabled and offline. After the policy was written netsh reported it as disabled by Group Policy, and removing the policy brought back Default. Microsoft's privacy guide says that turning Teredo off stops some Xbox gaming features and Delivery Optimization with Group or Internet peering, and the setting says so.

### NVIDIA driver settings

Sparkle's tweak writes a profile of 92 settings with a bundled copy of nvidiaProfileInspector, a third party program, and its own page says it cannot be undone. NVIDIA publishes a better way: the NVAPI library (github.com/NVIDIA/nvapi, MIT licence) with the driver settings API, and the setting numbers and allowed values in NvApiDriverSettings.h. The driver installs `nvapi64.dll` in System32, and .Clarion calls it from there with the function numbers from NVIDIA's `nvapi_interface.h`.

Reading this PC's global profile through NVAPI showed that it already held 65 of Sparkle's 92 values and 25 were not stored, which matches a PC the tool had been used on. Several of the values equal the defaults in NVIDIA's header, for example Maximum pre-rendered frames 0, MFAA off and trilinear optimization off, so writing them changes nothing.

Four settings are built, each with a reason NVIDIA gives, and each stores what it replaced so Revert can put it back, including the case where nothing was stored:

- **Low latency mode On.** NVIDIA's control panel reference says On limits queued frames to one, and its 2019 driver announcement says the same. The announcement also says that in DirectX 12 and Vulkan games the game decides when to queue frames, and the Reflex article says Reflex overrides the driver setting. It is written as Maximum pre-rendered frames 1. Ultra is not built, because its setting number is not in NVIDIA's public header, and the driver names no other latency setting among the 130 it lists.
- **Power management mode, prefer maximum performance.** NVIDIA's support article offers it for a card that throttles its clock speed.
- **Shader cache with no size limit.** The header gives the default as 0x4000 megabytes, 16 GB. NVIDIA warns that a limit that is too small can make the driver delete shaders still in use.
- **Texture filtering quality, high performance.** NVIDIA says it gives the highest frame rate. The control panel default for GeForce cards is Quality.

Not built: vertical sync off (it causes tearing, so it is a taste), the anisotropic and trilinear optimizations, and the many settings that are already NVIDIA's default. A profile for a single game overrides these global settings. Each setting was applied and reverted on the RTX 2060 from two start states, not stored and stored with another value, and the profile was left as it was found.

### Windows graphics

- **Optimizations for windowed games.** Microsoft's DirectX blog describes it. The registry format was read by switching the checkbox in the real Settings page and reading `HKCU\Software\Microsoft\DirectX\UserGpuPreferences`: the value `DirectXUserGlobalSettings` is the text `SwapEffectUpgradeEnable=1;` for On and `=0;` for Off. It is one text value that can hold other entries, such as the variable refresh rate choice, so a new list entry operation sets only its own entry. With no value written Settings showed On on this build. Two exclusive settings were built, On and Off.
- **Multi-Plane Overlay off.** NVIDIA's support article attaches `mpo_disable.reg`, which sets `OverlayTestMode` to 5 under `HKLM\SOFTWARE\Microsoft\Windows\Dwm`, and `mpo_restore.reg`, which deletes it. The files were read, not run. Both `dwmcore.dll` and `win32kfull.sys` on this build contain the value name.

### Other additions from winutil and Sparkle

- **Seconds in the taskbar clock** (both tools). The Settings checkbox is worded Show seconds in system tray clock (uses more power). Switching it wrote `ShowSecondsInSystemClock` as a DWORD, 1 for On and 0 for Off under `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced`.
- **New Outlook** (winutil). Microsoft's two pages disagree on the first code sample for `HideNewOutlookToggle`, so the page on controlling access in Exchange Online, which says 1 hides the switch and 0 shows it, was used. The other setting is the policy `NewOutlookMigrationUserSetting` set to 0, which Microsoft says blocks automatic migration.
- **Edge** (winutil). Configure Do Not Track and the Asset Delivery Service policy, from Microsoft's policy pages. `CreateDesktopShortcutDefault` was left out, because Microsoft says it has no effect on an Edge that is already installed.

### Looked at and left out

- **Reserved storage.** Microsoft documents `DISM /Online /Set-ReservedStorageState` and warns that disabling fails while it is in use. It is already disabled on this PC, so turning it on and off again could not be tried without reserving several gigabytes, and a setting that was never applied and reverted is not added.
- **RDP file warnings off** (both tools). The warnings exist to stop a malicious file from starting a remote session, so turning them off weakens security.
- **S3 sleep, window snapping, Storage Sense and the notification centre.** Not checked, or a loss of function with no matching gain.

## Services

Read and measured on 2026-10-11, because both tools offer to "set services to manual" and .Clarion changed only one service before this.

### What Microsoft says

Microsoft's page "Guidance on configuring system services" for Windows IoT Enterprise (dated 2026-06-24) lists 255 services with their default start type and one of five recommendations. 188 are Manual by default, 60 Automatic (3 of them with a delayed start) and 7 Disabled. 133 are marked OK to disable, 91 Don't disable, 20 No guidance, 7 Already disabled and 4 Should be disabled. The page is written for fixed function devices. It defines OK to disable as useful to some but not all organisations, which can disable it if they do not use it. It does not say that disabling any of them makes a PC faster. Microsoft also says each default was chosen to balance performance, functionality and security for a typical user.

Two things follow from the table. A tweak that sets services to Manual is mostly a no-op, because 188 of 255 already are, on that table's Windows IoT Enterprise defaults. Those are not always the defaults of Windows 11 Home and Pro: on the test PC the table lists AudioEndpointBuilder, Audiosrv, mpssvc, WlanSvc and StateRepository as Manual, while all five run as Automatic there, and .Clarion never changes them because they are protected. For that reason the Services view says a difference from the table is not proof that anyone changed a service. Only 20 services are Automatic by default and marked OK to disable: CDPSvc, CDPUserSvc, DiagTrack, iphlpsvc, LanmanServer, MapsBroker, MSDTC, NlaSvc, OneSyncSvc, PcaSvc, RetailDemo, shpamsvc, ShellHWDetection, Spooler, Themes, TrkWks, Wcmsvc, Wcncsvc, wlpasvc and workfolderssvc. Several of those, such as Themes, NlaSvc, ShellHWDetection and LanmanServer, are on .Clarion's protected list on purpose.

### What it costs on a real PC

On the test PC (Windows 11 Professional, build 26200, 15 GB of memory) 99 of 293 services were running, in 67 service host processes. Microsoft's service host page says that with more than 3.5 GB of memory each service runs in its own host process, so the memory of one service can be read directly. Of the 20 services above, the ones that were running used between 1 and 7 MB each: CDPUserSvc 4.2, CDPSvc 3.4, TrkWks 1.0, iphlpsvc 2.7, PcaSvc 7.4, LanmanServer 4.1, ShellHWDetection 1.9, Themes 1.8 and Wcmsvc 3.4 MB. That is about 30 MB in all. The test PC is a trimmed image where several of the others were already disabled, so a stock PC may save somewhat more. The honest benefit of switching a service off is mostly fewer things listening and fewer features that can start, not speed.

### What the two tools do

- **winutil** now has five service entries: CscService, DiagTrack, MapsBroker, StorSvc and SharedAccess, plus a change to `SvcHostSplitThresholdInKB`. Microsoft's service host page names a per service switch (`SvcHostSplitDisable`) but not that threshold, so the threshold is only documented by community posts and is not built here.
- **Sparkle** has a 292 line script that sets a start type for 282 services (207 Manual, 56 Automatic, 6 Automatic delayed start and 13 Disabled). Compared with Microsoft's table, 7 of the 13 Disabled are already Disabled by default, 3 are marked OK to disable (AJRouter, DiagTrack, shpamsvc), 2 are marked Don't disable (AssignedAccessManagerSvc and RemoteRegistry) and 1 is not in the table. Most of the script restates Windows' own defaults.

### What .Clarion now does

Seven settings, each on its own, each naming what stops working, each with a source and a real revert (the start type that was there is recorded first, including the delayed start flag). Each was applied and reverted on the test PC, and the services that were running were started again afterward:

- **Turn off printing support** (Spooler), Medium risk, for people who never print. The Low risk alternative is the print server policy above.
- **Downloaded Maps Manager** (MapsBroker), **distributed link tracking** (TrkWks), **Connected Devices Platform** (CDPSvc).
- **UPnP and SSDP device discovery** (SSDPSRV, upnphost), **Xbox Live services** (XblAuthManager, XblGameSave, XboxNetApiSvc), **smart card services** (SCardSvr, ScDeviceEnum, SCPolicySvc). These start only on demand, so the gain is a smaller attack surface, not memory, and the text says so.

The change is allowed only for the services named in the allowlist test, so adding one is a deliberate step, and the runtime guard still refuses the protected ones.

Left out on purpose: every service Microsoft marks Don't disable (SysMain, DoSvc, DPS, WerSvc, RemoteRegistry and others); the Xbox accessory service (XboxGipSvc), which supports controllers and accessories; Windows Search; and the peer networking group (PNRPsvc, p2psvc, p2pimsvc, PNRPAutoReg). Microsoft's table lists those four, but on build 26200 the service control manager reports them as not installed (error 1060), leaving only registry keys behind, so a setting for them could not be applied or checked.

## What the tools do that .Clarion does not

- **Install and update apps.** Both tools do it. winutil has 236 entries and Sparkle 162, each with a winget ID and most with a Chocolatey name. .Clarion now has an Apps page (added in beta 12, 135 apps now), with winget only, one exact package ID each, a licence step and no Chocolatey. It does not update installed apps yet. The "Installing apps" section below looks at how they do it.
- **Uninstall any installed program.** Sparkle's Debloat page lists every installed program, Store apps and desktop programs alike, minus an exclusion list, and uninstalls the ones picked. .Clarion removes a curated list of 25 Store apps and can put them back.
- **Quick fixes.** Sparkle has one-click jobs for releasing and renewing the IP address, restarting the audio service, restarting Bluetooth services, restarting the graphics driver and restarting Explorer and Search. .Clarion's Repair page has flush DNS, network reset, Windows Update reset, Store reset, system file and DISM repair, disk check and drive optimize, and it also restarts the audio services, the Bluetooth service and Explorer and resyncs the clock. It still has no one-click graphics driver restart.
- **Shortcuts to classic Windows tools.** winutil has buttons that open Control Panel items such as Programs and Features, Region, Sound and Power. .Clarion has none.
- **A few extras on winutil's list:** a daily registry backup task (Windows turned automatic registry backup off, and winutil switches it back on), the legacy F8 boot menu, and a PowerShell profile.
- **A few repair jobs.** winutil's fixes include repairing winget and the Windows time server setting. .Clarion has network reset, Windows Update reset, Store reset, system file and DISM repair, disk check and drive optimize, which cover the rest.
- **Build a custom Windows image.** winutil has a Windows image builder. .Clarion should not.
- **Reach.** winutil has about 64,000 stars on GitHub, Sparkle about 2,100. A tool that is run from one line in PowerShell spreads easily. A signed installer does not.
- **A low-effort way to contribute a tweak.** Sparkle's folder format (apply, unapply, `meta.json`) is simple, and it has a request template. .Clarion's catalog format is stricter. CONTRIBUTING.md has an "Adding a setting" section that says what a setting needs, but there is no request template like Sparkle's.

## What .Clarion does that they do not

- **Undo restores what was really there.** .Clarion records the previous value before each change. winutil's undo writes a fixed original value, usually "remove the entry". If the setting was already something else, that is lost.
- **Verify.** .Clarion checks what it applied against Windows after updates, and says what to try when a setting keeps coming back. Neither tool looks again after it has applied something.
- **No scripts for settings.** Every .Clarion change is a typed operation (registry, service, task, app, feature, DNS, power) that can be read, checked and undone. 26 winutil tweaks and every Sparkle tweak are PowerShell.
- **A reason, an evidence grade and a source for each setting.** 92 settings are graded Proven and each has a source, which the catalog loader enforces.
- **Nothing is run from the internet.** .Clarion runs its own files.

## More debloat gaps, package by package

winutil lists 34 Store packages for removal. .Clarion removes 18 of them. The other 16, with what was found:

- **Core tools** (Calculator, Notepad, Photos, Paint, Snipping Tool, Clock, Camera, Sound Recorder, Media Player). .Clarion leaves them. They could be offered as Expert-only removals with a put-back, but nothing is gained for most people, and this PC cannot reinstall them because it has no Microsoft Store.
- **Xbox sign-in and overlay pieces** (Xbox Identity Provider, Xbox TCUI, Xbox Speech To Text Overlay). .Clarion protects the identity provider on purpose, because removing it breaks Xbox sign-in.
- **Start Experiences App.** Sparkle's own list excludes it. .Clarion protects it.
- **Dev Home.** Microsoft's own page says it stopped being supported in Windows 11 in May 2025, so it is not worth a setting.
- **The new Microsoft Teams (`MSTeams`) and Mobile devices (`MicrosoftWindows.CrossDevice`).** These are real gaps. .Clarion removes the older Teams and Phone Link packages, which are different names. Neither new package exists on the only PC available here, and no Microsoft page was found that names them, so the names are not confirmed. They need a clean Windows 11 install to check.
- **Windows AI removal.** winutil removes `MicrosoftWindows.Client.CoreAI`. On this PC (build 26200.9457) Windows marks that package as non-removable, so a script has to work around the block. .Clarion does not.

On this PC 82 packages are installed and only one of the packages named above (CoreAI) exists, so this part of the comparison cannot be finished without a second machine.

## Installing apps

### What the tools do, from their own code

| | winutil | Sparkle |
|---|---|---|
| Engine | winget or Chocolatey | winget or Chocolatey |
| Install command | `winget install --id <id> --accept-package-agreements --accept-source-agreements --source <source> --silent` | `winget install <id> --silent --accept-package-agreements --accept-source-agreements`, or `choco install <id> -y` with a retry using `--pre` |
| Update | `winget upgrade --all --accept-package-agreements --accept-source-agreements --include-unknown --silent`, or per package | Not found in the code that was read |
| Uninstall | `winget uninstall --id <id> --source <source> --silent` | `winget uninstall <id> --silent`, `choco uninstall <id> -y` |
| Installed state | A scan of installed packages | `winget list` |
| Missing winget | Installs the `Microsoft.WinGet.Client` PowerShell module with `-Force`, then runs `Repair-WinGetPackageManager -AllUsers` | A script that checks `winget --version` and installs winget |
| Missing Chocolatey | Installs it | Installs it by running `winget install --id chocolatey.chocolatey` |
| Agreements | Accepted for the user on every install | Accepted for the user on every install |
| Catalog | 236 entries, all with a winget ID | 162 entries, 154 with a winget ID, 145 with a Chocolatey name |

### What was verified about winget

- Microsoft's `install` page: the query is a case-insensitive substring match on name, ID and moniker, unless `--exact` is used, and `--id` with `-e` is the documented way to select exactly one package. Neither tool passes `--exact`. A search for `Mozilla.Firefox` without it returned Firefox Beta, Developer Edition and dozens of language builds on this PC (winget 1.29.380).
- `--accept-package-agreements` accepts the package's own licence terms only, not optional components or bundled software an installer may offer, and `--accept-source-agreements` covers the source's terms. Microsoft says so on the same page. Both tools accept both on the user's behalf every time.
- Before anything is installed, `winget show --id <id> -e --source winget` prints the version, publisher, licence, installer type, installer URL and installer SHA-256. On this PC it took 8 seconds for 7-Zip.
- winget checks the downloaded installer against the manifest's hash, and a mismatch stops the install with return code `0x8A150011`. Other documented codes a screen can explain: `0x8A150010` no applicable installer, `0x8A150041` package agreements not accepted, `0x8A150046` source agreements not accepted, `0x8A150061` already installed, `0x8A15003A` blocked by Group Policy, `0x8A15001B` Microsoft Store blocked by policy (winget-cli repository, `returnCodes.md`).
- `--ignore-security-hash` exists and Microsoft calls it not recommended. Installing from a local manifest has to be enabled by an administrator.
- `--include-unknown` upgrades packages whose installed version winget cannot work out. winutil's upgrade-all passes it blindly.
- About the community repository's checks: maintainers and pull request bots describe an installer hash, static and dynamic scanning, a SmartScreen check and a manual review by a moderator before a package is accepted. That comes from GitHub discussions and bot comments, not an official policy page.
- On this PC winget 1.29.380 is installed even though the Microsoft Store is not, so a trimmed image can have it. The official `Microsoft.WinGet.Client` module is not installed.
- winget has a COM API and PowerShell module in the winget-cli repository. A reference page for the COM API was not found, so how well it works from a desktop app is not confirmed.

### How good are their catalogs

Every winget ID in both catalogs was looked up. winutil's 234 winget IDs all exist, and 2 more are Microsoft Store IDs, a separate source. Sparkle has 7 of 154 that do not resolve today with winget's exact match: `hluk.copyq` (the real ID is `hluk.CopyQ`, and exact matching is case-sensitive), `localsend` (`LocalSend.LocalSend`), `ABDownloadManager.ABDownloadManager` (`amir1376.ABDownloadManager`), and `Sublime.Text`, `RustDesk.RustDesk`, `Pursa3D.PursaSlicer` and `Mozilla.Firefox.MSIX.Nightly`, which have no match. 86 winget IDs are in both catalogs, and all 86 exist in Microsoft's winget-pkgs repository. That overlap is a good first list.

### How to do it better

1. **winget only to start.** It is Microsoft's tool, it checks installer hashes, and it needs no extra installer. Chocolatey would mean installing a second package manager from the internet first. No remote script is run.
2. **Pin every install exactly:** `--id <id> --exact --source winget --silent`, so one named package is installed and nothing else.
3. **Show before installing.** For each app, the review screen shows the version, publisher, licence, the host the installer comes from and its SHA-256, taken from `winget show`. Agreements are accepted per app by the person, on that screen, with the licence link, not on their behalf.
4. **Plain results.** The documented return codes become sentences, such as "Windows blocked this by policy" or "The download did not match its checksum and was stopped", with the winget log path.
5. **A record and a way back.** An install goes in the same journal as a setting: the ID, version, source and time. Revert uninstalls exactly that package. Verify says if it has gone, and What broke includes recent installs.
6. **A catalog that stays true.** The aim was about 100 apps, and there are 135, each with an honest one-line description, licence type, and flags for offers in the installer or needing administrator rights. A monthly check, like the source link check, looks up every ID. Neither tool does that, and 7 of Sparkle's have rotted.
7. **Per-user installs when the package allows it** (`--scope user`), so a copy running without administrator rights can still install what does not need them. Microsoft lists known scope issues, so this needs testing per app.
8. **Updates the person picks.** A list of what can be upgraded, with the version change, and nothing is upgraded blind.
9. **A list that works outside .Clarion.** Saving and loading a set of apps uses the same JSON as `winget export` and `winget import`, which Microsoft documents, so the file is not locked to .Clarion.
10. **When winget is missing,** say so and link to Microsoft's own instructions. Do not install a module from the PowerShell Gallery or run a script without being asked.
11. **Later:** search the whole winget source with the same review screen, an installed-programs list that can uninstall what winget knows (so it can be put back), and the official COM API in place of reading text.

### What is not decided

Whether to include Chocolatey later, whether the installed-programs uninstaller is wanted (.Clarion's rule is that every change can be put back, which only holds for packages winget knows), and whether the catalog should carry any app that bundles offers.

## Custom settings: how to do it better than a script folder

Sparkle's model asks a contributor to write PowerShell. The better version is data, not code, so that anyone can write and share a setting without being able to run anything on someone else's PC.

1. **Typed operations only.** A custom setting is the same registry, service and scheduled task operations the catalog uses, written in a small JSON file with a name, what it does, the benefit and the risk. No free PowerShell in the first version.
2. **Record what Windows changes.** An "Observe a change" step takes a snapshot of the registry areas where settings live, asks the person to flip a switch in Windows, takes a second snapshot and offers the difference as operations. That makes a setting without knowing the registry.
3. **Everything the catalog has.** Because it uses the same engine, a custom setting gets the journal, Revert to the real previous value, Verify and drift history for nothing.
4. **Guard rails.** Protected keys that no setting may touch (Defender, update services, boot), a risk level the author must choose, and Medium or High risk never marked Recommended, as in the catalog.
5. **Sharing without trust.** Export is a data file. Import shows the exact operations in plain words, with a hash, and never applies anything by itself.
6. **Stored where only administrators can write.** The data folder is locked to administrators when .Clarion starts with administrator rights, so another account cannot add a setting that an administrator later applies.
7. **Labelled honestly.** Custom settings show as "Custom, not reviewed by .Clarion", graded Unproven, like any setting without a source.

Not built. It needs a decision on the points above before work starts.

## Sources

- winutil: `config/tweaks.json`, `config/feature.json`, `config/appx.json`, `config/applications.json`, `config/preset.json`, `functions/`, `README.md` on the main branch, read 2026-10-11. Licence MIT.
- Sparkle: `tweaks/`, `docs/docs/creating-tweaks.md`, `docs/docs/tweaks/what-are-tweaks.md`, `src/main/tweakHandler.ts` on the `v2` branch, read 2026-10-11. Licence GPL-3.0.
- Brave: `brave/brave-core`, policy definitions under `components/policy/resources/templates/policy_definitions/BraveSoftware`, and `chromium_src/chrome/install_static/user_data_dir.cc`.
- Microsoft Edge policy reference: https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies, and the page for each policy named above.
- IPv6: https://learn.microsoft.com/en-us/troubleshoot/windows-server/networking/configure-ipv6-in-windows
- Reserved storage: https://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/dism-storage-reserve
- Wi-Fi Sense: https://learn.microsoft.com/en-us/archive/blogs/tip_of_the_day/tip-of-the-day-whats-new-in-windows-10-insider-builds-wi-fi-sense-changes-and-more
- Detailed blue screen: https://support.microsoft.com/en-us/topic/stop-error-information-isn-t-displayed-on-the-blue-screen-in-windows-216528fb-94fd-11a2-2675-398ecf5cc237
- Chromium `MetricsReportingEnabled`: `chromium/chromium`, `components/policy/resources/templates/policy_definitions/Miscellaneous/MetricsReportingEnabled.yaml`
