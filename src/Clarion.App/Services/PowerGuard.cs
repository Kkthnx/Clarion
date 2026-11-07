using Clarion.Core.SystemInfo;
using Clarion.Engine;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Services;

/// <summary>Asks before a long job starts on a laptop that is nearly flat and not plugged in. See <see cref="BatteryGuard"/>.</summary>
public static class PowerGuard
{
    /// <summary>True to go on. A desktop, a laptop on its charger or with enough charge goes on without a question.</summary>
    public static async Task<bool> ConfirmAsync(XamlRoot root, string job, int? minutes = null)
    {
        var warning = BatteryGuard.Warning(WindowsMachineTraits.ReadPower(), job, minutes);
        if (warning is null) return true;
        Log.Write($"Battery warning before: {job}");
        var dialog = new ContentDialog
        {
            Title = "Plug in before you start",
            Content = new TextBlock { Text = warning, TextWrapping = TextWrapping.Wrap, MaxWidth = 460 },
            PrimaryButtonText = "Start anyway",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = root,
        };
        return await Dialogs.ShowWhenFree(dialog) == ContentDialogResult.Primary;
    }
}
