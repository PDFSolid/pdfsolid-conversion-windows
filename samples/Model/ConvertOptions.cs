using PDFSolid_Conversion.Common;

namespace PDFSolid_Conversion_Demo
{
  /// <summary>
  /// Option values chosen in the options panel. One instance is shared by every tool, so a value
  /// set for one conversion carries over to the next; the defaults are the SDK defaults.
  /// </summary>
  public class ConvertOptions
  {
    /// <summary>Returns an independent snapshot for a conversion worker.</summary>
    public ConvertOptions Copy()
    {
      return (ConvertOptions)MemberwiseClone();
    }

    public OCRLanguage OCRLanguage = OCRLanguage.e_ENGLISH;
    public bool ContainAnnotations = true;
    public bool CsvFormat = false;
    public bool AllContent = false;
    public bool ContainImages = true;
    public bool ContainPageBackgroundImage = true;
    public bool TransparentText = true;
    public bool AutoCreateFolder = true;
    public bool OutputDocumentPerPage = false;
    public bool EnableAiLayout = true;
    public bool EnableAiTableRecognition = true;
    public bool EnableOCR = false;
    public bool EnableDocumentOrientationClassification = false;
    public bool EnableDocumentDewarp = false;
    public bool TxtTableFormat = true;
    public bool FormulaToImage = true;
    public bool ImagePathEnhance = false;
    public bool ContainTables = true;
    public float ImageRatio = 4.0f;
    public PageLayoutMode LayoutMode = PageLayoutMode.e_Flow;
    public ExcelWorksheetOption WorksheetOption = ExcelWorksheetOption.e_ForTable;
    public HtmlPageOption htmlOption = HtmlPageOption.e_SinglePage;
    public ImageType ImageFormat = ImageType.JPG;
    public ImageColorMode ImageMode = ImageColorMode.Color;
    public OCROption OcrOption = OCROption.e_All;
    public string FontName = "";
    public string PageRanges = "";
    public WordPdfBookmarkMode WordBookmarkMode = WordPdfBookmarkMode.e_None;

    /// <summary>
    /// Output page for an image input. The SDK takes a size in points rather than a paper
    /// name, so the name shown in the UI is resolved to its dimensions here.
    /// </summary>
    public ImagePageSize ImagePageSize = ImagePageSize.Original;
  }

  /// <summary>
  /// The page an image input is laid out on. Only meaningful when converting an image file.
  /// </summary>
  public enum ImagePageSize
  {
    /// <summary>One image pixel becomes one page point, whatever the image's resolution.</summary>
    Original,
    /// <summary>Measure the image from its resolution and use the standard paper it is closest to.</summary>
    Auto,
    A3,
    A4,
    A5,
    /// <summary>JIS B4, 257 x 364 mm.</summary>
    B4,
    /// <summary>JIS B5, 182 x 257 mm.</summary>
    B5,
    /// <summary>JIS B6, 128 x 182 mm.</summary>
    B6,
    Letter,
    Legal,
    Tabloid
  }

  /// <summary>
  /// Turns a paper name into the page size the SDK expects.
  /// </summary>
  public static class ImagePageSizes
  {
    private const float MillimetresPerInch = 25.4f;
    private const float PointsPerInch = 72.0f;

    private static float Mm(float millimetres)
    {
      return millimetres / MillimetresPerInch * PointsPerInch;
    }

    private static float In(float inches)
    {
      return inches * PointsPerInch;
    }

    /// <summary>
    /// Applies the chosen page to a set of conversion options. Original and Auto name no
    /// paper, so they leave the custom size unset and let the SDK decide.
    /// </summary>
    public static void Apply(ImagePageSize size, out bool enableAuto, out float width, out float height)
    {
      enableAuto = size == ImagePageSize.Auto;
      width = 0.0f;
      height = 0.0f;

      switch (size)
      {
        // ISO 216 and JIS P 0138 are millimetre standards; US sizes are inch standards.
        case ImagePageSize.A3: width = Mm(297); height = Mm(420); break;
        case ImagePageSize.A4: width = Mm(210); height = Mm(297); break;
        case ImagePageSize.A5: width = Mm(148); height = Mm(210); break;
        case ImagePageSize.B4: width = Mm(257); height = Mm(364); break;
        case ImagePageSize.B5: width = Mm(182); height = Mm(257); break;
        case ImagePageSize.B6: width = Mm(128); height = Mm(182); break;
        case ImagePageSize.Letter: width = In(8.5f); height = In(11); break;
        case ImagePageSize.Legal: width = In(8.5f); height = In(14); break;
        case ImagePageSize.Tabloid: width = In(11); height = In(17); break;
      }
    }
  }
}
