# Settings reference

This page is written from the catalog by `Clarion.exe --catalog-doc`. Do not edit it by hand. Change the catalog and write the page again.

472 settings. Each one names what it changes, how risky it is, how well the effect is documented, and where that comes from.

## Appearance

### Look and feel

#### Classic right-click menu

`appearance.classic-context-menu` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Show the full right-click menu without the extra Show more options step.

- **What it does:** Creates an empty default value under a per-user shell class key so Explorer loads the full menu directly.
- **Benefit:** One fewer click for every extended action such as Open with or Send to.
- **Risk:** Low. Explorer restarts once. Removing the key restores the compact menu.
- **Changes:**
  - `Set HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32\(Default) to  (String)`
- **Needs:** Windows build 22000 or later; File Explorer restarts

#### Dark mode

`appearance.dark-mode` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Use the dark theme for Windows and apps.

- **What it does:** Sets the app and system theme values to dark for the current user.
- **Benefit:** Less glare in dim rooms and a consistent look across apps that follow the system theme.
- **Risk:** None. Apps that ignore the system theme are not affected.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\SystemUsesLightTheme to 0 (DWord)`

#### Left-aligned taskbar

`appearance.taskbar-left` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Move the Start button and icons to the left edge.

- **What it does:** Sets the taskbar alignment value to left for the current user.
- **Benefit:** Start stays in a fixed corner as icons are added.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarAl to 0 (DWord)`
- **Needs:** Windows build 22000 or later

#### Show seconds in the taskbar clock

`appearance.clock-seconds` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Count the seconds in the clock at the end of the taskbar.

- **What it does:** Sets ShowSecondsInSystemClock to 1 for your account, the same value the checkbox Show seconds in system tray clock under Settings, Time and language, Date and time writes. Revert puts back what was there before.
- **Benefit:** The taskbar clock shows seconds, which helps when you time something or check that the clock is right.
- **Risk:** None. Windows labels the option as using more power.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\ShowSecondsInSystemClock to 1 (DWord)`
- **Needs:** Windows build 22621 or later

### Sign-in screen

#### Show a clear sign-in background

`logon.clear-background` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Remove the blur from the sign-in screen background.

- **What it does:** Sets the policy Show clear logon background, which turns off the acrylic blur behind the sign-in box.
- **Benefit:** A sharp sign-in picture instead of a blurred one.
- **Risk:** None. Only the look of the sign-in screen changes.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\System\DisableAcrylicBackgroundOnLogon to 1 (DWord)`
- **Needs:** Windows build 18362 or later; Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-admx-logon>

#### Skip the lock screen

`lock.skip-lock-screen` · Safe · Documented by the vendor · Optional: your choice · checked on build 26200

Go straight to sign-in after locking, with no lock screen to dismiss first.

- **What it does:** Sets the policy Do not display the lock screen. After you lock the PC you see your sign-in tile directly.
- **Benefit:** One fewer click or key press every time you wake or unlock the PC.
- **Risk:** None. You sign in the same way as before. Only the lock screen step is skipped.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization\NoLockScreen to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-admx-controlpaneldisplay>

### Taskbar and widgets

#### Hide the Chat button

`taskbar.chat-off` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Remove the Chat (Teams) button from the taskbar.

- **What it does:** Hides the taskbar Chat button for your account.
- **Benefit:** More room on the taskbar.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarMn to 0 (DWord)`
- **Needs:** Windows build 22000 or later; File Explorer restarts

#### Hide the Copilot button

`taskbar.copilot-button-off` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Remove the Copilot button from the taskbar.

- **What it does:** Hides the taskbar Copilot button for your account.
- **Benefit:** More room on the taskbar.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\ShowCopilotButton to 0 (DWord)`
- **Needs:** Windows build 22621 or later; File Explorer restarts

#### Hide the Task View button

`taskbar.hide-task-view` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Remove the Task View button from the taskbar.

- **What it does:** Turns off the Task View taskbar button for the current user. Win+Tab still works.
- **Benefit:** More room on the taskbar.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\ShowTaskViewButton to 0 (DWord)`
- **Needs:** File Explorer restarts

#### Hide the taskbar search box

`taskbar.hide-search` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Remove the search box or icon from the taskbar.

- **What it does:** Sets the taskbar search mode to hidden. Search still opens from the Start menu.
- **Benefit:** More room on the taskbar.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Search\SearchboxTaskbarMode to 0 (DWord)`

#### Turn off Widgets

`taskbar.widgets-policy-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Remove the Widgets board and its taskbar button.

- **What it does:** Applies the policy that disallows the widgets feature, including content on the taskbar.
- **Benefit:** No news feed in the taskbar and one less background process.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Dsh\AllowNewsAndInterests to 0 (DWord)`
- **Needs:** Windows build 22000 or later; Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-newsandinterests>

## Debloat

### Apps

#### Remove 3D Viewer

`debloat.app.microsoft3dviewer` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Uninstall 3D Viewer for your account.

- **What it does:** Removes the Microsoft.Microsoft3DViewer package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.Microsoft3DViewer for this account`

#### Remove Bing Search

`debloat.app.bingsearch` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Uninstall Bing Search for your account.

- **What it does:** Removes the Microsoft.BingSearch package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.BingSearch for this account`

#### Remove Chat (Microsoft Teams, personal)

`debloat.app.microsoftteams` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Uninstall Chat (Microsoft Teams, personal) for your account.

- **What it does:** Removes the MicrosoftTeams package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app MicrosoftTeams for this account`

#### Remove Clipchamp

`debloat.app.clipchamp.clipchamp` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Uninstall Clipchamp for your account.

- **What it does:** Removes the Clipchamp.Clipchamp package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Clipchamp.Clipchamp for this account`

#### Remove Copilot app

`debloat.app.copilot` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Uninstall Copilot app for your account.

- **What it does:** Removes the Microsoft.Copilot package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.Copilot for this account`

#### Remove Feedback Hub

`debloat.app.windowsfeedbackhub` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Feedback Hub for your account.

- **What it does:** Removes the Microsoft.WindowsFeedbackHub package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.WindowsFeedbackHub for this account`

#### Remove Get Help

`debloat.app.gethelp` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Get Help for your account.

- **What it does:** Removes the Microsoft.GetHelp package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.GetHelp for this account`

#### Remove Maps

`debloat.app.windowsmaps` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Uninstall Maps for your account.

- **What it does:** Removes the Microsoft.WindowsMaps package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.WindowsMaps for this account`

#### Remove Microsoft 365 app

`debloat.app.microsoftofficehub` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Uninstall Microsoft 365 app for your account.

- **What it does:** Removes the Microsoft.MicrosoftOfficeHub package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.MicrosoftOfficeHub for this account`

#### Remove Microsoft To Do

`debloat.app.todos` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Microsoft To Do for your account.

- **What it does:** Removes the Microsoft.Todos package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.Todos for this account`

#### Remove Mixed Reality Portal

`debloat.app.mixedreality.portal` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Uninstall Mixed Reality Portal for your account.

- **What it does:** Removes the Microsoft.MixedReality.Portal package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.MixedReality.Portal for this account`

#### Remove Movies and TV

`debloat.app.zunevideo` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Uninstall Movies and TV for your account.

- **What it does:** Removes the Microsoft.ZuneVideo package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.ZuneVideo for this account`

#### Remove News

`debloat.app.bingnews` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Uninstall News for your account.

- **What it does:** Removes the Microsoft.BingNews package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.BingNews for this account`

#### Remove Outlook (new)

`debloat.app.outlookforwindows` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Outlook (new) for your account.

- **What it does:** Removes the Microsoft.OutlookForWindows package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.OutlookForWindows for this account`

#### Remove People

`debloat.app.people` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Uninstall People for your account.

- **What it does:** Removes the Microsoft.People package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.People for this account`

#### Remove Phone Link

`debloat.app.yourphone` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Phone Link for your account.

- **What it does:** Removes the Microsoft.YourPhone package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.YourPhone for this account`

#### Remove Power Automate

`debloat.app.powerautomatedesktop` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Uninstall Power Automate for your account.

- **What it does:** Removes the Microsoft.PowerAutomateDesktop package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.PowerAutomateDesktop for this account`

#### Remove Quick Assist

`debloat.app.quickassist` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Quick Assist for your account.

- **What it does:** Removes the MicrosoftCorporationII.QuickAssist package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app MicrosoftCorporationII.QuickAssist for this account`

#### Remove Skype

`debloat.app.skypeapp` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Uninstall Skype for your account.

- **What it does:** Removes the Microsoft.SkypeApp package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.SkypeApp for this account`

#### Remove Solitaire Collection

`debloat.app.microsoftsolitairecollection` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Uninstall Solitaire Collection for your account.

- **What it does:** Removes the Microsoft.MicrosoftSolitaireCollection package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.MicrosoftSolitaireCollection for this account`

#### Remove Sticky Notes

`debloat.app.microsoftstickynotes` · Medium · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Sticky Notes for your account.

- **What it does:** Removes the Microsoft.MicrosoftStickyNotes package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Medium. Notes that are not synced to your Microsoft account are deleted with the app, and Revert cannot bring them back.
- **Changes:**
  - `Remove app Microsoft.MicrosoftStickyNotes for this account`
- **Sources:** <https://learn.microsoft.com/en-us/windows/apps/design/app-settings/store-and-retrieve-app-data>

#### Remove Tips

`debloat.app.getstarted` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Uninstall Tips for your account.

- **What it does:** Removes the Microsoft.Getstarted package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.Getstarted for this account`

#### Remove Weather

`debloat.app.bingweather` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Uninstall Weather for your account.

- **What it does:** Removes the Microsoft.BingWeather package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.BingWeather for this account`

#### Remove Xbox Game Bar

`debloat.app.xboxgamingoverlay` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Xbox Game Bar for your account.

- **What it does:** Removes the Microsoft.XboxGamingOverlay package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.XboxGamingOverlay for this account`

#### Remove Xbox app

`debloat.app.gamingapp` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Xbox app for your account.

- **What it does:** Removes the Microsoft.GamingApp package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.GamingApp for this account`

### Brave

#### Stop Brave's usage pings

`brave.usage-reports-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

No anonymous usage ping and no product analytics from Brave.

- **What it does:** Applies the two Brave policies that turn off the anonymous stats usage ping and Privacy Preserving Product Analytics (P3A).
- **Benefit:** Brave stops sending the usage ping it uses to count active users and the product analytics answers it collects.
- **Risk:** None. Brave's own text calls both anonymous. Brave may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\BraveSoftware\Brave\BraveStatsPingEnabled to 0 (DWord)`
  - `Set HKLM\SOFTWARE\Policies\BraveSoftware\Brave\BraveP3AEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/brave/brave-core/blob/master/components/policy/resources/templates/policy_definitions/BraveSoftware/BraveStatsPingEnabled.yaml>, <https://github.com/brave/brave-core/blob/master/components/policy/resources/templates/policy_definitions/BraveSoftware/BraveP3AEnabled.yaml>

#### Turn off Brave AI Chat

`brave.ai-chat-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

No built-in AI assistant in Brave.

- **What it does:** Applies the Brave policy that turns Brave's AI Chat assistant off.
- **Benefit:** Removes the assistant from the sidebar and the page summary prompts in Brave.
- **Risk:** Low. You cannot use AI Chat until you revert. Brave may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\BraveSoftware\Brave\BraveAIChatEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/brave/brave-core/blob/master/components/policy/resources/templates/policy_definitions/BraveSoftware/BraveAIChatEnabled.yaml>

#### Turn off Brave News and Brave Talk

`brave.news-talk-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

No news feed on the new tab page and no video call feature.

- **What it does:** Applies the two Brave policies that disable Brave News and Brave Talk and hide their interface.
- **Benefit:** A plain new tab page with no curated news feed, and no Talk video call button.
- **Risk:** Low. You cannot use either until you revert. Brave may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\BraveSoftware\Brave\BraveNewsDisabled to 1 (DWord)`
  - `Set HKLM\SOFTWARE\Policies\BraveSoftware\Brave\BraveTalkDisabled to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/brave/brave-core/blob/master/components/policy/resources/templates/policy_definitions/BraveSoftware/BraveNewsDisabled.yaml>, <https://github.com/brave/brave-core/blob/master/components/policy/resources/templates/policy_definitions/BraveSoftware/BraveTalkDisabled.yaml>

#### Turn off Brave Rewards

`brave.rewards-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

No Brave Rewards icon, ads or tokens.

- **What it does:** Applies the Brave policy that disables Brave Rewards, the feature that pays users for ads they see.
- **Benefit:** Removes the Rewards icon and its ad notifications from Brave.
- **Risk:** Low. You cannot turn Rewards back on from inside Brave until you revert. Brave may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\BraveSoftware\Brave\BraveRewardsDisabled to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/brave/brave-core/blob/master/components/policy/resources/templates/policy_definitions/BraveSoftware/BraveRewardsDisabled.yaml>

#### Turn off Brave VPN

`brave.vpn-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

No Brave VPN offer or button in Brave.

- **What it does:** Applies the Brave policy that disables Brave VPN, the paid VPN built into the browser.
- **Benefit:** Removes the VPN button and its upsell from Brave.
- **Risk:** Low. Do not use it if you subscribe to Brave VPN. Brave may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\BraveSoftware\Brave\BraveVPNDisabled to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/brave/brave-core/blob/master/components/policy/resources/templates/policy_definitions/BraveSoftware/BraveVPNDisabled.yaml>

#### Turn off Brave Wallet

`brave.wallet-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

No built-in crypto wallet in Brave.

- **What it does:** Applies the Brave policy that disables Brave Wallet, the built-in crypto wallet.
- **Benefit:** Removes the wallet button and its code paths from Brave.
- **Risk:** Low. Do not use it if you keep crypto in Brave Wallet, since you could not open it until you revert. Brave may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\BraveSoftware\Brave\BraveWalletDisabled to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/brave/brave-core/blob/master/components/policy/resources/templates/policy_definitions/BraveSoftware/BraveWalletDisabled.yaml>

### Microsoft Edge

#### Always send Do Not Track from Edge

`edge.do-not-track-on` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Edge tells every site that asks that you do not want to be tracked.

- **What it does:** Applies the Edge policy Configure Do Not Track with the value to always send the request to sites that ask for tracking information. Edge does not send it by default.
- **Benefit:** Every site that asks gets your request not to be tracked.
- **Risk:** None. It is only a request, and a site decides whether to honor it. Edge may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\ConfigureDoNotTrack to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/configuredonottrack>

#### Hide Microsoft Rewards in Edge

`edge.rewards-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

No Rewards points, icon or notifications in Edge.

- **What it does:** Applies the Edge policy that hides the Microsoft Rewards experience and switches its setting off.
- **Benefit:** No Rewards prompts or notifications on the new tab page and in search while you use Edge.
- **Risk:** Low. You stop collecting Rewards points through Edge. Edge may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\ShowMicrosoftRewards to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/showmicrosoftrewards>

#### Hide the Edge Insider promotion

`edge.insider-promo-off` · Safe · Look and feel only · Optional: your choice · checked on build 26200

No Edge Insider ad on Edge's About page.

- **What it does:** Applies the Edge policy that stops the About Microsoft Edge page from promoting the Edge Insider preview channels.
- **Benefit:** A cleaner About page. It changes nothing else in Edge.
- **Risk:** None. Edge may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\MicrosoftEdgeInsiderPromotionEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/microsoftedgeinsiderpromotionenabled>

#### Hide the Edge sidebar

`edge.sidebar` · Safe · Documented by the vendor · Optional: your choice · checked on build 26200

Remove the launcher bar on the right side of Edge.

- **What it does:** Applies the Edge policy that never shows the sidebar.
- **Benefit:** More room for pages and fewer built in shortcuts and promotions.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\HubsSidebarEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/hubssidebarenabled>

#### Send Microsoft only the required Edge data

`edge.diagnostic-data-required` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Optional browsing and crash data stays on your PC.

- **What it does:** Applies the Edge policy that sets diagnostic data to required data only, which turns off the optional data.
- **Benefit:** Edge stops sending the optional data about how you use the browser, the sites you visit and crash reports.
- **Risk:** None for security. Required data is what Microsoft says keeps Edge secure, up to date and performing. Edge may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\DiagnosticData to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/diagnosticdata>

#### Skip Edge's welcome screens

`edge.first-run-off` · Low · Look and feel only · Optional: your choice · checked on build 26200

No first-run tour or splash screen for a new Edge profile.

- **What it does:** Applies the Edge policy that hides the first-run experience and the splash screen.
- **Benefit:** A new Edge profile opens straight to the browser, with no tour or sign-in and sync pitch.
- **Risk:** Low. Sync stays off until you turn it on, and Edge asks about it when it starts.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\HideFirstRunExperience to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/hidefirstrunexperience>

#### Stop Edge asking to be your default

`edge.default-browser-prompt-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

No prompts to make Edge the default browser and Bing the default search.

- **What it does:** Applies the Edge policy that turns off the default browser settings campaign.
- **Benefit:** Edge stops asking to become your default browser and to make Bing your default search engine.
- **Risk:** None. Your current default browser and search engine are not changed. Edge may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\DefaultBrowserSettingsCampaignEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/defaultbrowsersettingscampaignenabled>

#### Stop Edge feature tips and suggestions

`edge.recommendations-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

No pop-ups that suggest Edge features.

- **What it does:** Applies the Edge policy that turns off feature recommendations and browser assistance notifications.
- **Benefit:** No dialogs, flyouts or banners nudging you to try Edge features, such as vertical tabs when many tabs are open.
- **Risk:** None. Every feature is still in Edge settings. Edge may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\ShowRecommendationsEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/showrecommendationsenabled>

#### Stop Edge fetching feature assets from Microsoft

`edge.asset-delivery-off` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Edge features can no longer download their files from the Asset Delivery Service.

- **What it does:** Applies the Edge policy that disables the Asset Delivery Service. Microsoft describes it as a delivery pipeline whose payloads can be configuration files or machine learning models for Edge features.
- **Benefit:** Edge stops fetching those files for its features in the background.
- **Risk:** Low. Microsoft says features that depend on the service cannot download what they need to run correctly, so some Edge features may stop working.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\EdgeAssetDeliveryServiceEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-browser-policies/edgeassetdeliveryserviceenabled>

#### Stop Edge from personalizing ads with your browsing

`edge.personalization` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Edge will not send browsing data to Microsoft to personalize ads, search and news.

- **What it does:** Applies the Edge policy that stops sending browsing history, favorites and usage to Microsoft for personalization.
- **Benefit:** Your browsing is not used to personalize ads and services.
- **Risk:** None. Ads are not removed, they are just less personalized.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\PersonalizationReportingEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/personalizationreportingenabled>

#### Stop Edge from starting with Windows

`edge.startup-boost` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Edge will not preload itself at sign in.

- **What it does:** Applies the Edge policy that turns startup boost off.
- **Benefit:** Less memory use at sign in and a lighter background footprint.
- **Risk:** Low. Edge may take a moment longer to open the first time.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\StartupBoostEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/startupboostenabled>

#### Stop Edge running after you close it

`edge.background` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Edge will fully quit when you close the last window.

- **What it does:** Applies the Edge policy that turns background mode off.
- **Benefit:** No Edge process or tray icon after you close it.
- **Risk:** Low. Web apps and notifications from Edge stop when it is closed.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\BackgroundModeEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/backgroundmodeenabled>

#### Stop Edge suggesting pages for failed addresses

`edge.error-suggestions-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Edge stops sending a mistyped or unreachable address to a web service for suggestions.

- **What it does:** Applies the Edge policy Suggest similar pages when a webpage can't be found, set to off.
- **Benefit:** Addresses that fail to load are not sent to a suggestion service.
- **Risk:** None. You just see the plain error page. Edge may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\AlternateErrorPagesEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/alternateerrorpagesenabled>

#### Turn off Edge feedback and surveys

`edge.feedback-off` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Edge can no longer open its feedback tool.

- **What it does:** Applies the Edge policy that disables Edge Feedback, which Edge uses for suggestions, issue reports and customer surveys.
- **Benefit:** No feedback or survey requests from inside Edge.
- **Risk:** None. You can still report an Edge problem through Windows Feedback Hub. Edge may show a managed by your organization notice while any policy is set.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\UserFeedbackAllowed to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/userfeedbackallowed>

#### Turn off Edge shopping features

`edge.shopping` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

No price comparison, coupons or checkout autofill banners.

- **What it does:** Applies the Edge policy that disables the shopping assistant.
- **Benefit:** No banners about coupons and prices, and fewer requests to shopping servers about the pages you visit.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Edge\EdgeShoppingAssistantEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/edgeshoppingassistantenabled>

### Outlook

#### Hide the Try the new Outlook switch in classic Outlook

`office.new-outlook-toggle-hidden` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Remove the switch that moves you to the new Outlook.

- **What it does:** Sets HideNewOutlookToggle to 1 in the classic Outlook options for your account, which Microsoft documents for hiding the Try the new Outlook switch. Only classic Outlook reads it. Revert deletes the value, and the switch comes back the next time classic Outlook starts.
- **Benefit:** Classic Outlook no longer offers the switch, so nobody moves to the new Outlook by accident.
- **Risk:** Low. The new Outlook stays installed and opens from its own icon. Microsoft says that if you already use it, opening classic Outlook switches you back.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Office\16.0\Outlook\Options\General\HideNewOutlookToggle to 1 (DWord)`
- **Sources:** <https://learn.microsoft.com/en-us/exchange/clients-and-mobile-in-exchange-online/outlook-on-the-web/enable-disable-employee-access-new-outlook>, <https://learn.microsoft.com/en-us/microsoft-365-apps/outlook/get-started/control-install>

#### Stop Outlook moving you to the new Outlook by itself

`office.new-outlook-auto-move-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Block the automatic switch from classic Outlook to the new Outlook.

- **What it does:** Sets the policy Manage user setting for new Outlook automatic migration to 0 for your account. Microsoft says this turns the setting for automatic migration off and stops users changing it. Revert deletes the policy value.
- **Benefit:** Classic Outlook stays your Outlook until you choose otherwise.
- **Risk:** Low. Microsoft says the policy does not apply to a migration that an administrator starts with the admin controlled migration policy.
- **Changes:**
  - `Set HKCU\Software\Policies\Microsoft\office\16.0\outlook\preferences\NewOutlookMigrationUserSetting to 0 (DWord)`
- **Sources:** <https://learn.microsoft.com/en-us/microsoft-365-apps/outlook/get-started/control-install>

### Suggestions and ads

#### Hide Start recommendations

`ads.start-recommendations-off` · Safe · Helps in some cases · Suggested: turn on · checked on build 26200

Remove tips, shortcuts and suggested apps from the Start menu.

- **What it does:** Turns off recommendations and account notices in the Start menu for your account.
- **Benefit:** A cleaner Start menu without suggested apps and promotions.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Start_IrisRecommendations to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Start_AccountNotifications to 0 (DWord)`
- **Needs:** Windows build 22621 or later

#### Hide promotions in File Explorer

`ads.explorer-promos-off` · Safe · Helps in some cases · Suggested: turn on · checked on build 26200

Stop sync provider notices and offers inside File Explorer.

- **What it does:** Turns off sync provider notifications in File Explorer for your account.
- **Benefit:** No OneDrive and Microsoft 365 offers in the folder view.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\ShowSyncProviderNotifications to 0 (DWord)`

#### No Spotlight or tips on the lock screen

`ads.lockscreen-tips-off` · Safe · Helps in some cases · Suggested: turn on · checked on build 26200

Stop the rotating lock screen pictures and tips for your account.

- **What it does:** Turns off the rotating lock screen and its tips for your account.
- **Benefit:** The lock screen stays a plain picture with no promotions.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\RotatingLockScreenEnabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\RotatingLockScreenOverlayEnabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-338387Enabled to 0 (DWord)`

#### No third party suggestions in Spotlight

`ads.third-party-suggestions-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Stop Windows from suggesting other companies content.

- **What it does:** Applies the per account policy that stops third party content suggestions.
- **Benefit:** No ads for other companies apps and services in Spotlight.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Policies\Microsoft\Windows\CloudContent\DisableThirdPartySuggestions to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-experience>

#### Stop online tips in Settings

`ads.settings-tips-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

The Settings app will not fetch tips and help from Microsoft.

- **What it does:** Applies the policy that stops Settings from retrieving online tips and help content.
- **Benefit:** Settings works offline and does not contact Microsoft for tips.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\AllowOnlineTips to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-settings>

#### Stop promoted app installs and Start suggestions

`debloat.consumer-features-policy` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Turn off Microsoft consumer experiences through policy.

- **What it does:** Applies the Cloud Content policy that blocks Start suggestions, post-setup app installs and promoted tiles.
- **Benefit:** No surprise sponsored apps after updates or on new accounts.
- **Risk:** None. Windows only honors this policy on Enterprise and Education, so it is hidden on Home and Pro. Use the per user version there.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent\DisableWindowsConsumerFeatures to 1 (DWord)`
- **Needs:** Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-experience>

#### Stop suggestions and silent app installs (this account)

`debloat.content-delivery-user` · Safe · Helps in some cases · Suggested: turn on · checked on build 26200

Turn off tips, suggested apps and silent installs for the current user.

- **What it does:** Sets the per user content delivery values that feed Start suggestions, lock screen tips and silent app installs to off.
- **Benefit:** Works on every edition, including Home and Pro, where the policy version is ignored.
- **Risk:** None. These are per account preferences and are not documented as a supported policy, so Windows can reset them after a feature update. .Clarion detects that and offers to reapply.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SilentInstalledAppsEnabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SystemPaneSuggestionsEnabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\OemPreInstalledAppsEnabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\PreInstalledAppsEnabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-338388Enabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-338389Enabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-353694Enabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-353696Enabled to 0 (DWord)`

#### Turn off Windows Spotlight everywhere

`ads.spotlight-off` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Remove Spotlight pictures, tips and suggestions from the lock screen, desktop and Settings.

- **What it does:** Applies the Spotlight policies for the lock screen, desktop, Settings, notifications and the welcome experience.
- **Benefit:** No rotating pictures with promotional tips, and no Spotlight content in Settings.
- **Risk:** Low. The lock screen uses a fixed picture instead of Spotlight.
- **Changes:**
  - `Set HKCU\Software\Policies\Microsoft\Windows\CloudContent\DisableWindowsSpotlightFeatures to 1 (DWord)`
  - `Set HKCU\Software\Policies\Microsoft\Windows\CloudContent\DisableSpotlightCollectionOnDesktop to 1 (DWord)`
  - `Set HKCU\Software\Policies\Microsoft\Windows\CloudContent\DisableWindowsSpotlightOnSettings to 1 (DWord)`
  - `Set HKCU\Software\Policies\Microsoft\Windows\CloudContent\DisableWindowsSpotlightOnActionCenter to 1 (DWord)`
  - `Set HKCU\Software\Policies\Microsoft\Windows\CloudContent\DisableWindowsSpotlightWindowsWelcomeExperience to 1 (DWord)`
- **Needs:** Enterprise, Education, IoTEnterprise
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-experience>

#### Turn off Windows tips

`ads.windows-tips-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Stop Windows from showing tips, tricks and suggestions.

- **What it does:** Applies the policy that turns off Windows tips.
- **Benefit:** No tip pop-ups or notifications that push features.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\CloudContent\DisableSoftLanding to 1 (DWord)`
- **Needs:** Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-experience>

## Explorer

### File Explorer

#### End task from the taskbar menu

`explorer.end-task-menu` · Safe · Look and feel only · Suggested: turn on · checked on build 26200

Add End task to the right-click menu of taskbar apps.

- **What it does:** Turns on the developer setting that shows End task on taskbar items.
- **Benefit:** Kill a stuck app in two clicks without opening Task Manager.
- **Risk:** None. Unsaved work in the app is lost when you use it.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings\TaskbarEndTask to 1 (DWord)`
- **Needs:** Windows build 22631 or later

#### Open File Explorer to This PC

`explorer.open-to-this-pc` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Start in drives and folders instead of Home.

- **What it does:** Sets the Explorer launch target to This PC.
- **Benefit:** Straight to your drives, no recent files list.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\LaunchTo to 1 (DWord)`

#### Show file extensions

`explorer.show-extensions` · Safe · Look and feel only · Suggested: turn on · checked on build 26200

Always show the end of file names, like .exe or .pdf.

- **What it does:** Turns off the setting that hides known file extensions.
- **Benefit:** Makes disguised files such as invoice.pdf.exe easy to spot.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\HideFileExt to 0 (DWord)`

#### Show hidden files

`explorer.show-hidden-files` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Show files and folders marked hidden.

- **What it does:** Sets the Explorer hidden file view option to show.
- **Benefit:** See folders like AppData without changing view options each time.
- **Risk:** None. Be careful deleting files you do not recognize.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Hidden to 1 (DWord)`

## Features

### Windows features

#### .NET Framework 3.5

`features.dotnet35` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Support for older programs built on .NET 2.0 to 3.5.

- **What it does:** Turns on .NET Framework 3.5, which includes 2.0 and 3.0.
- **Benefit:** Lets older apps and installers that ask for .NET 3.5 run.
- **Risk:** Low. Windows may need to download files through Windows Update, so an internet connection is needed.
- **Changes:**
  - `Turn on Windows feature NetFx3`
- **Needs:** administrator rights, applies to every account

#### Hyper-V virtual machines

`features.hyper-v` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Run virtual machines with the built in hypervisor.

- **What it does:** Turns on all Hyper-V components, including the hypervisor, management tools and services.
- **Benefit:** Create and run virtual machines with no extra software.
- **Risk:** Low. Windows then runs on top of the hypervisor, which a few older emulators and some anti-cheat or VR tools do not support.
- **Changes:**
  - `Turn on Windows feature Microsoft-Hyper-V-All`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education; administrator rights, applies to every account; a restart

#### Legacy game components

`features.legacy-games` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Support for very old games that use DirectPlay.

- **What it does:** Turns on Legacy Components and DirectPlay.
- **Benefit:** Lets a small number of games from the late 1990s and 2000s start and use their networking.
- **Risk:** Low.
- **Changes:**
  - `Turn on Windows feature LegacyComponents`
  - `Turn on Windows feature DirectPlay`
- **Needs:** administrator rights, applies to every account

#### NFS client

`features.nfs-client` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Connect to Network File System shares from Linux and NAS devices.

- **What it does:** Turns on the client side of Services for NFS.
- **Benefit:** Mount shares from NFS servers as drives.
- **Risk:** Low.
- **Changes:**
  - `Turn on Windows feature ServicesForNFS-ClientOnly`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education; administrator rights, applies to every account

#### OpenSSH server

`features.openssh-server` · Medium · Helps in some cases · Only if it fits you · checked on build 26200

Let other computers sign in to this PC over SSH.

- **What it does:** Installs the OpenSSH server component. It does not start the service for you.
- **Benefit:** Remote command line access and file copy over an encrypted connection.
- **Risk:** Medium. Once started it is a remote sign in service. Use key based sign in and a firewall.
- **Changes:**
  - `Install Windows capability OpenSSH.Server~~~~0.0.1.0`
- **Needs:** administrator rights, applies to every account

#### Windows Sandbox

`features.sandbox` · Low · Helps in some cases · Optional: your choice · checked on build 26200

A throwaway Windows desktop for testing unknown files.

- **What it does:** Turns on the disposable desktop environment. Everything inside is erased when the window closes.
- **Benefit:** Open a suspicious download or try an installer without risking your real system.
- **Risk:** Low. Needs a restart and virtualization support.
- **Changes:**
  - `Turn on Windows feature Containers-DisposableClientVM`
- **Needs:** Windows build 18362 or later; Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education; administrator rights, applies to every account; a restart

#### Windows Subsystem for Linux

`features.wsl` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Run Linux command line tools inside Windows.

- **What it does:** Turns on the Windows Subsystem for Linux and the Virtual Machine Platform it needs for version 2.
- **Benefit:** Use Linux shells, package managers and developer tools without a separate virtual machine.
- **Risk:** Low. Needs a restart. Hardware virtualization must be enabled in the firmware.
- **Changes:**
  - `Turn on Windows feature Microsoft-Windows-Subsystem-Linux`
  - `Turn on Windows feature VirtualMachinePlatform`
- **Needs:** Windows build 19041 or later; administrator rights, applies to every account; a restart

## Input

### Mouse and keyboard

#### Instant menus

`input.menu-show-delay-zero` · Safe · Look and feel only · Optional: your choice · checked on build 26200

Open cascading menus without the short delay.

- **What it does:** Sets the menu show delay to 0 milliseconds. The default is 400.
- **Benefit:** Menus feel snappier.
- **Risk:** None. Sign out to apply.
- **Changes:**
  - `Set HKCU\Control Panel\Desktop\MenuShowDelay to 0 (String)`
- **Needs:** signing out

#### Turn off mouse acceleration

`input.mouse-accel-off` · Safe · Look and feel only · Only if it fits you · checked on build 26200

Make pointer distance match hand distance.

- **What it does:** Sets the pointer acceleration and both threshold values to 0 for the current user.
- **Benefit:** Predictable aim and cursor control, which matters most in games and design tools.
- **Risk:** Pointer feels slower on fast flicks until you raise the speed setting. Sign out to apply.
- **Changes:**
  - `Set HKCU\Control Panel\Mouse\MouseSpeed to 0 (String)`
  - `Set HKCU\Control Panel\Mouse\MouseThreshold1 to 0 (String)`
  - `Set HKCU\Control Panel\Mouse\MouseThreshold2 to 0 (String)`
- **Needs:** signing out

#### Turn off the Sticky Keys shortcut

`input.sticky-keys-prompt-off` · Safe · Look and feel only · Suggested: turn on · checked on build 26200

Stop the pop-up when Shift is pressed five times.

- **What it does:** Clears the keyboard shortcut flag that launches the Sticky Keys prompt.
- **Benefit:** No interruptions in games and fast typing.
- **Risk:** None. Sticky Keys can still be turned on from Settings.
- **Changes:**
  - `Turn bit 0x4 off in HKCU\Control Panel\Accessibility\StickyKeys\Flags (String)`
- **Needs:** signing out

## Install

### Browsers

#### Brave

`app.brave` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A browser that blocks ads and trackers by default.

- **What it does:** Installs Brave with winget, pinned to the package Brave.Brave in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Brave without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Brave.Brave with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/Brave/Brave>, <https://brave.com/download>, <https://github.com/brave/brave-browser/blob/master/LICENSE>

#### Chromium (Hibbiki build)

`app.chromium-hibbiki-build` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

The open source Chromium browser, in builds made by Hibbiki.

- **What it does:** Installs Chromium (Hibbiki build) with winget, pinned to the package Hibbiki.Chromium in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Chromium (Hibbiki build) without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. These builds are made by a person or group other than the project that makes the program, so you are trusting them as well. Read the licence terms on the review screen.
- **Changes:**
  - `Install Hibbiki.Chromium with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/h/Hibbiki/Chromium>, <https://github.com/Hibbiki/chromium-win64>, <https://chromium.googlesource.com/chromium/src.git/+/refs/heads/master/LICENSE>

#### Firefox

`app.firefox` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Mozilla's open source web browser.

- **What it does:** Installs Firefox with winget, pinned to the package Mozilla.Firefox in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Firefox without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Mozilla.Firefox with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Mozilla/Firefox>, <https://www.mozilla.org/firefox/>, <https://www.mozilla.org/MPL/2.0>

#### Firefox ESR

`app.firefox-esr` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The Extended Support Release of Firefox, which changes less often.

- **What it does:** Installs Firefox ESR with winget, pinned to the package Mozilla.Firefox.ESR in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Firefox ESR without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Mozilla.Firefox.ESR with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Mozilla/Firefox/ESR>, <https://www.mozilla.org/firefox/enterprise/>, <https://www.mozilla.org/MPL/2.0>

#### Floorp

`app.floorp` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Firefox based browser with extra tab and layout options.

- **What it does:** Installs Floorp with winget, pinned to the package Ablaze.Floorp in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Floorp without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Ablaze.Floorp with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Ablaze/Floorp>, <https://floorp.app/en>, <https://github.com/Floorp-Projects/Floorp/blob/main/LICENSE/>

#### Google Chrome

`app.chrome` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Google's web browser.

- **What it does:** Installs Google Chrome with winget, pinned to the package Google.Chrome in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Google Chrome without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Google.Chrome with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/Google/Chrome>, <https://www.google.com/chrome/>, <https://www.google.com/chrome/terms/>

#### Helium

`app.helium` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Chromium based browser with a plain interface.

- **What it does:** Installs Helium with winget, pinned to the package ImputNet.Helium in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Helium without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install ImputNet.Helium with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/i/ImputNet/Helium>, <https://helium.computer/>, <https://github.com/imputnet/helium/blob/HEAD/LICENSE>

#### LibreWolf

`app.librewolf` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Firefox based browser with privacy hardening already switched on.

- **What it does:** Installs LibreWolf with winget, pinned to the package LibreWolf.LibreWolf in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get LibreWolf without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install LibreWolf.LibreWolf with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/l/LibreWolf/LibreWolf>, <https://librewolf.net/>, <https://librewolf.net/license-disclaimers/>

#### Microsoft Edge

`app.microsoft-edge` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Microsoft's browser, which Windows already includes.

- **What it does:** Installs Microsoft Edge with winget, pinned to the package Microsoft.Edge in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Microsoft Edge without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Windows treats Edge as part of the system, so its uninstaller may refuse to run. If it does run, links and some Windows features fall back to another browser.
- **Changes:**
  - `Install Microsoft.Edge with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/Edge>, <https://www.microsoft.com/en-us/edge>, <https://www.microsoft.com/en-us/servicesagreement>

#### Mullvad Browser

`app.mullvad-browser` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A browser made with the Tor Project to limit tracking and fingerprinting.

- **What it does:** Installs Mullvad Browser with winget, pinned to the package MullvadVPN.MullvadBrowser in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Mullvad Browser without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install MullvadVPN.MullvadBrowser with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/MullvadVPN/MullvadBrowser>, <https://mullvad.net/browser>, <https://github.com/mullvad/mullvad-browser/blob/14.0.7/LICENSE>

#### Tor Browser

`app.tor-browser` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A browser that sends its traffic through the Tor network.

- **What it does:** Installs Tor Browser with winget, pinned to the package TorProject.TorBrowser in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Tor Browser without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install TorProject.TorBrowser with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/TorProject/TorBrowser>, <https://www.torproject.org/>, <https://support.torproject.org/about/distribute-tor/>

#### Ungoogled Chromium

`app.ungoogled-chromium` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Chromium with Google's web services taken out.

- **What it does:** Installs Ungoogled Chromium with winget, pinned to the package eloston.ungoogled-chromium in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Ungoogled Chromium without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install eloston.ungoogled-chromium with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/e/eloston/ungoogled-chromium>, <https://github.com/ungoogled-software/ungoogled-chromium-windows/releases>, <https://github.com/ungoogled-software/ungoogled-chromium-windows/blob/HEAD/LICENSE>

#### Vivaldi

`app.vivaldi` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Chromium based browser with a lot of layout options.

- **What it does:** Installs Vivaldi with winget, pinned to the package Vivaldi.Vivaldi in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Vivaldi without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Vivaldi.Vivaldi with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/v/Vivaldi/Vivaldi>, <https://vivaldi.com/desktop/>, <https://vivaldi.com/privacy/vivaldi-end-user-license-agreement/>

#### Waterfox

`app.waterfox` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A privacy focused browser built on Firefox.

- **What it does:** Installs Waterfox with winget, pinned to the package Waterfox.Waterfox in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Waterfox without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Waterfox.Waterfox with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/Waterfox/Waterfox>, <https://www.waterfox.net/>, <https://github.com/BrowserWorks/waterfox/blob/HEAD/toolkit/content/license.html>

#### Zen Browser

`app.zen-browser` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Firefox based browser with a tidy, customisable layout.

- **What it does:** Installs Zen Browser with winget, pinned to the package Zen-Team.Zen-Browser in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Zen Browser without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Zen-Team.Zen-Browser with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/z/Zen-Team/Zen-Browser>, <https://zen-browser.app/>, <https://github.com/zen-browser/desktop/blob/HEAD/LICENSE>

### Communication

#### Betterbird

`app.betterbird` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A fork of Thunderbird with extra features and fixes.

- **What it does:** Installs Betterbird with winget, pinned to the package Betterbird.Betterbird in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Betterbird without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Betterbird.Betterbird with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/Betterbird/Betterbird>, <https://www.betterbird.eu/downloads/>, <https://www.betterbird.eu/legal/>

#### Chatterino

`app.chatterino` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A chat client for Twitch.

- **What it does:** Installs Chatterino with winget, pinned to the package ChatterinoTeam.Chatterino in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Chatterino without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install ChatterinoTeam.Chatterino with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/ChatterinoTeam/Chatterino>, <https://github.com/Chatterino/chatterino2>, <https://github.com/Chatterino/chatterino2/blob/HEAD/LICENSE>

#### Discord

`app.discord` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Voice, video and text chat for friends and communities.

- **What it does:** Installs Discord with winget, pinned to the package Discord.Discord in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Discord without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Discord.Discord with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/Discord/Discord>, <https://discord.com/download>, <https://discord.com/terms>

#### Dorion

`app.dorion` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

A lightweight alternative Discord client.

- **What it does:** Installs Dorion with winget, pinned to the package SpikeHD.Dorion in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Dorion without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Discord's terms do not allow modified clients, so using one can put your account at risk.
- **Changes:**
  - `Install SpikeHD.Dorion with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/SpikeHD/Dorion>, <https://github.com/SpikeHD/Dorion>, <https://github.com/SpikeHD/Dorion/blob/HEAD/LICENSE>

#### Element

`app.element` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A chat app for the open Matrix network.

- **What it does:** Installs Element with winget, pinned to the package Element.Element in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Element without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Element.Element with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/e/Element/Element>, <https://element.io/download>, <https://github.com/element-hq/element-desktop/blob/HEAD/LICENSE-GPL-3.0>

#### Microsoft Teams

`app.microsoft-teams` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Microsoft's chat and meeting app.

- **What it does:** Installs Microsoft Teams with winget, pinned to the package Microsoft.Teams in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Microsoft Teams without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.Teams with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/Teams>, <https://www.microsoft.com/microsoft-teams/group-chat-software>, <https://www.microsoft.com/legal/terms-of-use>

#### Proton Mail

`app.proton-mail` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The desktop app for the encrypted Proton Mail service.

- **What it does:** Installs Proton Mail with winget, pinned to the package Proton.ProtonMail in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Proton Mail without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Proton.ProtonMail with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Proton/ProtonMail>, <https://proton.me/mail>, <https://proton.me/legal/terms>

#### Signal

`app.signal` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A messenger with end to end encryption.

- **What it does:** Installs Signal with winget, pinned to the package OpenWhisperSystems.Signal in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Signal without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install OpenWhisperSystems.Signal with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/OpenWhisperSystems/Signal>, <https://www.signal.org/>, <https://github.com/signalapp/Signal-Desktop/blob/HEAD/LICENSE>

#### Slack

`app.slack` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Team chat for work.

- **What it does:** Installs Slack with winget, pinned to the package SlackTechnologies.Slack in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Slack without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install SlackTechnologies.Slack with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/SlackTechnologies/Slack>, <https://slack.com/>, <https://slack.com/terms-of-service/user>

#### TeamSpeak 3

`app.teamspeak-3` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A voice chat program used by gaming groups.

- **What it does:** Installs TeamSpeak 3 with winget, pinned to the package TeamSpeakSystems.TeamSpeakClient in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get TeamSpeak 3 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install TeamSpeakSystems.TeamSpeakClient with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/TeamSpeakSystems/TeamSpeakClient>, <https://www.teamspeak.com/en/>, <https://www.teamspeak.com/en/features/licensing/>

#### TeamSpeak 6 (beta)

`app.teamspeak-6-beta` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The next TeamSpeak, still in beta.

- **What it does:** Installs TeamSpeak 6 (beta) with winget, pinned to the package TeamSpeakSystems.TeamSpeakClient.Beta.6 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get TeamSpeak 6 (beta) without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install TeamSpeakSystems.TeamSpeakClient.Beta.6 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/TeamSpeakSystems/TeamSpeakClient/Beta/6>, <https://teamspeak.com/en/downloads/#ts6client>, <https://www.teamspeak.com/en/features/licensing/>

#### Telegram

`app.telegram` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A cloud based messenger.

- **What it does:** Installs Telegram with winget, pinned to the package Telegram.TelegramDesktop in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Telegram without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Telegram.TelegramDesktop with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/Telegram/TelegramDesktop>, <https://desktop.telegram.org/>, <https://raw.githubusercontent.com/telegramdesktop/tdesktop/master/LICENSE>

#### Thunderbird

`app.thunderbird` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Mozilla's email app, with calendar and contacts.

- **What it does:** Installs Thunderbird with winget, pinned to the package Mozilla.Thunderbird in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Thunderbird without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Mozilla.Thunderbird with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Mozilla/Thunderbird>, <https://www.thunderbird.net/>, <https://www.mozilla.org/MPL/2.0>

#### Vesktop

`app.vesktop` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

A Discord client with Vencord built in.

- **What it does:** Installs Vesktop with winget, pinned to the package Vencord.Vesktop in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Vesktop without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Discord's terms do not allow modified clients, so using one can put your account at risk.
- **Changes:**
  - `Install Vencord.Vesktop with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/v/Vencord/Vesktop>, <https://github.com/Vencord/Vesktop>, <https://github.com/Vencord/Vesktop/blob/HEAD/LICENSE>

#### Viber

`app.viber` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A messaging and calling app.

- **What it does:** Installs Viber with winget, pinned to the package Rakuten.Viber in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Viber without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Rakuten.Viber with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/Rakuten/Viber>, <https://www.viber.com/>, <https://www.viber.com/terms/viber-terms-use/>

#### Zoom

`app.zoom` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Video meetings and calls.

- **What it does:** Installs Zoom with winget, pinned to the package Zoom.Zoom in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Zoom without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Zoom.Zoom with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/z/Zoom/Zoom>, <https://zoom.us/>, <https://explore.zoom.us/en/terms/>

#### qTox

`app.qtox` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A chat and video client for the Tox peer to peer network.

- **What it does:** Installs qTox with winget, pinned to the package Tox.qTox in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get qTox without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Tox.qTox with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/Tox/qTox>, <https://github.com/TokTok/qTox>, <https://github.com/TokTok/qTox/blob/HEAD/LICENSE>

### Development

#### .NET 9 SDK

`app.dotnet-sdk` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Microsoft's kit for building .NET 9 apps.

- **What it does:** Installs .NET 9 SDK with winget, pinned to the package Microsoft.DotNet.SDK.9 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get .NET 9 SDK without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.DotNet.SDK.9 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/DotNet/SDK/9>, <https://dotnet.microsoft.com>

#### Bruno

`app.bruno` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Git friendly, open source API client.

- **What it does:** Installs Bruno with winget, pinned to the package Bruno.Bruno in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Bruno without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Bruno.Bruno with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/Bruno/Bruno>, <https://www.usebruno.com/>, <https://github.com/usebruno/bruno/blob/HEAD/license.md>

#### CMake

`app.cmake` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A build system generator for C and C++ projects.

- **What it does:** Installs CMake with winget, pinned to the package Kitware.CMake in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get CMake without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Kitware.CMake with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/k/Kitware/CMake>, <https://cmake.org/>, <https://github.com/Kitware/CMake/blob/HEAD/LICENSE.rst>

#### Claude

`app.claude` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Anthropic's desktop app for its Claude assistant.

- **What it does:** Installs Claude with winget, pinned to the package Anthropic.Claude in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Claude without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. The program sends what you type to the publisher's service, under their terms. Read them on the review screen.
- **Changes:**
  - `Install Anthropic.Claude with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Anthropic/Claude>

#### Claude Code

`app.claude-code` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Anthropic's coding assistant for the command line.

- **What it does:** Installs Claude Code with winget, pinned to the package Anthropic.ClaudeCode in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Claude Code without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. The program sends what you type to the publisher's service, under their terms. Read them on the review screen.
- **Changes:**
  - `Install Anthropic.ClaudeCode with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Anthropic/ClaudeCode>

#### Codex CLI

`app.codex-cli` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

OpenAI's coding assistant for the command line.

- **What it does:** Installs Codex CLI with winget, pinned to the package OpenAI.Codex in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Codex CLI without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. The program sends what you type to the publisher's service, under their terms. Read them on the review screen.
- **Changes:**
  - `Install OpenAI.Codex with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/OpenAI/Codex>, <https://github.com/openai/codex>, <https://github.com/openai/codex/blob/HEAD/LICENSE>

#### Cursor

`app.cursor` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A code editor built on VS Code with AI features.

- **What it does:** Installs Cursor with winget, pinned to the package Anysphere.Cursor in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Cursor without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. The program sends what you type to the publisher's service, under their terms. Read them on the review screen.
- **Changes:**
  - `Install Anysphere.Cursor with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Anysphere/Cursor>, <https://www.cursor.com/>, <https://www.cursor.com/terms-of-service>

#### DBeaver Community

`app.dbeaver` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A client for working with many kinds of databases.

- **What it does:** Installs DBeaver Community with winget, pinned to the package DBeaver.DBeaver.Community in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get DBeaver Community without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install DBeaver.DBeaver.Community with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/DBeaver/DBeaver/Community>, <https://dbeaver.io/download/>, <https://github.com/dbeaver/dbeaver/blob/HEAD/LICENSE.md>

#### Docker Desktop

`app.docker-desktop` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Runs containers on Windows.

- **What it does:** Installs Docker Desktop with winget, pinned to the package Docker.DockerDesktop in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Docker Desktop without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Docker.DockerDesktop with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/Docker/DockerDesktop>, <https://www.docker.com/products/docker-desktop>, <https://www.docker.com/legal/docker-subscription-service-agreement>

#### Fast Node Manager (fnm)

`app.fast-node-manager-fnm` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A fast Node.js version manager.

- **What it does:** Installs Fast Node Manager (fnm) with winget, pinned to the package Schniz.fnm in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Fast Node Manager (fnm) without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Schniz.fnm with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/Schniz/fnm>, <https://github.com/Schniz/fnm>, <https://github.com/Schniz/fnm/blob/HEAD/LICENSE>

#### Git

`app.git` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The version control system, with Git Bash.

- **What it does:** Installs Git with winget, pinned to the package Git.Git in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Git without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Git.Git with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/Git/Git>, <https://gitforwindows.org/>, <https://github.com/git-for-windows/build-extra/blob/HEAD/LICENSE.txt>

#### Git Extensions

`app.git-extensions` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A graphical tool for Git.

- **What it does:** Installs Git Extensions with winget, pinned to the package GitExtensionsTeam.GitExtensions in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Git Extensions without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install GitExtensionsTeam.GitExtensions with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/GitExtensionsTeam/GitExtensions>, <https://gitextensions.github.io/>, <https://github.com/gitextensions/gitextensions/blob/HEAD/LICENSE.md>

#### GitHub CLI

`app.github-cli` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

GitHub pull requests, issues and releases from the command line.

- **What it does:** Installs GitHub CLI with winget, pinned to the package GitHub.cli in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get GitHub CLI without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install GitHub.cli with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/GitHub/cli>, <https://cli.github.com/>, <https://github.com/cli/cli/blob/HEAD/LICENSE>

#### GitHub Desktop

`app.github-desktop` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A friendly app for working with Git repositories on GitHub.

- **What it does:** Installs GitHub Desktop with winget, pinned to the package GitHub.GitHubDesktop in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get GitHub Desktop without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install GitHub.GitHubDesktop with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/GitHub/GitHubDesktop>, <https://github.com/apps/desktop>, <https://github.com/desktop/desktop/blob/HEAD/LICENSE>

#### Go

`app.go` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The Go programming language and its tools.

- **What it does:** Installs Go with winget, pinned to the package GoLang.Go in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Go without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install GoLang.Go with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/GoLang/Go>, <https://go.dev/dl/>, <https://github.com/golang/go/blob/HEAD/LICENSE>

#### Hugo (Extended)

`app.hugo-extended` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A static site generator.

- **What it does:** Installs Hugo (Extended) with winget, pinned to the package Hugo.Hugo.Extended in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Hugo (Extended) without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Hugo.Hugo.Extended with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/h/Hugo/Hugo/Extended>, <https://gohugo.io/>, <https://github.com/gohugoio/hugo/blob/HEAD/LICENSE>

#### Java 21 (Temurin JDK)

`app.temurin-21` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A free build of the Java 21 development kit.

- **What it does:** Installs Java 21 (Temurin JDK) with winget, pinned to the package EclipseAdoptium.Temurin.21.JDK in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Java 21 (Temurin JDK) without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install EclipseAdoptium.Temurin.21.JDK with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/e/EclipseAdoptium/Temurin/21/JDK>, <https://adoptium.net/>, <https://github.com/adoptium/jdk/blob/HEAD/LICENSE>

#### JetBrains Toolbox

`app.jetbrains-toolbox` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Installs and updates JetBrains code editors such as IntelliJ and PyCharm.

- **What it does:** Installs JetBrains Toolbox with winget, pinned to the package JetBrains.Toolbox in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get JetBrains Toolbox without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install JetBrains.Toolbox with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/j/JetBrains/Toolbox>, <https://www.jetbrains.com/toolbox-app/>, <https://www.jetbrains.com/legal/docs/toolbox/user/>

#### Lua for Windows

`app.lua-for-windows` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The Lua language with a set of common libraries. An old package that is rarely updated.

- **What it does:** Installs Lua for Windows with winget, pinned to the package rjpcomputing.luaforwindows in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Lua for Windows without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install rjpcomputing.luaforwindows with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/rjpcomputing/luaforwindows>

#### Neovim

`app.neovim` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A modern take on the Vim text editor.

- **What it does:** Installs Neovim with winget, pinned to the package Neovim.Neovim in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Neovim without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Neovim.Neovim with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/n/Neovim/Neovim>, <https://neovim.io/>, <https://github.com/neovim/neovim/blob/HEAD/LICENSE.txt>

#### Node.js

`app.nodejs` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The JavaScript runtime, with npm.

- **What it does:** Installs Node.js with winget, pinned to the package OpenJS.NodeJS in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Node.js without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install OpenJS.NodeJS with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/OpenJS/NodeJS>, <https://nodejs.org/>, <https://github.com/nodejs/node/blob/v26.8.1/LICENSE>

#### Node.js (LTS)

`app.node-js-lts` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The long term support release of Node.js.

- **What it does:** Installs Node.js (LTS) with winget, pinned to the package OpenJS.NodeJS.LTS in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Node.js (LTS) without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install OpenJS.NodeJS.LTS with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/OpenJS/NodeJS/LTS>, <https://nodejs.org/>, <https://github.com/nodejs/node/blob/v24.20.0/LICENSE>

#### Notepad++

`app.notepadpp` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A fast text and code editor.

- **What it does:** Installs Notepad++ with winget, pinned to the package Notepad++.Notepad++ in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Notepad++ without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Notepad++.Notepad++ with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/n/Notepad++/Notepad++>, <https://notepad-plus-plus.org/>, <https://github.com/notepad-plus-plus/notepad-plus-plus/blob/HEAD/LICENSE>

#### NuGet

`app.nuget` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The command line tool for .NET packages.

- **What it does:** Installs NuGet with winget, pinned to the package Microsoft.NuGet in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get NuGet without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.NuGet with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/NuGet>, <https://www.nuget.org/>, <https://github.com/NuGet/NuGet.Client/blob/HEAD/LICENSE.txt>

#### Oh My Posh

`app.oh-my-posh` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A prompt theme engine for PowerShell and other shells.

- **What it does:** Installs Oh My Posh with winget, pinned to the package JanDeDobbeleer.OhMyPosh in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Oh My Posh without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install JanDeDobbeleer.OhMyPosh with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/j/JanDeDobbeleer/OhMyPosh>, <https://ohmyposh.dev/>, <https://github.com/JanDeDobbeleer/oh-my-posh/raw/main/COPYING>

#### Postman

`app.postman` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A tool for building and testing web APIs.

- **What it does:** Installs Postman with winget, pinned to the package Postman.Postman in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Postman without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Postman.Postman with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Postman/Postman>, <https://www.postman.com/downloads/>, <https://www.postman.com/legal/terms/>

#### PowerShell 7

`app.powershell` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The current cross platform PowerShell, next to the one built into Windows.

- **What it does:** Installs PowerShell 7 with winget, pinned to the package Microsoft.PowerShell in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get PowerShell 7 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.PowerShell with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/PowerShell>, <https://microsoft.com/PowerShell>, <https://github.com/PowerShell/PowerShell/blob/master/LICENSE.txt>

#### Python 3.13

`app.python` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The Python language and its tools, version 3.13.

- **What it does:** Installs Python 3.13 with winget, pinned to the package Python.Python.3.13 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Python 3.13 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Python.Python.3.13 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Python/Python/3/13>, <https://www.python.org/>, <https://docs.python.org/3/license.html>

#### Python 3.14

`app.python-3-14` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The Python programming language, version 3.14.

- **What it does:** Installs Python 3.14 with winget, pinned to the package Python.Python.3.14 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Python 3.14 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Python.Python.3.14 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Python/Python/3/14>, <https://www.python.org/>, <https://docs.python.org/3/license.html>

#### Ruby 4.0

`app.ruby-4-0` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The Ruby programming language, version 4.0, with its build tools.

- **What it does:** Installs Ruby 4.0 with winget, pinned to the package RubyInstallerTeam.Ruby.4.0 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Ruby 4.0 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install RubyInstallerTeam.Ruby.4.0 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/RubyInstallerTeam/Ruby/4/0>, <https://rubyinstaller.org/downloads/>, <https://github.com/oneclick/rubyinstaller2/blob/HEAD/LICENSE.txt>

#### Rust

`app.rust` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The Rust toolchain for Windows, built for the MSVC compiler.

- **What it does:** Installs Rust with winget, pinned to the package Rustlang.Rust.MSVC in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Rust without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Rustlang.Rust.MSVC with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/Rustlang/Rust/MSVC>, <https://www.rust-lang.org/>, <https://github.com/rust-lang/rust/blob/HEAD/COPYRIGHT>

#### Starship

`app.starship` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A fast, customisable prompt for any shell.

- **What it does:** Installs Starship with winget, pinned to the package Starship.Starship in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Starship without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Starship.Starship with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/Starship/Starship>, <https://starship.rs/>, <https://github.com/starship/starship/blob/HEAD/LICENSE>

#### Sublime Text 4

`app.sublime-text` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A fast text and code editor, free to try.

- **What it does:** Installs Sublime Text 4 with winget, pinned to the package SublimeHQ.SublimeText.4 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Sublime Text 4 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install SublimeHQ.SublimeText.4 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/SublimeHQ/SublimeText/4>, <https://www.sublimetext.com/>, <https://www.sublimetext.com/eula>

#### Unity Hub

`app.unity-hub` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The launcher and installer for the Unity game engine.

- **What it does:** Installs Unity Hub with winget, pinned to the package Unity.UnityHub in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Unity Hub without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Unity.UnityHub with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/u/Unity/UnityHub>, <https://unity.com/unity-hub>, <https://unity.com/legal/terms-of-service>

#### VSCodium

`app.vscodium` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Visual Studio Code built from source, without Microsoft's branding.

- **What it does:** Installs VSCodium with winget, pinned to the package VSCodium.VSCodium in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get VSCodium without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install VSCodium.VSCodium with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/v/VSCodium/VSCodium>, <https://vscodium.com/>, <https://github.com/VSCodium/vscodium/blob/HEAD/LICENSE>

#### Vagrant

`app.vagrant` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A tool for building and managing virtual machine environments.

- **What it does:** Installs Vagrant with winget, pinned to the package Hashicorp.Vagrant in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Vagrant without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Hashicorp.Vagrant with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/h/Hashicorp/Vagrant>, <https://developer.hashicorp.com/vagrant>, <https://github.com/hashicorp/vagrant/blob/HEAD/LICENSE>

#### Visual Studio 2022 Community

`app.visual-studio-2022-community` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Microsoft's full IDE, the 2022 version, free for individuals and small teams.

- **What it does:** Installs Visual Studio 2022 Community with winget, pinned to the package Microsoft.VisualStudio.2022.Community in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Visual Studio 2022 Community without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.VisualStudio.2022.Community with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/VisualStudio/2022/Community>, <https://visualstudio.microsoft.com/>, <https://visualstudio.microsoft.com/license-terms/>

#### Visual Studio 2026 Community

`app.visual-studio-2026-community` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Microsoft's full IDE, the 2026 version, free for individuals and small teams.

- **What it does:** Installs Visual Studio 2026 Community with winget, pinned to the package Microsoft.VisualStudio.Community in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Visual Studio 2026 Community without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.VisualStudio.Community with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/VisualStudio/Community>, <https://visualstudio.microsoft.com/>, <https://visualstudio.microsoft.com/license-terms/>

#### Visual Studio Code

`app.vscode` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Microsoft's code editor.

- **What it does:** Installs Visual Studio Code with winget, pinned to the package Microsoft.VisualStudioCode in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Visual Studio Code without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.VisualStudioCode with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/VisualStudioCode>, <https://code.visualstudio.com/>, <https://code.visualstudio.com/license>

#### WinSCP

`app.winscp` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Copies files to servers over SFTP and SCP.

- **What it does:** Installs WinSCP with winget, pinned to the package WinSCP.WinSCP in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get WinSCP without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install WinSCP.WinSCP with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/WinSCP/WinSCP>, <https://winscp.net/>, <https://github.com/winscp/winscp/blob/HEAD/license.txt>

#### Windows Terminal

`app.windows-terminal` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Microsoft's tabbed terminal for PowerShell, Command Prompt and more.

- **What it does:** Installs Windows Terminal with winget, pinned to the package Microsoft.WindowsTerminal in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Windows Terminal without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.WindowsTerminal with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/WindowsTerminal>, <https://docs.microsoft.com/windows/terminal>, <https://github.com/microsoft/terminal/blob/main/LICENSE>

#### Yarn

`app.yarn` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Node.js package manager.

- **What it does:** Installs Yarn with winget, pinned to the package Yarn.Yarn in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Yarn without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Yarn.Yarn with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/y/Yarn/Yarn>, <https://yarnpkg.com/>, <https://github.com/yarnpkg/yarn/blob/HEAD/LICENSE>

#### Zed

`app.zed` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A fast code editor made for working with others and with AI.

- **What it does:** Installs Zed with winget, pinned to the package ZedIndustries.Zed in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Zed without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install ZedIndustries.Zed with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/z/ZedIndustries/Zed>, <https://zed.dev/windows>, <https://github.com/zed-industries/zed/blob/main/LICENSE-GPL>

#### gsudo

`app.gsudo` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A sudo for Windows: run a command with administrator rights from your shell.

- **What it does:** Installs gsudo with winget, pinned to the package gerardog.gsudo in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get gsudo without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install gerardog.gsudo with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/gerardog/gsudo>, <https://gerardog.github.io/gsudo/>, <https://raw.githubusercontent.com/gerardog/gsudo/refs/heads/master/LICENSE.txt>

#### lazygit

`app.lazygit` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A terminal interface for Git commands.

- **What it does:** Installs lazygit with winget, pinned to the package JesseDuffield.lazygit in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get lazygit without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install JesseDuffield.lazygit with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/j/JesseDuffield/lazygit>, <https://github.com/jesseduffield/lazygit>, <https://github.com/jesseduffield/lazygit/blob/HEAD/LICENSE>

#### pnpm

`app.pnpm` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A fast, disk efficient Node.js package manager.

- **What it does:** Installs pnpm with winget, pinned to the package pnpm.pnpm in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get pnpm without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install pnpm.pnpm with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/pnpm/pnpm>, <https://pnpm.io/>, <https://github.com/pnpm/pnpm/blob/HEAD/LICENSE>

#### uv

`app.uv` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A fast Python package and project manager.

- **What it does:** Installs uv with winget, pinned to the package astral-sh.uv in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get uv without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install astral-sh.uv with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/astral-sh/uv>, <https://github.com/astral-sh/uv>, <https://github.com/astral-sh/uv/blob/HEAD/README.md#license>

### Files and cloud

#### 7-Zip

`app.7zip` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A file archiver for ZIP, 7z, RAR and more.

- **What it does:** Installs 7-Zip with winget, pinned to the package 7zip.7zip in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get 7-Zip without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install 7zip.7zip with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/7/7zip/7zip>, <https://7-zip.org/download.html>, <https://7-zip.org/license.txt>

#### AB Download Manager

`app.ab-download-manager` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A download manager for the desktop.

- **What it does:** Installs AB Download Manager with winget, pinned to the package amir1376.ABDownloadManager in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get AB Download Manager without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install amir1376.ABDownloadManager with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/amir1376/ABDownloadManager>, <https://abdownloadmanager.com/>, <https://github.com/amir1376/ab-download-manager/blob/HEAD/LICENSE>

#### Dropbox

`app.dropbox` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Cloud file storage and sync.

- **What it does:** Installs Dropbox with winget, pinned to the package Dropbox.Dropbox in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Dropbox without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Dropbox.Dropbox with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/Dropbox/Dropbox>, <https://www.dropbox.com/>, <https://www.dropbox.com/terms>

#### Files

`app.files` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A modern file manager with tabs.

- **What it does:** Installs Files with winget, pinned to the package FilesCommunity.Files in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Files without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install FilesCommunity.Files with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/f/FilesCommunity/Files>, <https://github.com/files-community/Files>, <https://github.com/files-community/Files/blob/main/LICENSE-MPL>

#### Google Drive

`app.google-drive` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Google's cloud storage, as a drive in File Explorer.

- **What it does:** Installs Google Drive with winget, pinned to the package Google.GoogleDrive in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Google Drive without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Google.GoogleDrive with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/Google/GoogleDrive>, <https://workspace.google.com/products/drive/>, <https://www.google.com/drive/terms-of-service/>

#### Internet Download Manager

`app.internet-download-manager` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A download manager that speeds up and resumes downloads.

- **What it does:** Installs Internet Download Manager with winget, pinned to the package Tonec.InternetDownloadManager in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Internet Download Manager without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Tonec.InternetDownloadManager with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/Tonec/InternetDownloadManager>, <https://www.internetdownloadmanager.com/>, <https://www.internetdownloadmanager.com/support/terms-and-conditions.html>

#### LocalSend

`app.localsend` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shares files with nearby devices, with no account and no internet.

- **What it does:** Installs LocalSend with winget, pinned to the package LocalSend.LocalSend in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get LocalSend without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install LocalSend.LocalSend with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/l/LocalSend/LocalSend>, <https://localsend.org/>, <https://github.com/localsend/localsend/blob/HEAD/LICENSE>

#### MEGAsync

`app.megasync` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Syncs files with MEGA cloud storage.

- **What it does:** Installs MEGAsync with winget, pinned to the package Mega.MEGASync in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get MEGAsync without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Mega.MEGASync with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Mega/MEGASync>, <https://mega.nz/sync>, <https://github.com/meganz/MEGAsync/blob/master/LICENCE.md>

#### NanaZip

`app.nanazip` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A 7-Zip fork made for Windows 11.

- **What it does:** Installs NanaZip with winget, pinned to the package M2Team.NanaZip in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get NanaZip without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install M2Team.NanaZip with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/M2Team/NanaZip>, <https://github.com/M2Team/NanaZip>, <https://github.com/M2Team/NanaZip/blob/HEAD/License.md>

#### Nextcloud

`app.nextcloud` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Syncs files with a Nextcloud server.

- **What it does:** Installs Nextcloud with winget, pinned to the package Nextcloud.NextcloudDesktop in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Nextcloud without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Nextcloud.NextcloudDesktop with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/n/Nextcloud/NextcloudDesktop>, <https://nextcloud.com/>, <https://raw.githubusercontent.com/nextcloud/desktop/master/COPYING>

#### OneDrive

`app.onedrive` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Microsoft's cloud storage, which Windows already includes.

- **What it does:** Installs OneDrive with winget, pinned to the package Microsoft.OneDrive in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get OneDrive without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Microsoft.OneDrive with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/OneDrive>, <https://www.microsoft.com/microsoft-365/onedrive/download>, <https://www.microsoft.com/legal/terms-of-use>

#### PeaZip

`app.peazip` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A file archiver that opens over 200 archive types.

- **What it does:** Installs PeaZip with winget, pinned to the package Giorgiotani.Peazip in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get PeaZip without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Giorgiotani.Peazip with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/Giorgiotani/Peazip>, <https://peazip.github.io/>, <https://github.com/peazip/PeaZip/blob/HEAD/LICENSE>

#### Proton Drive

`app.proton-drive` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The desktop app for Proton's end to end encrypted cloud storage.

- **What it does:** Installs Proton Drive with winget, pinned to the package Proton.ProtonDrive in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Proton Drive without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Proton.ProtonDrive with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Proton/ProtonDrive>, <https://proton.me/drive>, <https://proton.me/legal/terms>

#### SyncTrayzor

`app.synctrayzor` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A tray app and window for Syncthing.

- **What it does:** Installs SyncTrayzor with winget, pinned to the package GermanCoding.SyncTrayzor in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get SyncTrayzor without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install GermanCoding.SyncTrayzor with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/GermanCoding/SyncTrayzor>, <https://github.com/GermanCoding/SyncTrayzor>, <https://github.com/GermanCoding/SyncTrayzor/blob/HEAD/LICENSE.txt>

#### Syncthing

`app.syncthing` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Syncs files between your own devices without a cloud service.

- **What it does:** Installs Syncthing with winget, pinned to the package Syncthing.Syncthing in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Syncthing without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Syncthing.Syncthing with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/Syncthing/Syncthing>, <https://github.com/syncthing/syncthing>, <https://github.com/syncthing/syncthing/blob/HEAD/LICENSE>

#### Total Commander

`app.total-commander` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A two pane file manager.

- **What it does:** Installs Total Commander with winget, pinned to the package Ghisler.TotalCommander in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Total Commander without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Ghisler.TotalCommander with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/Ghisler/TotalCommander>, <https://www.ghisler.com/>, <https://www.ghisler.com/order.htm>

#### WinRAR

`app.winrar` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A file archiver for RAR and ZIP, as a trial.

- **What it does:** Installs WinRAR with winget, pinned to the package RARLab.WinRAR in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get WinRAR without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install RARLab.WinRAR with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/RARLab/WinRAR>, <https://www.win-rar.com/>, <https://www.win-rar.com/winrarlicense.html>

#### qBittorrent

`app.qbittorrent` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A BitTorrent client.

- **What it does:** Installs qBittorrent with winget, pinned to the package qBittorrent.qBittorrent in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get qBittorrent without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install qBittorrent.qBittorrent with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/q/qBittorrent/qBittorrent>, <https://www.qbittorrent.org/>, <https://github.com/qbittorrent/qBittorrent/blob/master/COPYING>

### Games

#### Battle.net

`app.battle-net` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Blizzard's game launcher.

- **What it does:** Installs Battle.net with winget, pinned to the package Blizzard.BattleNet in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Battle.net without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Blizzard.BattleNet with winget into %ProgramFiles(x86)%\Battle.net`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/Blizzard/BattleNet>, <https://download.battle.net/en-us/desktop>, <https://www.blizzard.com/legal/fba4d00f-c7e4-4883-b8b9-1b4500a402ea/blizzard-end-user-license-agreement>

#### Cemu

`app.cemu` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Wii U emulator.

- **What it does:** Installs Cemu with winget, pinned to the package Cemu.Cemu in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Cemu without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Cemu.Cemu with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/Cemu/Cemu>, <https://github.com/cemu-project/Cemu>, <https://github.com/cemu-project/Cemu/blob/HEAD/LICENSE.txt>

#### CurseForge

`app.curseforge` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A mod manager for many games.

- **What it does:** Installs CurseForge with winget, pinned to the package Overwolf.CurseForge in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get CurseForge without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Overwolf.CurseForge with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/Overwolf/CurseForge>, <https://curseforge.overwolf.com/>, <https://www.overwolf.com/legal/terms/>

#### EA app

`app.ea-app` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Electronic Arts' game launcher.

- **What it does:** Installs EA app with winget, pinned to the package ElectronicArts.EADesktop in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get EA app without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install ElectronicArts.EADesktop with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/e/ElectronicArts/EADesktop>, <https://www.ea.com/ea-app>, <https://tos.ea.com/legalapp/WEBTERMS/US/en/PC/>

#### EmulationStation Desktop Edition

`app.emulationstation-desktop-edition` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A front end that gathers your emulators and games in one place.

- **What it does:** Installs EmulationStation Desktop Edition with winget, pinned to the package ES-DE.EmulationStation-DE in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get EmulationStation Desktop Edition without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install ES-DE.EmulationStation-DE with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/e/ES-DE/EmulationStation-DE>, <https://www.es-de.org/>, <https://gitlab.com/es-de/emulationstation-de/-/blob/HEAD/LICENSE>

#### Epic Games Launcher

`app.epic-games` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The Epic Games store and launcher.

- **What it does:** Installs Epic Games Launcher with winget, pinned to the package EpicGames.EpicGamesLauncher in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Epic Games Launcher without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install EpicGames.EpicGamesLauncher with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/e/EpicGames/EpicGamesLauncher>, <https://epicgames.com/download>, <https://epicgames.com/eula>

#### GOG Galaxy

`app.gog-galaxy` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The GOG store and launcher, for games without copy protection.

- **What it does:** Installs GOG Galaxy with winget, pinned to the package GOG.Galaxy in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get GOG Galaxy without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install GOG.Galaxy with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/GOG/Galaxy>, <https://www.gog.com/galaxy>

#### GeForce NOW

`app.geforce-now` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

NVIDIA's cloud game streaming client.

- **What it does:** Installs GeForce NOW with winget, pinned to the package Nvidia.GeForceNow in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get GeForce NOW without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Nvidia.GeForceNow with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/n/Nvidia/GeForceNow>, <https://www.nvidia.com/en-us/geforce-now>, <https://www.nvidia.com/en-us/geforce-now/terms-of-use>

#### Heroic Games Launcher

`app.heroic` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

An open source launcher for Epic, GOG and Amazon games.

- **What it does:** Installs Heroic Games Launcher with winget, pinned to the package HeroicGamesLauncher.HeroicGamesLauncher in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Heroic Games Launcher without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install HeroicGamesLauncher.HeroicGamesLauncher with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/h/HeroicGamesLauncher/HeroicGamesLauncher>, <https://github.com/Heroic-Games-Launcher/HeroicGamesLauncher>, <https://raw.githubusercontent.com/Heroic-Games-Launcher/HeroicGamesLauncher/main/COPYING>

#### Modrinth App

`app.modrinth` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Minecraft launcher and mod manager.

- **What it does:** Installs Modrinth App with winget, pinned to the package Modrinth.ModrinthApp in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Modrinth App without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Modrinth.ModrinthApp with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Modrinth/ModrinthApp>, <https://modrinth.com/app>, <https://github.com/modrinth/code/blob/main/apps/app/LICENSE>

#### Moonlight

`app.moonlight` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Plays games streamed from another PC.

- **What it does:** Installs Moonlight with winget, pinned to the package MoonlightGameStreamingProject.Moonlight in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Moonlight without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install MoonlightGameStreamingProject.Moonlight with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/MoonlightGameStreamingProject/Moonlight>, <https://moonlight-stream.org/>, <https://github.com/moonlight-stream/moonlight-qt/blob/HEAD/LICENSE>

#### Parsec

`app.parsec` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Streams your PC's screen for remote play and remote desktop.

- **What it does:** Installs Parsec with winget, pinned to the package Parsec.Parsec in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Parsec without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Once you set it up, this program lets someone connect to this PC. Use a strong password, allow only people you trust, and turn it off or uninstall it when you do not need it.
- **Changes:**
  - `Install Parsec.Parsec with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Parsec/Parsec>, <https://parsec.app/>, <https://parsec.app/terms>

#### Playnite

`app.playnite` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

One library for the games from all your launchers.

- **What it does:** Installs Playnite with winget, pinned to the package Playnite.Playnite in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Playnite without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Playnite.Playnite with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Playnite/Playnite>, <https://playnite.link/>, <https://github.com/JosefNemec/Playnite/blob/HEAD/LICENSE.md>

#### Prism Launcher

`app.prism-launcher` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A launcher that manages several Minecraft installs.

- **What it does:** Installs Prism Launcher with winget, pinned to the package PrismLauncher.PrismLauncher in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Prism Launcher without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install PrismLauncher.PrismLauncher with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/PrismLauncher/PrismLauncher>, <https://github.com/PrismLauncher/PrismLauncher>, <https://github.com/PrismLauncher/PrismLauncher/blob/HEAD/LICENSE>

#### RetroArch

`app.retroarch` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

One front end for many classic game console emulators.

- **What it does:** Installs RetroArch with winget, pinned to the package Libretro.RetroArch in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get RetroArch without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Libretro.RetroArch with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/l/Libretro/RetroArch>, <https://www.retroarch.com/?page=platforms>, <https://github.com/libretro/RetroArch/blob/master/COPYING>

#### Roblox

`app.roblox` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The player for Roblox games.

- **What it does:** Installs Roblox with winget, pinned to the package Roblox.Roblox in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Roblox without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Roblox.Roblox with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/Roblox/Roblox>, <https://www.roblox.com/download>

#### Steam

`app.steam` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Valve's game store and launcher.

- **What it does:** Installs Steam with winget, pinned to the package Valve.Steam in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Steam without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Valve.Steam with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/v/Valve/Steam>, <https://store.steampowered.com/about/>, <https://store.steampowered.com/subscriber_agreement/>

#### Sunshine

`app.sunshine` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Streams games from this PC to other devices with Moonlight.

- **What it does:** Installs Sunshine with winget, pinned to the package LizardByte.Sunshine in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Sunshine without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install LizardByte.Sunshine with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/l/LizardByte/Sunshine>, <https://github.com/LizardByte/Sunshine>, <https://github.com/LizardByte/Sunshine/blob/HEAD/LICENSE>

#### Ubisoft Connect

`app.ubisoft-connect` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Ubisoft's game store and launcher.

- **What it does:** Installs Ubisoft Connect with winget, pinned to the package Ubisoft.Connect in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Ubisoft Connect without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Ubisoft.Connect with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/u/Ubisoft/Connect>, <https://ubisoftconnect.com/>, <https://legal.ubi.com/LegalInfoUbisoftEMEA>

#### Virtual Desktop Streamer

`app.virtual-desktop-streamer` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Streams your PC to a VR headset with Virtual Desktop.

- **What it does:** Installs Virtual Desktop Streamer with winget, pinned to the package VirtualDesktop.Streamer in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Virtual Desktop Streamer without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install VirtualDesktop.Streamer with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/v/VirtualDesktop/Streamer>

#### itch.io

`app.itch` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The itch.io app for indie games.

- **What it does:** Installs itch.io with winget, pinned to the package ItchIo.Itch in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get itch.io without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install ItchIo.Itch with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/i/ItchIo/Itch>, <https://itch.io/app>, <https://github.com/itchio/itch/blob/HEAD/LICENSE>

### Media

#### AIMP

`app.aimp` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A music player.

- **What it does:** Installs AIMP with winget, pinned to the package AIMP.AIMP in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get AIMP without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install AIMP.AIMP with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/AIMP/AIMP>, <https://www.aimp.ru>, <https://www.aimp.ru/?do=eula&os=windows>

#### Audacity

`app.audacity` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

An audio recorder and editor.

- **What it does:** Installs Audacity with winget, pinned to the package Audacity.Audacity in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Audacity without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Audacity.Audacity with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Audacity/Audacity>, <https://www.audacityteam.org/>, <https://github.com/audacity/audacity/blob/HEAD/LICENSE.txt>

#### Blender

`app.blender` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A 3D modelling, animation and video tool.

- **What it does:** Installs Blender with winget, pinned to the package BlenderFoundation.Blender in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Blender without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install BlenderFoundation.Blender with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/BlenderFoundation/Blender>

#### FFmpeg

`app.ffmpeg` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The command line tool that converts and processes audio and video.

- **What it does:** Installs FFmpeg with winget, pinned to the package Gyan.FFmpeg in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get FFmpeg without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Gyan.FFmpeg with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/Gyan/FFmpeg>, <https://www.gyan.dev/ffmpeg/builds/>, <https://www.ffmpeg.org/legal.html>

#### File Converter

`app.file-converter` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Converts files from the right click menu.

- **What it does:** Installs File Converter with winget, pinned to the package AdrienAllard.FileConverter in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get File Converter without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install AdrienAllard.FileConverter with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/AdrienAllard/FileConverter>, <https://file-converter.io/>, <https://github.com/Tichau/FileConverter/blob/HEAD/LICENSE.md>

#### GIMP

`app.gimp` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A full featured image editor.

- **What it does:** Installs GIMP with winget, pinned to the package GIMP.GIMP.3 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get GIMP without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install GIMP.GIMP.3 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/GIMP/GIMP/3>, <https://www.gimp.org/downloads/>, <https://www.gimp.org/about/COPYING>

#### Greenshot

`app.greenshot` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Screenshots with quick markup.

- **What it does:** Installs Greenshot with winget, pinned to the package Greenshot.Greenshot in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Greenshot without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Greenshot.Greenshot with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/Greenshot/Greenshot>, <https://getgreenshot.org/>, <https://github.com/greenshot/greenshot/blob/HEAD/LICENSE>

#### HandBrake

`app.handbrake` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Converts video between formats.

- **What it does:** Installs HandBrake with winget, pinned to the package HandBrake.HandBrake in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get HandBrake without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install HandBrake.HandBrake with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/h/HandBrake/HandBrake>, <https://handbrake.fr/>, <https://github.com/HandBrake/HandBrake/blob/HEAD/LICENSE>

#### ImageGlass

`app.imageglass` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A light image viewer.

- **What it does:** Installs ImageGlass with winget, pinned to the package DuongDieuPhap.ImageGlass in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get ImageGlass without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install DuongDieuPhap.ImageGlass with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/DuongDieuPhap/ImageGlass>, <https://imageglass.org/>, <https://imageglass.org/license>

#### Inkscape

`app.inkscape` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A vector graphics editor.

- **What it does:** Installs Inkscape with winget, pinned to the package Inkscape.Inkscape in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Inkscape without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Inkscape.Inkscape with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/i/Inkscape/Inkscape>, <https://inkscape.org/release/>, <https://inkscape.org/about/license/>

#### IrfanView

`app.irfanview` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A small, fast image viewer with batch conversion.

- **What it does:** Installs IrfanView with winget, pinned to the package IrfanSkiljan.IrfanView in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get IrfanView without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install IrfanSkiljan.IrfanView with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/i/IrfanSkiljan/IrfanView>, <https://www.irfanview.com/>, <https://www.irfanview.com/eula.htm>

#### JPEGView

`app.jpegview` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A small, fast image viewer for JPEG and other pictures.

- **What it does:** Installs JPEGView with winget, pinned to the package sylikc.JPEGView in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get JPEGView without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install sylikc.JPEGView with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/sylikc/JPEGView>, <https://github.com/sylikc/jpegview>, <https://github.com/sylikc/jpegview/blob/HEAD/LICENSE.txt>

#### Jellyfin Media Player

`app.jellyfin-media-player` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The desktop app for watching from a Jellyfin server.

- **What it does:** Installs Jellyfin Media Player with winget, pinned to the package Jellyfin.JellyfinMediaPlayer in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Jellyfin Media Player without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Jellyfin.JellyfinMediaPlayer with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/j/Jellyfin/JellyfinMediaPlayer>, <https://github.com/jellyfin/jellyfin-media-player>, <https://github.com/jellyfin/jellyfin-media-player/blob/HEAD/LICENSE>

#### Jellyfin Server

`app.jellyfin-server` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A free media server you run yourself.

- **What it does:** Installs Jellyfin Server with winget, pinned to the package Jellyfin.Server in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Jellyfin Server without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Jellyfin.Server with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/j/Jellyfin/Server>, <https://jellyfin.org/>, <https://github.com/jellyfin/jellyfin-server-windows/blob/HEAD/LICENSE>

#### K-Lite Codec Pack Standard

`app.k-lite-codec-pack-standard` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A set of codecs and filters, with Media Player Classic.

- **What it does:** Installs K-Lite Codec Pack Standard with winget, pinned to the package CodecGuide.K-LiteCodecPack.Standard in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get K-Lite Codec Pack Standard without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install CodecGuide.K-LiteCodecPack.Standard with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/CodecGuide/K-LiteCodecPack/Standard>, <https://codecguide.com/download_k-lite_codec_pack_standard.htm>, <https://codecguide.com/license_standard.rtf>

#### Kdenlive

`app.kdenlive` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A video editor with multiple tracks.

- **What it does:** Installs Kdenlive with winget, pinned to the package KDE.Kdenlive in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Kdenlive without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install KDE.Kdenlive with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/k/KDE/Kdenlive>, <https://kdenlive.org/>, <https://invent.kde.org/multimedia/kdenlive/-/blob/HEAD/COPYING>

#### Kodi

`app.kodi` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A free media center for videos, music and photos.

- **What it does:** Installs Kodi with winget, pinned to the package XBMCFoundation.Kodi in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Kodi without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install XBMCFoundation.Kodi with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/x/XBMCFoundation/Kodi>, <https://kodi.tv/download/windows/>, <https://github.com/xbmc/xbmc/blob/HEAD/LICENSE.md>

#### Krita

`app.krita` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A painting and drawing app.

- **What it does:** Installs Krita with winget, pinned to the package KDE.Krita in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Krita without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install KDE.Krita with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/k/KDE/Krita>, <https://krita.org/>, <https://krita.org/en/about/license/>

#### MKVToolNix

`app.mkvtoolnix` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Edits and combines the parts of MKV video files.

- **What it does:** Installs MKVToolNix with winget, pinned to the package MoritzBunkus.MKVToolNix in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get MKVToolNix without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install MoritzBunkus.MKVToolNix with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/MoritzBunkus/MKVToolNix>, <https://mkvtoolnix.download/>, <https://gitlab.com/mbunkus/mkvtoolnix/-/blob/main/COPYING>

#### MPC-HC

`app.mpc-hc` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A light video player.

- **What it does:** Installs MPC-HC with winget, pinned to the package clsid2.mpc-hc in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get MPC-HC without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install clsid2.mpc-hc with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/clsid2/mpc-hc>, <https://github.com/clsid2/mpc-hc>, <https://github.com/clsid2/mpc-hc/blob/HEAD/COPYING.txt>

#### Media Player Classic Qute Theater

`app.media-player-classic-qute-theater` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A media player in the style of Media Player Classic, built on mpv.

- **What it does:** Installs Media Player Classic Qute Theater with winget, pinned to the package mpc-qt.mpc-qt in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Media Player Classic Qute Theater without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install mpc-qt.mpc-qt with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/mpc-qt/mpc-qt>, <https://github.com/mpc-qt/mpc-qt>, <https://github.com/mpc-qt/mpc-qt/blob/master/LICENSE>

#### OBS Studio

`app.obs-studio` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Records and live streams video.

- **What it does:** Installs OBS Studio with winget, pinned to the package OBSProject.OBSStudio in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get OBS Studio without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install OBSProject.OBSStudio with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/OBSProject/OBSStudio>, <https://obsproject.com/>, <https://github.com/obsproject/obs-studio/blob/HEAD/COPYING>

#### Paint.NET

`app.paint-net` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

An image editor with layers.

- **What it does:** Installs Paint.NET with winget, pinned to the package dotPDN.PaintDotNet in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Paint.NET without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install dotPDN.PaintDotNet with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/dotPDN/PaintDotNet>, <https://getpaint.net/>, <https://getpaint.net/license.html>

#### Plex

`app.plex` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The desktop app for watching from Plex.

- **What it does:** Installs Plex with winget, pinned to the package Plex.Plex in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Plex without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Plex.Plex with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Plex/Plex>, <https://www.plex.tv/>, <https://www.plex.tv/about/privacy-legal/plex-terms-of-service>

#### Plex Media Server

`app.plex-media-server` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A media server that streams your library to your devices.

- **What it does:** Installs Plex Media Server with winget, pinned to the package Plex.PlexMediaServer in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Plex Media Server without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Plex.PlexMediaServer with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Plex/PlexMediaServer>, <https://www.plex.tv/>, <https://www.plex.tv/about/privacy-legal/plex-terms-of-service>

#### ShareX

`app.sharex` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Screenshots and screen recording with upload options.

- **What it does:** Installs ShareX with winget, pinned to the package ShareX.ShareX in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get ShareX without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install ShareX.ShareX with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/ShareX/ShareX>, <https://getsharex.com/>, <https://github.com/ShareX/ShareX/blob/HEAD/LICENSE.txt>

#### VLC media player

`app.vlc` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Plays almost any video or audio file.

- **What it does:** Installs VLC media player with winget, pinned to the package VideoLAN.VLC in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get VLC media player without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install VideoLAN.VLC with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/v/VideoLAN/VLC>, <https://www.videolan.org/vlc/>, <https://code.videolan.org/videolan/vlc/-/blob/HEAD/COPYING>

#### foobar2000

`app.foobar2000` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A customisable music player.

- **What it does:** Installs foobar2000 with winget, pinned to the package PeterPawlowski.foobar2000 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get foobar2000 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install PeterPawlowski.foobar2000 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/PeterPawlowski/foobar2000>, <https://www.foobar2000.org/>, <https://www.foobar2000.org/license>

#### iTunes

`app.itunes` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Apple's music player and device manager.

- **What it does:** Installs iTunes with winget, pinned to the package Apple.iTunes in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get iTunes without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Apple.iTunes with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Apple/iTunes>, <https://www.apple.com/itunes/>, <https://www.apple.com/legal/sla/docs/iTunesWindows.pdf>

#### mpv

`app.mpv` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A free media player, in builds made by shinchiro.

- **What it does:** Installs mpv with winget, pinned to the package shinchiro.mpv in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get mpv without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install shinchiro.mpv with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/shinchiro/mpv>, <https://github.com/shinchiro/mpv-winbuild-cmake>, <https://raw.githubusercontent.com/mpv-player/mpv/master/LICENSE.GPL>

#### nomacs

`app.nomacs` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

An image viewer that opens common formats, including RAW and PSD.

- **What it does:** Installs nomacs with winget, pinned to the package nomacs.nomacs in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get nomacs without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install nomacs.nomacs with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/n/nomacs/nomacs>, <https://nomacs.org/>, <https://github.com/nomacs/nomacs/blob/HEAD/LICENSE>

#### yt-dlp

`app.yt-dlp` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A command line downloader for video and audio from many sites.

- **What it does:** Installs yt-dlp with winget, pinned to the package yt-dlp.yt-dlp in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get yt-dlp without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install yt-dlp.yt-dlp with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/y/yt-dlp/yt-dlp>, <https://github.com/yt-dlp/yt-dlp>, <https://github.com/yt-dlp/yt-dlp/blob/HEAD/LICENSE>

### Office and notes

#### Adobe Acrobat Reader

`app.adobe-acrobat-reader` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Adobe's PDF reader.

- **What it does:** Installs Adobe Acrobat Reader with winget, pinned to the package Adobe.Acrobat.Reader.64-bit in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Adobe Acrobat Reader without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Adobe.Acrobat.Reader.64-bit with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Adobe/Acrobat/Reader/64-bit>

#### Foxit PDF Reader

`app.foxit-pdf-reader` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A PDF reader.

- **What it does:** Installs Foxit PDF Reader with winget, pinned to the package Foxit.FoxitReader in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Foxit PDF Reader without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Foxit.FoxitReader with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/f/Foxit/FoxitReader>, <https://www.foxitsoftware.com/pdf-reader/>, <https://www.foxit.com/pdf-editor/eula.html>

#### Joplin

`app.joplin` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Notes with sync and end to end encryption.

- **What it does:** Installs Joplin with winget, pinned to the package Joplin.Joplin in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Joplin without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Joplin.Joplin with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/j/Joplin/Joplin>, <https://joplinapp.org/>, <https://github.com/laurent22/joplin/blob/HEAD/LICENSE>

#### LibreOffice

`app.libreoffice` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A free office suite for documents, spreadsheets and slides.

- **What it does:** Installs LibreOffice with winget, pinned to the package TheDocumentFoundation.LibreOffice in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get LibreOffice without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install TheDocumentFoundation.LibreOffice with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/TheDocumentFoundation/LibreOffice>, <https://www.libreoffice.org/>, <https://www.libreoffice.org/licenses/>

#### NAPS2

`app.naps2` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A document scanning app that saves PDF, TIFF, JPEG or PNG, with text recognition.

- **What it does:** Installs NAPS2 with winget, pinned to the package Cyanfish.NAPS2 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get NAPS2 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Cyanfish.NAPS2 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/Cyanfish/NAPS2>, <https://www.naps2.com/>, <https://github.com/cyanfish/naps2/blob/HEAD/LICENSE>

#### ONLYOFFICE Desktop Editors

`app.onlyoffice` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Office editors that open Microsoft Office formats.

- **What it does:** Installs ONLYOFFICE Desktop Editors with winget, pinned to the package ONLYOFFICE.DesktopEditors in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get ONLYOFFICE Desktop Editors without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install ONLYOFFICE.DesktopEditors with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/ONLYOFFICE/DesktopEditors>, <https://www.onlyoffice.com/en/desktop.aspx>, <https://github.com/ONLYOFFICE/DesktopEditors/blob/HEAD/LICENSE>

#### Obsidian

`app.obsidian` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Notes kept as plain files, with links between them.

- **What it does:** Installs Obsidian with winget, pinned to the package Obsidian.Obsidian in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Obsidian without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Obsidian.Obsidian with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/Obsidian/Obsidian>, <https://obsidian.md/>, <https://obsidian.md/terms>

#### Okular

`app.okular` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

KDE's document viewer for PDF, EPUB and more.

- **What it does:** Installs Okular with winget, pinned to the package KDE.Okular in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Okular without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install KDE.Okular with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/k/KDE/Okular>, <https://okular.kde.org/>

#### PDF-XChange Editor

`app.pdf-xchange-editor` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A PDF editor.

- **What it does:** Installs PDF-XChange Editor with winget, pinned to the package TrackerSoftware.PDF-XChangeEditor in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get PDF-XChange Editor without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install TrackerSoftware.PDF-XChangeEditor with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/TrackerSoftware/PDF-XChangeEditor>, <https://www.pdf-xchange.com/product/pdf-xchange-editor>, <https://www.pdf-xchange.com/terms-of-use>

#### PDF24 Creator

`app.pdf24-creator` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A set of PDF tools: merge, split, compress and convert.

- **What it does:** Installs PDF24 Creator with winget, pinned to the package geeksoftwareGmbH.PDF24Creator in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get PDF24 Creator without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install geeksoftwareGmbH.PDF24Creator with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/geeksoftwareGmbH/PDF24Creator>, <https://www.pdf24.org/en/terms-of-use>

#### PDFgear

`app.pdfgear` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A free PDF reader and editor.

- **What it does:** Installs PDFgear with winget, pinned to the package PDFgear.PDFgear in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get PDFgear without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install PDFgear.PDFgear with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/PDFgear/PDFgear>

#### PDFsam Basic

`app.pdfsam-basic` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Splits, merges, rotates and extracts pages from PDF files.

- **What it does:** Installs PDFsam Basic with winget, pinned to the package PDFsam.PDFsam in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get PDFsam Basic without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install PDFsam.PDFsam with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/PDFsam/PDFsam>, <https://pdfsam.org/pdfsam-basic/>, <https://github.com/torakiki/pdfsam/blob/HEAD/LICENSE>

#### QOwnNotes

`app.qownnotes` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A Markdown note taking app that works with Nextcloud notes.

- **What it does:** Installs QOwnNotes with winget, pinned to the package pbek.QOwnNotes in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get QOwnNotes without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install pbek.QOwnNotes with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/pbek/QOwnNotes>, <https://github.com/pbek/QOwnNotes>, <https://github.com/pbek/QOwnNotes/blob/HEAD/LICENSE>

#### Simplenote

`app.simplenote` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A simple note taking app that syncs across devices.

- **What it does:** Installs Simplenote with winget, pinned to the package Automattic.Simplenote in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Simplenote without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Automattic.Simplenote with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Automattic/Simplenote>, <https://simplenote.com/>, <https://github.com/Automattic/simplenote-electron/blob/HEAD/LICENSE.md>

#### SumatraPDF

`app.sumatrapdf` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A very light reader for PDF, ePub and other documents.

- **What it does:** Installs SumatraPDF with winget, pinned to the package SumatraPDF.SumatraPDF in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get SumatraPDF without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install SumatraPDF.SumatraPDF with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/SumatraPDF/SumatraPDF>, <https://github.com/sumatrapdfreader/sumatrapdf>, <https://github.com/sumatrapdfreader/sumatrapdf/blob/3.6rel/COPYING>

#### Xournal++

`app.xournal` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A note taking app for handwriting and annotating PDFs.

- **What it does:** Installs Xournal++ with winget, pinned to the package Xournal++.Xournal++ in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Xournal++ without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Xournal++.Xournal++ with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/x/Xournal++/Xournal++>, <https://xournalpp.github.io/>, <https://github.com/xournalpp/xournalpp/blob/master/LICENSE>

#### Zotero

`app.zotero` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Collects and cites research sources.

- **What it does:** Installs Zotero with winget, pinned to the package DigitalScholar.Zotero in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Zotero without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install DigitalScholar.Zotero with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/DigitalScholar/Zotero>, <https://www.zotero.org/>, <https://www.zotero.org/support/licensing>

#### calibre

`app.calibre` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Manages and converts an e-book library.

- **What it does:** Installs calibre with winget, pinned to the package calibre.calibre in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get calibre without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install calibre.calibre with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/calibre/calibre>, <https://calibre-ebook.com/>, <https://github.com/kovidgoyal/calibre/blob/HEAD/LICENSE>

### Runtimes

#### .NET Desktop Runtime 10

`app.net-desktop-runtime-10` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Runs desktop programs built for .NET 10.

- **What it does:** Installs .NET Desktop Runtime 10 with winget, pinned to the package Microsoft.DotNet.DesktopRuntime.10 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get .NET Desktop Runtime 10 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Microsoft.DotNet.DesktopRuntime.10 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/DotNet/DesktopRuntime/10>, <https://dotnet.microsoft.com>

#### .NET Desktop Runtime 6

`app.net-desktop-runtime-6` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Runs desktop programs built for .NET 6. This version is out of support.

- **What it does:** Installs .NET Desktop Runtime 6 with winget, pinned to the package Microsoft.DotNet.DesktopRuntime.6 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get .NET Desktop Runtime 6 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Microsoft.DotNet.DesktopRuntime.6 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/DotNet/DesktopRuntime/6>, <https://dotnet.microsoft.com/en-us/download/dotnet/6.0>

#### .NET Desktop Runtime 8

`app.net-desktop-runtime-8` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Runs desktop programs built for .NET 8.

- **What it does:** Installs .NET Desktop Runtime 8 with winget, pinned to the package Microsoft.DotNet.DesktopRuntime.8 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get .NET Desktop Runtime 8 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Microsoft.DotNet.DesktopRuntime.8 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/DotNet/DesktopRuntime/8>, <https://dotnet.microsoft.com>

#### .NET Desktop Runtime 9

`app.net-desktop-runtime-9` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Runs desktop programs built for .NET 9.

- **What it does:** Installs .NET Desktop Runtime 9 with winget, pinned to the package Microsoft.DotNet.DesktopRuntime.9 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get .NET Desktop Runtime 9 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Microsoft.DotNet.DesktopRuntime.9 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/DotNet/DesktopRuntime/9>, <https://dotnet.microsoft.com>

#### Amazon Corretto 21 JDK

`app.amazon-corretto-21-jdk` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Amazon's build of the Java 21 development kit.

- **What it does:** Installs Amazon Corretto 21 JDK with winget, pinned to the package Amazon.Corretto.21.JDK in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Amazon Corretto 21 JDK without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Amazon.Corretto.21.JDK with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Amazon/Corretto/21/JDK>, <https://docs.aws.amazon.com/corretto/latest/corretto-21-ug/what-is-corretto-21.html>, <https://aws.amazon.com/corretto/faqs/#Licensing_and_Open_Source>

#### Amazon Corretto 25 JDK

`app.amazon-corretto-25-jdk` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Amazon's build of the Java 25 development kit.

- **What it does:** Installs Amazon Corretto 25 JDK with winget, pinned to the package Amazon.Corretto.25.JDK in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Amazon Corretto 25 JDK without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Amazon.Corretto.25.JDK with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Amazon/Corretto/25/JDK>, <https://docs.aws.amazon.com/corretto/>, <https://aws.amazon.com/corretto/faqs/#Licensing_and_Open_Source>

#### Amazon Corretto 8 JDK

`app.amazon-corretto-8-jdk` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Amazon's build of the Java 8 development kit.

- **What it does:** Installs Amazon Corretto 8 JDK with winget, pinned to the package Amazon.Corretto.8.JDK in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Amazon Corretto 8 JDK without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Amazon.Corretto.8.JDK with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/Amazon/Corretto/8/JDK>, <https://docs.aws.amazon.com/corretto/latest/corretto-8-ug/what-is-corretto-8.html>, <https://aws.amazon.com/corretto/faqs/#Licensing_and_Open_Source>

#### Visual C++ 2015 to 2022 Redistributable (32-bit)

`app.visual-c-2015-to-2022-redistributable-32-bit` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

The 32-bit Microsoft C++ runtime that older programs and games need.

- **What it does:** Installs Visual C++ 2015 to 2022 Redistributable (32-bit) with winget, pinned to the package Microsoft.VCRedist.2015+.x86 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Visual C++ 2015 to 2022 Redistributable (32-bit) without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Microsoft.VCRedist.2015+.x86 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/VCRedist/2015+/x86>, <https://learn.microsoft.com/cpp/windows/latest-supported-vc-redist>, <https://www.microsoft.com/legal/terms-of-use>

#### Visual C++ 2015 to 2022 Redistributable (64-bit)

`app.visual-c-2015-to-2022-redistributable-64-bit` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

The Microsoft C++ runtime that many programs and games need.

- **What it does:** Installs Visual C++ 2015 to 2022 Redistributable (64-bit) with winget, pinned to the package Microsoft.VCRedist.2015+.x64 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Visual C++ 2015 to 2022 Redistributable (64-bit) without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Other programs may rely on this runtime. Turning it off later runs its uninstaller, and any program that needs it stops starting until it is installed again. Keep it if you are not sure.
- **Changes:**
  - `Install Microsoft.VCRedist.2015+.x64 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/VCRedist/2015+/x64>, <https://learn.microsoft.com/cpp/windows/latest-supported-vc-redist>, <https://www.microsoft.com/legal/terms-of-use>

### Security and privacy

#### 1Password

`app.1password` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A password manager that runs as a subscription.

- **What it does:** Installs 1Password with winget, pinned to the package AgileBits.1Password in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get 1Password without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install AgileBits.1Password with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/AgileBits/1Password>, <https://1password.com/downloads/windows>, <https://1password.com/legal/terms-of-service>

#### Bitwarden

`app.bitwarden` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

An open source password manager.

- **What it does:** Installs Bitwarden with winget, pinned to the package Bitwarden.Bitwarden in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Bitwarden without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Bitwarden.Bitwarden with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/Bitwarden/Bitwarden>, <https://bitwarden.com/>, <https://github.com/bitwarden/clients/blob/HEAD/LICENSE.txt>

#### Cloudflare WARP

`app.cloudflare-warp` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Sends your internet traffic through Cloudflare's network.

- **What it does:** Installs Cloudflare WARP with winget, pinned to the package Cloudflare.Warp in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Cloudflare WARP without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Cloudflare.Warp with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/Cloudflare/Warp>, <https://one.one.one.one/>, <https://www.cloudflare.com/terms/>

#### Cryptomator

`app.cryptomator` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Encrypts the files you keep in cloud storage.

- **What it does:** Installs Cryptomator with winget, pinned to the package Cryptomator.Cryptomator in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Cryptomator without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Cryptomator.Cryptomator with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/Cryptomator/Cryptomator>, <https://cryptomator.org/>, <https://github.com/cryptomator/cryptomator/blob/HEAD/LICENSE.txt>

#### Ente Auth

`app.ente-auth` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

An open source two factor code app with encrypted backups.

- **What it does:** Installs Ente Auth with winget, pinned to the package ente-io.auth-desktop in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Ente Auth without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install ente-io.auth-desktop with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/e/ente-io/auth-desktop>, <https://github.com/ente-io/ente>, <https://github.com/ente-io/ente/blob/HEAD/LICENSE>

#### KeePassXC

`app.keepassxc` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A password manager that keeps your passwords in a file you own.

- **What it does:** Installs KeePassXC with winget, pinned to the package KeePassXCTeam.KeePassXC in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get KeePassXC without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install KeePassXCTeam.KeePassXC with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/k/KeePassXCTeam/KeePassXC>, <https://keepassxc.org/>, <https://github.com/keepassxreboot/keepassxc/blob/HEAD/COPYING>

#### Mullvad VPN

`app.mullvad-vpn` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The client for the Mullvad VPN service.

- **What it does:** Installs Mullvad VPN with winget, pinned to the package MullvadVPN.MullvadVPN in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Mullvad VPN without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install MullvadVPN.MullvadVPN with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/MullvadVPN/MullvadVPN>, <https://mullvad.net/>, <https://github.com/mullvad/mullvadvpn-app/blob/HEAD/LICENSE.md>

#### NetBird

`app.netbird` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A private network between your devices, based on WireGuard.

- **What it does:** Installs NetBird with winget, pinned to the package Netbird.Netbird in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get NetBird without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Netbird.Netbird with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/n/Netbird/Netbird>, <https://netbird.io/>, <https://github.com/netbirdio/netbird/blob/HEAD/LICENSE>

#### OpenVPN Connect

`app.openvpn-connect` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The official client for OpenVPN servers.

- **What it does:** Installs OpenVPN Connect with winget, pinned to the package OpenVPNTechnologies.OpenVPNConnect in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get OpenVPN Connect without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install OpenVPNTechnologies.OpenVPNConnect with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/OpenVPNTechnologies/OpenVPNConnect>, <https://openvpn.net/client/>, <https://openvpn.net/legal/>

#### Proton Authenticator

`app.proton-authenticator` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Proton's two factor code app.

- **What it does:** Installs Proton Authenticator with winget, pinned to the package Proton.ProtonAuthenticator in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Proton Authenticator without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Proton.ProtonAuthenticator with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Proton/ProtonAuthenticator>, <https://proton.me/authenticator>, <https://github.com/protonpass/proton-pass-common/blob/HEAD/LICENSE>

#### Proton Pass

`app.proton-pass` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The password manager from Proton.

- **What it does:** Installs Proton Pass with winget, pinned to the package Proton.ProtonPass in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Proton Pass without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Proton.ProtonPass with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Proton/ProtonPass>, <https://proton.me/pass>, <https://github.com/protonpass/proton-pass-common/blob/HEAD/LICENSE>

#### Proton VPN

`app.proton-vpn` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

The client for the Proton VPN service.

- **What it does:** Installs Proton VPN with winget, pinned to the package Proton.ProtonVPN in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Proton VPN without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Proton.ProtonVPN with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/Proton/ProtonVPN>, <https://protonvpn.com/>, <https://github.com/ProtonVPN/win-app/blob/HEAD/LICENSE>

#### Tailscale

`app.tailscale` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A private network between your own devices.

- **What it does:** Installs Tailscale with winget, pinned to the package Tailscale.Tailscale in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Tailscale without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Tailscale.Tailscale with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/Tailscale/Tailscale>, <https://tailscale.com/download>, <https://github.com/tailscale/tailscale/blob/HEAD/LICENSE>

#### VeraCrypt

`app.veracrypt` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Encrypts whole drives and creates encrypted file containers.

- **What it does:** Installs VeraCrypt with winget, pinned to the package IDRIX.VeraCrypt in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get VeraCrypt without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install IDRIX.VeraCrypt with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/i/IDRIX/VeraCrypt>, <https://veracrypt.io/>, <https://veracrypt.io/en/VeraCrypt%20License.html>

#### WireGuard

`app.wireguard` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A small, fast VPN client.

- **What it does:** Installs WireGuard with winget, pinned to the package WireGuard.WireGuard in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get WireGuard without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install WireGuard.WireGuard with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/WireGuard/WireGuard>, <https://www.wireguard.com/install/>, <https://git.zx2c4.com/wireguard-windows/tree/COPYING>

#### simplewall

`app.simplewall` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

A small front end for the Windows Filtering Platform, to block or allow programs.

- **What it does:** Installs simplewall with winget, pinned to the package Henry++.simplewall in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get simplewall without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how Windows looks or behaves, and a Windows update can break it. Turning it off runs its uninstaller.
- **Changes:**
  - `Install Henry++.simplewall with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/h/Henry++/simplewall>, <https://github.com/henrypp/simplewall/releases/tag/v.3.8.7>, <https://github.com/henrypp/simplewall/blob/HEAD/LICENSE>

### System tools

#### Advanced IP Scanner

`app.advanced-ip-scanner` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Finds the devices on your network.

- **What it does:** Installs Advanced IP Scanner with winget, pinned to the package Famatech.AdvancedIPScanner in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Advanced IP Scanner without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Famatech.AdvancedIPScanner with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/f/Famatech/AdvancedIPScanner>, <https://www.advanced-ip-scanner.com/>

#### Angry IP Scanner

`app.angry-ip-scanner` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A fast network scanner.

- **What it does:** Installs Angry IP Scanner with winget, pinned to the package angryziber.AngryIPScanner in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Angry IP Scanner without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install angryziber.AngryIPScanner with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/angryziber/AngryIPScanner>, <https://angryip.org/>, <https://github.com/angryip/ipscan/blob/HEAD/LICENSE>

#### AnyDesk

`app.anydesk` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Remote access to a PC from another device.

- **What it does:** Installs AnyDesk with winget, pinned to the package AnyDesk.AnyDesk in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get AnyDesk without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Once you set it up, this program lets someone connect to this PC. Use a strong password, allow only people you trust, and turn it off or uninstall it when you do not need it.
- **Changes:**
  - `Install AnyDesk.AnyDesk with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/AnyDesk/AnyDesk>

#### AutoHotkey

`app.autohotkey` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A scripting language for keyboard shortcuts and automation.

- **What it does:** Installs AutoHotkey with winget, pinned to the package AutoHotkey.AutoHotkey in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get AutoHotkey without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install AutoHotkey.AutoHotkey with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/AutoHotkey/AutoHotkey>, <https://github.com/AutoHotkey/AutoHotkey/blob/alpha/license.txt>

#### Autoruns

`app.autoruns` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shows everything that starts with Windows.

- **What it does:** Installs Autoruns with winget, pinned to the package Microsoft.Sysinternals.Autoruns in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Autoruns without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.Sysinternals.Autoruns with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/Sysinternals/Autoruns>, <https://learn.microsoft.com/en-us/sysinternals/downloads/autoruns>, <https://learn.microsoft.com/en-us/sysinternals/license-terms>

#### Blur AutoClicker

`app.blur-autoclicker` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

An open source auto clicker that clicks at a rate you set.

- **What it does:** Installs Blur AutoClicker with winget, pinned to the package Blur009.BlurAutoClicker in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Blur AutoClicker without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Many online games treat an auto clicker as cheating and ban the account. Check the game's rules before you use it.
- **Changes:**
  - `Install Blur009.BlurAutoClicker with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/Blur009/BlurAutoClicker>, <https://github.com/Blur009/Blur-AutoClicker>, <https://github.com/Blur009/Blur-AutoClicker/blob/HEAD/LICENSE>

#### Bulk Crap Uninstaller

`app.bulk-crap-uninstaller` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Uninstalls several programs at once and cleans up leftovers.

- **What it does:** Installs Bulk Crap Uninstaller with winget, pinned to the package Klocman.BulkCrapUninstaller in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Bulk Crap Uninstaller without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Klocman.BulkCrapUninstaller with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/k/Klocman/BulkCrapUninstaller>, <https://www.bcuninstaller.com/>, <https://github.com/BCUninstaller/Bulk-Crap-Uninstaller/blob/HEAD/Licence.txt>

#### CPU-Z

`app.cpu-z` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shows details about the processor, memory and motherboard.

- **What it does:** Installs CPU-Z with winget, pinned to the package CPUID.CPU-Z in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get CPU-Z without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install CPUID.CPU-Z with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/CPUID/CPU-Z>, <https://www.cpuid.com/softwares/cpu-z.html>, <https://www.cpuid.com/terms-of-service.html>

#### Cinebench R23

`app.cinebench-r23` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A CPU benchmark that renders a scene.

- **What it does:** Installs Cinebench R23 with winget, pinned to the package Maxon.CinebenchR23 in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Cinebench R23 without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Maxon.CinebenchR23 with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Maxon/CinebenchR23>, <https://www.maxon.net/en/cinebench>

#### CrystalDiskInfo

`app.crystaldiskinfo` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shows the health of hard drives and SSDs.

- **What it does:** Installs CrystalDiskInfo with winget, pinned to the package CrystalDewWorld.CrystalDiskInfo in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get CrystalDiskInfo without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install CrystalDewWorld.CrystalDiskInfo with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/CrystalDewWorld/CrystalDiskInfo>, <https://crystalmark.info/en/software/crystaldiskinfo/>, <https://crystalmark.info/en/software/crystaldiskinfo/crystaldiskinfo-license/>

#### CrystalDiskMark

`app.crystaldiskmark` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Measures how fast a drive reads and writes.

- **What it does:** Installs CrystalDiskMark with winget, pinned to the package CrystalDewWorld.CrystalDiskMark in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get CrystalDiskMark without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install CrystalDewWorld.CrystalDiskMark with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/CrystalDewWorld/CrystalDiskMark>, <https://crystalmark.info/en/software/crystaldiskmark/>, <https://crystalmark.info/en/software/crystaldiskmark/crystaldiskmark-license/>

#### DISMTools

`app.dismtools` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

A graphical front end for DISM, the Windows image servicing tool.

- **What it does:** Installs DISMTools with winget, pinned to the package CodingWondersSoftware.DISMTools.Stable in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get DISMTools without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It edits Windows images and settings, and a wrong choice can leave an image that will not start. Keep a backup of anything you edit.
- **Changes:**
  - `Install CodingWondersSoftware.DISMTools.Stable with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/CodingWondersSoftware/DISMTools/Stable>, <https://github.com/CodingWonders/DISMTools>, <https://github.com/CodingWonders/DISMTools/blob/stable/LICENSE>

#### Deskflow

`app.deskflow` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shares one keyboard and mouse across several PCs.

- **What it does:** Installs Deskflow with winget, pinned to the package Deskflow.Deskflow in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Deskflow without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Deskflow.Deskflow with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/Deskflow/Deskflow>, <https://github.com/deskflow/deskflow>, <https://github.com/deskflow/deskflow/blob/HEAD/LICENSE>

#### Display Driver Uninstaller

`app.ddu` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Removes graphics drivers completely before a clean install.

- **What it does:** Installs Display Driver Uninstaller with winget, pinned to the package Wagnardsoft.DisplayDriverUninstaller in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Display Driver Uninstaller without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Wagnardsoft.DisplayDriverUninstaller with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/Wagnardsoft/DisplayDriverUninstaller>, <https://www.wagnardsoft.com/display-driver-uninstaller-ddu>, <https://github.com/Wagnard/display-drivers-uninstaller/blob/WPF/LICENSE>

#### EarTrumpet

`app.eartrumpet` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A volume control with a slider for each app.

- **What it does:** Installs EarTrumpet with winget, pinned to the package File-New-Project.EarTrumpet in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get EarTrumpet without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install File-New-Project.EarTrumpet with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/f/File-New-Project/EarTrumpet>, <https://eartrumpet.app>, <https://github.com/File-New-Project/EarTrumpet/blob/master/LICENSE>

#### Everything

`app.everything` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Finds files on NTFS drives instantly by name.

- **What it does:** Installs Everything with winget, pinned to the package voidtools.Everything in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Everything without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install voidtools.Everything with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/v/voidtools/Everything>, <https://www.voidtools.com/>, <https://www.voidtools.com/License.txt>

#### FanControl

`app.fancontrol` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Sets fan speeds from temperature curves.

- **What it does:** Installs FanControl with winget, pinned to the package Rem0o.FanControl in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get FanControl without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Rem0o.FanControl with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/Rem0o/FanControl>, <https://getfancontrol.com/>, <https://github.com/Rem0o/FanControl.Releases/blob/master/LICENSE>

#### Flow Launcher

`app.flow-launcher` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A keyboard launcher for apps, files and web searches.

- **What it does:** Installs Flow Launcher with winget, pinned to the package Flow-Launcher.Flow-Launcher in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Flow Launcher without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Flow-Launcher.Flow-Launcher with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/f/Flow-Launcher/Flow-Launcher>, <https://github.com/Flow-Launcher/Flow.Launcher>, <https://github.com/Flow-Launcher/Flow.Launcher/blob/HEAD/LICENSE>

#### GPU-Z

`app.gpu-z` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shows details and live readings of the graphics card.

- **What it does:** Installs GPU-Z with winget, pinned to the package TechPowerUp.GPU-Z in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get GPU-Z without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install TechPowerUp.GPU-Z with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/TechPowerUp/GPU-Z>, <https://www.techpowerup.com/gpuz/>

#### GlazeWM

`app.glazewm` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

A tiling window manager for Windows.

- **What it does:** Installs GlazeWM with winget, pinned to the package glzr-io.glazewm in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get GlazeWM without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how Windows looks or behaves, and a Windows update can break it. Turning it off runs its uninstaller.
- **Changes:**
  - `Install glzr-io.glazewm with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/glzr-io/glazewm>, <https://github.com/glzr-io/glazewm>, <https://github.com/glzr-io/glazewm/blob/HEAD/LICENSE.md>

#### HWMonitor

`app.hwmonitor` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shows temperatures, voltages and fan speeds.

- **What it does:** Installs HWMonitor with winget, pinned to the package CPUID.HWMonitor in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get HWMonitor without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install CPUID.HWMonitor with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/CPUID/HWMonitor>, <https://www.cpuid.com/softwares/hwmonitor.html>, <https://www.cpuid.com/softwares/hwmonitor-pro/eula.html>

#### HWiNFO

`app.hwinfo` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Detailed hardware information and live sensor readings.

- **What it does:** Installs HWiNFO with winget, pinned to the package REALiX.HWiNFO in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get HWiNFO without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install REALiX.HWiNFO with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/REALiX/HWiNFO>, <https://www.hwinfo.com/download/>, <https://www.hwinfo.com/files/license.pdf>

#### HxD

`app.hxd` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

A hex editor that also edits raw disks and memory.

- **What it does:** Installs HxD with winget, pinned to the package MHNexus.HxD in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get HxD without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how Windows looks or behaves, and a Windows update can break it. Turning it off runs its uninstaller.
- **Changes:**
  - `Install MHNexus.HxD with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/MHNexus/HxD>, <https://mh-nexus.de/en/hxd/>, <https://mh-nexus.de/en/hxd/license.php>

#### LibreHardwareMonitor

`app.libre-hardware-monitor` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

An open source monitor for temperatures, fan speeds and load.

- **What it does:** Installs LibreHardwareMonitor with winget, pinned to the package LibreHardwareMonitor.LibreHardwareMonitor in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get LibreHardwareMonitor without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install LibreHardwareMonitor.LibreHardwareMonitor with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/l/LibreHardwareMonitor/LibreHardwareMonitor>, <https://github.com/LibreHardwareMonitor/LibreHardwareMonitor>, <https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/blob/HEAD/LICENSE>

#### MSEdgeRedirect

`app.msedgeredirect` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Sends Windows links that force Edge to your own default browser.

- **What it does:** Installs MSEdgeRedirect with winget, pinned to the package rcmaehl.MSEdgeRedirect in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get MSEdgeRedirect without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how Windows looks or behaves, and a Windows update can break it. Turning it off runs its uninstaller.
- **Changes:**
  - `Install rcmaehl.MSEdgeRedirect with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/rcmaehl/MSEdgeRedirect>, <https://github.com/rcmaehl/MSEdgeRedirect>, <https://github.com/rcmaehl/MSEdgeRedirect/blob/HEAD/LICENSE>

#### MSI Afterburner

`app.msi-afterburner` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Graphics card monitoring and overclocking.

- **What it does:** Installs MSI Afterburner with winget, pinned to the package Guru3D.Afterburner in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get MSI Afterburner without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Guru3D.Afterburner with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/Guru3D/Afterburner>

#### MiniTool Partition Wizard Free

`app.minitool-partition-wizard-free` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Resizes, moves and copies disk partitions.

- **What it does:** Installs MiniTool Partition Wizard Free with winget, pinned to the package MiniTool.PartitionWizard.Free in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get MiniTool Partition Wizard Free without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It edits Windows images and settings, and a wrong choice can leave an image that will not start. Keep a backup of anything you edit.
- **Changes:**
  - `Install MiniTool.PartitionWizard.Free with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/MiniTool/PartitionWizard/Free>, <https://www.partitionwizard.com/free-partition-manager.html>, <https://www.partitionwizard.com/terms_conditions.html>

#### NTLite

`app.ntlite` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Edits and slims down Windows images.

- **What it does:** Installs NTLite with winget, pinned to the package Nlitesoft.NTLite in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get NTLite without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It edits Windows images and settings, and a wrong choice can leave an image that will not start. Keep a backup of anything you edit.
- **Changes:**
  - `Install Nlitesoft.NTLite with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/n/Nlitesoft/NTLite>, <https://ntlite.com/download/>, <https://ntlite.com/eula/>

#### NVCleanstall

`app.nvcleanstall` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Installs an NVIDIA graphics driver without the extras you do not pick.

- **What it does:** Installs NVCleanstall with winget, pinned to the package TechPowerUp.NVCleanstall in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get NVCleanstall without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how drivers are installed, and a wrong choice can leave a driver that does not work. .Clarion makes a restore point first unless you untick it.
- **Changes:**
  - `Install TechPowerUp.NVCleanstall with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/TechPowerUp/NVCleanstall>, <https://www.techpowerup.com/download/techpowerup-nvcleanstall/>

#### Nilesoft Shell

`app.nilesoft-shell` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Lets you choose what is in the File Explorer right click menu.

- **What it does:** Installs Nilesoft Shell with winget, pinned to the package Nilesoft.Shell in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Nilesoft Shell without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how Windows looks or behaves, and a Windows update can break it. Turning it off runs its uninstaller.
- **Changes:**
  - `Install Nilesoft.Shell with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/n/Nilesoft/Shell>, <https://nilesoft.org/>, <https://raw.githubusercontent.com/moudey/shell/main/LICENSE>

#### Nmap

`app.nmap` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Scans a network for devices and open ports.

- **What it does:** Installs Nmap with winget, pinned to the package Insecure.Nmap in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Nmap without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Insecure.Nmap with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/i/Insecure/Nmap>, <https://nmap.org>, <https://nmap.org/book/man-legal.html>

#### OFGB (Oh Frick Go Back)

`app.ofgb-oh-frick-go-back` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Turns off the ads Windows shows in its own screens.

- **What it does:** Installs OFGB (Oh Frick Go Back) with winget, pinned to the package xM4ddy.OFGB in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get OFGB (Oh Frick Go Back) without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how Windows looks or behaves, and a Windows update can break it. Turning it off runs its uninstaller.
- **Changes:**
  - `Install xM4ddy.OFGB with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/x/xM4ddy/OFGB>, <https://github.com/xM4ddy/OFGB>, <https://raw.githubusercontent.com/xM4ddy/OFGB/main/LICENSE>

#### OP Auto Clicker

`app.op-auto-clicker` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Clicks the mouse for you at a rate you set. Free for non commercial use.

- **What it does:** Installs OP Auto Clicker with winget, pinned to the package OPAutoClicker.OPAutoClicker in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get OP Auto Clicker without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Many online games treat an auto clicker as cheating and ban the account. Check the game's rules before you use it.
- **Changes:**
  - `Install OPAutoClicker.OPAutoClicker with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/OPAutoClicker/OPAutoClicker>, <https://www.opautoclicker.com/>

#### OpenRGB

`app.openrgb` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Controls RGB lighting from many brands in one app.

- **What it does:** Installs OpenRGB with winget, pinned to the package OpenRGB.OpenRGB in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get OpenRGB without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install OpenRGB.OpenRGB with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/OpenRGB/OpenRGB>, <https://openrgb.org/>, <https://github.com/CalcProgrammer1/OpenRGB/blob/HEAD/LICENSE>

#### Policy Plus

`app.policy-plus` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

A Group Policy editor that works on Windows Home too.

- **What it does:** Installs Policy Plus with winget, pinned to the package Fleex255.PolicyPlus in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Policy Plus without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how Windows looks or behaves, and a Windows update can break it. Turning it off runs its uninstaller.
- **Changes:**
  - `Install Fleex255.PolicyPlus with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/f/Fleex255/PolicyPlus>, <https://github.com/Fleex255/PolicyPlus>

#### PowerToys

`app.powertoys` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Microsoft's set of Windows power user tools.

- **What it does:** Installs PowerToys with winget, pinned to the package Microsoft.PowerToys in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get PowerToys without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.PowerToys with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/PowerToys>, <https://github.com/microsoft/PowerToys>, <https://github.com/microsoft/PowerToys/blob/HEAD/LICENSE>

#### Process Explorer

`app.process-explorer` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A detailed view of running processes, from Microsoft Sysinternals.

- **What it does:** Installs Process Explorer with winget, pinned to the package Microsoft.Sysinternals.ProcessExplorer in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Process Explorer without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.Sysinternals.ProcessExplorer with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/Sysinternals/ProcessExplorer>, <https://learn.microsoft.com/en-us/sysinternals/downloads/process-explorer>, <https://learn.microsoft.com/en-us/sysinternals/license-terms>

#### Process Lasso

`app.process-lasso` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Manages process priorities and CPU use.

- **What it does:** Installs Process Lasso with winget, pinned to the package BitSum.ProcessLasso in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Process Lasso without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how Windows looks or behaves, and a Windows update can break it. Turning it off runs its uninstaller.
- **Changes:**
  - `Install BitSum.ProcessLasso with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/BitSum/ProcessLasso>, <https://bitsum.com/>, <https://bitsum.com/terms-of-use/>

#### Process Monitor

`app.process-monitor` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shows live file, registry and process activity, from Microsoft Sysinternals.

- **What it does:** Installs Process Monitor with winget, pinned to the package Microsoft.Sysinternals.ProcessMonitor in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Process Monitor without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.Sysinternals.ProcessMonitor with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/Sysinternals/ProcessMonitor>, <https://learn.microsoft.com/en-us/sysinternals/downloads/procmon>, <https://learn.microsoft.com/en-us/sysinternals/license-terms>

#### PuTTY

`app.putty` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

An SSH and telnet client.

- **What it does:** Installs PuTTY with winget, pinned to the package PuTTY.PuTTY in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get PuTTY without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install PuTTY.PuTTY with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/p/PuTTY/PuTTY>, <https://www.chiark.greenend.org.uk/~sgtatham/putty/>, <https://www.chiark.greenend.org.uk/~sgtatham/putty/licence.html>

#### Remote Desktop Connection Manager

`app.remote-desktop-connection-manager` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Manages many Remote Desktop connections in one window.

- **What it does:** Installs Remote Desktop Connection Manager with winget, pinned to the package Microsoft.Sysinternals.RDCMan in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Remote Desktop Connection Manager without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.Sysinternals.RDCMan with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/Sysinternals/RDCMan>, <https://learn.microsoft.com/en-US/sysinternals/downloads/rdcman>, <https://learn.microsoft.com/en-us/sysinternals/license-terms>

#### Revo Uninstaller

`app.revo-uninstaller` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Uninstalls programs and scans for leftovers.

- **What it does:** Installs Revo Uninstaller with winget, pinned to the package RevoUninstaller.RevoUninstaller in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Revo Uninstaller without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install RevoUninstaller.RevoUninstaller with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/RevoUninstaller/RevoUninstaller>, <https://www.revouninstaller.com/products/revo-uninstaller-free/>

#### Rufus

`app.rufus` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Makes bootable USB drives.

- **What it does:** Installs Rufus with winget, pinned to the package Rufus.Rufus in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Rufus without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Rufus.Rufus with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/r/Rufus/Rufus>, <https://rufus.ie/>, <https://github.com/pbatard/rufus/blob/HEAD/LICENSE.txt>

#### SignalRGB

`app.signalrgb` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Controls RGB lighting on many brands from one app.

- **What it does:** Installs SignalRGB with winget, pinned to the package WhirlwindFX.SignalRgb in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get SignalRGB without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install WhirlwindFX.SignalRgb with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/WhirlwindFX/SignalRgb>, <https://www.signalrgb.com/>, <https://www.signalrgb.com/terms>

#### Snappy Driver Installer Origin

`app.snappy-driver-installer-origin` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Finds and installs drivers from an offline collection.

- **What it does:** Installs Snappy Driver Installer Origin with winget, pinned to the package GlennDelahoy.SnappyDriverInstallerOrigin in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Snappy Driver Installer Origin without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how drivers are installed, and a wrong choice can leave a driver that does not work. .Clarion makes a restore point first unless you untick it.
- **Changes:**
  - `Install GlennDelahoy.SnappyDriverInstallerOrigin with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/GlennDelahoy/SnappyDriverInstallerOrigin>, <https://www.glenn.delahoy.com/snappy-driver-installer-origin/>, <https://sourceforge.net/p/snappy-driver-installer-origin/code/HEAD/tree/trunk/resources/gpl-3.0.txt>

#### StartAllBack

`app.startallback` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Brings back the classic Start menu and taskbar styles.

- **What it does:** Installs StartAllBack with winget, pinned to the package StartIsBack.StartAllBack in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get StartAllBack without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. It changes how Windows looks or behaves, and a Windows update can break it. Turning it off runs its uninstaller.
- **Changes:**
  - `Install StartIsBack.StartAllBack with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/s/StartIsBack/StartAllBack>, <https://www.startallback.com/>, <https://www.startisback.com/license.txt>

#### System Informer

`app.system-informer` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Watches processes, services and the network, and can end them.

- **What it does:** Installs System Informer with winget, pinned to the package WinsiderSS.SystemInformer in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get System Informer without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install WinsiderSS.SystemInformer with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/WinsiderSS/SystemInformer>, <https://system-informer.com/>, <https://github.com/winsiderss/systeminformer/blob/master/LICENSE.txt>

#### TCPView

`app.tcpview` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Lists every TCP and UDP connection and the program that owns it.

- **What it does:** Installs TCPView with winget, pinned to the package Microsoft.Sysinternals.TCPView in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get TCPView without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Microsoft.Sysinternals.TCPView with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/m/Microsoft/Sysinternals/TCPView>, <https://learn.microsoft.com/en-us/sysinternals/downloads/tcpview>, <https://learn.microsoft.com/en-us/sysinternals/license-terms>

#### TeamViewer

`app.teamviewer` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

Remote access and support.

- **What it does:** Installs TeamViewer with winget, pinned to the package TeamViewer.TeamViewer in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get TeamViewer without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Once you set it up, this program lets someone connect to this PC. Use a strong password, allow only people you trust, and turn it off or uninstall it when you do not need it.
- **Changes:**
  - `Install TeamViewer.TeamViewer with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/t/TeamViewer/TeamViewer>, <https://www.teamviewer.com/en/download/windows/>, <https://www.teamviewer.com/en/legal/eula/>

#### TightVNC

`app.tightvnc` · Medium · Documented by the vendor · Optional: your choice · checked on build 26200

A VNC server and viewer for remote control.

- **What it does:** Installs TightVNC with winget, pinned to the package GlavSoft.TightVNC in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get TightVNC without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Medium. Once you set it up, this program lets someone connect to this PC. Use a strong password, allow only people you trust, and turn it off or uninstall it when you do not need it.
- **Changes:**
  - `Install GlavSoft.TightVNC with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/g/GlavSoft/TightVNC>, <https://www.tightvnc.com/>, <https://www.gnu.org/licenses/old-licenses/gpl-2.0.html>

#### TranslucentTB

`app.translucenttb` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Makes the Windows taskbar transparent.

- **What it does:** Installs TranslucentTB with winget, pinned to the package CharlesMilette.TranslucentTB in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get TranslucentTB without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install CharlesMilette.TranslucentTB with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/c/CharlesMilette/TranslucentTB>, <https://github.com/TranslucentTB/TranslucentTB>, <https://github.com/TranslucentTB/TranslucentTB/blob/HEAD/LICENSE.md>

#### TreeSize Free

`app.treesize-free` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shows which folders use the most space.

- **What it does:** Installs TreeSize Free with winget, pinned to the package JAMSoftware.TreeSize.Free in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get TreeSize Free without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install JAMSoftware.TreeSize.Free with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/j/JAMSoftware/TreeSize/Free>, <https://www.jam-software.com/treesize_free>, <https://www.jam-software.com/company/license_agreement.shtml>

#### UniGetUI

`app.unigetui` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A graphical front end for winget and other package managers.

- **What it does:** Installs UniGetUI with winget, pinned to the package Devolutions.UniGetUI in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get UniGetUI without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Devolutions.UniGetUI with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/d/Devolutions/UniGetUI>, <https://devolutions.net/unigetui/>, <https://github.com/Devolutions/UniGetUI/blob/HEAD/LICENSE>

#### Ventoy

`app.ventoy` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Makes one USB drive boot many different installers.

- **What it does:** Installs Ventoy with winget, pinned to the package Ventoy.Ventoy in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Ventoy without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Ventoy.Ventoy with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/v/Ventoy/Ventoy>, <https://www.ventoy.net/>, <https://github.com/ventoy/Ventoy/blob/master/COPYING>

#### VirtualBox

`app.virtualbox` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Runs other operating systems in virtual machines.

- **What it does:** Installs VirtualBox with winget, pinned to the package Oracle.VirtualBox in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get VirtualBox without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Oracle.VirtualBox with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/o/Oracle/VirtualBox>, <https://www.virtualbox.org/>, <https://www.virtualbox.org/wiki/Licensing_FAQ>

#### WinDirStat

`app.windirstat` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shows disk use as a map of coloured blocks.

- **What it does:** Installs WinDirStat with winget, pinned to the package WinDirStat.WinDirStat in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get WinDirStat without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install WinDirStat.WinDirStat with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/WinDirStat/WinDirStat>, <https://github.com/windirstat/windirstat/releases/>, <https://github.com/windirstat/windirstat/blob/HEAD/LICENSE.md>

#### WinMerge

`app.winmerge` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Compares and merges files and folders.

- **What it does:** Installs WinMerge with winget, pinned to the package WinMerge.WinMerge in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get WinMerge without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install WinMerge.WinMerge with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/WinMerge/WinMerge>, <https://winmerge.org/>, <https://github.com/WinMerge/winmerge/blob/v2.16.46/LICENSE.md>

#### Wireshark

`app.wireshark` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Captures and inspects network traffic.

- **What it does:** Installs Wireshark with winget, pinned to the package WiresharkFoundation.Wireshark in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Wireshark without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install WiresharkFoundation.Wireshark with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/WiresharkFoundation/Wireshark>, <https://www.wireshark.org/>, <https://gitlab.com/wireshark/wireshark/-/blob/HEAD/COPYING>

#### Wise Program Uninstaller

`app.wise-program-uninstaller` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Removes programs and the leftovers they leave behind.

- **What it does:** Installs Wise Program Uninstaller with winget, pinned to the package WiseCleaner.WiseProgramUninstaller in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Wise Program Uninstaller without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install WiseCleaner.WiseProgramUninstaller with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/w/WiseCleaner/WiseProgramUninstaller>, <https://www.wisecleaner.com/wise-program-uninstaller.html>

#### WizTree

`app.wiztree` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Shows what is using the space on a disk.

- **What it does:** Installs WizTree with winget, pinned to the package AntibodySoftware.WizTree in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get WizTree without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install AntibodySoftware.WizTree with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/a/AntibodySoftware/WizTree>, <https://diskanalyzer.com/>, <https://diskanalyzer.com/eula>

#### balenaEtcher

`app.balena-etcher` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Writes disk images to USB drives and SD cards.

- **What it does:** Installs balenaEtcher with winget, pinned to the package Balena.Etcher in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get balenaEtcher without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Balena.Etcher with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/Balena/Etcher>, <https://etcher.balena.io/>, <https://github.com/balena-io/etcher/blob/HEAD/LICENSE>

#### f.lux

`app.f-lux` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Warms the screen colour at night.

- **What it does:** Installs f.lux with winget, pinned to the package flux.flux in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get f.lux without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install flux.flux with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/f/flux/flux>, <https://justgetflux.com/>, <https://justgetflux.com/eula/>

## Network

### DNS

#### Use AdGuard DNS

`dns.adguard` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Look up websites through AdGuard instead of your provider.

- **What it does:** Sets the DNS servers of every connected network adapter to 94.140.14.14 and 94.140.15.15. A resolver whose default servers block ads, trackers and known malicious domains.
- **Benefit:** Websites can load faster and you are no longer tied to your internet provider DNS.
- **Risk:** Low. Networks that require their own DNS, such as work and school networks, may stop resolving internal names.
- **Changes:**
  - `Use adguard DNS on connected network adapters`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://adguard-dns.io/en/public-dns.html>

#### Use AdGuard DNS with encryption

`dns.adguard-encrypted` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Use AdGuard and encrypt the lookups so others on the network cannot read them.

- **What it does:** Sets the DNS servers of every connected network adapter to 94.140.14.14 and 94.140.15.15 and turns on encrypted DNS (DNS over HTTPS) for them. A resolver whose default servers block ads, trackers and known malicious domains.
- **Benefit:** Websites can load faster, and the sites you look up are hidden from anyone watching the network between you and the resolver.
- **Risk:** Low. Some networks that force their own DNS, such as work and school networks, may stop resolving names.
- **Changes:**
  - `Use adguard DNS with encryption on connected network adapters`
- **Needs:** Windows build 22000 or later; administrator rights, applies to every account
- **Sources:** <https://adguard-dns.io/en/public-dns.html>, <https://learn.microsoft.com/en-us/windows-server/networking/dns/doh-client-support>

#### Use Cloudflare DNS

`dns.cloudflare` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Look up websites through Cloudflare instead of your provider.

- **What it does:** Sets the DNS servers of every connected network adapter to 1.1.1.1 and 1.0.0.1. A fast public resolver run by Cloudflare. It does not filter anything.
- **Benefit:** Websites can load faster and you are no longer tied to your internet provider DNS.
- **Risk:** Low. Networks that require their own DNS, such as work and school networks, may stop resolving internal names.
- **Changes:**
  - `Use cloudflare DNS on connected network adapters`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://developers.cloudflare.com/1.1.1.1/ip-addresses/>

#### Use Cloudflare DNS with encryption

`dns.cloudflare-encrypted` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Use Cloudflare and encrypt the lookups so others on the network cannot read them.

- **What it does:** Sets the DNS servers of every connected network adapter to 1.1.1.1 and 1.0.0.1 and turns on encrypted DNS (DNS over HTTPS) for them. A fast public resolver run by Cloudflare. It does not filter anything.
- **Benefit:** Websites can load faster, and the sites you look up are hidden from anyone watching the network between you and the resolver.
- **Risk:** Low. Some networks that force their own DNS, such as work and school networks, may stop resolving names.
- **Changes:**
  - `Use cloudflare DNS with encryption on connected network adapters`
- **Needs:** Windows build 22000 or later; administrator rights, applies to every account
- **Sources:** <https://developers.cloudflare.com/1.1.1.1/ip-addresses/>, <https://learn.microsoft.com/en-us/windows-server/networking/dns/doh-client-support>

#### Use Google DNS

`dns.google` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Look up websites through Google instead of your provider.

- **What it does:** Sets the DNS servers of every connected network adapter to 8.8.8.8 and 8.8.4.4. The long running public resolver run by Google. It does not filter anything.
- **Benefit:** Websites can load faster and you are no longer tied to your internet provider DNS.
- **Risk:** Low. Networks that require their own DNS, such as work and school networks, may stop resolving internal names.
- **Changes:**
  - `Use google DNS on connected network adapters`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://developers.google.com/speed/public-dns/docs/using>

#### Use Google DNS with encryption

`dns.google-encrypted` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Use Google and encrypt the lookups so others on the network cannot read them.

- **What it does:** Sets the DNS servers of every connected network adapter to 8.8.8.8 and 8.8.4.4 and turns on encrypted DNS (DNS over HTTPS) for them. The long running public resolver run by Google. It does not filter anything.
- **Benefit:** Websites can load faster, and the sites you look up are hidden from anyone watching the network between you and the resolver.
- **Risk:** Low. Some networks that force their own DNS, such as work and school networks, may stop resolving names.
- **Changes:**
  - `Use google DNS with encryption on connected network adapters`
- **Needs:** Windows build 22000 or later; administrator rights, applies to every account
- **Sources:** <https://developers.google.com/speed/public-dns/docs/using>, <https://learn.microsoft.com/en-us/windows-server/networking/dns/doh-client-support>

#### Use OpenDNS DNS

`dns.opendns` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Look up websites through OpenDNS instead of your provider.

- **What it does:** Sets the DNS servers of every connected network adapter to 208.67.222.222 and 208.67.220.220. The Cisco run resolver that can add content filtering through an account.
- **Benefit:** Websites can load faster and you are no longer tied to your internet provider DNS.
- **Risk:** Low. Networks that require their own DNS, such as work and school networks, may stop resolving internal names.
- **Changes:**
  - `Use opendns DNS on connected network adapters`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://support.opendns.com/hc/en-us/articles/227986867-Welcome-to-OpenDNS-Support>

#### Use OpenDNS DNS with encryption

`dns.opendns-encrypted` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Use OpenDNS and encrypt the lookups so others on the network cannot read them.

- **What it does:** Sets the DNS servers of every connected network adapter to 208.67.222.222 and 208.67.220.220 and turns on encrypted DNS (DNS over HTTPS) for them. The Cisco run resolver that can add content filtering through an account.
- **Benefit:** Websites can load faster, and the sites you look up are hidden from anyone watching the network between you and the resolver.
- **Risk:** Low. Some networks that force their own DNS, such as work and school networks, may stop resolving names.
- **Changes:**
  - `Use opendns DNS with encryption on connected network adapters`
- **Needs:** Windows build 22000 or later; administrator rights, applies to every account
- **Sources:** <https://support.opendns.com/hc/en-us/articles/227986867-Welcome-to-OpenDNS-Support>, <https://learn.microsoft.com/en-us/windows-server/networking/dns/doh-client-support>

#### Use Quad9 DNS

`dns.quad9` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Look up websites through Quad9 instead of your provider.

- **What it does:** Sets the DNS servers of every connected network adapter to 9.9.9.9 and 149.112.112.112. A non profit resolver that blocks domains known to host malware and phishing.
- **Benefit:** Websites can load faster and you are no longer tied to your internet provider DNS.
- **Risk:** Low. Networks that require their own DNS, such as work and school networks, may stop resolving internal names.
- **Changes:**
  - `Use quad9 DNS on connected network adapters`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://docs.quad9.net/>

#### Use Quad9 DNS with encryption

`dns.quad9-encrypted` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Use Quad9 and encrypt the lookups so others on the network cannot read them.

- **What it does:** Sets the DNS servers of every connected network adapter to 9.9.9.9 and 149.112.112.112 and turns on encrypted DNS (DNS over HTTPS) for them. A non profit resolver that blocks domains known to host malware and phishing.
- **Benefit:** Websites can load faster, and the sites you look up are hidden from anyone watching the network between you and the resolver.
- **Risk:** Low. Some networks that force their own DNS, such as work and school networks, may stop resolving names.
- **Changes:**
  - `Use quad9 DNS with encryption on connected network adapters`
- **Needs:** Windows build 22000 or later; administrator rights, applies to every account
- **Sources:** <https://docs.quad9.net/>, <https://learn.microsoft.com/en-us/windows-server/networking/dns/doh-client-support>

### IPv6

#### Turn off Teredo tunneling

`network.teredo-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Stop Windows from using the Teredo IPv6 tunnel.

- **What it does:** Sets the policy Set Teredo State to Disabled, which is how Microsoft's Windows privacy guide says to turn Teredo off. No Teredo interface is then present on this PC. Revert removes the policy, and Windows goes back to its own default.
- **Benefit:** Removes a tunnel that lets Windows reach IPv6 sites through a Microsoft Teredo server, win1910.ipv6.microsoft.com by default, when the PC has no IPv6 of its own.
- **Risk:** Low. Microsoft says some Xbox gaming features and Delivery Optimization with Group or Internet peering do not work without Teredo.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\TCPIP\v6Transition\Teredo_State to Disabled (String)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/privacy/manage-connections-from-windows-operating-system-components-to-microsoft-services>, <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-admx-tcpip>

### Network

#### Prefer IPv4 over IPv6

`network.prefer-ipv4` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Make Windows try IPv4 first when a site offers both.

- **What it does:** Sets the IPv6 DisabledComponents value to 0x20, the setting Microsoft documents for preferring IPv4. IPv6 itself stays on.
- **Benefit:** Can avoid slow page loads and timeouts on networks where IPv6 is broken or slow, without turning IPv6 off, which Microsoft advises against.
- **Risk:** Low. Needs a restart. Sites and services that only offer IPv6 still work, because IPv6 stays on.
- **Changes:**
  - `Turn bit 0x20 on in HKLM\SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters\DisabledComponents (DWord)`
- **Needs:** administrator rights, applies to every account; a restart
- **Sources:** <https://learn.microsoft.com/en-us/troubleshoot/windows-server/networking/configure-ipv6-in-windows>

#### Stop sharing updates with other PCs

`network.delivery-optimization-http` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Download Windows updates directly, with no peer uploads.

- **What it does:** Sets the Delivery Optimization download mode to HTTP only with no peering.
- **Benefit:** Your connection is not used to upload update files to other PCs.
- **Risk:** None. Updates still download from Microsoft. Hidden on Home, which has no policy support. Use the setting in Windows Update advanced options there.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization\DODownloadMode to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-deliveryoptimization>

## Power

### Power

#### Balanced power plan

`power.plan-balanced` · Safe · Documented by the vendor · Optional: your choice · checked on build 26200

The Windows default. Speed follows what you are doing.

- **What it does:** Makes the Balanced plan the active power plan.
- **Benefit:** Good performance when you need it and lower power use when idle.
- **Risk:** None.
- **Changes:**
  - `Use power plan balanced`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/powercfg-command-line-options>

#### High performance power plan

`power.plan-high-performance` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Favor speed over saving energy.

- **What it does:** Makes the High performance plan the active power plan.
- **Benefit:** Keeps the processor more ready, which can help games and heavy work feel steadier.
- **Risk:** Low. Uses more power, runs warmer and drains a laptop battery faster.
- **Changes:**
  - `Use power plan high-performance`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/powercfg-command-line-options>

#### Power saver plan

`power.plan-saver` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Save energy by running slower.

- **What it does:** Makes the Power saver plan the active power plan.
- **Benefit:** Longer battery life and lower heat.
- **Risk:** Low. The PC can feel slower under load.
- **Changes:**
  - `Use power plan power-saver`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/powercfg-command-line-options>

#### Turn off hibernation

`power.hibernate-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Free disk space by removing the hibernation file.

- **What it does:** Turns hibernation off, which deletes the hibernation file on the system drive.
- **Benefit:** Frees the disk space the hibernation file used. By default Windows sizes it as a share of your memory.
- **Risk:** Low. Hibernate is no longer offered. Fast Startup also turns off because it relies on hibernation.
- **Changes:**
  - `Turn hibernation off and delete the hibernation file`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/powercfg-command-line-options>

#### Ultimate Performance power plan

`power.plan-ultimate` · Low · Helps in some cases · Only if it fits you · checked on build 26200

A workstation plan that cuts small power saving delays.

- **What it does:** Creates the Ultimate Performance plan from the built in template if it is missing, then makes it active.
- **Benefit:** Removes tiny wake and sleep delays in the processor, which can help latency sensitive work on a plugged in desktop.
- **Risk:** Low. The highest power use of any plan. Some laptops do not offer it.
- **Changes:**
  - `Use power plan ultimate`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/powercfg-command-line-options>

## Privacy

### App permissions

#### Block app access to Bluetooth and other radios (all accounts)

`perm.all-radios` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to Bluetooth and other radios off for every account.

- **What it does:** Applies the app privacy policy that forces access to Bluetooth and other radios to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use Bluetooth and other radios, and nobody can change that from Settings.
- **Risk:** Medium. Apps cannot switch Bluetooth or other radios. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessRadios to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to Bluetooth and other radios (this account)

`perm.user-radios` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to Bluetooth and other radios for your account.

- **What it does:** Sets the per account app permission for Bluetooth and other radios to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use Bluetooth and other radios until you allow it again.
- **Risk:** Low. Apps cannot switch Bluetooth or other radios. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\radios\Value to Deny (String)`

#### Block app access to an eye tracker (all accounts)

`perm.all-eye-tracker` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to an eye tracker off for every account.

- **What it does:** Applies the app privacy policy that forces access to an eye tracker to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use an eye tracker, and nobody can change that from Settings.
- **Risk:** Medium. Apps cannot read eye tracking data. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessGazeInput to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to an eye tracker (this account)

`perm.user-eye-tracker` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to an eye tracker for your account.

- **What it does:** Sets the per account app permission for an eye tracker to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use an eye tracker until you allow it again.
- **Risk:** Low. Apps cannot read eye tracking data. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\gazeInput\Value to Deny (String)`

#### Block app access to diagnostics about other apps (all accounts)

`perm.all-app-diagnostics` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to diagnostics about other apps off for every account.

- **What it does:** Applies the app privacy policy that forces access to diagnostics about other apps to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use diagnostics about other apps, and nobody can change that from Settings.
- **Risk:** Medium. Task managers from the Store cannot see what other apps are doing. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsGetDiagnosticInfo to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to diagnostics about other apps (this account)

`perm.user-app-diagnostics` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to diagnostics about other apps for your account.

- **What it does:** Sets the per account app permission for diagnostics about other apps to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use diagnostics about other apps until you allow it again.
- **Risk:** Low. Task managers from the Store cannot see what other apps are doing. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics\Value to Deny (String)`

#### Block app access to motion sensors (all accounts)

`perm.all-motion` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to motion sensors off for every account.

- **What it does:** Applies the app privacy policy that forces access to motion sensors to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use motion sensors, and nobody can change that from Settings.
- **Risk:** Medium. Apps cannot read motion and activity data from sensors. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessMotion to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to motion sensors (this account)

`perm.user-motion` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to motion sensors for your account.

- **What it does:** Sets the per account app permission for motion sensors to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use motion sensors until you allow it again.
- **Risk:** Low. Apps cannot read motion and activity data from sensors. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\activity\Value to Deny (String)`

#### Block app access to phone calls (all accounts)

`perm.all-phone-calls` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to phone calls off for every account.

- **What it does:** Applies the app privacy policy that forces access to phone calls to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use phone calls, and nobody can change that from Settings.
- **Risk:** Medium. Apps cannot place phone calls through Windows. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessPhone to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to phone calls (this account)

`perm.user-phone-calls` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to phone calls for your account.

- **What it does:** Sets the per account app permission for phone calls to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use phone calls until you allow it again.
- **Risk:** Low. Apps cannot place phone calls through Windows. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\phoneCall\Value to Deny (String)`

#### Block app access to presence sensing (all accounts)

`perm.all-presence` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to presence sensing off for every account.

- **What it does:** Applies the app privacy policy that forces access to presence sensing to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use presence sensing, and nobody can change that from Settings.
- **Risk:** Medium. Apps cannot tell whether you are in front of the PC. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessHumanPresence to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to presence sensing (this account)

`perm.user-presence` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to presence sensing for your account.

- **What it does:** Sets the per account app permission for presence sensing to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use presence sensing until you allow it again.
- **Risk:** Low. Apps cannot tell whether you are in front of the PC. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\humanPresence\Value to Deny (String)`

#### Block app access to screenshots of other windows (all accounts)

`perm.all-screenshots` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to screenshots of other windows off for every account.

- **What it does:** Applies the app privacy policy that forces access to screenshots of other windows to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use screenshots of other windows, and nobody can change that from Settings.
- **Risk:** Medium. Store apps cannot capture other windows without you picking them. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessGraphicsCaptureProgrammatic to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to screenshots of other windows (this account)

`perm.user-screenshots` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to screenshots of other windows for your account.

- **What it does:** Sets the per account app permission for screenshots of other windows to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use screenshots of other windows until you allow it again.
- **Risk:** Low. Store apps cannot capture other windows without you picking them. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\graphicsCaptureProgrammatic\Value to Deny (String)`

#### Block app access to the camera (all accounts)

`perm.all-camera` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to the camera off for every account.

- **What it does:** Applies the app privacy policy that forces access to the camera to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use the camera, and nobody can change that from Settings.
- **Risk:** Medium. Video calls and photo apps from the Microsoft Store stop seeing the camera until you allow it again. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessCamera to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to the camera (this account)

`perm.user-camera` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to the camera for your account.

- **What it does:** Sets the per account app permission for the camera to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use the camera until you allow it again.
- **Risk:** Low. Video calls and photo apps from the Microsoft Store stop seeing the camera until you allow it again. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam\Value to Deny (String)`

#### Block app access to the microphone (all accounts)

`perm.all-microphone` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to the microphone off for every account.

- **What it does:** Applies the app privacy policy that forces access to the microphone to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use the microphone, and nobody can change that from Settings.
- **Risk:** Medium. Voice calls, dictation and recorders from the Microsoft Store stop hearing you until you allow it again. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessMicrophone to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to the microphone (this account)

`perm.user-microphone` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to the microphone for your account.

- **What it does:** Sets the per account app permission for the microphone to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use the microphone until you allow it again.
- **Risk:** Low. Voice calls, dictation and recorders from the Microsoft Store stop hearing you until you allow it again. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone\Value to Deny (String)`

#### Block app access to your account information (all accounts)

`perm.all-account-info` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to your account information off for every account.

- **What it does:** Applies the app privacy policy that forces access to your account information to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use your account information, and nobody can change that from Settings.
- **Risk:** Medium. Apps cannot read your name, picture and account details. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessAccountInfo to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to your account information (this account)

`perm.user-account-info` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to your account information for your account.

- **What it does:** Sets the per account app permission for your account information to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use your account information until you allow it again.
- **Risk:** Low. Apps cannot read your name, picture and account details. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation\Value to Deny (String)`

#### Block app access to your calendar (all accounts)

`perm.all-calendar` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to your calendar off for every account.

- **What it does:** Applies the app privacy policy that forces access to your calendar to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use your calendar, and nobody can change that from Settings.
- **Risk:** Medium. Calendar and scheduling apps cannot read or add events. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessCalendar to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to your calendar (this account)

`perm.user-calendar` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to your calendar for your account.

- **What it does:** Sets the per account app permission for your calendar to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use your calendar until you allow it again.
- **Risk:** Low. Calendar and scheduling apps cannot read or add events. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appointments\Value to Deny (String)`

#### Block app access to your call history (all accounts)

`perm.all-call-history` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to your call history off for every account.

- **What it does:** Applies the app privacy policy that forces access to your call history to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use your call history, and nobody can change that from Settings.
- **Risk:** Medium. Apps that show or log phone calls cannot read the history. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessCallHistory to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to your call history (this account)

`perm.user-call-history` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to your call history for your account.

- **What it does:** Sets the per account app permission for your call history to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use your call history until you allow it again.
- **Risk:** Low. Apps that show or log phone calls cannot read the history. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\phoneCallHistory\Value to Deny (String)`

#### Block app access to your contacts (all accounts)

`perm.all-contacts` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to your contacts off for every account.

- **What it does:** Applies the app privacy policy that forces access to your contacts to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use your contacts, and nobody can change that from Settings.
- **Risk:** Medium. Mail and chat apps cannot read your contact list. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessContacts to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to your contacts (this account)

`perm.user-contacts` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to your contacts for your account.

- **What it does:** Sets the per account app permission for your contacts to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use your contacts until you allow it again.
- **Risk:** Low. Mail and chat apps cannot read your contact list. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\contacts\Value to Deny (String)`

#### Block app access to your email (all accounts)

`perm.all-email` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to your email off for every account.

- **What it does:** Applies the app privacy policy that forces access to your email to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use your email, and nobody can change that from Settings.
- **Risk:** Medium. Apps cannot read mail that Windows stores for you. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessEmail to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to your email (this account)

`perm.user-email` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to your email for your account.

- **What it does:** Sets the per account app permission for your email to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use your email until you allow it again.
- **Risk:** Low. Apps cannot read mail that Windows stores for you. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\email\Value to Deny (String)`

#### Block app access to your notifications (all accounts)

`perm.all-notifications` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to your notifications off for every account.

- **What it does:** Applies the app privacy policy that forces access to your notifications to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use your notifications, and nobody can change that from Settings.
- **Risk:** Medium. Apps that mirror or read notifications stop working. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessNotifications to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to your notifications (this account)

`perm.user-notifications` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to your notifications for your account.

- **What it does:** Sets the per account app permission for your notifications to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use your notifications until you allow it again.
- **Risk:** Low. Apps that mirror or read notifications stop working. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userNotificationListener\Value to Deny (String)`

#### Block app access to your tasks (all accounts)

`perm.all-tasks` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to your tasks off for every account.

- **What it does:** Applies the app privacy policy that forces access to your tasks to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use your tasks, and nobody can change that from Settings.
- **Risk:** Medium. To do apps cannot read your Windows task list. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessTasks to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to your tasks (this account)

`perm.user-tasks` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to your tasks for your account.

- **What it does:** Sets the per account app permission for your tasks to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use your tasks until you allow it again.
- **Risk:** Low. To do apps cannot read your Windows task list. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userDataTasks\Value to Deny (String)`

#### Block app access to your text messages (all accounts)

`perm.all-messaging` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force app access to your text messages off for every account.

- **What it does:** Applies the app privacy policy that forces access to your text messages to Deny. Nobody can turn it back on in Settings.
- **Benefit:** No Windows app on this PC can use your text messages, and nobody can change that from Settings.
- **Risk:** Medium. Apps cannot read or send text messages through Windows. The Settings switch is locked until you revert.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessMessaging to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block app access to your text messages (this account)

`perm.user-messaging` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off app access to your text messages for your account.

- **What it does:** Sets the per account app permission for your text messages to Deny. This is the value the Windows Settings privacy page writes.
- **Benefit:** Apps that ask Windows for access cannot use your text messages until you allow it again.
- **Risk:** Low. Apps cannot read or send text messages through Windows. Allow it again in Settings, Privacy and security.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\chat\Value to Deny (String)`

#### Block apps from listening for a wake word

`perm.all-voice` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Stop apps from starting by voice.

- **What it does:** Applies the app privacy policy. Forces voice activation for Windows apps to Deny.
- **Benefit:** No Windows app can use this feature, and nobody can change that from Settings.
- **Risk:** Medium. Apps that start with a spoken phrase stop responding to it.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsActivateWithVoice to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block apps from reading text in other apps

`perm.all-text` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Stop apps from reading on screen text of other apps.

- **What it does:** Applies the app privacy policy. Forces access to text content from foreground apps to Deny.
- **Benefit:** No Windows app can use this feature, and nobody can change that from Settings.
- **Risk:** Medium. Helper apps that read other windows cannot do it.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessForegroundText to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block apps from talking to unpaired devices

`perm.all-unpaired` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Stop apps from connecting to nearby devices you have not paired.

- **What it does:** Applies the app privacy policy. Forces apps communicating with unpaired devices to Deny.
- **Benefit:** No Windows app can use this feature, and nobody can change that from Settings.
- **Risk:** Medium. Apps that connect to nearby beacons or gadgets without pairing stop working.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsSyncWithDevices to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block apps from trusted devices

`perm.all-trusted` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Stop apps from using trusted devices.

- **What it does:** Applies the app privacy policy. Forces app access to trusted devices to Deny.
- **Benefit:** No Windows app can use this feature, and nobody can change that from Settings.
- **Risk:** Medium. Apps that use trusted devices cannot reach them.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessTrustedDevices to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Block voice activation on the lock screen

`perm.all-voice-locked` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Stop apps from starting by voice while the PC is locked.

- **What it does:** Applies the app privacy policy. Forces voice activation while the PC is locked to Deny.
- **Benefit:** No Windows app can use this feature, and nobody can change that from Settings.
- **Risk:** Medium. Voice start on the lock screen stops working.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsActivateWithVoiceAboveLock to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Stop apps from running in the background (all accounts)

`perm.all-background-apps` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Force Windows apps to stay quiet in the background with a policy.

- **What it does:** Applies the app privacy policy that forces background activity to Deny.
- **Benefit:** No Store app runs in the background for any account.
- **Risk:** Medium. Notifications and live tiles from Store apps stop until you open them.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsRunInBackground to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Stop apps from running in the background (this account)

`perm.user-background-apps` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Turn off the master switch that lets Store apps run when you are not using them.

- **What it does:** Sets the per account value that disables background activity for Microsoft Store apps.
- **Benefit:** Less background network and battery use, and fewer apps that keep tracking in the background.
- **Risk:** Low. Store apps may not update tiles or send notifications until you open them.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications\GlobalUserDisabled to 1 (DWord)`

#### Turn off location for the whole PC

`privacy.location-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Force the location service off for every app and account.

- **What it does:** Applies the machine policy that turns location off. The Location settings page becomes read only.
- **Benefit:** No app can read where the PC is. Fewer background lookups.
- **Risk:** Low. Maps, weather, find my device and time zone auto detect stop working.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors\DisableLocation to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-system>

### Copilot and Recall

#### Make Recall unavailable

`ai.recall-unavailable` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Remove the Recall component so nobody can turn it on.

- **What it does:** Applies the policy that disables the Recall component. Windows removes its files and deletes saved snapshots, and needs a restart.
- **Benefit:** Recall cannot be enabled by anyone on this PC.
- **Risk:** Low. Recall is gone until you revert and restart.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI\AllowRecallEnablement to 0 (DWord)`
- **Needs:** Windows build 26100 or later; Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account; a restart
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-windowsai>

#### Turn off Notepad AI features

`ai.notepad-ai-off` · Safe · Documented by the vendor · Optional: your choice · checked on build 26200

Remove Rewrite, Summarize and the other AI features from Notepad.

- **What it does:** Applies Microsoft's Notepad policy that disables its AI features for every account.
- **Benefit:** Notepad stays a plain text editor and sends no text to an online AI service.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\WindowsNotepad\DisableAIFeatures to 1 (DWord)`
- **Needs:** Windows build 22621 or later; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/manage-notepad>

#### Turn off Paint image creation tools

`ai.paint-tools-off` · Safe · Documented by the vendor · Optional: your choice · checked on build 26200

Disable Cocreator, generative fill and Image Creator in Paint.

- **What it does:** Applies the three Paint policies that disable these features.
- **Benefit:** Paint works as a plain editor and sends nothing to online image services.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\Paint\DisableCocreator to 1 (DWord)`
  - `Set HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\Paint\DisableGenerativeFill to 1 (DWord)`
  - `Set HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\Paint\DisableImageCreator to 1 (DWord)`
- **Needs:** Windows build 22621 or later; Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-windowsai>

#### Turn off Recall snapshots

`ai.recall-snapshots-off` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Windows will not save snapshots of your screen for Recall.

- **What it does:** Applies the policy that turns off saving snapshots for use with Recall. Snapshots saved earlier are deleted when it is enabled.
- **Benefit:** Nothing on your screen is recorded for later search.
- **Risk:** Low. Recall has nothing to search.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI\DisableAIDataAnalysis to 1 (DWord)`
- **Needs:** Windows build 26100 or later; Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-windowsai>

#### Turn off the Copilot pane

`ai.copilot-pane-off` · Safe · Documented by the vendor · Optional: your choice · checked on build 26200

Block the older Copilot in Windows pane with a policy.

- **What it does:** Applies the per account policy that turns off Windows Copilot and removes its taskbar icon.
- **Benefit:** Copilot cannot be opened from the taskbar on builds that include the pane.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot\TurnOffWindowsCopilot to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-windowsai>

### Diagnostics

#### Keep your PC name out of diagnostic data

`privacy.device-name-hidden` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Stop Windows from adding the name of this PC to diagnostic data.

- **What it does:** Applies the policy that stops the device name from being included in Windows diagnostic data.
- **Benefit:** Your PC name is not attached to what is sent to Microsoft.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\DataCollection\AllowDeviceNameInTelemetry to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-system>

#### Lowest diagnostic data level

`privacy.diagnostic-data-minimum` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Send only the minimum data Windows allows.

- **What it does:** Sets the diagnostic data policy to 0. Home and Pro treat 0 as the Required level, which is the lowest those editions support. Enterprise and Education honor the true Security level.
- **Benefit:** Reduces the amount of diagnostic data sent to Microsoft to the least the edition permits.
- **Risk:** Low. Some feedback and troubleshooting features have less data to work with.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection\AllowTelemetry to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/privacy/configure-windows-diagnostic-data-in-your-organization>

#### Never ask for feedback

`privacy.feedback-frequency-never` · Safe · Helps in some cases · Suggested: turn on · checked on build 26200

Set the feedback frequency to Never for your account.

- **What it does:** Sets the number of feedback requests per period to zero for your account.
- **Benefit:** Windows does not ask you for feedback.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Siuf\Rules\NumberOfSIUFInPeriod to 0 (DWord)`

#### Stop device companion app downloads

`privacy.device-metadata-offline` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Do not fetch extra vendor software and icons when a device is plugged in.

- **What it does:** Applies the policy that prevents Windows from retrieving device metadata from the internet.
- **Benefit:** Fewer vendor promotions and surprise app prompts when connecting hardware.
- **Risk:** Low. Some devices show a generic icon and name.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\Device Metadata\PreventDeviceMetadataFromNetwork to 1 (DWord)`
- **Needs:** administrator rights, applies to every account

#### Stop feedback pop-ups

`privacy.feedback-prompts-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Windows will no longer ask you to rate or comment on it.

- **What it does:** Applies the policy that stops Windows from showing feedback notifications.
- **Benefit:** No more interruptions asking for feedback.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\DataCollection\DoNotShowFeedbackNotifications to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-experience>

#### Stop inking and typing personalization

`privacy.inking-typing-personalization-off` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Windows will not learn from what you type and write.

- **What it does:** Turns off implicit ink and text collection and contact harvesting for your account.
- **Benefit:** Your typing and handwriting are not used to build a personal dictionary.
- **Risk:** Low. Autocorrect and suggestions learn less about your words.
- **Changes:**
  - `Set HKCU\Software\Microsoft\InputPersonalization\RestrictImplicitInkCollection to 1 (DWord)`
  - `Set HKCU\Software\Microsoft\InputPersonalization\RestrictImplicitTextCollection to 1 (DWord)`
  - `Set HKCU\Software\Microsoft\InputPersonalization\TrainedDataStore\HarvestContacts to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Personalization\Settings\AcceptedPrivacyPolicy to 0 (DWord)`

#### Stop sending typing data to improve Windows

`privacy.improve-typing-off` · Safe · Helps in some cases · Suggested: turn on · checked on build 26200

Turn off the inking and typing improvement program.

- **What it does:** Sets the per account value that disables sending inking and typing samples to Microsoft.
- **Benefit:** Samples of what you type are not sent.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Input\TIPC\Enabled to 0 (DWord)`

#### Stop speech model downloads

`privacy.speech-updates-off` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Windows will not download updates to its speech recognition data.

- **What it does:** Applies the policy that stops automatic updates of speech recognition and speech synthesis models.
- **Benefit:** Saves bandwidth and removes one background download.
- **Risk:** Low. Speech recognition may be a little less accurate over time.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Speech\AllowSpeechModelUpdate to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-system>

#### Stop the Microsoft configuration download

`privacy.onesettings-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Windows will not fetch configuration and experiment settings from Microsoft.

- **What it does:** Applies the policy that stops Windows from connecting to the OneSettings service.
- **Benefit:** One less background connection and fewer remote changes to how Windows behaves.
- **Risk:** Low. Microsoft cannot switch Windows features on or off remotely, and some diagnostic features get less guidance.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\DataCollection\DisableOneSettingsDownloads to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-system>

#### Stop the diagnostic tracking services

`privacy.telemetry-services` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Turn off the services that collect and forward diagnostic data.

- **What it does:** Sets the Connected User Experiences and Telemetry service (DiagTrack) to Disabled and stops it.
- **Benefit:** Removes the background process that gathers and uploads diagnostic data.
- **Risk:** Low. Some feedback and diagnostic features report less. Windows Update is not affected.
- **Changes:**
  - `Set service DiagTrack start type to Disabled`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/privacy/configure-windows-diagnostic-data-in-your-organization>

#### Turn off Windows Error Reporting

`privacy.error-reporting-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Stop crash reports from being created and sent.

- **What it does:** Applies the policy that disables Windows Error Reporting.
- **Benefit:** No crash data about your apps leaves the PC and fewer reporting jobs run after a crash.
- **Risk:** Low. Microsoft and app makers get no crash reports, and the built in solutions check for known problems stops.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting\Disabled to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-system>

#### Turn off diagnostic scheduled tasks

`privacy.telemetry-tasks` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Disable the scheduled jobs that gather app and usage data.

- **What it does:** Disables the Compatibility Appraiser, Program Data Updater, Consolidator and USB CEIP tasks.
- **Benefit:** Stops periodic data collection jobs and the CPU and disk bursts they cause.
- **Risk:** Low. Feature updates may take slightly longer to check app compatibility.
- **Changes:**
  - `Disable scheduled task \Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser`
  - `Disable scheduled task \Microsoft\Windows\Application Experience\ProgramDataUpdater`
  - `Disable scheduled task \Microsoft\Windows\Customer Experience Improvement Program\Consolidator`
  - `Disable scheduled task \Microsoft\Windows\Customer Experience Improvement Program\UsbCeip`
- **Needs:** administrator rights, applies to every account

#### Turn off online speech recognition

`privacy.online-speech-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Keep your voice from being sent to Microsoft for recognition.

- **What it does:** Applies the policy that turns off online speech recognition services and hides the switch in Settings.
- **Benefit:** Your voice is not sent to cloud speech services.
- **Risk:** Low. Voice typing and cloud dictation stop working.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\InputPersonalization\AllowInputPersonalization to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Turn off online speech recognition (this account)

`privacy.online-speech-account-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Keep your voice from being sent for recognition, for your account only.

- **What it does:** Sets the per account value that records that you did not accept the online speech privacy statement.
- **Benefit:** Your voice is not sent to cloud speech services.
- **Risk:** Low. Voice typing and cloud dictation stop working.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy\HasAccepted to 0 (DWord)`

### Gaming

#### Stop background gameplay recording

`gaming.background-recording-off` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Keep Game Bar, but stop it recording in the background while you play.

- **What it does:** Turns off the Record what happened setting for your account. Game Bar still opens and you can still start a recording yourself.
- **Benefit:** No background capture running during games, so no extra disk writes or GPU encoding.
- **Risk:** Low. Game Bar can no longer save the last moments of play after the fact. Manual recording is not affected.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR\HistoricalCaptureEnabled to 0 (DWord)`
- **Sources:** <https://www.elevenforum.com/t/enable-or-disable-record-what-happened-for-gaming-captures-in-windows-11.17900>

#### Turn off Game Bar recording

`gaming.game-recording-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Stop background game recording and broadcasting for your account.

- **What it does:** Turns off Game DVR background capture for your account.
- **Benefit:** No background recording costs performance or disk space.
- **Risk:** Low. Game Bar clips and the record last 30 seconds feature stop working.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR\AppCaptureEnabled to 0 (DWord)`
  - `Set HKCU\System\GameConfigStore\GameDVR_Enabled to 0 (DWord)`

### Lock screen

#### Block the lock screen camera

`lock.camera-off` · Safe · Documented by the vendor · Optional: your choice · checked on build 26200

The camera cannot be started from the lock screen.

- **What it does:** Applies the policy that prevents enabling the lock screen camera.
- **Benefit:** Nobody can open the camera without signing in.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\Personalization\NoLockScreenCamera to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-settings>

#### Hide app notifications on the lock screen

`lock.app-notifications-off` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Notifications do not show while the PC is locked.

- **What it does:** Applies the policy that turns off app notifications on the lock screen.
- **Benefit:** Nobody can read your messages without signing in.
- **Risk:** Low. You will not see notifications until you sign in.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\System\DisableLockScreenAppNotifications to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-settings>

### Search

#### Block Cortana on the lock screen

`search.cortana-lockscreen-off` · Safe · Documented by the vendor · Optional: your choice · checked on build 26200

Stop Cortana from answering while the PC is locked.

- **What it does:** Applies the policy that disallows Cortana above the lock screen.
- **Benefit:** Nobody can use Cortana without signing in.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowCortanaAboveLock to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-search>

#### Hide search highlights

`search.highlights-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Remove the daily pictures and trending items from the search box.

- **What it does:** Sets the Windows Search policy that turns off search highlights.
- **Benefit:** A plain search box with no trending content or tracking of what you click.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search\EnableDynamicContentInWSB to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-search>

#### Keep search from using your location

`search.location-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Stop Windows search from using where you are.

- **What it does:** Applies the policy that stops search from using location information.
- **Benefit:** Search results are not tailored to your location.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowSearchToUseLocation to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-search>

#### Keep search on this PC

`search.cloud-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Stop Windows search from looking in cloud sources like OneDrive and SharePoint.

- **What it does:** Sets the Windows Search policy that disallows cloud search.
- **Benefit:** Search only looks at this PC.
- **Risk:** Low. Files that exist only in the cloud are not found from the search box.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowCloudSearch to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-search>

#### Keep web results out of Start search

`privacy.web-search-start` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Search your PC only when typing in Start.

- **What it does:** Applies the per-user policy that turns off search box suggestions.
- **Benefit:** Typed searches stay on the PC and Start returns local results faster.
- **Risk:** None. You can still search the web in a browser.
- **Changes:**
  - `Set HKCU\Software\Policies\Microsoft\Windows\Explorer\DisableSearchBoxSuggestions to 1 (DWord)`
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-search>

#### No web results in search (all accounts)

`search.web-results-machine` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Stop Windows search from querying the web.

- **What it does:** Sets the Windows Search policy that disables web queries and web results.
- **Benefit:** Nothing you type in search is sent to the web.
- **Risk:** Low. Search shows only results from this PC.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search\ConnectedSearchUseWeb to 0 (DWord)`
- **Needs:** Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-search>

#### Stop saving search history

`search.history-off` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Windows will not remember what you searched for on this device or in your account.

- **What it does:** Turns off device search history and cloud content search for Microsoft and work or school accounts.
- **Benefit:** Searches are not stored and cloud files are not searched from Windows search.
- **Risk:** Low. Search no longer suggests your past searches or finds cloud only files.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\SearchSettings\IsDeviceSearchHistoryEnabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\SearchSettings\IsMSACloudSearchEnabled to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\SearchSettings\IsAADCloudSearchEnabled to 0 (DWord)`

#### Turn off Cortana

`search.cortana-off` · Safe · Documented by the vendor · Optional: your choice · checked on build 26200

Block Cortana with a policy.

- **What it does:** Applies the policy that disallows Cortana.
- **Benefit:** Cortana cannot run or collect searches on builds that still include it.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowCortana to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-search>

### Suggestions and ads

#### Hide recent files and frequent folders

`ads.recent-items-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Stop showing and tracking recently opened items.

- **What it does:** Turns off recent item tracking in Start, jump lists and File Explorer for your account.
- **Benefit:** Nobody sees what you opened lately by looking at Start or Quick access.
- **Risk:** Low. Recent lists and jump lists stay empty.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Start_TrackDocs to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\ShowRecent to 0 (DWord)`
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\ShowFrequent to 0 (DWord)`

#### Stop tracking which apps you launch

`ads.track-launches-off` · Low · Helps in some cases · Suggested: turn on · checked on build 26200

Windows will not record the apps you open to improve Start.

- **What it does:** Turns off app launch tracking for your account.
- **Benefit:** Start does not keep a list of the apps you use most.
- **Risk:** Low. The most used apps list in Start stays empty.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Start_TrackProgs to 0 (DWord)`

#### Turn off advertising ID

`privacy.advertising-id` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Stop apps from using a per-user ID to profile you for ads.

- **What it does:** Sets the per-user advertising ID switch to off.
- **Benefit:** Apps can no longer build an ad profile from a shared identifier.
- **Risk:** None. Ads you see in apps become less targeted, not fewer.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo\Enabled to 0 (DWord)`
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Turn off tailored experiences

`privacy.tailored-experiences` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Stop Windows from using diagnostic data to suggest tips and ads.

- **What it does:** Turns off the per-user setting that lets diagnostic data personalize tips, ads and recommendations.
- **Benefit:** Fewer personalized suggestions and less use of your diagnostic data.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\Privacy\TailoredExperiencesWithDiagnosticDataEnabled to 0 (DWord)`
- **Sources:** <https://learn.microsoft.com/en-us/windows/privacy/configure-windows-diagnostic-data-in-your-organization>

### Sync and clipboard

#### Block OneDrive file sync

`privacy.onedrive-sync-policy` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Stop OneDrive from syncing files without uninstalling it.

- **What it does:** Applies the policy that prevents apps and features from using OneDrive for file storage.
- **Benefit:** Nothing uploads to OneDrive and it disappears from the File Explorer sidebar.
- **Risk:** Medium. Files already synced stay on the PC but stop updating. If Desktop, Documents or Pictures were moved into OneDrive, move them back first.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\OneDrive\DisableFileSyncNGSC to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-system>

#### Stop cloud backup of text messages

`sync.message-backup-off` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Cellular text messages are not backed up to Microsoft.

- **What it does:** Applies the policy that stops message backup and restore through Microsoft cloud services.
- **Benefit:** Your messages are not stored in the cloud.
- **Risk:** Low. You cannot restore messages from the cloud.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\Messaging\AllowMessageSync to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Stop syncing settings to the cloud

`sync.settings-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Windows will not sync your settings, themes and passwords between PCs.

- **What it does:** Applies the policy that turns off Sync your settings and locks the switch.
- **Benefit:** Your preferences and passwords are not uploaded.
- **Risk:** Low. A new PC does not get your theme and settings automatically.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\SettingSync\DisableSettingSync to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-experience>

#### Stop syncing the clipboard across devices

`sync.clipboard-cloud-off` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

What you copy stays on this PC.

- **What it does:** Applies the policy that stops clipboard contents from being synced to other devices.
- **Benefit:** Passwords and text you copy are not sent to the cloud.
- **Risk:** None.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\System\AllowCrossDeviceClipboard to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-experience>

#### Turn off Find My Device

`sync.find-my-device-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Stop registering this PC and its location in the cloud.

- **What it does:** Applies the policy that turns Find My Device off.
- **Benefit:** Your PC location is not stored in your Microsoft account.
- **Risk:** Low. You cannot locate this PC from account.microsoft.com if it is lost.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\FindMyDevice\AllowFindMyDevice to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-experience>

#### Turn off Phone link

`sync.phone-link-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Stop this PC from linking to a phone.

- **What it does:** Applies the policy that turns off phone and PC linking.
- **Benefit:** No continuing tasks from a phone and no phone data synced to this PC.
- **Risk:** Low. Phone Link and Continue on PC stop working.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\System\EnableMmx to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-system>

#### Turn off activity history

`privacy.activity-history` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Stop Windows from recording and uploading what you do.

- **What it does:** Applies the three machine policies for the activity feed, publishing and uploading of user activities.
- **Benefit:** No activity timeline is kept or sent to Microsoft.
- **Risk:** Low. Features that resume work across devices stop working.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\System\EnableActivityFeed to 0 (DWord)`
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\System\PublishUserActivities to 0 (DWord)`
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\System\UploadUserActivities to 0 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-privacy>

#### Turn off clipboard history

`sync.clipboard-history-off` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Windows will not keep a list of what you copied.

- **What it does:** Applies the policy that stops the history of clipboard contents from being stored.
- **Benefit:** Old copied text and images are not kept in memory.
- **Risk:** Low. The Win+V history list stays empty.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\System\AllowClipboardHistory to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-experience>

## Security

### Defender sharing

#### Never send file samples to Microsoft

`defender.samples-never` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Defender will not upload suspicious files for analysis.

- **What it does:** Applies the Defender policy that sends no file samples.
- **Benefit:** Your files are never uploaded by Defender.
- **Risk:** Low. Defender cannot ask the cloud to analyze unknown files.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows Defender\Spynet\SubmitSamplesConsent to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-defender>

#### Stop sharing detections with Microsoft MAPS

`defender.maps-off` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Keep Microsoft Defender from sending data to the Microsoft cloud protection service.

- **What it does:** Applies the Defender policy that leaves Microsoft MAPS. Real time protection and local scanning stay on.
- **Benefit:** Less data about files and detections is sent to Microsoft.
- **Risk:** Medium. Cloud protection reacts more slowly to brand new threats.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows Defender\Spynet\SpynetReporting to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-defender>

### Security

#### Stop other computers printing through this PC

`printing.no-print-server` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Make the print spooler refuse connections from other computers.

- **What it does:** Sets the policy Allow Print Spooler to accept client connections to disabled. Printing from this PC is not affected.
- **Benefit:** Other computers on your network can no longer reach the print spooler here. Microsoft offered exactly this as a mitigation for the 2021 PrintNightmare flaw.
- **Risk:** Low. Other computers can no longer print through this PC, and you cannot share a printer from it. The print spooler must restart, so a restart finishes the change.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows NT\Printers\RegisterSpoolerRemoteRpcEndPoint to 2 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account; a restart
- **Sources:** <https://msrc.microsoft.com/en-US/security-guidance/advisory/CVE-2021-34527>, <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-admx-printing2>

#### Turn off AutoPlay

`security.autoplay-off` · Safe · Helps in some cases · Suggested: turn on · checked on build 26200

Stop Windows from acting on drives and discs when you connect them.

- **What it does:** Turns off AutoPlay for all media and devices for your account.
- **Benefit:** A USB stick cannot start something by itself or pop up choices. It closes a common way to spread malware.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\Windows\CurrentVersion\AutoplayHandlers\DisableAutoplay to 1 (DWord)`

#### Turn off Remote Assistance

`security.remote-assistance-off` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Block people from being invited to control this PC.

- **What it does:** Applies the policies that turn off solicited and unsolicited Remote Assistance.
- **Benefit:** Nobody can connect to this PC through Remote Assistance.
- **Risk:** Low. You cannot invite someone to help you remotely this way.
- **Changes:**
  - `Set HKLM\Software\policies\Microsoft\Windows NT\Terminal Services\fAllowToGetHelp to 0 (DWord)`
  - `Set HKLM\Software\policies\Microsoft\Windows NT\Terminal Services\fAllowUnsolicited to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-system>

#### Turn off SMB 1.0

`security.smb1-off` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Disable the old, insecure file sharing protocol.

- **What it does:** Turns off the SMB 1.0 client and server features.
- **Benefit:** Closes a protocol the vendor tells everyone to stop using.
- **Risk:** Low. Very old network drives and printers that only speak SMB 1.0 stop connecting.
- **Changes:**
  - `Turn off Windows feature SMB1Protocol-Client`
  - `Turn off Windows feature SMB1Protocol-Server`
  - `Turn off Windows feature SMB1Protocol`
- **Needs:** administrator rights, applies to every account; a restart
- **Sources:** <https://learn.microsoft.com/en-us/windows-server/storage/file-server/troubleshoot/detect-enable-and-disable-smbv1-v2-v3>

## System

### Background services

#### Turn off UPnP and SSDP device discovery

`services.device-discovery-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Stop the services that find and talk to UPnP devices on your network.

- **What it does:** Sets the SSDP Discovery and UPnP Device Host services to Disabled. They start only when something asks for them.
- **Benefit:** Fewer network services that other devices on your network can reach.
- **Risk:** Low. Windows can no longer discover some network media players, speakers and similar UPnP devices, and apps that rely on that discovery may not find them.
- **Changes:**
  - `Set service SSDPSRV start type to Disabled`
  - `Set service upnphost start type to Disabled`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/iot/iot-enterprise/optimize/services>

#### Turn off Xbox Live services

`services.xbox-live-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Stop the services for Xbox sign-in, cloud saves and Xbox networking.

- **What it does:** Sets the Xbox Live Auth Manager, Xbox Live Game Save and Xbox Live Networking services to Disabled.
- **Benefit:** Three fewer services that talk to Xbox Live. They start only when an Xbox feature asks for them.
- **Risk:** Low. Signing in to Xbox, cloud saves for Xbox games and Xbox multiplayer stop working until you revert.
- **Changes:**
  - `Set service XblAuthManager start type to Disabled`
  - `Set service XblGameSave start type to Disabled`
  - `Set service XboxNetApiSvc start type to Disabled`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/iot/iot-enterprise/optimize/services>

#### Turn off distributed link tracking

`services.link-tracking-off` · Low · Helps in some cases · Optional: your choice · checked on build 26200

Stop the service that follows moved files for old shortcuts and linked documents.

- **What it does:** Sets the Distributed Link Tracking Client service to Disabled. It keeps links between NTFS files working after a file is moved.
- **Benefit:** One less background service. On a test PC it used about 1 MB while running.
- **Risk:** Low. A shortcut or linked document may no longer find a file that was moved to another drive or PC. Most people never notice.
- **Changes:**
  - `Set service TrkWks start type to Disabled`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/iot/iot-enterprise/optimize/services>

#### Turn off printing support

`services.print-spooler-off` · Medium · Documented by the vendor · Only if it fits you · checked on build 26200

Stop and disable the Print Spooler service if you never print.

- **What it does:** Stops the Print Spooler and sets it to Disabled. Microsoft offered this as a workaround for the 2021 PrintNightmare flaw where printing is not needed.
- **Benefit:** Removes a service that has been a target of serious attacks, and frees a little memory.
- **Risk:** Medium. Nothing can print from this PC, locally or over the network, and Print to PDF stops working. Revert sets the start type back.
- **Changes:**
  - `Set service Spooler start type to Disabled`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://msrc.microsoft.com/en-US/security-guidance/advisory/CVE-2021-34527>, <https://learn.microsoft.com/en-us/windows/iot/iot-enterprise/optimize/services>

#### Turn off smart card services

`services.smart-card-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Stop the services that read smart cards and apply their removal rules.

- **What it does:** Sets the Smart Card, Smart Card Device Enumeration and Smart Card Removal Policy services to Disabled.
- **Benefit:** Three fewer services for a feature most PCs never use.
- **Risk:** Low. Smart cards and security keys that sign in as a smart card, including some banking and work card readers, stop working.
- **Changes:**
  - `Set service SCardSvr start type to Disabled`
  - `Set service ScDeviceEnum start type to Disabled`
  - `Set service SCPolicySvc start type to Disabled`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/iot/iot-enterprise/optimize/services>

#### Turn off the Connected Devices Platform

`services.connected-devices-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Stop the service behind sharing with nearby devices and linking phones and other devices.

- **What it does:** Sets the Connected Devices Platform Service to Disabled. Windows uses it to connect to and share with other devices.
- **Benefit:** One less background service that talks to nearby devices. On a test PC it used a few megabytes while running.
- **Risk:** Low. Sharing with nearby devices and features that link this PC to a phone or other device may stop working.
- **Changes:**
  - `Set service CDPSvc start type to Disabled`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/iot/iot-enterprise/optimize/services>

#### Turn off the Downloaded Maps Manager

`services.maps-manager-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Stop the service that keeps offline maps up to date for the Maps app.

- **What it does:** Sets the Downloaded Maps Manager service to Disabled. Windows uses it only for maps downloaded for offline use.
- **Benefit:** One less background service that starts with Windows.
- **Risk:** Low. Offline maps used by the Maps app stop updating and may stop working.
- **Changes:**
  - `Set service MapsBroker start type to Disabled`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/iot/iot-enterprise/optimize/services>

### Gaming

#### Hardware accelerated GPU scheduling

`gaming.hags-on` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Let the graphics card manage its own memory scheduling.

- **What it does:** Turns on hardware accelerated GPU scheduling. It only has an effect on a supported graphics card and driver.
- **Benefit:** Can lower latency in some games and enable features such as frame generation.
- **Risk:** Low. A few games or drivers run worse with it on. Needs a restart.
- **Changes:**
  - `Set HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\HwSchMode to 2 (DWord)`
- **Needs:** Windows build 19041 or later; administrator rights, applies to every account; a restart

#### Optimizations for windowed games: Off

`gaming.windowed-optimizations-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Keep windowed games on their own presentation model.

- **What it does:** Writes the entry SwapEffectUpgradeEnable=0 into the graphics preferences Windows keeps for your account, the same entry the switch under Settings, System, Display, Graphics writes. Other entries in that value are left as they are, and Revert puts back the whole text that was there.
- **Benefit:** Stops Windows moving a DirectX 10 or 11 game in a window onto the flip model, for a game that tears or flickers with it.
- **Risk:** Low. Windowed and borderless DirectX 10 and 11 games lose the latency gain, Auto HDR and variable refresh rate that Microsoft says the optimization allows.
- **Changes:**
  - `In HKCU\Software\Microsoft\DirectX\UserGpuPreferences\DirectXUserGlobalSettings, set the entry SwapEffectUpgradeEnable to 0`
- **Needs:** Windows build 22621 or later
- **Sources:** <https://devblogs.microsoft.com/directx/updates-in-graphics-and-gaming/>

#### Optimizations for windowed games: On

`gaming.windowed-optimizations-on` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Present DirectX 10 and 11 games in a window with the newer flip model.

- **What it does:** Writes the entry SwapEffectUpgradeEnable=1 into the graphics preferences Windows keeps for your account, the same entry the switch under Settings, System, Display, Graphics writes. Other entries in that value, such as the variable refresh rate choice, are left as they are, and Revert puts back the whole text that was there.
- **Benefit:** Microsoft says it lowers latency in windowed and borderless DirectX 10 and 11 games and allows Auto HDR and variable refresh rate in them.
- **Risk:** Low. A few games can tear or flicker with it on. DirectX 12 games already use the newer model, so it changes nothing for them.
- **Changes:**
  - `In HKCU\Software\Microsoft\DirectX\UserGpuPreferences\DirectXUserGlobalSettings, set the entry SwapEffectUpgradeEnable to 1`
- **Needs:** Windows build 22621 or later
- **Sources:** <https://devblogs.microsoft.com/directx/updates-in-graphics-and-gaming/>

#### Turn off Multi-Plane Overlay

`gaming.mpo-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Make Windows draw the desktop without separate hardware layers.

- **What it does:** Sets the value OverlayTestMode to 5 under HKLM\SOFTWARE\Microsoft\Windows\Dwm, which is the value in the mpo_disable.reg file that NVIDIA's support article gives for turning Multi-Plane Overlay off. Revert deletes the value, which is what NVIDIA's mpo_restore.reg does.
- **Benefit:** A way to test whether Multi-Plane Overlay is behind screen flicker, black flashes or glitches in windows and games.
- **Risk:** Low. NVIDIA says the feature moves some composition work to the graphics hardware to improve performance and use less power, so the desktop may use a little more of both without it. Needs a restart.
- **Changes:**
  - `Set HKLM\SOFTWARE\Microsoft\Windows\Dwm\OverlayTestMode to 5 (DWord)`
- **Needs:** Windows build 22000 or later; administrator rights, applies to every account; a restart
- **Sources:** <https://nvidia.custhelp.com/app/answers/detail/a_id/5157/~/what-is-multi-plane-overlay-%28mpo%29-in-windows-11>, <https://nvidia.custhelp.com/ci/fattach/get/824301808/0/filename/mpo_disable.reg>, <https://nvidia.custhelp.com/ci/fattach/get/824301809/0/filename/mpo_restore.reg>

#### Turn on Game Mode

`gaming.game-mode-on` · Safe · Helps in some cases · Optional: your choice · checked on build 26200

Let Windows favor games when one is running.

- **What it does:** Turns on Game Mode for your account.
- **Benefit:** Windows reduces background work while a game is in focus, which can steady frame rates on busy PCs.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\GameBar\AutoGameModeEnabled to 1 (DWord)`

### NVIDIA graphics

#### NVIDIA low latency mode: On

`gpu.nvidia-low-latency` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Limit how many frames the CPU queues ahead of the graphics card.

- **What it does:** Sets Maximum pre-rendered frames to 1 in the NVIDIA driver's global profile, which is what Low Latency Mode On in NVIDIA's own control panel does. The game can then queue one frame ahead instead of up to three. It writes the driver's own settings through NVIDIA's NVAPI, and Revert puts back what was stored before.
- **Benefit:** Less delay between a mouse click and the frame on screen in some games, mostly when the graphics card is the limit.
- **Risk:** Low. The average frame rate can drop a little. A game that uses NVIDIA Reflex overrides it, and in DirectX 12 and Vulkan games the game decides when to queue frames.
- **Changes:**
  - `Set the NVIDIA driver setting "Maximum pre-rendered frames" to 1 frame`
- **Needs:** Windows build 19041 or later; administrator rights, applies to every account
- **Sources:** <https://www.nvidia.com/content/Control-Panel-Help/vLatest/en-us/mergedProjects/nv3d/Manage_3D_Settings_(reference).htm>, <https://www.nvidia.com/en-us/geforce/news/gamescom-2019-game-ready-driver/>, <https://github.com/NVIDIA/nvapi/blob/main/NvApiDriverSettings.h>

#### NVIDIA power management: prefer maximum performance

`gpu.nvidia-max-performance` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Keep the graphics card at full speed while a 3D program runs.

- **What it does:** Sets Power management mode to Prefer maximum performance in the NVIDIA driver's global profile, so the card is not allowed to drop to lower clock speeds while a 3D program runs. It writes the driver's own setting through NVIDIA's NVAPI, and Revert puts back what was stored before.
- **Benefit:** Steadier frame rates in a game where the card clocks down too early and the frame rate dips.
- **Risk:** Low. The card uses more power and runs warmer while a 3D program is open, which shortens battery life on a laptop.
- **Changes:**
  - `Set the NVIDIA driver setting "Power management mode" to prefer maximum performance`
- **Needs:** Windows build 19041 or later; administrator rights, applies to every account
- **Sources:** <https://www.nvidia.com/content/Control-Panel-Help/vLatest/en-us/mergedProjects/nv3d/Manage_3D_Settings_(reference).htm>, <https://nvidia.custhelp.com/app/answers/detail/a_id/3130/~/setting-power-management-mode-from-normal-to-maximum-performance>, <https://github.com/NVIDIA/nvapi/blob/main/NvApiDriverSettings.h>

#### NVIDIA shader cache: no size limit

`gpu.nvidia-shader-cache-unlimited` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Let the driver keep compiled shaders for as many games as you have.

- **What it does:** Sets the shader disk cache maximum size to no limit in the NVIDIA driver's global profile. The driver's own default is 16 GB. Revert puts back what was stored before.
- **Benefit:** Compiled shaders of many large games stay stored, so a game does not rebuild them and stutter after the cache was trimmed.
- **Risk:** Low. The cache can take more disk space as you play more games, and a driver install clears it. NVIDIA warns that a limit that is too small can make the driver delete shaders you still use.
- **Changes:**
  - `Set the NVIDIA driver setting "Shader disk cache maximum size" to no limit`
- **Needs:** Windows build 19041 or later; administrator rights, applies to every account
- **Sources:** <https://www.nvidia.com/content/Control-Panel-Help/vLatest/en-us/mergedProjects/nv3d/Manage_3D_Settings_(reference).htm>, <https://nvidia.custhelp.com/app/answers/detail/a_id/5735/~/deleting-nvidia-shader-cache-files>, <https://github.com/NVIDIA/nvapi/blob/main/NvApiDriverSettings.h>

#### NVIDIA texture filtering: high performance

`gpu.nvidia-texture-performance` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Trade a little texture smoothness for a higher frame rate.

- **What it does:** Sets Texture filtering - Quality to High performance in the NVIDIA driver's global profile. NVIDIA's default for GeForce cards is Quality. Revert puts back what was stored before.
- **Benefit:** A few more frames per second on an older or weaker card.
- **Risk:** Low. Textures can look slightly less smooth, mostly in the distance. A game with its own filtering setting can override it.
- **Changes:**
  - `Set the NVIDIA driver setting "Texture filtering - Quality" to high performance`
- **Needs:** Windows build 19041 or later; administrator rights, applies to every account
- **Sources:** <https://www.nvidia.com/content/Control-Panel-Help/vLatest/en-us/mergedProjects/nv3d/Manage_3D_Settings_(reference).htm>, <https://github.com/NVIDIA/nvapi/blob/main/NvApiDriverSettings.h>

### System

#### Allow long file paths

`system.long-paths` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Let apps use paths longer than 260 characters.

- **What it does:** Turns on the long path setting for the file system. Apps must also declare support.
- **Benefit:** Fewer path too long errors in developer tools and deep folder trees.
- **Risk:** None. Apps that do not declare support behave as before.
- **Changes:**
  - `Set HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation>

#### Show detailed startup and shutdown messages

`system.verbose-status` · Safe · Look and feel only · Optional: your choice · checked on build 26200

See what Windows is doing while it starts, shuts down, signs in and signs out.

- **What it does:** Turns on the policy that replaces messages like Please wait with a message for each step, such as which service is starting.
- **Benefit:** Shows what a slow or stuck startup, shutdown or sign-in is waiting for.
- **Risk:** None. It only changes the text shown. Windows ignores it if the policy that removes these status messages is turned on.
- **Changes:**
  - `Set HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\VerboseStatus to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/troubleshoot/windows-server/performance/enable-verbose-startup-shutdown-logon-logoff-status-messages>

#### Turn off Fast Startup

`system.fast-startup-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Make Shut down a full shutdown.

- **What it does:** Sets the hybrid boot value to 0 so the kernel session is not saved on shutdown.
- **Benefit:** Clears stale driver and update state, helps dual boot and some device issues.
- **Risk:** Low. Cold boot can take a few seconds longer, mostly on hard drives.
- **Changes:**
  - `Set HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power\HiberbootEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account; a restart

#### Turn off the Program Compatibility Assistant

`compat.pca-off` · Low · Helps in some cases · Optional: your choice · checked on build 26200

No more pop-ups offering compatibility fixes after an older program runs badly.

- **What it does:** Sets the policy Turn off Program Compatibility Assistant. Windows stops offering solutions to known compatibility problems when programs run.
- **Benefit:** No compatibility pop-ups after an older installer or program closes in a way Windows thinks is a problem.
- **Risk:** Low. You lose the offered fixes for old programs, and you can still set compatibility options by hand on a program's properties.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\AppCompat\DisablePCA to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-admx-appcompat>

## Updates

### Updates

#### Delay feature updates by 180 days

`updates.defer-feature-180` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Wait 180 days after release before a new Windows version is offered.

- **What it does:** Applies the Windows Update policy that defers feature updates for 180 days.
- **Benefit:** Other people find the early problems first. Security updates still arrive on time.
- **Risk:** Low. New features arrive 180 days later than they would otherwise.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\DeferFeatureUpdates to 1 (DWord)`
  - `Set HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\DeferFeatureUpdatesPeriodInDays to 180 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-update>

#### Delay feature updates by 30 days

`updates.defer-feature-30` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

Wait 30 days after release before a new Windows version is offered.

- **What it does:** Applies the Windows Update policy that defers feature updates for 30 days.
- **Benefit:** Other people find the early problems first. Security updates still arrive on time.
- **Risk:** Low. New features arrive 30 days later than they would otherwise.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\DeferFeatureUpdates to 1 (DWord)`
  - `Set HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\DeferFeatureUpdatesPeriodInDays to 30 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-update>

#### Delay feature updates by 90 days

`updates.defer-feature-90` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Wait 90 days after release before a new Windows version is offered.

- **What it does:** Applies the Windows Update policy that defers feature updates for 90 days.
- **Benefit:** Other people find the early problems first. Security updates still arrive on time.
- **Risk:** Low. New features arrive 90 days later than they would otherwise.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\DeferFeatureUpdates to 1 (DWord)`
  - `Set HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\DeferFeatureUpdatesPeriodInDays to 90 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-update>

#### Do not wake the PC for updates

`updates.no-wake-for-updates` · Safe · Documented by the vendor · Suggested: turn on · checked on build 26200

Windows Update will not wake a sleeping PC to install updates.

- **What it does:** Applies the policy that turns off Windows Update power management wake up.
- **Benefit:** The PC stays asleep at night and does not wake to restart.
- **Risk:** None. Updates install when the PC is on.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\AUPowerManagement to 0 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-update>

#### Keep drivers out of Windows Update

`updates.exclude-drivers` · Low · Documented by the vendor · Only if it fits you · checked on build 26200

Windows Update will not install hardware drivers.

- **What it does:** Applies the policy that excludes drivers from Windows quality updates.
- **Benefit:** No surprise driver swaps. Graphics, audio and network drivers stay what you chose.
- **Risk:** Low. You update drivers yourself from the maker or a tool you trust.
- **Changes:**
  - `Set HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\ExcludeWUDriversInQualityUpdate to 1 (DWord)`
- **Needs:** Professional, ProfessionalWorkstation, ProfessionalEducation, Enterprise, Education, IoTEnterprise; administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-update>

#### No forced restart while signed in

`updates.no-auto-restart` · Low · Documented by the vendor · Suggested: turn on · checked on build 26200

Windows will not restart on its own while someone is signed in.

- **What it does:** Applies the update policy that blocks automatic restarts when a user is signed in.
- **Benefit:** No lost work from surprise restarts. Updates still install.
- **Risk:** Low. Restart yourself soon after updates install, or protections stay pending.
- **Changes:**
  - `Set HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU\NoAutoRebootWithLoggedOnUsers to 1 (DWord)`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://learn.microsoft.com/en-us/windows/deployment/update/waas-restart>
