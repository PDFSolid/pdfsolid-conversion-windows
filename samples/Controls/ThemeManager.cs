using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace PDFSolid_Conversion_Demo
{
  /// <summary>
  /// Follows the Windows light/dark setting and keeps the accent on the selected tool's format
  /// (docs/demo_ui/design_spec.md §2.3). Controls reference every color as a DynamicResource,
  /// so swapping resources here restyles the running window.
  /// </summary>
  public static class ThemeManager
  {
    private const int DwmUseImmersiveDarkMode = 20;
    private const int DwmCaptionColor = 35;
    private const int DwmTextColor = 36;

    private static ResourceDictionary palette;
    private static DocFormat accent = DocFormat.Word;
    private static Window window;

    public static bool IsDark { get; private set; }

    /// <summary>Raised after the palette has been swapped.</summary>
    public static event EventHandler Changed;

    /// <summary>Loads the palette that matches the system setting and starts following it.</summary>
    public static void Initialize()
    {
      Apply(ReadSystemDark());
      SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
      Application.Current.Exit += (sender, args) => SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
    }

    /// <summary>Colors the native title bar of the window to match the sidebar.</summary>
    public static void Attach(Window mainWindow)
    {
      window = mainWindow;
      mainWindow.SourceInitialized += (sender, args) => ColorTitleBar();
    }

    public static void SetAccent(DocFormat format)
    {
      accent = format;
      Color hi = IsDark ? format.Hi : format.Base;
      ResourceDictionary resources = Application.Current.Resources;
      resources["AccentBrush"] = Brush(format.Base);
      resources["AccentHoverBrush"] = Brush(Mix(format.Base, Colors.Black, 0.10));
      resources["AccentPressedBrush"] = Brush(Mix(format.Base, Colors.Black, 0.18));
      resources["AccentHiBrush"] = Brush(hi);
      // 11% of the accent in light, 18% in dark, laid over the surface below.
      resources["SelSoftBrush"] = Brush(Color.FromArgb((byte)(IsDark ? 46 : 28), hi.R, hi.G, hi.B));
    }

    /// <summary>Whether the user has asked Windows to cut down on animation.</summary>
    public static bool ReduceMotion => !SystemParameters.ClientAreaAnimation;

    private static void Apply(bool dark)
    {
      IsDark = dark;
      var next = new ResourceDictionary { Source = new Uri(dark ? "/Themes/Dark.xaml" : "/Themes/Light.xaml", UriKind.Relative) };
      var merged = Application.Current.Resources.MergedDictionaries;
      if (palette != null)
      {
        merged.Remove(palette);
      }
      merged.Insert(0, next);
      palette = next;

      SetAccent(accent);
      ColorTitleBar();
      Changed?.Invoke(null, EventArgs.Empty);
    }

    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
      if (e.Category != UserPreferenceCategory.General)
      {
        return;
      }
      Application.Current.Dispatcher.BeginInvoke(new Action(() =>
      {
        bool dark = ReadSystemDark();
        if (dark != IsDark)
        {
          Apply(dark);
        }
      }));
    }

    private static bool ReadSystemDark()
    {
      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
      {
        object value = key?.GetValue("AppsUseLightTheme");
        return value is int light && light == 0;
      }
    }

    // Windows 11 lets an app color its title bar. Older systems reject the call and keep the default.
    private static void ColorTitleBar()
    {
      if (window == null)
      {
        return;
      }
      IntPtr hwnd = new WindowInteropHelper(window).Handle;
      if (hwnd == IntPtr.Zero)
      {
        return;
      }

      int dark = IsDark ? 1 : 0;
      DwmSetWindowAttribute(hwnd, DwmUseImmersiveDarkMode, ref dark, sizeof(int));
      int caption = ColorRef((Color)Application.Current.Resources["Paper2Color"]);
      DwmSetWindowAttribute(hwnd, DwmCaptionColor, ref caption, sizeof(int));
      int text = ColorRef((Color)Application.Current.Resources["InkColor"]);
      DwmSetWindowAttribute(hwnd, DwmTextColor, ref text, sizeof(int));
    }

    private static int ColorRef(Color color)
    {
      return color.R | (color.G << 8) | (color.B << 16);
    }

    private static Color Mix(Color color, Color with, double amount)
    {
      return Color.FromRgb(
        (byte)Math.Round(color.R + (with.R - color.R) * amount),
        (byte)Math.Round(color.G + (with.G - color.G) * amount),
        (byte)Math.Round(color.B + (with.B - color.B) * amount));
    }

    private static SolidColorBrush Brush(Color color)
    {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      return brush;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
  }
}
