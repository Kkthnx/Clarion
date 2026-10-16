using Clarion.App.Services;
using Clarion.Core.Apps;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Clarion.App.Controls;

/// <summary>An app's logo, drawn from its outline in the brand colour, or a plain tile with its first letter.</summary>
public sealed partial class AppMark : UserControl
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(AppIcon), typeof(AppMark), new PropertyMetadata(null, (d, _) => ((AppMark)d).Refresh()));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(AppMark), new PropertyMetadata("", (d, _) => ((AppMark)d).Refresh()));

    public static readonly DependencyProperty IsWatermarkProperty =
        DependencyProperty.Register(nameof(IsWatermark), typeof(bool), typeof(AppMark), new PropertyMetadata(false, (d, _) => ((AppMark)d).Refresh()));

    public static readonly DependencyProperty ImagePathProperty =
        DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(AppMark), new PropertyMetadata(null, (d, _) => ((AppMark)d).OnImagePathChanged()));
    // Icons already read, so a row that scrolls out and back does not read its file again. Used on the UI thread only.
    private static readonly Dictionary<string, BitmapImage> Photos = new(StringComparer.OrdinalIgnoreCase);

    // One shared reader of the High Contrast setting, since making one for every refresh of every mark is wasted work.
    private static readonly Windows.UI.ViewManagement.AccessibilitySettings Accessibility = new();

    private string? _shownPath;
    private int _photoRequest;
    private bool _loaded;

    public AppMark()
    {
        InitializeComponent();
        // The name sits beside it in the row, so a screen reader has nothing to add by announcing a picture or a letter.
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetAccessibilityView(this, Microsoft.UI.Xaml.Automation.Peers.AccessibilityView.Raw);
        // The colour depends on the theme, so it is worked out again when the theme changes.
        Loaded += (_, _) => { _loaded = true; ActualThemeChanged += OnThemeChanged; Refresh(); };
        Unloaded += (_, _) => { _loaded = false; ActualThemeChanged -= OnThemeChanged; };
    }

    public AppIcon? Icon { get => (AppIcon?)GetValue(IconProperty); set => SetValue(IconProperty, value); }

    /// <summary>The app's name. Its first character is shown when there is no logo.</summary>
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    /// <summary>A faded mark behind other content. With no logo nothing is drawn, since a lone letter would only be noise.</summary>
    public bool IsWatermark { get => (bool)GetValue(IsWatermarkProperty); set => SetValue(IsWatermarkProperty, value); }

    /// <summary>The icon file of the installed app. It stands in for the letter tile when there is no logo.</summary>
    public string? ImagePath { get => (string?)GetValue(ImagePathProperty); set => SetValue(ImagePathProperty, value); }
    private void OnThemeChanged(FrameworkElement sender, object args) => Refresh();

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var side = Math.Min(e.NewSize.Width, e.NewSize.Height);
        Letter.FontSize = Math.Max(10, side * 0.45);
        _padding = side * 0.17;
        if (_tiled) Glyph.Margin = new Thickness(_padding);
    }

    private double _padding;
    private bool _tiled;

    private async void OnImagePathChanged()
    {
        var path = ImagePath;
        var request = ++_photoRequest;
        Photo.Source = null;
        if (!string.IsNullOrEmpty(path))
        {
            // The file's write time is part of the key, so an app that was updated and changed its icon is read again. Any trouble
            // reading it means no picture, and must never end the app, since this runs in an async void method.
            string? key = null;
            try { key = path + "|" + File.GetLastWriteTimeUtc(path).Ticks; }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                Log.Write($"Icon {path} could not be checked: {ex.Message}");
            }
            if (key is not null && Photos.TryGetValue(key, out var cached)) Photo.Source = cached;
            else if (key is not null)
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(path);
                    using var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 64, ThumbnailOptions.UseCurrentScale);
                    if (thumbnail is { Size: > 0 } && request == _photoRequest)
                    {
                        var image = new BitmapImage();
                        await image.SetSourceAsync(thumbnail);
                        if (Photos.Count > 400) Photos.Clear();
                        Photos[key] = image;
                        if (request == _photoRequest) Photo.Source = image;
                    }
                }
                catch (Exception ex) when (ex is not OutOfMemoryException)
                {
                    // A file that is gone, cannot be read or has no usable icon leaves the letter tile. This runs in an async void method,
                    // where an exception that escapes would end the app, and a missing picture is never worth that.
                    Log.Write($"Icon {path} could not be read: {ex.Message}");
                }
            }
        }
        if (request == _photoRequest) Refresh();
    }
    private void Refresh()
    {
        // Drawing waits for the control to be in the tree, where the theme is known. Loaded draws it once with every property already set.
        if (!_loaded) return;
        // A thin outline is not readable at icon size, so it gives way to the letter tile: a very thin one everywhere, and a wide one
        // in a small slot. A faded watermark has the room, so it always shows the real outline.
        var small = (double.IsNaN(Width) ? ActualWidth : Width) < 32;
        var icon = Icon is { } i && (IsWatermark || i.Shape == LogoShape.Square || (i.Shape == LogoShape.Wide && !small)) ? i : null;

        if (icon is not null && TryShowOutline(icon)) return;
        ShowLetterOrPhoto();
    }

    /// <summary>
    /// Draws the logo's outline. A bad outline or colour in the catalog, for example after a careless merge, falls back to the letter tile
    /// and is logged with the app's name, because this runs on the UI thread and an error here would otherwise reach every row that shows it.
    /// </summary>
    private bool TryShowOutline(AppIcon icon)
    {
        try
        {
            ShowOutline(icon);
            return true;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Log.Write($"The logo for {icon.Title} could not be drawn, so the letter tile is used: {ex.Message}");
            _shownPath = null;
            return false;
        }
    }

    private void ShowOutline(AppIcon icon)
    {
        // A wide outline sits inside a plain tile so it reads as a mark and not a stray line.
        _tiled = !IsWatermark && icon.Shape == LogoShape.Wide;
        Tile.Visibility = _tiled ? Visibility.Visible : Visibility.Collapsed;
        Letter.Visibility = Visibility.Collapsed;
        Glyph.Margin = _tiled ? new Thickness(_padding) : new Thickness(0);
        Glyph.Visibility = Visibility.Visible;
        Photo.Visibility = Visibility.Collapsed;
        if (_shownPath != icon.Path)
        {
            // F1 is the non-zero fill rule, which is what SVG uses and what the outlines were drawn for.
            Glyph.Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), "F1" + icon.Path);
            _shownPath = icon.Path;
        }
        // A dark logo on a dark card, or a pale one on a light card, would disappear, so the text colour stands in.
        var dark = ActualTheme != ElementTheme.Light;
        // In a High Contrast theme the colours are the person's own choice, so a brand colour is not used at all.
        var highContrast = Accessibility.HighContrast;
        Glyph.Visibility = IsWatermark && highContrast ? Visibility.Collapsed : Visibility.Visible;
        if (!highContrast && icon.ReadableOn(dark))
        {
            var (r, g, b) = icon.Rgb;
            Glyph.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));
        }
        else
        {
            Glyph.Fill = ThemeBrushes.Get("ClTextPrimary", ActualTheme);
        }
    }

    private void ShowLetterOrPhoto()
    {
        _tiled = false;
        Glyph.Visibility = Visibility.Collapsed;
        _shownPath = null;
        // No logo: the real icon of the installed app if Windows lists one, otherwise the letter tile.
        var photo = !IsWatermark && Photo.Source is not null;
        Photo.Visibility = photo ? Visibility.Visible : Visibility.Collapsed;
        Tile.Visibility = IsWatermark || photo ? Visibility.Collapsed : Visibility.Visible;
        Letter.Visibility = Visibility.Visible;
        Letter.Text = Label.Length > 0 ? char.ToUpperInvariant(Label[0]).ToString() : "";
    }
}
