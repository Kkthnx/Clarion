using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Services;

/// <summary>Small message dialogs used all over the app.</summary>
public static class Dialogs
{
    // E_ASYNC_OPERATION_NOT_STARTED, the answer WinUI gives when a dialog is shown while another one is open ("Only a single ContentDialog
    // can be open at any time"). A job log or a question can be open when something else wants to speak, such as the window being closed.
    private const int AnotherDialogOpen = unchecked((int)0x80000019);

    public static async Task Say(XamlRoot root, string title, string text) =>
        await ShowWhenFree(new ContentDialog
        {
            Title = title,
            Content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap },
            CloseButtonText = "OK",
            XamlRoot = root,
        });

    /// <summary>Shows the dialog as soon as no other one is open, trying twice a second for a minute. Null when it never got its turn.</summary>
    public static async Task<ContentDialogResult?> ShowWhenFree(ContentDialog dialog)
    {
        for (var attempt = 0; attempt < 120; attempt++)
        {
            if (await TryShow(dialog) is { } result) return result;
            await Task.Delay(500);
        }
        return null;
    }

    /// <summary>Shows the dialog only when no other one is open. Null when one is, which already has the person's attention.</summary>
    public static Task<ContentDialogResult?> ShowIfFree(ContentDialog dialog) => TryShow(dialog);

    private static async Task<ContentDialogResult?> TryShow(ContentDialog dialog)
    {
        try { return await dialog.ShowAsync(); }
        catch (COMException ex) when (ex.HResult == AnotherDialogOpen) { return null; }
    }
}
