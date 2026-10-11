# Other Windows tweak tools, compared

What two widely used tools do, what Clarion does not do yet, and what is worth doing, with the source for each claim. Read on 2026-10-11 from the tools' own repositories and from Microsoft's and Brave's own documentation. Where a source is a forum or a community answer, it says so.

The two tools are winutil (github.com/ChrisTitusTech/winutil) and Sparkle (github.com/thedogecraft/sparkle).

## Licences, and how they are used here

- winutil is MIT. Sparkle is GPL-3.0. Clarion is all rights reserved with public source, so nothing from Sparkle, not code and not text, may be copied in. winutil's MIT licence would need its notice kept.
- Neither tool's definitions are copied. They are read as leads. Every setting that is added is written from the vendor's own documentation, and checked against it, as the catalog rules already say.

## What was read

| | winutil | Sparkle |
|---|---|---|
| Tweaks | 67 entries in `config/tweaks.json`, 50 of them change the registry (122 values) | 43 folders under `tweaks/` |
| Undo | 58 of 67 have an undo script or recorded original values, but the original values are fixed text such as `<RemoveEntry>`, not what was on the PC | 39 of 43 have an `unapply.ps1` |
| How a tweak is defined | Registry rows, service rows, or a PowerShell script (26 entries have one) | A PowerShell script per tweak, plus `meta.json` |
| Beyond tweaks | Install apps (236 entries, winget or Chocolatey), Windows features (33), app removal (34), update modes, repair fixes, presets (Minimal 4, Standard 12, Advanced 17), configuration import and export, a command line run mode, a Windows image builder | Install apps (162 entries, winget or Chocolatey), Debloat that lists every installed program and uninstalls the ones picked, Utilities (15 quick fixes), Backup (create and delete restore points), DNS, Clean, a self-updater, per tweak documentation pages, a recommended mark and a risk level per tweak, an NVIDIA profile import |
| How it is run | `irm https://christitus.com/win \| iex`, a remote script fetched and run each time, from its own README | An installed Electron app |

The Sparkle numbers come from the `v2` branch. Its loader (`src/main/tweakHandler.ts`) reads tweaks only from the folder bundled inside the app. No loader for tweaks a user adds on their own PC was found in `src/` or `tweaks/`, so its "create your own tweaks" page is a guide for contributing a tweak to the project, with a tweak request issue template, not a feature for end users. That is read from file names and the loader, not from running it.

## How much of winutil Clarion already covers

By registry value (path and name), Clarion sets 55 of the 122 values winutil sets, 45 percent. Before the browser settings added with this document it was 40 of 122, 33 percent. Fifteen winutil tweaks are covered completely.

Clarion is broader in other places: 41 app permission settings, 25 app removals, 10 DNS entries, power plans, and Verify, History and What broke, which neither tool has.

## winutil and Sparkle settings that Clarion does not have

Sorted by what the evidence allows.

### Added with this document

- **Brave.** winutil's "Brave debloat" sets 12 policy values. Nine are Brave's own policies. All nine exist in Brave's source (`brave/brave-core`, `components/policy/resources/templates/policy_definitions/BraveSoftware`), with the same direction (`Disabled` is 1, `Enabled` is 0). Six Clarion settings now cover them: Rewards, Wallet, VPN, AI Chat, News and Talk together, and the two usage pings. Brave's own text says a set policy cannot be changed by the user. The registry key is `SOFTWARE\Policies\BraveSoftware\Brave`, from Brave's own source (`chromium_src/chrome/install_static/user_data_dir.cc`).
- **Left out of Brave:** `TorDisabled` removes a privacy feature, so it is not a debloat. `MetricsReportingEnabled`, `UrlKeyedAnonymizedDataCollectionEnabled` and `SafeBrowsingExtendedReportingEnabled` are Chromium policies. Brave's own list does not include them, and Chromium's text for the first describes reporting to Google, so what they do inside Brave was not confirmed.
- **Edge.** winutil sets 17 Edge values. Clarion had 5 Edge settings and now has 12. Each new one is checked against Microsoft's policy page: feature tips (`ShowRecommendationsEnabled`), Rewards (`ShowMicrosoftRewards`), welcome screens (`HideFirstRunExperience`), feedback (`UserFeedbackAllowed`), the Insider promotion (`MicrosoftEdgeInsiderPromotionEnabled`), the default browser prompt (`DefaultBrowserSettingsCampaignEnabled`) and diagnostic data (`DiagnosticData`). For diagnostic data winutil sets 0, which Microsoft calls "Off (Not recommended)", so Clarion sets 1, required data only.
- **winutil sets policies Microsoft marks dead.** Microsoft's Edge policy list marks `EdgeCollectionsEnabled` as obsolete, and `WebWidgetAllowed` and `WalletDonationEnabled` as deprecated. winutil still sets all three. Setting them does nothing.

### Worth doing, documented by Microsoft, not built yet

- **Prefer IPv4 over IPv6.** Microsoft's "Configure IPv6 for advanced users" says it does not recommend disabling IPv6 or unbinding it, and gives `DisabledComponents` = 32 (0x20) as the supported way to prefer IPv4. winutil offers both a disable and a prefer IPv4 tweak. Only the second matches Microsoft's guidance. Small value, low risk.
- **Reserved storage off.** Microsoft documents `DISM /Online /Set-ReservedStorageState /State:Disabled` and warns it is refused while reserved storage is in use. winutil has it. It needs a typed operation in the engine, and a read of the current state.

### Not confirmed, so not added

- `DisableWpbtExecution` (WPBT): a search of Microsoft's documentation found the Intune firmware setting that disables WPBT, but not this registry value.
- Num Lock at startup (`InitialKeyboardIndicators`), and scrollbars always visible: only community answers were found, no Microsoft page.
- Detailed blue screen (`DisplayParameters`): a Microsoft support article exists but covers Windows 8 and Windows Server 2012. It does not say it holds on Windows 11.
- Window snapping, Settings home page, logon blur, verbose logon, notification centre and the sleep, display and graphics tweaks: not checked yet.

### Declined, with the reason

- **Disable Windows Update, or "security only".** Clarion's rule is never to turn security updates off. The System page already reports whether updates are working.
- **Defender real-time protection off, BitLocker off, core isolation off** (Sparkle and winutil). They weaken security.
- **Disable IPv6.** Microsoft advises against it, as above.
- **Remove Edge, remove OneDrive, remove Widgets** by script. Scripted removals with no put-back. Clarion has a Widgets policy and OneDrive sync policy, both reversible.
- **Set most services to manual.** Clarion changes two services. No Microsoft source says the long lists help.
- **Hosts file blocklists from a third party.** winutil's Adobe block downloads a list from another GitHub repository and applies it, which is a runtime download of data Clarion cannot review.
- **Wi-Fi Sense** (Sparkle). Microsoft removed the contact sharing part during Windows 10 development. What remains, connecting to open hotspots, has a Group Policy.
- **Network tuning, `Win32PrioritySeparation`, dynamic tick and the NVIDIA profile.** Performance claims with no Microsoft source or measurement.

## What the tools do that Clarion does not

- **Install and update apps.** Both tools do it. winutil has 236 entries and Sparkle 162, each with a winget ID and most with a Chocolatey name. Clarion now has an Apps page (added in beta 12, 135 apps in the next release), with winget only, one exact package ID each, a licence step and no Chocolatey. It does not update installed apps yet. The "Installing apps" section below looks at how they do it.
- **Uninstall any installed program.** Sparkle's Debloat page lists every installed program, Store apps and desktop programs alike, minus an exclusion list, and uninstalls the ones picked. Clarion removes a curated list of 25 Store apps and can put them back.
- **Quick fixes.** Sparkle has one-click jobs for releasing and renewing the IP address, restarting the audio service, restarting Bluetooth services, restarting the graphics driver and restarting Explorer and Search. Clarion's Repair page has flush DNS, network reset, Windows Update reset, Store reset, system file and DISM repair, disk check and drive optimize, and in the next release it also restarts the audio services and the Bluetooth service and resyncs the clock. It still has no one-click graphics driver or Explorer restart.
- **Shortcuts to classic Windows tools.** winutil has buttons that open Control Panel items such as Programs and Features, Region, Sound and Power. Clarion has none.
- **A few extras on winutil's list:** a daily registry backup task (Windows turned automatic registry backup off, and winutil switches it back on), the legacy F8 boot menu, and a PowerShell profile.
- **A few repair jobs.** winutil's fixes include repairing winget and the Windows time server setting. Clarion has network reset, Windows Update reset, Store reset, system file and DISM repair, disk check and drive optimize, which cover the rest.
- **Build a custom Windows image.** winutil has a Windows image builder. Clarion should not.
- **Reach.** winutil has about 64,000 stars on GitHub, Sparkle about 2,100. A tool that is run from one line in PowerShell spreads easily. A signed installer does not.
- **A low-effort way to contribute a tweak.** Sparkle's folder format (apply, unapply, `meta.json`) is simple, and it has a request template. Clarion's catalog format is stricter. CONTRIBUTING.md has an "Adding a setting" section that says what a setting needs, but there is no request template like Sparkle's.

## What Clarion does that they do not

- **Undo restores what was really there.** Clarion records the previous value before each change. winutil's undo writes a fixed original value, usually "remove the entry". If the setting was already something else, that is lost.
- **Verify.** Clarion checks what it applied against Windows after updates, and says what to try when a setting keeps coming back. Neither tool looks again after it has applied something.
- **No scripts for settings.** Every Clarion change is a typed operation (registry, service, task, app, feature, DNS, power) that can be read, checked and undone. 26 winutil tweaks and every Sparkle tweak are PowerShell.
- **A reason, an evidence grade and a source for each setting.** 92 settings are graded Proven and each has a source, which the catalog loader enforces.
- **Nothing is run from the internet.** Clarion runs its own files.

## More debloat gaps, package by package

winutil lists 34 Store packages for removal. Clarion removes 18 of them. The other 16, with what was found:

- **Core tools** (Calculator, Notepad, Photos, Paint, Snipping Tool, Clock, Camera, Sound Recorder, Media Player). Clarion leaves them. They could be offered as Expert-only removals with a put-back, but nothing is gained for most people, and this PC cannot reinstall them because it has no Microsoft Store.
- **Xbox sign-in and overlay pieces** (Xbox Identity Provider, Xbox TCUI, Xbox Speech To Text Overlay). Clarion protects the identity provider on purpose, because removing it breaks Xbox sign-in.
- **Start Experiences App.** Sparkle's own list excludes it. Clarion protects it.
- **Dev Home.** Microsoft's own page says it stopped being supported in Windows 11 in May 2025, so it is not worth a setting.
- **The new Microsoft Teams (`MSTeams`) and Mobile devices (`MicrosoftWindows.CrossDevice`).** These are real gaps. Clarion removes the older Teams and Phone Link packages, which are different names. Neither new package exists on the only PC available here, and no Microsoft page was found that names them, so the names are not confirmed. They need a clean Windows 11 install to check.
- **Windows AI removal.** winutil removes `MicrosoftWindows.Client.CoreAI`. On this PC (build 26200.9457) Windows marks that package as non-removable, so a script has to work around the block. Clarion does not.

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
6. **A catalog that stays true.** About 100 apps, each with an honest one-line description, licence type, and flags for offers in the installer or needing administrator rights. A monthly check, like the source link check, looks up every ID. Neither tool does that, and 7 of Sparkle's have rotted.
7. **Per-user installs when the package allows it** (`--scope user`), so a copy running without administrator rights can still install what does not need them. Microsoft lists known scope issues, so this needs testing per app.
8. **Updates the person picks.** A list of what can be upgraded, with the version change, and nothing is upgraded blind.
9. **A list that works outside Clarion.** Saving and loading a set of apps uses the same JSON as `winget export` and `winget import`, which Microsoft documents, so the file is not locked to Clarion.
10. **When winget is missing,** say so and link to Microsoft's own instructions. Do not install a module from the PowerShell Gallery or run a script without being asked.
11. **Later:** search the whole winget source with the same review screen, an installed-programs list that can uninstall what winget knows (so it can be put back), and the official COM API in place of reading text.

### What is not decided

Whether to include Chocolatey later, whether the installed-programs uninstaller is wanted (Clarion's rule is that every change can be put back, which only holds for packages winget knows), and whether the catalog should carry any app that bundles offers.

## Custom settings: how to do it better than a script folder

Sparkle's model asks a contributor to write PowerShell. The better version is data, not code, so that anyone can write and share a setting without being able to run anything on someone else's PC.

1. **Typed operations only.** A custom setting is the same registry, service and scheduled task operations the catalog uses, written in a small JSON file with a name, what it does, the benefit and the risk. No free PowerShell in the first version.
2. **Record what Windows changes.** An "Observe a change" step takes a snapshot of the registry areas where settings live, asks the person to flip a switch in Windows, takes a second snapshot and offers the difference as operations. That makes a setting without knowing the registry.
3. **Everything the catalog has.** Because it uses the same engine, a custom setting gets the journal, Revert to the real previous value, Verify and drift history for nothing.
4. **Guard rails.** Protected keys that no setting may touch (Defender, update services, boot), a risk level the author must choose, and Medium or High risk never marked Recommended, as in the catalog.
5. **Sharing without trust.** Export is a data file. Import shows the exact operations in plain words, with a hash, and never applies anything by itself.
6. **Stored where only administrators can write.** The data folder is locked to administrators when Clarion starts with administrator rights, so another account cannot add a setting that an administrator later applies.
7. **Labelled honestly.** Custom settings show as "Custom, not reviewed by Clarion", graded Unproven, like any setting without a source.

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
