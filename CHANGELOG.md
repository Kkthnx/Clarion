# Changelog

## Unreleased

Fixed
- Pressing the window's close button while a repair job or an app update was running threw an error each time. WinUI shows one dialog at a time, and the notice that .Clarion is still working was a second one on top of the job's log. The error was written to crash.log twice in a row when tried, and the window stayed open with no word. The notice now waits its turn or is left out when another dialog is already in front of you, and the same care is taken for the unexpected-error notice and the link dialog.
- Turn off the Sticky Keys shortcut showed as changed back, and was offered again by presets, on a PC where the shortcut was already off. The setting wrote the whole flags value (506), and Windows can leave that value as 498 when the shortcut is turned off in Settings, because it also clears the confirm bit. Only bit 4, the one that opens the prompt, matters, so the setting now owns that bit alone, checks it alone and leaves the other bits as they are. Revert still puts back the whole value that was there. History written before this is checked the same way.
- Restart Explorer now closes only the Explorer of your own sign in. With a second account signed in at the same time, its Explorer was closed too.

New
- Safe Mode is recognised. Windows apps, scheduled tasks and Windows features cannot be read there, so their settings used to show as not available with nothing said, and the log held raw error codes. Home now says that Windows is in Safe Mode, what stops working, that settings kept in the registry still work, and that a restart brings everything back. The status line says so too.
- Browser caches in Clean up: Google Chrome, Microsoft Edge, Brave and Firefox. Each one clears the cache folders of every profile (Cache, Code Cache and GPUCache for the Chromium browsers, cache2 for Firefox) and leaves logins, history, bookmarks and passwords alone. A row is skipped while its browser is running. The README already listed browser caches, and until now only game launcher caches were there. On the PC this was tried on, Chrome held 418 MB, the same as the folders add up to by hand.
- A cleanup rule that names folders inside every profile of a program, so a second browser profile is found without being listed.
## 1.1.0

Safer
- An unexpected error no longer carries on as if nothing happened. The global handler used to log it and mark it handled. While .Clarion is idle it now empties the queue, reads every setting again from Windows and tells you. During a run, the run ends with its own read of Windows. Five errors in a minute end the program.
- The command line apply no longer installs apps into the wrong account. A preset with apps run from an elevated terminal of another account is left out of the install, with the reason, and the exit code is 1. The Apps page already refused this. The account check also looked at the first Explorer it could open, which on a PC with more than one session can be another person's. It uses the Explorer of this session.
- A web link is no longer opened with administrator rights. If the launch through the signed in shell failed, the link fell back to an ordinary launch and the browser started elevated. A dialog now shows the address to copy.
- The restart of Explorer says what it does: it closes every open folder window, interrupts a file copy running in one and removes the taskbar for a moment.

Fixed
- The review reads app details from winget four at a time, with 60 seconds for the whole read, instead of one at a time with up to two minutes each. A row that is still waiting says winget took too long, and the licence box no longer stays locked behind it.
- Revert of an app checks that it is really gone. winget uses an app's quiet uninstall command only when the app registered one, and otherwise runs its ordinary uninstaller and reports success as soon as it starts. Xournal++ is one: its uninstall wizard opened while winget said it was done. Revert now looks again after a few seconds, and if the app is still installed says that its uninstaller cannot run silently and may be waiting in its own window.
- An app logo that cannot be drawn is logged with the app's name and the row shows the letter tile.
- A tool run from a repair job could lose its output on a busy PC. The readers of its output ran on the thread pool, which can take longer than the two seconds the runner waits for them once the tool has exited, so the log could come back empty. Each pipe is now read on its own thread.
- An installed app showed the icon Windows lists for it ahead of the logo saved inside the program, so .NET 9 SDK showed a generic installer icon. The saved logo now comes first, as the notes always said, and the SDK uses the same .NET picture as the four runtimes.
- The log window of a repair job or an app update could be closed with Escape while the job was still running, leaving it going with no window and no Stop button. It now stays until the job has reported.

New
- 120 more apps, 255 in all, each one an exact winget package ID read from winget's own record. Windows runtimes (.NET Desktop Runtime 6 to 10, Visual C++ 2015 to 2022, Amazon Corretto), remote access tools, driver tools, Windows shell mods and auto clickers are in, each with its risk written plainly: turning a runtime off later runs its uninstaller and any program that needs it stops starting, a remote access tool lets someone connect once you set it up, a driver tool can leave a driver that does not work, and many online games ban an auto clicker. A new Runtimes topic and a symptom for a program that will not start for want of a runtime lead to them.
- Logos on the Apps page for most apps: 105 outlines and 85 pictures, 190 of the 255. They are saved inside the program, so nothing is fetched while it runs. Where each picture came from is written down, checked by a test, and credited on the About page. An app with none shows its first letter. See docs/RESEARCH-OTHER-TOOLS.md for how winutil and Sparkle get theirs and why Icons8 was not used.
- Install for this account only, a choice in the review for the 118 apps that can do it. It goes in your own profile and not in Program Files, other accounts do not get it, and Revert removes it from the same account. winget accepts the option for more apps than that, but Microsoft says EXE installers do not reliably follow it, so an app is offered only when its manifest declares the scope or winget itself carries it out (portable and MSIX apps). Tried for real through .Clarion's own engine on Inno, portable, MSIX and EXE apps, each installed into the profile and reverted cleanly. `--user-scope` does it from the command line.
- Check for app updates on the Apps page, and `--update-apps` on the command line. winget is asked which installed apps have a newer version, and they are listed with nothing ticked. You choose which to update, and they are updated one at a time with a live log and a Stop button. It never updates everything at once, never includes apps whose version winget cannot read or that are pinned, and accepts licence terms only if you tick the box. An update cannot be undone from .Clarion. Tried on this PC: it found the same four catalog apps winget lists, and updated an older Greenshot from 1.3.312 to 1.3.323 in about 10 seconds from the command and from the window.
- Save my apps as a winget file and Load a winget file in Settings, in the shape winget export and winget import use. Loading queues only the apps .Clarion lists, and names the others it did not queue.
- Restart the graphics driver, a repair job. It makes one request to Windows per running adapter, the property change request Device Manager makes, which Microsoft documents as stopping and restarting the device. Sparkle's version turns each device off and on again, which leaves the adapter off if the second call fails. Tried on a PC with two adapters: both restarted in 3.2 seconds, and Windows shows both arriving again at that moment.
- Ten shortcuts on the Repair page to classic Windows tools: Programs and Features, Network connections, Sound, Power options, Date and time, Region, System properties, Device Manager, Disk Management and Event Viewer. Each is started as the signed in user, so the tool asks for administrator rights itself.
- Flush the DNS cache beside the DNS provider choice, so a changed provider takes effect at once.
- Save report on the System page. Copy report and Save report both remove the computer name, user name, network addresses and account IDs first.
- Each item in the review has Leave out. It takes that one item out of the queue and leaves the rest. Before, the choice was to apply everything or clear everything. If it was the last app install, the licence box goes away.
- Save change report on the Safety page. It saves what .Clarion changed and is still on, newest first, with the step that undoes each one, as a text file with the computer name, user name and network addresses removed. Before, the history could only be read on screen.
- A setting request template for issues, and docs/GUIDES.md with task by task guides.

Polish
- Two or three cards across in a wide window. On a maximized window each setting card used to stretch over the whole width with most of it empty. The list now fits as many cards as have room for their name, summary and switch (480 wide each, up to three), and one wide card in a narrow window, as before. Each row is as tall as its tallest card, so no card is cut off when its summary takes two lines, and the list still creates only the rows on screen. The compact list stays one line per setting. Sparkle's Tweaks page does the same with a grid that goes from one to four columns at fixed window widths, which .Clarion replaces with the width the list really has.
- A check of every winget package ID in the app list against winget's own source (scripts/check-winget-ids.ps1) runs monthly and on any pull request that changes the list. All 255 pass. scripts/new-app-entry.ps1 writes a catalog entry from winget's record, and scripts/add-simple-icon.ps1 and scripts/fetch-app-logos.ps1 add logos.
- A long review is sorted into sections. From six changes with more than one kind, the review shows a heading and a count for settings to turn on, apps to install, Windows apps to remove, apps to uninstall and settings to revert, and each section folds. Nothing starts folded, because a section that has not been looked at has not been agreed to. A revert row used to say only that it puts back the recorded value. It now lists the steps, read from the history the way the engine reads it, or says that nothing was recorded and the default shown is used.
- The welcome dialog shows the loop in four short lines above the presets: Queue, Review, Verify, Revert. Microsoft says a teaching tip is impermanent and must not hold anything critical to the app, so the loop is plain text in the dialog a new person has to read anyway.
- The navigation pane and the caption buttons follow the High Contrast theme. The pane kept its dark background while its text took the person's colours, and the caption buttons used fixed colours. Both now read the same theme colours as the rest of the window, and in High Contrast the caption buttons use the system's own.
- The build no longer prints 45 warnings about logo file names. The resource indexer reads whatever follows a dot in a file name as a qualifier, so the logo files are now named app-name.png and not app.name.png.
- Continuous integration lints the workflow files with actionlint and checks whitespace formatting. The release workflow starts the published program and checks that --version, --list-presets and --list-settings answer, since the tests start the built program and not the files that ship.
- docs/DESIGN.md no longer lists static analysis, a script linter, virtual machine tests or ARM64 builds. It says what CI runs and what is not built yet.

## 1.0.1

New
- Four NVIDIA graphics driver settings: low latency mode On, power management prefer maximum performance, no shader cache size limit and high performance texture filtering. They read and write the driver's global profile through NVIDIA's own NVAPI library, with the setting numbers and values from NVIDIA's public header, and no third party program. Each one stores what it replaced, including that nothing was stored, so Revert puts back exactly that. Each was applied and reverted on a GeForce RTX 2060 from two start states. They show as not available on a PC without an NVIDIA card.
- Settings for Optimizations for windowed games, on and off, written as the one entry they change inside the text value Windows keeps for graphics choices, so other entries such as the variable refresh rate choice are kept. Also Multi-Plane Overlay off (the value in NVIDIA's own mpo_disable.reg), Teredo off (Microsoft's policy), seconds in the taskbar clock, two Outlook settings (hide the Try the new Outlook switch, stop the automatic move) and two Edge policies (Do Not Track, the Asset Delivery Service).
- Two repair jobs. Put TCP settings back to Windows defaults runs netsh int tcp reset. On the PC it was tried on, another tool had set ECN on, timestamps off and the CTCP congestion provider, and Windows 11 build 26200 defaults to ECN off, timestamps allowed and CUBIC. Release and renew the IP address runs ipconfig /release and /renew.
- docs/RESEARCH-OTHER-TOOLS.md has what Sparkle's network and NVIDIA tweaks set, the measured Windows defaults, and why the other network tweaks that were looked up were not built.

Polish
- The pane on the right of every list is gone. Each setting, repair job and cleanup row has a circled i that opens the same explanation, including the exact changes and sources in Expert mode, in a flyout beside it. It opens with a click or the keyboard, so it also works for touch and screen readers, and the lists use the full width.
- The topic buttons above a list now count what the search box and filter leave. Searching for NVIDIA showed All 343 over five results before.

## 1.0.0

Safer
- Cleanup no longer follows links. After a clean, the empty folders inside a cache were found with a listing that goes into a junction, so an empty folder behind a link in a cache could be removed from wherever the link pointed. It now walks the tree itself and removes a link without opening it. The check that a folder is allowed also looked only at the last folder of the path, so a link in the middle of a path below an allowed folder led the clean elsewhere. It now checks every folder in between.
- A setting can no longer overwrite a registry value that .Clarion cannot read back. A list of strings was read as an empty binary value, so applying a setting to one and reverting it wrote an empty binary over the original. None of the values the catalog changes is one on the test PC, so this guards against a future setting or a different PC.
- The monthly check runs with the account's administrator rights only when .Clarion is in Program Files or Windows. From a folder that any program of the same account can write to, such as Downloads, the program file could be replaced and would then run elevated on the second Wednesday. From there it now runs without them, and Settings says it may not be able to read every setting.
- The monthly check's task definition is written to the Windows temp folder, where users can add files but cannot change one that an administrator made. It used to go through the account's own temp folder, where a program of the same account without administrator rights could swap it before Task Scheduler read it and so choose what the elevated task runs.
- Every action in the build workflows is named by its full commit instead of a version tag that its owner can move, and Dependabot proposes updates monthly. The release workflow no longer pastes the tag name into its script.
- A package, feature, power plan or setup id that ended in a line break passed validation. It no longer does.

Fixed
- Reverting a setting that stopped part way no longer hides the steps that did not run. The history marked each step undone as soon as it ran, so after a failure the next Revert said there was nothing to revert. The undo is now written only when every step worked, and running Revert again finishes the job.
- The restart that .Clarion offers after a change used a reason code that Microsoft does not list. It now uses the documented one for a restart after a settings change.
- Closing the window during a clean or a repair job was allowed and could cut it off half way. Only applying settings was guarded before.
- If a run failed in an unexpected way, the activity panel stayed open with a Stop button that did nothing. If winget failed while the review window read an app's details, the licence box stayed disabled. Both now recover.
- The status glyphs in the activity panel and the card on the System page took their colors from the Windows theme, not from the theme chosen in .Clarion, so they could be the wrong colors.
- Deleting a folder in a repair job counted the size of files behind a link, and failed on a junction inside the folder on this Windows build. Links are now removed first and not counted.
- A command line apply read every setting in the catalog first, which ran winget and the feature and app queries even for a preset that uses none of them. It now reads only the settings you asked for.
- Unexpected output from the network commands is treated as no adapters, not as an error that stopped the page.
- A size of one byte reads 1 byte, not 1 bytes.

Polish
- This is version 1.0.0, the first release that is not called a beta. It is still not code signed, so Windows SmartScreen warns on first run, and every setting has been checked on one PC running Windows 11 build 26200. From here a change that breaks how people use .Clarion or the files it saves bumps the major number.
- Refreshing reads the history file once instead of several times, and the feedback preview no longer reads the log and the history on every keystroke.
- Journal lines that parse but hold no step, such as null where the step should be, are skipped and counted like a damaged line.
- The logo beside the app name is hidden from screen readers, and a test now requires every picture to have a name or be hidden.
- One symptom tip was reworded.

## 0.1.0-beta.17

New
- A Services tab on the Windows page lists every Windows service on this PC next to Microsoft's table of default start types and recommendations for 255 services. Each row shows how Windows starts the service, whether it is running and how much memory it uses, whether Windows can start it when needed, whether .Clarion protects it, and what Microsoft says about switching it off. Where .Clarion has a setting for the service, the row links to it. The page only reads and changes nothing. It says plainly that a difference from the table is not proof that somebody changed a service, because the table is written for Windows IoT Enterprise. The same table is on the documentation site, searchable, and each service setting there links to its row.
- Thirteen settings, each checked against Microsoft's own documentation and applied and reverted on a real PC before it was added. Seven switch off background services you may not use, and each says what stops working: printing support, the Downloaded Maps Manager, distributed link tracking, the Connected Devices Platform, UPnP and SSDP device discovery, Xbox Live services and smart card services. The other six are a policy that stops other computers printing through this PC while printing from it keeps working, skipping the lock screen, a clear sign-in background, turning off the Program Compatibility Assistant, preferring IPv4 over IPv6 the way Microsoft documents it, and stopping Edge sending failed addresses to a suggestion service. docs/RESEARCH-OTHER-TOOLS.md has the measurements and sources, including why a single set everything to manual tweak was not built.
- There is a documentation site at https://kkthnx.github.io/Clarion/ that explains every setting, app, repair job and cleanup row, in the same colors as the window. Each entry shows what it does, the benefit, the risk, the exact changes and the sources, and there is search and a filter by risk and suggestion. It is written from the same catalog the app runs on, so it cannot fall out of step with it. The About page links to it.

## 0.1.0-beta.16

Safer
- If a history file that another account made is set aside when .Clarion starts, Home now says so, and where the file went. Before, it was only in the log, so a person's own undo history could seem to have vanished.
- Each release file now carries a build attestation from GitHub, which is free: a signed statement that GitHub's runner built that exact file from this commit. Check one with gh attestation verify. It does not replace a Windows signature, so SmartScreen still warns, and docs/SIGNING.md sets out the options for a real one and what each costs.

Fixed
- Restarting as administrator no longer throws away the queue. Someone who picked a starting point, pressed Review and was asked to restart used to land on an empty window and a second welcome. The queue is now written down just before the restart and comes back in the new window with the review open. It is only read by the account that restarts, it is ignored after ten minutes, and it is removed if the prompt is refused.
- Putting Battle.net back after it was removed, and removing it again after that, now keep its install folder. Before, the second step failed with the same missing-folder code as the first release did.
- A scripted install with --apply now prints when an installer asks for a restart, as the window already did.
- Reading an app's icon file can no longer end the program if the file goes missing or cannot be checked.
- The apps read could be tracked wrongly after Re-check, so a Verify fix queued for an app might not wait for it. It now waits for the read that is really running.
- Stopping a run no longer fails when Windows will not let .Clarion end a child process, and it waits for the process to go before returning.
- The install folder is also checked after its variable is filled in, so a stray quote in that value cannot reach the winget command.
- Icons for per-user installs from 32-bit programs are found too.
- The Bluetooth, audio, Explorer and clock repairs now say Low risk and what can go wrong, where they began with None. The Bluetooth one warns that a keyboard or mouse that does not reconnect may need switching off and on.
- Uninstalling .Clarion removes the flag for starting without the administrator prompt, for the account that runs the uninstall.

## 0.1.0-beta.15

Polish
- The name is now .Clarion, with the leading dot, the way .NET is written. It is changed in the window title, every page, the dialogs, the command line help, the setting descriptions, the installer and its shortcuts, the readme and the other docs. The program file is still Clarion.exe, and the install folder, data folder, history and monthly check keep their names, so an update changes nothing about what is on your PC. The installer removes the old Clarion shortcuts when it upgrades. The icon, the logo in the app and the readme banner are new too. Earlier entries below still say Clarion.

## 0.1.0-beta.14

New
- Three more fixes on the Repair page, each one a step Microsoft documents: restart the Windows audio services (for an app that is silent while Windows sounds still play), restart the Bluetooth service (for devices that keep disconnecting), and resync the clock (starts the Windows Time service if it is stopped, then runs w32tm /resync). Each stopped service is started again even if a step fails.
- Restart Windows Explorer, on the Repair page, for a frozen taskbar, Start menu or desktop. It only ends Explorer, and Windows starts it again by itself, so Explorer does not come back running with administrator rights. Open File Explorer windows close.
- Show detailed startup and shutdown messages, on the Tweaks page. It sets the VerboseStatus policy, so Windows names each step instead of saying Please wait, which helps when you are chasing a slow startup or shutdown. It does not make anything faster. Microsoft's page for it names Windows Server 2003, so it was also checked on this build: winlogon.exe and wininit.exe on Windows 11 build 26200 both contain the value name.
- 60 more apps on the Apps page, 135 in all, each one an exact winget package whose record was read first. New are mail and chat (Thunderbird, Element), developer tools (Python, Java, .NET, Windows Terminal, PowerShell 7, Sublime Text, JetBrains Toolbox, WinSCP, DBeaver, GitHub CLI), game launchers (GOG Galaxy, Ubisoft Connect, Heroic, RetroArch), media tools (HandBrake, GIMP, Krita, Inkscape, FFmpeg, yt-dlp, MPC-HC, Kdenlive and more), privacy tools (KeePassXC, Cryptomator, VeraCrypt, WireGuard, OpenVPN Connect, Proton Pass), system tools (CPU-Z, HWiNFO, CrystalDiskInfo, Process Explorer, TreeSize, WinDirStat and more), cloud storage (Google Drive, Nextcloud, MEGAsync, Syncthing) and a new Office and notes group (Obsidian, Joplin, Zotero, LibreOffice, ONLYOFFICE, SumatraPDF, calibre). Spotify is left out because its installer refuses to run with administrator rights, which Clarion needs.
- More logos on the Apps page, 76 in all. A wide logo now sits in a plain tile in the larger slots, and a logo more than about twice as wide as it is tall, such as the Zoom, Go and .NET wordmarks, is not squeezed into an icon slot at all, because as a thin strip it could not be read. Those get the app's own icon or the letter tile, and behind the card they are still drawn in full.
- An app you have installed shows its own icon when Clarion has no usable logo for it. Windows lists an icon for each installed program in its Uninstall registry key, and Clarion finds that key by the code the app's winget record names, never by guessing from a name. It reads the icon file Windows already names, only from a local drive, and only exe and ico files. 104 of the 135 apps have such a code.
- Stop now stops an app install that is running, along with the installer winget started, within a fraction of a second. Before, it waited for the install to finish, which could take many minutes.

Fixed
- Battle.net failed to install with winget code 0x8A15005F. Its winget record says the installer cannot run without an install folder, so Clarion now gives it the folder the record names, Program Files (x86)\Battle.net. Winget codes for a missing install folder, an installer that refuses administrator rights, a failed or empty download and a few more now say what happened in plain words. The check of all 135 records found no other app with this requirement.
- Winget's output is read as UTF-8, so a publisher or licence name with an accented letter or a registered sign no longer shows as garbage in the review. A failed install now also names the installer's own exit code when winget prints one, and when winget cannot be read, the log says so once with a count instead of once for every app.
- Uninstalling Clarion removes the Monthly check scheduled task if you had turned it on. It used to stay behind and try to start a program that was no longer there.
- When Clarion was started from a different account than the one you are signed in as, the Apps page now says so and does not run winget, instead of listing and installing apps for the other account. When winget is there but does not answer, for example with no connection, the page and the details pane say that, and no longer say the app is not on this PC.
- If an installer or uninstaller asks for a restart, the summary after the run now says so. Nothing reported it, so the notice never appeared. The licence box in the review stays off until winget has shown the details it is accepting, and an app whose record names no homepage or licence page links to its package record. The same page listed as both http and https shows once.
- The Prefetch row is gone from Clean up. Microsoft's own engineers say clearing that folder only makes Windows rebuild it, and the sources found say boot and app starts are slower until it has. The row said it made nothing faster, so there was no reason left to offer it. Stop the diagnostic tracking services now stops DiagTrack only. The other service it touched, dmwappushservice, starts only when asked and device management such as Intune needs it, so it is left alone.

Safer
- Web links, such as the winget, licence and GitHub links, now open in your browser with your normal rights. Clarion runs as administrator, and a browser started from it could run as administrator too, with its protections weakened, just to show a page. Clarion now starts it with the token of your desktop. If your desktop itself runs as administrator, which some setups do, the browser does as well.
- Clarion no longer runs winget when it starts. Which apps are installed is read the first time you open the Apps page, or when you load a setup that lists apps. Winget may contact Microsoft's package source, and a person who never uses apps should not have that started for them every time, nor pay for it in start-up work. The readme, contributing page and Settings page say what the Apps page does on the network.
- The change journal and the restart queue are no longer trusted just because their permissions were tightened. If one of them was made by an ordinary account, or has permissions of its own, or is a link, Clarion moves it to a quarantine folder inside its data folder and starts a fresh one, and says so. Before, only its permissions were reset, so its content survived, and a later Revert would have replayed it with administrator rights. Nothing is deleted, so a file that was fine all along can be put back by hand. These files are now checked first and by name, so a crowd of other files cannot push them past the 5000 entry limit, and going past it is reported.

## 0.1.0-beta.13

New
- An Apps page installs programs with winget: 75 browsers, chat apps, developer tools, game launchers, media tools, privacy tools, system utilities and file tools. Each one is a single exact winget package, never a search, so the same entry always installs the same program. It works like every other setting: queue it, review it, apply it, and Revert uninstalls it. Verify tells you when an app you installed has been removed since.
- The review for apps shows, for each one, the version, publisher, licence, who serves the installer and its SHA-256 checksum, read from winget while the dialog is open. Apply stays off until you tick that you have read and accept the licence terms, and that covers that one run only. From the command line, apps are skipped unless you add --accept-package-agreements.
- Clarion finds winget at its normal per-user path, does not install it if it is missing, and passes winget's own source terms for you because winget stalls without them. The Apps page says so.
- App logos on the Apps page: a small logo beside each name, the same logo faded behind the card, and one beside the name in the details pane. 43 of the 75 apps have a logo from the Simple Icons project. The rest get a plain tile with the first letter. A very dark or very pale logo is drawn in the text colour so it does not vanish in the other theme. The About page credits the set.
- Brave settings on the Debloat page: turn off Brave Rewards, Wallet, VPN, AI Chat, News and Talk, and Brave's usage pings. Each is a Brave policy value, taken from Brave's own policy definitions, so the names and meaning are the vendor's. Brave locks a policy setting, so it cannot be switched back inside Brave until you revert here.
- Seven more Edge settings, each from Microsoft's Edge policy reference: stop feature tips and suggestions, hide Microsoft Rewards, skip the welcome screens, turn off Edge feedback and surveys, hide the Edge Insider promotion, stop Edge asking to be your default browser, and send Microsoft only the required Edge diagnostic data. Microsoft marks "off" for that last one as not recommended, so it sets required data only. Three Edge policies that other tools set are left out because Microsoft lists them as obsolete or deprecated, so setting them does nothing.

Polish
- Tweaks, Power, Network, Windows features and Updates are now tabs of one Windows page. With the old group open in the sidebar, Verify, What broke, Safety, System and Report a problem were pushed out of sight at the normal window size and had to be scrolled to. Every sidebar entry now fits, down to the smallest window, which is a little taller than before (640 instead of 580 device independent pixels) so the sidebar is never cut off.
- The Storage tile on Home shows the disk Windows starts from, and the System page marks it "Windows drive". Windows lists disks in no fixed order, so the tile used to show whichever came first, which could be a USB stick or a data drive. Internal disks follow, largest first, and plug-in drives come last.

Fixed
- Cleaning a report of personal details missed three things: a user folder with a space in the name kept everything after the first space (C:\Users\Jane Doe became C:\Users\[user] Doe), IPv6 addresses written in the short form with :: were left in, and the name of another PC in a network path was kept. It also replaced an account called Administrator everywhere, which turned "Running as administrator" into "Running as [user]". All of that is fixed, and a name is now replaced only as a whole word.
- A repair job could stay on "Running" after its tool had finished, when the tool left a helper process behind that kept the output open. The job now ends when the tool does, after a two second wait for the last lines.

Safer
- Service names are checked before they are put on the sc.exe command line or into a registry path, so a name could not carry extra arguments. Every name the catalog uses passes.

## 0.1.0-beta.12

New
- When a setting has been changed back twice or more, Verify now says what is worth trying. It compares the Windows version at each time the setting came back. If it came back on the same version every time, an update did not do it, so something running on this PC did, such as a workplace or school policy, another tweak tool or a scheduled task. If Windows was on a newer version each time, putting it back again is the right fix. It does not name a program, because Clarion cannot tell which one it is.

Polish
- On Home, the processor and graphics names wrap onto a second line instead of being cut off with dots, and so does the storage detail. The version number that sat next to the big title is gone, since it is already in the title bar.
- Review and apply and Clear queue only show on pages where something can be queued, or when something is already queued. System, Settings, About, What's new, Report a problem, Clean up and Repair show just Re-check.
- Clarion starts with the page list focused, not the search box, so there is no caret in a field you did not click. The first key press shows the usual focus outline.

Fixed
- Pressing "Make a restore point now" on Safety without administrator rights did nothing visible: the message stayed on "Making a restore point" for good and the error only reached the crash log. It now says that administrator rights are needed, and the button explains it before trying.
- If reading the system or applying changes failed part way, for example on an unreadable journal, the app stayed on "Reading your system" or "Applying changes" with its buttons locked. The read now always finishes, and a failed run reads your settings again and tells you.
- The check for a DNS server address accepted a trailing line break, and an address written with digits from another script made it fail with an error instead of being refused. It now takes plain digits only. An encrypted DNS address that contains an apostrophe is refused.

Safer
- Clarion's shared data folder under ProgramData is locked every time Clarion starts with administrator rights: administrators and the system get full control, ordinary accounts can only read. Windows lets any account create files there, so an ordinary account could have written the change journal before Clarion first ran with administrator rights, and a later Revert would have replayed it as an administrator. Anything already in the folder that another account owned, or gave itself rights on, goes back to the folder's permissions, and a folder that turns out to be a link is renamed and replaced. Nothing is deleted. A copy running without administrator rights can still read the folder, but cannot save settings or history there, as before.
- Notepad and System Restore are started from the Windows folder, like the other tools, instead of by name alone.
- The crash log rolls over at 1 MB like the main log, so an error that repeats cannot fill the disk.

## 0.1.0-beta.11

Polish
- Starting Clarion read the system report twice, because Home and the image notes both asked for it before the first read finished. Each read took about six seconds on a test PC, so the second only added load. They now share the read that is running, and a failed read is tried again the next time it is asked for.
- The system read asks Windows only for the facts it shows. The processor query no longer measures live load, and disks come straight from Windows storage instead of loading the storage module first. Each saved about a second here, and the answers are identical.

Fixed
- A program that started a helper and exited could hold Clarion's command output until the helper ended: 24 seconds in a test with a 3 second limit. Output is now collected as it arrives and Clarion waits two seconds after the program exits.
- The update check buttons on Settings and What's New, and the restore point button on Safety, stayed greyed out if their call failed. They now come back either way.

Safer
- The program loads its native Windows libraries from the Windows System32 folder only, never from the folder it runs from.

## 0.1.0-beta.10

Safer
- Clarion now starts Windows tools (PowerShell, Task Scheduler, the service tool, the power tool) by their full path in the Windows folder. Started by name alone, Windows looks in the program's own folder and the current folder first, and Clarion runs with administrator rights, so a file of the same name placed beside a portable copy would have run with those rights. The repair tools already did this. PowerShell is now always Windows PowerShell 5.1.
- Every app removal now says that the app's own data is removed too, and that Revert reinstalls the app but not that data. It shows in the details pane and again in the review window. This is from Microsoft's app data documentation: the system removes an app's data stores completely when the app is uninstalled.
- Removing Sticky Notes is now rated Medium risk, and no longer says that notes sync with your Microsoft account as if it were certain. Notes that were never synced live only in the app and are deleted with it.

Fixed
- The Windows Update card claimed more than Microsoft's documentation supports. The policy that removes the "Check for updates" button was reported as blocking updates, but Microsoft says background scans, downloads and installs continue as configured, so it is now a note. The policy about not contacting Microsoft's update locations was reported as a problem even with no update server, but Microsoft says it applies only when an update server is used, so it is a note unless one is set. The card also reads the "Configure Automatic Updates" option the way Microsoft defines it: it says so when a policy turns automatic updating off, or makes Windows ask before it downloads.
- A small "F5" tip popped up in odd places, which looked like a stray button. F5 (re-check) works the same, and the tip is gone. The Re-check button still says F5 in its tooltip.

Polish
- AtlasOS and ReviOS are recognized from what their own playbooks write. AtlasOS leaves "Atlas Playbook" in the OEM model and registered organization, a registry key, and folders under the Windows folder. ReviOS leaves "ReviOS" in the same places, an edition sub version, and the Revision Tool in Program Files.
- Hovering a setting, a cleanup row or a repair job no longer opens a large tooltip that repeated the details pane on the right. Select a row and the pane explains it. The "Suggested" and risk labels have a short tooltip of their own that says what the label means, which is information the pane does not give. Microsoft's own guidance is that a tooltip is not needed for what is already on screen.

## 0.1.0-beta.9

New
- A switch in Settings, "Ask for administrator rights when Clarion starts". Turn it off to start without the Windows prompt every time, which suits an account that cannot get the rights. Clarion then opens in the look and verify mode. The choice is kept in your own folder, so it works for an account that cannot write to Clarion's shared data folder, and the "Restart as administrator" button on Home takes it back.
- Verify says when a setting has been changed back more than once, for example "Changed back 3 times so far", from a small history Clarion keeps on this PC. A setting that stays changed back across many scans counts once. Nothing about it is sent anywhere.

Fixed
- Starting Clarion a second time while it was already open in the look and verify mode showed the Windows prompt before bringing the open window forward. The window that is already open is now found first, and a second launch never shows the prompt.
- The DNS speed test asked only a provider's first address, so a provider whose first address was unreachable but whose second worked was listed as not answering. It now tries the second address when the first is silent, and asks every provider at the same moment, so the worst case is a few seconds instead of about half a minute.
- The plan for putting back an app that was not installed when it was removed now says there is nothing to put back, instead of reading as if something would be restored.

## 0.1.0-beta.8

New
- Progress on the taskbar button while changes are being made, so a long run can be followed from another window. It moves as settings are done, turns yellow if you stop the run, and red if something did not finish. It goes away when you close the activity panel.
- The System page has a "Windows Update right now" card. It reads the update services, the policies that can block updates, the background scan task, any stored pause, and the dates Windows last checked and last installed, then says in plain words whether updates are working, limited or held back, with the reason for each finding. A pause stored far past the 35 days Windows allows is reported as a note when Windows itself says it has ended, because updates still install in that case. Home mentions it when updates are limited or held.

Polish
- Settings that do not apply on this PC are dimmed, and say why. An app that is not installed reads "Not installed" instead of "Nothing to change here", and the details pane explains that nothing needs removing and that the switch works again if Windows brings the app back. A new "Can be changed here" filter hides everything that does not apply.
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
