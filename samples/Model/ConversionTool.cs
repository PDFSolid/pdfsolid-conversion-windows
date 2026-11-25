using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PDFSolid_Conversion_Demo
{
  public enum ToolGroup
  {
    FromPdf,
    MakeSearchable,
    CreatePdf
  }

  public enum ToolKind
  {
    Word,
    Excel,
    Ppt,
    Html,
    Rtf,
    Txt,
    Markdown,
    Json,
    Image,
    SearchablePdf,
    Ofd,
    WordToPdf
  }

  /// <summary>
  /// One entry in the sidebar: what it converts, how it is described, and which options it shows.
  /// The table matches docs/demo_ui/design_spec.md §5 and the macOS demo.
  /// </summary>
  public sealed class ConversionTool
  {
    private static readonly string[] PdfAndImageExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff", ".webp", ".jp2" };
    private static readonly string[] WordExtensions = { ".docx" };

    // Option keys by tool, in the order they are listed in the spec; OptionsPanel groups them into sections.
    private static readonly string[] OfficeRecognition = { "aiLayout", "aiTable", "ocr", "ocrLang", "ocrScope" };
    private static readonly string[] OcrOnly = { "ocr", "ocrLang", "ocrScope" };
    private static readonly string[] PagesAndPageSize = { "pages", "perPage", "pageSize" };

    public static readonly IReadOnlyList<ConversionTool> All = new List<ConversionTool>
    {
      new ConversionTool(ToolKind.Word, ToolGroup.FromPdf, "Word", "PDF to Word",
        "Turn PDFs and scans into editable Word documents.", DocFormat.Pdf, DocFormat.Word, ".docx",
        Keys(new[] { "images", "background", "annotations", "formulas", "layout", "font" }, OfficeRecognition, PagesAndPageSize)),
      new ConversionTool(ToolKind.Excel, ToolGroup.FromPdf, "Excel", "PDF to Excel",
        "Pull the tables out of PDFs into worksheets.", DocFormat.Pdf, DocFormat.Excel, ".xlsx",
        Keys(new[] { "images", "annotations", "formulas", "allContent", "worksheet", "font" }, OfficeRecognition,
             new[] { "pages", "perPage", "csv", "autoFolder", "pageSize" })),
      new ConversionTool(ToolKind.Ppt, ToolGroup.FromPdf, "PowerPoint", "PDF to PowerPoint",
        "Turn each page into an editable slide.", DocFormat.Pdf, DocFormat.Ppt, ".pptx",
        Keys(new[] { "images", "background", "annotations", "formulas", "font" }, OfficeRecognition, PagesAndPageSize)),
      new ConversionTool(ToolKind.Html, ToolGroup.FromPdf, "HTML", "PDF to HTML",
        "Publish PDFs as web pages.", DocFormat.Pdf, DocFormat.Html, ".html",
        Keys(new[] { "images", "annotations", "formulas", "layout", "htmlPages" }, OfficeRecognition, PagesAndPageSize)),
      new ConversionTool(ToolKind.Rtf, ToolGroup.FromPdf, "RTF", "PDF to RTF",
        "Rich text that opens in any word processor.", DocFormat.Pdf, DocFormat.Rtf, ".rtf",
        Keys(new[] { "images", "annotations", "formulas" }, OfficeRecognition, PagesAndPageSize)),
      new ConversionTool(ToolKind.Txt, ToolGroup.FromPdf, "Text", "PDF to Text",
        "Extract the text as plain .txt files.", DocFormat.Pdf, DocFormat.Txt, ".txt",
        Keys(new[] { "tableFormat" }, OfficeRecognition, PagesAndPageSize)),
      new ConversionTool(ToolKind.Markdown, ToolGroup.FromPdf, "Markdown", "PDF to Markdown",
        "Headings, lists and tables as Markdown.", DocFormat.Pdf, DocFormat.Markdown, ".md",
        Keys(new[] { "images", "annotations" }, OfficeRecognition, PagesAndPageSize)),
      new ConversionTool(ToolKind.Json, ToolGroup.FromPdf, "JSON", "PDF to JSON",
        "Document structure and content as JSON.", DocFormat.Pdf, DocFormat.Json, ".json",
        Keys(new[] { "images", "annotations", "tables" }, OfficeRecognition, PagesAndPageSize)),
      new ConversionTool(ToolKind.Image, ToolGroup.FromPdf, "Image", "PDF to Image",
        "Render every page as an image file.", DocFormat.Pdf, DocFormat.Image, "",
        new[] { "imgFormat", "colorMode", "scale", "pathEnhance", "pages" }),
      new ConversionTool(ToolKind.SearchablePdf, ToolGroup.MakeSearchable, "Searchable PDF", "Searchable PDF",
        "Add a text layer so scans can be searched and copied.", DocFormat.Pdf, DocFormat.Pdf, ".pdf",
        Keys(new[] { "images", "background", "transparent", "formulas" }, OcrOnly, PagesAndPageSize), ocrAlwaysOn: true),
      new ConversionTool(ToolKind.Ofd, ToolGroup.MakeSearchable, "Searchable OFD", "Searchable OFD",
        "Create OFD files with a searchable text layer.", DocFormat.Pdf, DocFormat.Ofd, ".ofd",
        Keys(new[] { "transparent" }, OcrOnly, PagesAndPageSize), ocrAlwaysOn: true),
      new ConversionTool(ToolKind.WordToPdf, ToolGroup.CreatePdf, "From Word", "Word to PDF",
        "Turn Word documents into PDF files.", DocFormat.Word, DocFormat.Pdf, ".pdf",
        new[] { "wordBookmarks", "pages" }, badge: "New"),
    };

    private ConversionTool(ToolKind kind, ToolGroup group, string name, string title, string description,
                           DocFormat source, DocFormat target, string outputExtension, string[] options,
                           bool ocrAlwaysOn = false, string badge = null)
    {
      Kind = kind;
      Group = group;
      Name = name;
      Title = title;
      Description = description;
      Source = source;
      Target = target;
      OutputExtension = outputExtension;
      Options = options;
      OcrAlwaysOn = ocrAlwaysOn;
      Badge = badge;
    }

    public ToolKind Kind { get; }

    public ToolGroup Group { get; }

    /// <summary>Label in the sidebar.</summary>
    public string Name { get; }

    public string Title { get; }

    public string Description { get; }

    public DocFormat Source { get; }

    /// <summary>The output format. Its color is the accent while the tool is selected.</summary>
    public DocFormat Target { get; }

    /// <summary>
    /// The chip shown in the sidebar: what varies inside the tool's group. Tools that start from a
    /// PDF differ by output, tools that create a PDF differ by input.
    /// </summary>
    public DocFormat Chip => Group == ToolGroup.CreatePdf ? Source : Target;

    /// <summary>Extension of the file the SDK writes, or empty when it writes a folder of images.</summary>
    public string OutputExtension { get; }

    /// <summary>Option keys shown in the options panel, see OptionsPanel.</summary>
    public string[] Options { get; }

    /// <summary>Searchable PDF and OFD always run OCR; the OCR switch is shown locked on.</summary>
    public bool OcrAlwaysOn { get; }

    /// <summary>Small pill next to the sidebar label, such as "New".</summary>
    public string Badge { get; }

    public bool TakesWordFiles => Group == ToolGroup.CreatePdf;

    /// <summary>What to drop, as used in "Drop … here".</summary>
    public string InputNoun => TakesWordFiles ? "Word documents" : "PDF or image files";

    public string InputSummary => TakesWordFiles ? "DOCX" : "PDF, PNG, JPG, BMP, TIFF, WEBP, JP2";

    public string OpenFileFilter => TakesWordFiles
      ? "Word documents (*.docx)|*.docx"
      : "PDF and image files|" + string.Join(";", PdfAndImageExtensions.Select(e => "*" + e));

    public bool Accepts(string path)
    {
      string extension = Path.GetExtension(path) ?? "";
      string[] accepted = TakesWordFiles ? WordExtensions : PdfAndImageExtensions;
      return accepted.Contains(extension.ToLowerInvariant());
    }

    private static string[] Keys(params string[][] parts)
    {
      return parts.SelectMany(p => p).ToArray();
    }
  }
}
