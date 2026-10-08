# Clarion Feature Matrix

Legend. Evidence: **P** Proven, **S** Situational, **C** Cosmetic, **U** Unproven (Expert only). Risk: Safe, Low, Med, High.

Exact values below are the intended implementation. Each must be re-verified against the target build before it ships, and its sources recorded in the catalog.

## 1. Bloatware removal

| Feature | Detail | Ev | Risk |
|---|---|---|---|
| Granular app list | Per package list for installed apps, grouped as Safe to remove, Optional, Protected. Shows publisher, size, install date. | P | Low |
| Remove for current user | Remove-AppxPackage on the selected package | P | Low |
| Remove for all users | Remove-AppxPackage with the all users switch | P | Med |
| Remove from new accounts | Remove the provisioned copy so new users do not receive it | P | Med |
| Reinstall | Restore from the Store or from the journal record | P | Safe |
| Suggested apps and promos | Turn off Start suggestions, lock screen tips, silent app installs through the Content Delivery Manager values | P | Safe |
| Start menu recommendations | Hide the recommended section where the edition honors the policy | C | Safe |
| Edge containment | Policies for startup boost, background mode, first run page, sidebar, shopping assistant, and desktop shortcut. Keeps the browser and the WebView2 runtime working. | S | Low |
| Edge removal (Expert) | Supported only where the OS allows uninstall. Uses the region policy file approach, never deletes system files. Leaves WebView2 untouched. | S | High |
| OneDrive unhook | Pause sync, check Known Folder Move, offer to move Desktop, Documents and Pictures back to local folders, then uninstall and set the file sync policy | S | Med |
| Widgets and news feed | Disable through the machine policy and remove the taskbar entry | C | Safe |
| Chat and Teams consumer | Remove the taskbar entry and consumer app | C | Safe |
| Copilot | Remove the app and apply the policy where the build supports it | S | Low |
| Recall | Remove the optional feature and set the machine policy that blocks snapshot analysis | S | Low |
| Xbox components | Optional. Warns that Game Bar, Game Pass and Xbox sign in rely on them | S | Med |
| Preinstalled third party apps | Detects OEM and promotional packages (casual games, streaming and social shortcuts) | P | Low |
| Old Windows features | Optional features page: legacy media components, Internet Explorer mode remnants, Work Folders client, fax and scan | S | Low |

## 2. Telemetry and privacy

| Feature | Detail | Ev | Risk |
|---|---|---|---|
| Diagnostic data level | Set `AllowTelemetry` under the DataCollection policy key to the lowest value the edition honors. On Home and Pro the floor is Required, and the app says so. | P | Safe |
| Tracking services | Set DiagTrack (Connected User Experiences and Telemetry) and dmwappushservice to Disabled | P | Low |
| Optional diagnostic data | Turn off, along with feedback frequency and tailored experiences | P | Safe |
| Diagnostic scheduled tasks | Disable Compatibility Appraiser, ProgramDataUpdater, and Customer Experience tasks | P | Low |
| Advertising ID | Per user value and policy | P | Safe |
| Activity history | Policy values for feed, publish and upload of user activities | P | Safe |
| Location | Policy and capability consent value, per app list view | P | Low |
| Web search in Start | `DisableSearchBoxSuggestions` under the Explorer user policy key | P | Safe |
| Search highlights | Turn off dynamic search box content | C | Safe |
| Cortana remnants | Legacy search policy, for builds that still read it | C | Safe |
| Handwriting and typing personalization | Stop sending inking and typing data | P | Safe |
| Speech and online recognition | Off by default, per user | P | Safe |
| Error reporting | Disable the reporting service and policy, with a note that it removes crash upload | S | Low |
| App launch tracking | Turn off start menu tracking of launched apps | P | Safe |
| Per app permissions overview | Reads camera, mic, location, contacts consent and lists apps with access | P | Safe |
| Hosts file blocking (Expert) | Optional curated list of tracking endpoints. Never edits it without a backup. | S | Med |

## 3. System tweaks

| Feature | Detail | Ev | Risk |
|---|---|---|---|
| Mouse acceleration | In `HKCU\Control Panel\Mouse` set `MouseSpeed`, `MouseThreshold1`, `MouseThreshold2` to 0 | C | Safe |
| Dark mode | `AppsUseLightTheme` and `SystemUsesLightTheme` under Themes Personalize | C | Safe |
| Classic context menu | Empty default value at `HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32`, then restart Explorer | C | Safe |
| Taskbar alignment | `TaskbarAl` under Explorer Advanced, 0 left, 1 center | C | Safe |
| Taskbar clean up | Search box mode, Task View, Widgets and Chat buttons | C | Safe |
| File extensions and hidden files | `HideFileExt`, `Hidden`, `ShowSuperHidden` | C | Safe |
| Open File Explorer to This PC | `LaunchTo` value | C | Safe |
| Remove Home and Gallery | Navigation pane entries | C | Safe |
| Sticky keys prompt | Turn off the shortcut dialog | C | Safe |
| Menu show delay | `MenuShowDelay` | C | Safe |
| Animations | Presets for visual effects, with reduced motion option | S | Safe |
| Startup delay | Remove the artificial startup app delay (`Serialize` key) | S | Low |
| Startup items manager | List, measure impact, disable user choices | P | Low |
| Game Mode and hardware scheduling | Toggle with a note that benefit depends on GPU and driver | S | Low |
| Windowed game optimizations | Toggle the per user setting | S | Safe |
| Fast Startup | Toggle `HiberbootEnabled`, explained as a cause of stale boot state | S | Safe |
| Long paths | Enable `LongPathsEnabled` | P | Safe |
| End task in taskbar menu | Enable developer setting | C | Safe |
| Print Screen opens snipping | Toggle | C | Safe |
| Background apps | Per app list, policy for default | S | Low |
| Storage Sense | Turn on and set schedule | P | Safe |
| Memory and scheduler values (Expert) | Priority separation and responsiveness values, shown with measured before and after | U | Med |

## 4. Network

| Feature | Detail | Ev | Risk |
|---|---|---|---|
| DNS presets | Set adapter DNS to chosen provider, with encrypted DNS templates where the build supports them. Shows current and reset value. | S | Low |
| Flush and reset | DNS cache flush, Winsock and IP stack reset with reboot note | P | Low |
| Teredo and IPv6 transition | Disable Teredo through the supported command. Does not disable IPv6. | S | Low |
| Delivery Optimization | Limit or disable peer upload | P | Safe |
| Metered connection awareness | Toggle | S | Safe |
| Network adapter power saving | Disable on selected adapters | S | Low |
| Network throttling and TCP values (Expert) | Shown only with measurements. Marked Unproven if gains cannot be reproduced. | U | Med |

## 5. Power and performance

| Feature | Detail | Ev | Risk |
|---|---|---|---|
| Power plan picker | Balanced, High Performance, Ultimate Performance (duplicated from its built in GUID on demand) | S | Safe |
| Hibernate | Toggle, with disk space freed shown | P | Safe |
| USB selective suspend | Toggle with note for desktops | S | Safe |
| PCIe link state | Toggle for desktops | S | Low |
| Processor boost mode | Presets with laptop heat warning | S | Low |
| Sleep and display timeouts | Quick presets | P | Safe |
| Laptop guard | Warns before applying desktop oriented plans on battery devices | P | Safe |

## 6. Updates and maintenance

| Feature | Detail | Ev | Risk |
|---|---|---|---|
| Delay feature updates | Defer by days, or pin a target release, using the Windows Update policy keys | P | Low |
| Keep quality updates | Security updates stay on. The app does not offer to stop them. | P | Safe |
| No forced restart | Block auto restart while users are signed in | P | Safe |
| Driver updates through Update | Toggle with a warning about vendor tools | S | Low |
| Pause updates | Time limited pause with a visible end date | P | Safe |
| Clear update cache | Stop services, rename the SoftwareDistribution folder, restart services, keep a backup until next success | P | Low |
| Windows Update repair | Reset components with logged steps | P | Low |
| Component cleanup | DISM StartComponentCleanup with space report | P | Safe |
| Health repair | DISM RestoreHealth then SFC scan, with logs | P | Safe |
| Temp and cache cleanup | Allow listed folders only, shows size before delete | P | Safe |
| Disk check scheduling | Queue a check for next boot | P | Safe |
| Reset and rebuild icon cache | One click | P | Safe |

## 7. Apps (package manager)

| Feature | Detail |
|---|---|
| Essentials catalog | Curated list by category: browsers, media, archivers, runtimes, dev tools, communication, utilities |
| Search | Query the package source, show publisher and version |
| Batch install | Queue with silent flags and progress |
| Upgrade all | List outdated, upgrade selected |
| Uninstall | Per package with leftovers report |
| Import and export | Save the app set as a file to rebuild a PC |
| Source safety | Show the source for each package, warn on unknown publishers, show hashes when available |

## 8. Safety and profiles

| Feature | Detail |
|---|---|
| Restore point per batch | See Design section 5 |
| Change history | Browsable journal, per entry Revert |
| Drift check | Highlights settings Windows has reset |
| Profiles | Recommended, Gaming, Privacy Max, Minimal, Custom. Exportable. |
| Dry run | Shows exact operations |
| Logs | Local, exportable, no upload |

## 9. Deliberately not included

- Disabling Defender, SmartScreen, firewall, or exploit mitigations.
- Disabling security updates.
- Deleting system files, patching binaries, or replacing system DLLs.
- Registry cleaners.
- Third party driver or activation tools.
- Any tweak that cannot be undone.

## 10. Compatibility

- Primary: Windows 11, current and previous feature releases.
- Best effort: Windows 10 22H2.
- Editions: Home, Pro, Enterprise, Education. Tweaks the edition ignores are hidden or labeled.
- Architectures: x64 and ARM64.

## 11. Additions from the research pass

Windows features and repair (all use supported cmdlets, each shows what it installs):

| Feature | Detail | Ev | Risk |
|---|---|---|---|
| Optional features page | One click enable for Hyper-V, Windows Subsystem for Linux, Windows Sandbox, .NET Framework 3.5, legacy media components, NFS client, OpenSSH server | P | Low |
| Network reset | Release and renew, flush DNS, reset Winsock and the IP stack, with reboot note | P | Low |
| Update repair | Reset update services and cache, then run a scan | P | Low |
| Package manager repair | Reinstall the package manager when it is missing or broken | P | Low |
| Time source | Switch to a public time pool and resync. UTC hardware clock toggle for dual boot | S | Low |
| Registry backup task | Daily scheduled export of the registry hives, with retention | P | Safe |
| Legacy boot menu | Toggle the F8 style boot recovery menu | S | Low |
| Reserved storage | Show size and allow turning off on small drives, with the update risk stated | S | Medium |
| Classic panels | Launch buttons for Computer Management, Mouse, Network Connections, Power, Programs, Region, Sound, System, Date and Time, Firewall, Recovery | P | Safe |
| Graphics driver restart | Same as the Win+Ctrl+Shift+B shortcut, as a button | P | Safe |
| System information | Build, edition, uptime, drives, memory, startup count | P | Safe |

More tweaks:

| Feature | Detail | Ev | Risk |
|---|---|---|---|
| Edge and Chromium browser policies | Per browser policy sets for startup boost, background mode, sidebar, shopping, sign in nags, first run | S | Low |
| Vendor software auto install | Block peripheral vendor installers that Windows Update keeps pulling in | S | Low |
| Folder type auto discovery | Stop Explorer from guessing folder types, which slows large folders | S | Safe |
| Verbose logon and crash screens | Show detail during sign in, shutdown and blue screens | C | Safe |
| Battery percentage, scrollbars, Num Lock | Small comfort toggles | C | Safe |
| Window snapping, lock screen blur, Settings home | Toggles | C | Safe |
| Start menu layout | Choose between the new and previous layout where the build supports it | C | Safe |
| Services to manual | Curated list only, each with measured memory or boot impact, never a blanket change | S | Low |
| Hosts file blocklists | Optional named lists for vendor telemetry and update nag hosts, with backup | S | Medium |
| DNS providers | Cloudflare, Google, Quad9, OpenDNS, AdGuard, automatic, with encrypted DNS where the build supports it | S | Low |

## 12. Presets

Four presets ship in the catalog. They never include High risk or Unproven items, and edition gated tweaks are skipped on editions that ignore them.

| Preset | Intent |
|---|---|
| Minimal | Stop promotions and the lowest diagnostic level |
| Standard | Minimal plus search, activity, update sharing and Explorer basics |
| Advanced | Standard plus telemetry services and tasks, location, classic menu, OneDrive-free setup items |
| Gaming | Input tweaks, fewer background jobs, no update sharing |

## 13. Findings that shaped the catalog

- Some policies are only honored on certain editions. The consumer experiences policy works on Enterprise and Education only, so Clarion hides it elsewhere and offers per account values instead.
- The lowest diagnostic level is honored only on Enterprise and Education. Home and Pro treat it as Required. Clarion says so on the card.
- Blanket service disabling is where most breakage comes from. Clarion keeps a protected list and refuses those changes.
- Debloat that cannot be undone erodes trust. Every removal in Clarion records what it removed so it can be restored.
- Security feature switches (memory integrity, Defender real time protection, BitLocker) are not offered.
- Network and scheduler tweaks without published measurements are held back as Unproven.
