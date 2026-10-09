using Clarion.App.Services;
using Clarion.Core.Updates;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Controls;

/// <summary>A list of releases drawn as a timeline of cards. Used by the What's new page.</summary>
public sealed partial class ReleaseTimeline : UserControl
{
    public static readonly DependencyProperty ReleasesProperty =
        DependencyProperty.Register(nameof(Releases), typeof(IReadOnlyList<ReleaseVm>), typeof(ReleaseTimeline),
            new PropertyMetadata(null, (d, e) => ((ReleaseTimeline)d).List.ItemsSource = e.NewValue));

    public ReleaseTimeline()
    {
        InitializeComponent();
    }

    public IReadOnlyList<ReleaseVm>? Releases
    {
        get => (IReadOnlyList<ReleaseVm>?)GetValue(ReleasesProperty);
        set => SetValue(ReleasesProperty, value);
    }

    private void OnOpenRelease(object sender, RoutedEventArgs e)
    {
        // The address came from the network when it was a newer release, so it is only opened when it is this project's release page.
        if (sender is HyperlinkButton { Tag: string url } && UpdateCheck.IsTrustedReleaseUrl(url)) Shell.Open(url);
    }
}
