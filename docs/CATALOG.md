# Settings reference

This page is written from the catalog by `Clarion.exe --catalog-doc`. Do not edit it by hand. Change the catalog and write the page again.

265 settings. Each one names what it changes, how risky it is, how well the effect is documented, and where that comes from.

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
- **Risk:** None. These are per account preferences and are not documented as a supported policy, so Windows can reset them after a feature update. Clarion detects that and offers to reapply.
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
  - `Set HKCU\Control Panel\Accessibility\StickyKeys\Flags to 506 (String)`
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

### Development

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
  - `Install Blizzard.BattleNet with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/Blizzard/BattleNet>, <https://download.battle.net/en-us/desktop>, <https://www.blizzard.com/legal/fba4d00f-c7e4-4883-b8b9-1b4500a402ea/blizzard-end-user-license-agreement>

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

#### IrfanView

`app.irfanview` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A small, fast image viewer.

- **What it does:** Installs IrfanView with winget, pinned to the package IrfanSkiljan.IrfanView in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get IrfanView without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install IrfanSkiljan.IrfanView with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/i/IrfanSkiljan/IrfanView>, <https://www.irfanview.com/>, <https://www.irfanview.com/eula.htm>

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

### Security and privacy

#### Bitwarden

`app.bitwarden` · Low · Documented by the vendor · Optional: your choice · checked on build 26200

A password manager.

- **What it does:** Installs Bitwarden with winget, pinned to the package Bitwarden.Bitwarden in winget's community source. Turning it off runs the program's own uninstaller through winget.
- **Benefit:** You get Bitwarden without hunting for the right download page, and winget checks the installer's checksum before it runs.
- **Risk:** Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them.
- **Changes:**
  - `Install Bitwarden.Bitwarden with winget`
- **Needs:** administrator rights, applies to every account
- **Sources:** <https://github.com/microsoft/winget-pkgs/tree/master/manifests/b/Bitwarden/Bitwarden>, <https://bitwarden.com/>, <https://github.com/bitwarden/clients/blob/HEAD/LICENSE.txt>

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

### System tools

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

### Network

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

- **What it does:** Sets the Connected User Experiences and Telemetry service (DiagTrack) and the WAP push message routing service (dmwappushservice) to Disabled and stops them.
- **Benefit:** Removes the background process that gathers and uploads diagnostic data.
- **Risk:** Low. Some feedback and diagnostic features report less. Windows Update is not affected.
- **Changes:**
  - `Set service DiagTrack start type to Disabled`
  - `Set service dmwappushservice start type to Disabled`
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

#### Turn on Game Mode

`gaming.game-mode-on` · Safe · Helps in some cases · Optional: your choice · checked on build 26200

Let Windows favor games when one is running.

- **What it does:** Turns on Game Mode for your account.
- **Benefit:** Windows reduces background work while a game is in focus, which can steady frame rates on busy PCs.
- **Risk:** None.
- **Changes:**
  - `Set HKCU\Software\Microsoft\GameBar\AutoGameModeEnabled to 1 (DWord)`

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

#### Turn off Fast Startup

`system.fast-startup-off` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Make Shut down a full shutdown.

- **What it does:** Sets the hybrid boot value to 0 so the kernel session is not saved on shutdown.
- **Benefit:** Clears stale driver and update state, helps dual boot and some device issues.
- **Risk:** Low. Cold boot can take a few seconds longer, mostly on hard drives.
- **Changes:**
  - `Set HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power\HiberbootEnabled to 0 (DWord)`
- **Needs:** administrator rights, applies to every account; a restart

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
