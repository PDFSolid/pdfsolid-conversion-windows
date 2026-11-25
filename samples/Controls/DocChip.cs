using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace PDFSolid_Conversion_Demo
{
  /// <summary>
  /// A dog-eared document drawn in the format's color, with its extension or two text lines on it
  /// (docs/demo_ui/design_spec.md §4.1). The geometry is defined in a 40x50 box and scaled to fit,
  /// so the macOS demo can draw the exact same shape.
  /// </summary>
  public class DocChip : FrameworkElement
  {
    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
      nameof(Fill), typeof(Brush), typeof(DocChip),
      new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
      nameof(Label), typeof(string), typeof(DocChip),
      new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Small chips have no room for text and show two white lines instead.</summary>
    public static readonly DependencyProperty ShowLinesProperty = DependencyProperty.Register(
      nameof(ShowLines), typeof(bool), typeof(DocChip),
      new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    private const double BoxWidth = 40;
    private const double BoxHeight = 50;

    private static readonly Geometry Body = Frozen(Geometry.Parse(
      "M6 0 L27 0 L40 13 L40 44 C40 47.31 37.31 50 34 50 L6 50 C2.69 50 0 47.31 0 44 L0 6 C0 2.69 2.69 0 6 0 Z"));
    private static readonly Geometry Fold = Frozen(Geometry.Parse("M27 0 L27 9 C27 11.21 28.79 13 31 13 L40 13 Z"));
    private static readonly Brush FoldBrush = Frozen(new SolidColorBrush(Color.FromArgb(97, 255, 255, 255)));
    private static readonly Brush LineBrush = Frozen(new SolidColorBrush(Color.FromArgb(191, 255, 255, 255)));
    private static readonly Typeface LabelFace = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

    public Brush Fill
    {
      get { return (Brush)GetValue(FillProperty); }
      set { SetValue(FillProperty, value); }
    }

    public string Label
    {
      get { return (string)GetValue(LabelProperty); }
      set { SetValue(LabelProperty, value); }
    }

    public bool ShowLines
    {
      get { return (bool)GetValue(ShowLinesProperty); }
      set { SetValue(ShowLinesProperty, value); }
    }

    protected override void OnRender(DrawingContext dc)
    {
      double scaleX = ActualWidth / BoxWidth;
      double scaleY = ActualHeight / BoxHeight;
      if (scaleX <= 0 || scaleY <= 0)
      {
        return;
      }

      dc.PushTransform(new ScaleTransform(scaleX, scaleY));
      dc.DrawGeometry(Fill, null, Body);
      dc.DrawGeometry(FoldBrush, null, Fold);
      if (ShowLines)
      {
        dc.DrawRoundedRectangle(LineBrush, null, new Rect(9, 25, 22, 3.5), 1.75, 1.75);
        dc.DrawRoundedRectangle(LineBrush, null, new Rect(9, 33, 15, 3.5), 1.75, 1.75);
      }
      dc.Pop();

      if (ShowLines || string.IsNullOrEmpty(Label))
      {
        return;
      }

      // Text is laid out in device space rather than under the scale, which keeps it sharp.
      double fontSize = (ActualWidth >= BoxWidth ? 11 : 12.5) * scaleX;
#pragma warning disable CS0618 // The pixelsPerDip overload needs .NET Framework 4.6.2.
      var text = new FormattedText(Label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, LabelFace, fontSize, Brushes.White);
#pragma warning restore CS0618
      double x = 20 * scaleX - text.Width / 2;
      double y = 40 * scaleY - text.Baseline;
      dc.DrawText(text, new Point(x, y));
    }

    private static T Frozen<T>(T freezable) where T : Freezable
    {
      freezable.Freeze();
      return freezable;
    }
  }
}
