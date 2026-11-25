using System.Windows.Media;

namespace PDFSolid_Conversion_Demo
{
  /// <summary>
  /// A document format and the color that stands for it (docs/demo_ui/design_spec.md §2.2).
  /// Base carries white text; Hi is the lighter variant used for lines and tints in the dark theme.
  /// </summary>
  public sealed class DocFormat
  {
    public static readonly DocFormat Pdf = new DocFormat("PDF", "#C9252C", "#F2615F");
    public static readonly DocFormat Word = new DocFormat("DOCX", "#185ABD", "#6E9CF0");
    public static readonly DocFormat Excel = new DocFormat("XLSX", "#107C41", "#4CC382");
    public static readonly DocFormat Ppt = new DocFormat("PPTX", "#C43E1C", "#F07A56");
    public static readonly DocFormat Html = new DocFormat("HTML", "#0F766E", "#3CC3B5");
    public static readonly DocFormat Rtf = new DocFormat("RTF", "#5B45D6", "#9C8CFF");
    public static readonly DocFormat Txt = new DocFormat("TXT", "#4B5563", "#A1ABB9");
    public static readonly DocFormat Markdown = new DocFormat("MD", "#8E3BBF", "#C88BEA");
    public static readonly DocFormat Json = new DocFormat("JSON", "#A16207", "#E3AE3C");
    public static readonly DocFormat Image = new DocFormat("IMG", "#BE185D", "#F06CA2");
    public static readonly DocFormat Ofd = new DocFormat("OFD", "#A61B29", "#E8646E");

    private DocFormat(string label, string baseColor, string hiColor)
    {
      Label = label;
      Base = (Color)ColorConverter.ConvertFromString(baseColor);
      Hi = (Color)ColorConverter.ConvertFromString(hiColor);
      BaseBrush = new SolidColorBrush(Base);
      BaseBrush.Freeze();
    }

    /// <summary>Extension shown on the document chip.</summary>
    public string Label { get; }

    public Color Base { get; }

    public Color Hi { get; }

    public SolidColorBrush BaseBrush { get; }

    /// <summary>
    /// The format an input file is shown as in the file list: PDFs are red, Word files blue,
    /// and anything else is treated as an image.
    /// </summary>
    public static DocFormat ForInputExtension(string extension)
    {
      switch (extension.ToLowerInvariant())
      {
        case ".pdf": return Pdf;
        case ".doc":
        case ".docx": return Word;
        default: return Image;
      }
    }
  }
}
