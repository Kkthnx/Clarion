# Settings reference

This page is written from the catalog by `Clarion.exe --catalog-doc`. Do not edit it by hand. Change the catalog and write the page again.

177 settings. Each one names what it changes, how risky it is, how well the effect is documented, and where that comes from.

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

`debloat.app.microsoftstickynotes` · Low · Helps in some cases · Only if it fits you · checked on build 26200

Uninstall Sticky Notes for your account.

- **What it does:** Removes the Microsoft.MicrosoftStickyNotes package for the current account. Other accounts are not touched.
- **Benefit:** Less clutter in Start and search, and no background updates for the app.
- **Risk:** Low. The app can be put back with Revert or from the Microsoft Store.
- **Changes:**
  - `Remove app Microsoft.MicrosoftStickyNotes for this account`

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

### Microsoft Edge

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
