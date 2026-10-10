namespace Clarion.Core.Appx;

/// <summary>
/// The one sentence that goes with every app removal. Microsoft's app data documentation says the system removes an app's data stores
/// completely when the app is uninstalled, so Revert can reinstall an app but cannot bring back what it saved. Anything in the app that
/// is not also kept somewhere else, such as notes that were never synced, is gone.
/// </summary>
public static class AppRemovalNotice
{
    public const string Text = "Removing an app also removes the data it saved for your account. Revert reinstalls the app, not that data.";

    public const string Source = "https://learn.microsoft.com/en-us/windows/apps/design/app-settings/store-and-retrieve-app-data";
}
