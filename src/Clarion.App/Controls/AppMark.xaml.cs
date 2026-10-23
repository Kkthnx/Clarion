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

/// <summary>An app's logo, first found of: its outline in the brand colour, the picture saved inside the program, the installed app's own icon, a plain tile with its first letter.</summary>
public sealed partial class AppMark : UserControl
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(AppIcon), typeof(AppMark), new PropertyMetadata(null, (d, _) => ((AppMark)d).Refresh()));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(AppMark), new PropertyMetadata("", (d, _) => ((AppMark)d).Refresh()));

    public static readonly DependencyProperty ImagePathProperty =
        DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(AppMark), new PropertyMetadata(null, (d, _) => ((AppMark)d).OnImagePathChanged()));
    public static readonly DependencyProperty LogoFileProperty =
        DependencyProperty.Register(nameof(LogoFile), typeof(string), typeof(AppMark), new PropertyMetadata(null, (d, _) => ((AppMark)d).OnLogoFileChanged()));

    // The logo pictures saved inside the program, decoded once and shared by every row that shows one. Used on the UI thread only.
    private static readonly Dictionary<string, BitmapImage> Logos = new(StringComparer.OrdinalIgnoreCase);

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

    /// <summary>The icon file of the installed app. It stands in for the letter tile when there is no logo.</summary>
    public string? ImagePath { get => (string?)GetValue(ImagePathProperty); set => SetValue(ImagePathProperty, value); }
    /// <summary>
    /// The logo picture saved inside the program, for an app with no outline. It is a file beside the program, so showing it never uses the
    /// network. A file that cannot be decoded leaves the letter tile.
    /// </summary>
    public string? LogoFile { get => (string?)GetValue(LogoFileProperty); set => SetValue(LogoFileProperty, value); }

    private void OnLogoFileChanged()
    {
        Bundled.Source = null;
        var path = LogoFile;
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                if (!Logos.TryGetValue(path, out var image))
                {
                    image = new BitmapImage { DecodePixelWidth = 128 };
                    var shared = image;
                    // A picture that will not decode must not leave an empty square, so it is forgotten and the letter tile comes back.
                    image.ImageFailed += (_, e) =>
                    {
                        Log.Write($"The logo file {path} could not be shown: {e.ErrorMessage}");
                        Logos.Remove(path);
                        if (ReferenceEquals(Bundled.Source, shared)) { Bundled.Source = null; Refresh(); }
                    };
                    image.UriSource = new Uri(path);
                    if (Logos.Count > 400) Logos.Clear();
                    Logos[path] = image;
                }
                Bundled.Source = image;
            }
            catch (Exception ex) when (ex is UriFormatException or ArgumentException or InvalidOperationException)
            {
                Log.Write($"The logo file {path} could not be opened: {ex.Message}");
            }
        }
        Refresh();
    }

    private void OnThemeChanged(FrameworkElement sender, object args) => Refresh();

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var side = Math.Min(e.NewSize.Width, e.NewSize.Height);
        Letter.FontSize = Math.Max(10, side * 0.45);
        // A mark in a tile is wide, so it gets less room around it than a square one has, and its words stay readable.
        _padding = side * 0.06;
        if (_tiled) Glyph.Margin = new Thickness(_padding);
    }

    /// <summary>The widest outline, as width over height, that is drawn inside a tile. Wider than this it would be a sliver at icon size.</summary>
    private const double MostWideAspect = 3.0;

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
                    Log.Write($"Icon {path} could not be read: {(ex.Message.Length > 0 ? ex.Message : ex.GetType().Name)} (0x{ex.HResult:X8})");
                }
            }
        }
        if (request == _photoRequest) Refresh();
    }
    private void Refresh()
    {
        // Drawing waits for the control to be in the tree, where the theme is known. Loaded draws it once with every property already set.
        if (!_loaded) return;
        // A thin outline is not readable at icon size, so it gives way to the letter tile: a very thin one everywhere, and any wide one
        // in a small slot. A wide or word shaped outline up to three times as wide as it is tall sits in a tile, and is readable in a full size slot.
        var small = (double.IsNaN(Width) ? ActualWidth : Width) < 32;
        var icon = Icon is { } i && (i.Shape == LogoShape.Square || (!small && i.Aspect < MostWideAspect)) ? i : null;

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
        // A wide outline sits inside a tile so it reads as a mark and not a stray line.
        _tiled = icon.Shape != LogoShape.Square;
        // In a High Contrast theme the colours are the person's own choice, so a brand colour is not used at all.
        var highContrast = Accessibility.HighContrast;
        // A brand colour dark enough for white to read on it fills the tile and the mark is white, the way an app icon usually looks. Any other
        // colour keeps the plain tile and its own colour for the mark.
        var onBrand = _tiled && !highContrast && icon.Luminance <= 0.30;
        if (_tiled)
        {
            var (br, bg, bb) = icon.Rgb;
            var brand = new SolidColorBrush(Windows.UI.Color.FromArgb(255, br, bg, bb));
            Tile.Background = onBrand ? brand : ThemeBrushes.Get("ClSurface2", ActualTheme);
            Tile.BorderBrush = onBrand ? brand : ThemeBrushes.Get("ClBorder", ActualTheme);
        }
        Tile.Visibility = _tiled ? Visibility.Visible : Visibility.Collapsed;
        Letter.Visibility = Visibility.Collapsed;
        Glyph.Margin = _tiled ? new Thickness(_padding) : new Thickness(0);
        Glyph.Visibility = Visibility.Visible;
        Photo.Visibility = Visibility.Collapsed;
        Bundled.Visibility = Visibility.Collapsed;
        if (_shownPath != icon.Path)
        {
            // F1 is the non-zero fill rule, which is what SVG uses and what the outlines were drawn for.
            Glyph.Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), "F1" + icon.Path);
            _shownPath = icon.Path;
        }
        // A dark logo on a dark card, or a pale one on a light card, would disappear, so the text colour stands in.
        var dark = ActualTheme != ElementTheme.Light;
        Glyph.Visibility = Visibility.Visible;
        if (onBrand)
        {
            Glyph.Fill = new SolidColorBrush(Microsoft.UI.Colors.White);
        }
        else if (!highContrast && icon.ReadableOn(dark))
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
        // No outline: the picture saved inside the program, which was looked at one by one, then the icon of the installed app if Windows lists one (some are a generic installer icon), and last the letter tile.
        var bundled = Bundled.Source is not null;
        var photo = !bundled && Photo.Source is not null;
        Photo.Visibility = photo ? Visibility.Visible : Visibility.Collapsed;
        Bundled.Visibility = bundled ? Visibility.Visible : Visibility.Collapsed;
        Tile.Visibility = photo || bundled ? Visibility.Collapsed : Visibility.Visible;
        Letter.Visibility = Visibility.Visible;
        var letter = AppTileColors.LetterOf(Label);
        Letter.Text = letter;
        // A tile of its own tint for each app, so a list of them is told apart at a glance and none looks like a missing picture.
        if (!Accessibility.HighContrast && letter.Length > 0)
        {
            var (tile, ink) = AppTileColors.For(Label, ActualTheme != ElementTheme.Light);
            Tile.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, tile.R, tile.G, tile.B));
            Tile.BorderBrush = ThemeBrushes.Get("ClBorder", ActualTheme);
            Letter.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, ink.R, ink.G, ink.B));
        }
        else
        {
            Tile.Background = ThemeBrushes.Get("ClSurface2", ActualTheme);
            Tile.BorderBrush = ThemeBrushes.Get("ClBorder", ActualTheme);
            Letter.Foreground = ThemeBrushes.Get("ClTextSecondary", ActualTheme);
        }
    }
}
