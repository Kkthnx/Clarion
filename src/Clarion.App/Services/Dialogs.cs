using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Services;

/// <summary>Small message dialogs used all over the app.</summary>
public static class Dialogs
{
    public static async Task Say(XamlRoot root, string title, string text) =>
        await new ContentDialog
        {
            Title = title,
            Content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap },
            CloseButtonText = "OK",
            XamlRoot = root,
        }.ShowAsync();
}
