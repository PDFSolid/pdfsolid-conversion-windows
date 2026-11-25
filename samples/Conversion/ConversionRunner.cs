using PDFSolid_Conversion.Common;
using PDFSolid_Conversion.Conversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PDFSolid_Conversion_Demo
{
  /// <summary>
  /// Sends one file through the SDK. This is the file to read for SDK usage: every conversion
  /// fills the SDK's options object from the values in the options panel and calls CPDFConversion.
  /// </summary>
  public static class ConversionRunner
  {
    /// <summary>
    /// Converts one file.
    /// </summary>
    /// <param name="revealPath">
    /// Where the result is expected, for the row's Show button. PDF-to-X conversions hand the SDK a
    /// folder and let it name the file, so this is only a guess; see <see cref="FindOutput"/>.
    /// </param>
    public static ErrorCode Run(ConversionTool tool, string input, string outputFolder, ConvertOptions options,
                                ConvertCallback callback, out string revealPath)
    {
      string name = Path.GetFileNameWithoutExtension(input);
      revealPath = Path.Combine(outputFolder, name + tool.OutputExtension);

      switch (tool.Kind)
      {
        case ToolKind.Word:
          return CPDFConversion.StartPDFToWord(input, "", outputFolder, WordOptionsFrom(options), callback);
        case ToolKind.Excel:
          return CPDFConversion.StartPDFToExcel(input, "", outputFolder, ExcelOptionsFrom(options), callback);
        case ToolKind.Ppt:
          return CPDFConversion.StartPDFToPpt(input, "", outputFolder, PptOptionsFrom(options), callback);
        case ToolKind.Html:
          return CPDFConversion.StartPDFToHtml(input, "", outputFolder, HtmlOptionsFrom(options), callback);
        case ToolKind.Rtf:
          return CPDFConversion.StartPDFToRtf(input, "", outputFolder, RtfOptionsFrom(options), callback);
        case ToolKind.Txt:
          return CPDFConversion.StartPDFToTxt(input, "", outputFolder, TxtOptionsFrom(options), callback);
        case ToolKind.Markdown:
          // This SDK selects Markdown's name by checking .zip first. Supply a vacant
          // explicit path as well, so an existing .md is never overwritten.
          revealPath = UniqueMarkdownPath(revealPath);
          return CPDFConversion.StartPDFToMarkdown(input, "", revealPath, MarkdownOptionsFrom(options), callback);
        case ToolKind.Json:
          return CPDFConversion.StartPDFToJson(input, "", outputFolder, JsonOptionsFrom(options), callback);
        case ToolKind.Image:
          // Page images go into a folder named after the document.
          revealPath = UniqueDirectoryPath(Path.Combine(outputFolder, name));
          return CPDFConversion.StartPDFToImage(input, "", revealPath, ImageOptionsFrom(options), callback);
        case ToolKind.SearchablePdf:
          return CPDFConversion.StartPDFToSearchablePDF(input, "", outputFolder, SearchablePdfOptionsFrom(options), callback);
        case ToolKind.Ofd:
          return CPDFConversion.StartPDFToOfd(input, "", outputFolder, OfdOptionsFrom(options), callback);
        case ToolKind.WordToPdf:
          // Word to PDF takes the output file itself; it uses only the page ranges and bookmark mode of WordOptions.
          revealPath = UniqueFilePath(revealPath);
          return CPDFConversion.StartWordToPDF(input, revealPath, WordToPdfWordOptionsFrom(options), callback);
        default:
          throw new ArgumentOutOfRangeException(nameof(tool), tool.Kind, "Unknown conversion");
      }
    }

    /// <summary>
    /// Compatibility overload for callers without a pre-conversion snapshot.
    /// </summary>
    public static string FindOutput(string outputFolder, string input, DateTime startedAt, string guess)
    {
      try
      {
        DateTime since = startedAt.AddSeconds(-2);
        string name = Path.GetFileNameWithoutExtension(input);
        FileSystemInfo newest = new DirectoryInfo(outputFolder)
          .EnumerateFileSystemInfos()
          .Where(entry => MatchesOutputName(entry, name, Path.GetExtension(guess)) && entry.LastWriteTime >= since
            && !string.Equals(entry.FullName, input, StringComparison.OrdinalIgnoreCase))
          .OrderByDescending(entry => entry.LastWriteTime)
          .FirstOrDefault();
        return newest != null ? newest.FullName : guess;
      }
      catch (IOException)
      {
        return guess;
      }
      catch (UnauthorizedAccessException)
      {
        return guess;
      }
    }

    /// <summary>Inputs with overlapping SDK numbered names share a batch worker lock.</summary>
    public static string OutputNameKey(string input)
    {
      return Regex.Replace(Path.GetFileNameWithoutExtension(input), @"(?: ?\([1-9][0-9]*\))+$", "");
    }

    /// <summary>Record all existing entries before the SDK starts; existing results are not this run's output.</summary>
    public static ISet<string> SnapshotOutput(string outputFolder)
    {
      return new HashSet<string>(Directory.EnumerateFileSystemEntries(outputFolder).Select(Path.GetFullPath), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Find only newly created results with the SDK's exact stem and numbered suffix syntax.</summary>
    public static string FindOutput(ConversionTool tool, ConvertOptions options, string outputFolder,
                                    string input, string guess, ISet<string> existingOutputs)
    {
      if (existingOutputs == null)
        return null;
      try
      {
        string name = Path.GetFileNameWithoutExtension(input);
        string inputPath = Path.GetFullPath(input);
        string guessPath = string.IsNullOrEmpty(guess) ? "" : Path.GetFullPath(guess);
        var extensions = new List<string> { tool.OutputExtension };
        if (tool.Kind == ToolKind.Excel && options.CsvFormat)
          extensions.Add(".csv");
        if (tool.Kind == ToolKind.Html || tool.Kind == ToolKind.Excel && options.CsvFormat && options.AutoCreateFolder)
          extensions.Add("");
        if (tool.Kind == ToolKind.Markdown || tool.Kind == ToolKind.Json)
          extensions.Add(".zip");
        if (tool.Kind == ToolKind.Image)
          extensions.AddRange(new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff", ".jp2", ".webp", ".tga", ".gif" });
        var candidates = new DirectoryInfo(outputFolder).EnumerateFileSystemInfos()
          .Where(entry => !existingOutputs.Contains(entry.FullName)
            && !string.Equals(entry.FullName, inputPath, StringComparison.OrdinalIgnoreCase)
            && extensions.Any(extension => MatchesOutputName(entry, name, extension)))
          .ToList();
        FileSystemInfo exact = candidates.FirstOrDefault(entry =>
          string.Equals(entry.FullName, guessPath, StringComparison.OrdinalIgnoreCase));
        FileSystemInfo output = exact ?? candidates.OrderBy(entry => entry is DirectoryInfo ? 1 : 0)
          .ThenByDescending(entry => entry.LastWriteTimeUtc).FirstOrDefault();
        return output == null ? null : output.FullName;
      }
      catch (IOException) { return null; }
      catch (UnauthorizedAccessException) { return null; }
    }

    private static bool MatchesOutputName(FileSystemInfo entry, string name, string extension)
    {
      if (entry is DirectoryInfo && extension != "")
        return false;
      string suffix = entry is DirectoryInfo ? "" : extension;
      return Regex.IsMatch(entry.Name, "^" + Regex.Escape(name) + @"(?: ?\([1-9][0-9]*\))*"
        + Regex.Escape(suffix) + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string UniqueDirectoryPath(string path)
    {
      if (!Directory.Exists(path) && !File.Exists(path))
        return path;
      for (int index = 1; ; index++)
      {
        string candidate = path + "(" + index + ")";
        if (!Directory.Exists(candidate) && !File.Exists(candidate))
          return candidate;
      }
    }

    private static string UniqueMarkdownPath(string path)
    {
      string folder = Path.GetDirectoryName(path);
      string stem = Path.GetFileNameWithoutExtension(path);
      for (int index = 0; ; index++)
      {
        string suffix = index == 0 ? "" : "(" + index + ")";
        string md = Path.Combine(folder, stem + suffix + ".md");
        string zip = Path.Combine(folder, stem + suffix + ".zip");
        if (!File.Exists(md) && !Directory.Exists(md) && !File.Exists(zip) && !Directory.Exists(zip))
          return md;
      }
    }

    #region SDK options

    private static WordOptions WordToPdfWordOptionsFrom(ConvertOptions options)
    {
      return new WordOptions
      {
        WordPdfBookmarkMode = options.WordBookmarkMode,
        PageRanges = options.PageRanges ?? "",
      };
    }

    private static WordOptions WordOptionsFrom(ConvertOptions options)
    {
      WordOptions wordOptions = new WordOptions();
      wordOptions.ContainImage = options.ContainImages;
      wordOptions.ContainAnnotation = options.ContainAnnotations;
      wordOptions.FormulaToImage = options.FormulaToImage;
      wordOptions.EnableAiLayout = options.EnableAiLayout;
      wordOptions.EnableAiTableRecognition = options.EnableAiTableRecognition;
      wordOptions.EnableOCR = options.EnableOCR;
      wordOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      wordOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      wordOptions.EnableAutoPageSize = autoPage;
      wordOptions.CustomPageWidth = pageWidth;
      wordOptions.CustomPageHeight = pageHeight;
      wordOptions.LayoutMode = options.LayoutMode;
      wordOptions.ContainPageBackgroundImage = options.ContainPageBackgroundImage;
      wordOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      wordOptions.OcrOption = options.OcrOption;
      wordOptions.FontName = options.FontName;
      wordOptions.PageRanges = options.PageRanges;
      wordOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return wordOptions;
    }

    private static ExcelOptions ExcelOptionsFrom(ConvertOptions options)
    {
      ExcelOptions excelOptions = new ExcelOptions();
      excelOptions.ContainImage = options.ContainImages;
      excelOptions.ContainAnnotation = options.ContainAnnotations;
      excelOptions.FormulaToImage = options.FormulaToImage;
      excelOptions.AllContent = options.AllContent;
      excelOptions.CsvFormat = options.CsvFormat;
      excelOptions.EnableAiLayout = options.EnableAiLayout;
      excelOptions.EnableAiTableRecognition = options.EnableAiTableRecognition;
      excelOptions.EnableOCR = options.EnableOCR;
      excelOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      excelOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      excelOptions.EnableAutoPageSize = autoPage;
      excelOptions.CustomPageWidth = pageWidth;
      excelOptions.CustomPageHeight = pageHeight;
      excelOptions.WorksheetOption = options.WorksheetOption;
      excelOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      excelOptions.AutoCreateFolder = options.AutoCreateFolder;
      excelOptions.OcrOption = options.OcrOption;
      excelOptions.FontName = options.FontName;
      excelOptions.PageRanges = options.PageRanges;
      excelOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return excelOptions;
    }

    private static PptOptions PptOptionsFrom(ConvertOptions options)
    {
      PptOptions pptOptions = new PptOptions();
      pptOptions.ContainImage = options.ContainImages;
      pptOptions.ContainAnnotation = options.ContainAnnotations;
      pptOptions.FormulaToImage = options.FormulaToImage;
      pptOptions.EnableAiLayout = options.EnableAiLayout;
      pptOptions.EnableAiTableRecognition = options.EnableAiTableRecognition;
      pptOptions.EnableOCR = options.EnableOCR;
      pptOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      pptOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      pptOptions.EnableAutoPageSize = autoPage;
      pptOptions.CustomPageWidth = pageWidth;
      pptOptions.CustomPageHeight = pageHeight;
      pptOptions.ContainPageBackgroundImage = options.ContainPageBackgroundImage;
      pptOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      pptOptions.OcrOption = options.OcrOption;
      pptOptions.FontName = options.FontName;
      pptOptions.PageRanges = options.PageRanges;
      pptOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return pptOptions;
    }

    private static HtmlOptions HtmlOptionsFrom(ConvertOptions options)
    {
      HtmlOptions htmlOptions = new HtmlOptions();
      htmlOptions.ContainImage = options.ContainImages;
      htmlOptions.ContainAnnotation = options.ContainAnnotations;
      htmlOptions.FormulaToImage = options.FormulaToImage;
      htmlOptions.EnableAiLayout = options.EnableAiLayout;
      htmlOptions.EnableAiTableRecognition = options.EnableAiTableRecognition;
      htmlOptions.EnableOCR = options.EnableOCR;
      htmlOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      htmlOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      htmlOptions.EnableAutoPageSize = autoPage;
      htmlOptions.CustomPageWidth = pageWidth;
      htmlOptions.CustomPageHeight = pageHeight;
      htmlOptions.LayoutMode = options.LayoutMode;
      htmlOptions.HtmlOption = options.htmlOption;
      htmlOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      htmlOptions.OcrOption = options.OcrOption;
      htmlOptions.PageRanges = options.PageRanges;
      htmlOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return htmlOptions;
    }

    private static RtfOptions RtfOptionsFrom(ConvertOptions options)
    {
      RtfOptions rtfOptions = new RtfOptions();
      rtfOptions.ContainImage = options.ContainImages;
      rtfOptions.ContainAnnotation = options.ContainAnnotations;
      rtfOptions.FormulaToImage = options.FormulaToImage;
      rtfOptions.EnableAiLayout = options.EnableAiLayout;
      rtfOptions.EnableAiTableRecognition = options.EnableAiTableRecognition;
      rtfOptions.EnableOCR = options.EnableOCR;
      rtfOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      rtfOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      rtfOptions.EnableAutoPageSize = autoPage;
      rtfOptions.CustomPageWidth = pageWidth;
      rtfOptions.CustomPageHeight = pageHeight;
      rtfOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      rtfOptions.OcrOption = options.OcrOption;
      rtfOptions.PageRanges = options.PageRanges;
      rtfOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return rtfOptions;
    }

    private static TxtOptions TxtOptionsFrom(ConvertOptions options)
    {
      TxtOptions txtOptions = new TxtOptions();
      txtOptions.TableFormat = options.TxtTableFormat;
      txtOptions.EnableAiLayout = options.EnableAiLayout;
      txtOptions.EnableAiTableRecognition = options.EnableAiTableRecognition;
      txtOptions.EnableOCR = options.EnableOCR;
      txtOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      txtOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      txtOptions.EnableAutoPageSize = autoPage;
      txtOptions.CustomPageWidth = pageWidth;
      txtOptions.CustomPageHeight = pageHeight;
      txtOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      txtOptions.OcrOption = options.OcrOption;
      txtOptions.PageRanges = options.PageRanges;
      txtOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return txtOptions;
    }

    private static MarkdownOptions MarkdownOptionsFrom(ConvertOptions options)
    {
      MarkdownOptions markdownOptions = new MarkdownOptions();
      markdownOptions.ContainImage = options.ContainImages;
      markdownOptions.ContainAnnotation = options.ContainAnnotations;
      markdownOptions.EnableAiLayout = options.EnableAiLayout;
      markdownOptions.EnableAiTableRecognition = options.EnableAiTableRecognition;
      markdownOptions.EnableOCR = options.EnableOCR;
      markdownOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      markdownOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      markdownOptions.EnableAutoPageSize = autoPage;
      markdownOptions.CustomPageWidth = pageWidth;
      markdownOptions.CustomPageHeight = pageHeight;
      markdownOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      markdownOptions.OcrOption = options.OcrOption;
      markdownOptions.PageRanges = options.PageRanges;
      markdownOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return markdownOptions;
    }

    private static JsonOptions JsonOptionsFrom(ConvertOptions options)
    {
      JsonOptions jsonOptions = new JsonOptions();
      jsonOptions.ContainImage = options.ContainImages;
      jsonOptions.ContainAnnotation = options.ContainAnnotations;
      jsonOptions.ContainTable = options.ContainTables;
      jsonOptions.EnableAiLayout = options.EnableAiLayout;
      jsonOptions.EnableAiTableRecognition = options.EnableAiTableRecognition;
      jsonOptions.EnableOCR = options.EnableOCR;
      jsonOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      jsonOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      jsonOptions.EnableAutoPageSize = autoPage;
      jsonOptions.CustomPageWidth = pageWidth;
      jsonOptions.CustomPageHeight = pageHeight;
      jsonOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      jsonOptions.OcrOption = options.OcrOption;
      jsonOptions.PageRanges = options.PageRanges;
      jsonOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return jsonOptions;
    }

    private static ImageOptions ImageOptionsFrom(ConvertOptions options)
    {
      ImageOptions imageOptions = new ImageOptions();
      imageOptions.ImageScaling = options.ImageRatio;
      imageOptions.PathEnhance = options.ImagePathEnhance;
      imageOptions.ImageType = options.ImageFormat;
      imageOptions.ImageColorMode = options.ImageMode;
      imageOptions.PageRanges = options.PageRanges;
      return imageOptions;
    }

    private static SearchablePdfOptions SearchablePdfOptionsFrom(ConvertOptions options)
    {
      SearchablePdfOptions pdfOptions = new SearchablePdfOptions();
      pdfOptions.ContainImage = options.ContainImages;
      pdfOptions.ContainPageBackgroundImage = options.ContainPageBackgroundImage;
      pdfOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      pdfOptions.OcrOption = options.OcrOption;
      pdfOptions.TransparentText = options.TransparentText;
      pdfOptions.FormulaToImage = options.FormulaToImage;
      // A searchable PDF is an OCR text layer, so OCR is always on.
      pdfOptions.EnableOCR = true;
      pdfOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      pdfOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      pdfOptions.EnableAutoPageSize = autoPage;
      pdfOptions.CustomPageWidth = pageWidth;
      pdfOptions.CustomPageHeight = pageHeight;
      pdfOptions.PageRanges = options.PageRanges;
      pdfOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return pdfOptions;
    }

    private static OfdOptions OfdOptionsFrom(ConvertOptions options)
    {
      OfdOptions ofdOptions = new OfdOptions();
      ofdOptions.TransparentText = options.TransparentText;
      ofdOptions.OutputDocumentPerPage = options.OutputDocumentPerPage;
      ofdOptions.OcrOption = options.OcrOption;
      // Like a searchable PDF, a searchable OFD always runs OCR.
      ofdOptions.EnableOCR = true;
      ofdOptions.EnableDocumentOrientationClassification = options.EnableDocumentOrientationClassification;
      ofdOptions.EnableDocumentDewarp = options.EnableDocumentDewarp;
      ImagePageSizes.Apply(options.ImagePageSize, out bool autoPage, out float pageWidth, out float pageHeight);
      ofdOptions.EnableAutoPageSize = autoPage;
      ofdOptions.CustomPageWidth = pageWidth;
      ofdOptions.CustomPageHeight = pageHeight;
      ofdOptions.PageRanges = options.PageRanges;
      ofdOptions.Languages = new List<OCRLanguage> { options.OCRLanguage };
      return ofdOptions;
    }

    #endregion

    #region Errors

    /// <summary>A few words for the file row, e.g. "Password required".</summary>
    public static string ShortReason(ErrorCode error)
    {
      switch (error)
      {
        case ErrorCode.e_ErrPDFPassword: return "Password required";
        case ErrorCode.e_ErrFile: return "Can't open the file";
        case ErrorCode.e_ErrPDFFormat: return "Not a valid PDF";
        case ErrorCode.e_ErrPDFSecurity: return "Unsupported encryption";
        case ErrorCode.e_ErrPDFPage: return "Damaged page";
        case ErrorCode.e_ErrOutOfMemory: return "Out of memory";
        case ErrorCode.e_ErrIO: return "Can't write the output";
        case ErrorCode.e_ErrCompress: return "Can't compress the output";
        case ErrorCode.e_ErrNoTable: return "No tables found";
        case ErrorCode.e_ErrOCRFailure: return "OCR failed";
        case ErrorCode.e_ErrConverting: return "Another conversion is running";
        case ErrorCode.e_ErrUnsupportedFeature: return "Unsupported content";
        case ErrorCode.e_ErrLibraryNotLoad: return "SDK not loaded";
        case ErrorCode.e_ErrLicenseInvalid:
        case ErrorCode.e_ErrLicenseExpire:
        case ErrorCode.e_ErrLicenseUnsupportedPlatform:
        case ErrorCode.e_ErrLicenseUnsupportedID:
        case ErrorCode.e_ErrLicenseUnsupportedDevice:
        case ErrorCode.e_ErrLicensePermissionDeny:
        case ErrorCode.e_ErrLicenseUninitialized:
        case ErrorCode.e_ErrLicenseIllegalAccess:
        case ErrorCode.e_ErrLicenseFileReadFailed:
        case ErrorCode.e_ErrLicenseOCRPermissionDeny:
        case ErrorCode.e_ErrLicenseConcurrencyExceeded:
        case ErrorCode.e_ErrLicensePageLimitExceeded:
        case ErrorCode.e_ErrLicenseQuotaCorrupted:
        case ErrorCode.e_ErrLicenseUnsupportedAPI:
        case ErrorCode.e_ErrLicenseMismatch:
          return "License problem";
        case ErrorCode.e_ErrModelInvalidFormat:
        case ErrorCode.e_ErrModelFunctionUnsupported:
        case ErrorCode.e_ErrModelFormatUnsupported:
        case ErrorCode.e_ErrModelSDKMismatch:
          return "AI model problem";
        default:
          return "Failed (" + (int)error + ")";
      }
    }

    /// <summary>The full explanation with the error code, for the tooltip and the activity log.</summary>
    public static string Describe(ErrorCode error)
    {
      string errorName = Enum.IsDefined(typeof(ErrorCode), error) ? error.ToString() : "Unrecognized";
      return GetErrorMessage(error) + " Error code " + (int)error + " (" + errorName + ").";
    }

    private static string GetErrorMessage(ErrorCode error)
    {
      switch (error)
      {
        case ErrorCode.e_ErrSuccess:
          return "No error occurred.";

        case ErrorCode.e_ErrCancel:
          return "The conversion was canceled.";

        case ErrorCode.e_ErrFile:
          return "The input file could not be found or opened.";

        case ErrorCode.e_ErrPDFPassword:
          return "The PDF requires a password, or the supplied password is incorrect.";

        case ErrorCode.e_ErrPDFPage:
          return "A PDF page could not be loaded because it is missing or contains invalid content.";

        case ErrorCode.e_ErrPDFFormat:
          return "The input is not a valid PDF file, or the file is corrupted.";

        case ErrorCode.e_ErrPDFSecurity:
          return "The PDF uses an unsupported security scheme.";

        case ErrorCode.e_ErrOutOfMemory:
          return "The SDK could not allocate enough memory to complete the operation.";

        case ErrorCode.e_ErrIO:
          return "A system input/output error occurred while reading or writing data.";

        case ErrorCode.e_ErrCompress:
          return "The SDK failed to compress the output folder.";

        case ErrorCode.e_ErrLicenseInvalid:
          return "The license is invalid.";

        case ErrorCode.e_ErrLicenseExpire:
          return "The license has expired.";

        case ErrorCode.e_ErrLicenseUnsupportedPlatform:
          return "The license does not support the current platform.";

        case ErrorCode.e_ErrLicenseUnsupportedID:
          return "The license does not support this application ID.";

        case ErrorCode.e_ErrLicenseUnsupportedDevice:
          return "The license does not support this device ID.";

        case ErrorCode.e_ErrLicensePermissionDeny:
          return "The license does not grant permission for this function.";

        case ErrorCode.e_ErrLicenseUninitialized:
          return "The license has not been initialized.";

        case ErrorCode.e_ErrLicenseIllegalAccess:
          return "The SDK rejected an illegal API access.";

        case ErrorCode.e_ErrLicenseFileReadFailed:
          return "The license file could not be read.";

        case ErrorCode.e_ErrLicenseOCRPermissionDeny:
          return "The license does not grant permission to use OCR.";

        case ErrorCode.e_ErrLicenseConcurrencyExceeded:
          return "The number of concurrent conversions exceeds the license limit.";

        case ErrorCode.e_ErrLicensePageLimitExceeded:
          return "The number of pages being converted exceeds the license limit.";

        case ErrorCode.e_ErrLicenseQuotaCorrupted:
          return "The stored license quota data is corrupted or has been tampered with.";

        case ErrorCode.e_ErrNoTable:
          return "No tables were found in the source file.";

        case ErrorCode.e_ErrOCRFailure:
          return "The SDK failed to perform OCR recognition.";

        case ErrorCode.e_ErrConverting:
          return "Another conversion task is already running.";

        case ErrorCode.e_ErrInvalidArg:
          return "An invalid argument was provided to the SDK.";

        case ErrorCode.e_ErrInvalidHandle:
          return "An SDK handle is invalid, uninitialized, or has already been released.";

        case ErrorCode.e_ErrModelInvalidFormat:
          return "The AI model file has an invalid format or is corrupted.";

        case ErrorCode.e_ErrModelFunctionUnsupported:
          return "The AI model does not support the requested function.";

        case ErrorCode.e_ErrModelFormatUnsupported:
          return "The AI model format is not supported by this SDK.";

        case ErrorCode.e_ErrModelSDKMismatch:
          return "The AI model is incompatible with the current SDK version.";

        case ErrorCode.e_ErrImageDataEmpty:
          return "The image data is empty or was not provided.";

        case ErrorCode.e_ErrImageWHError:
          return "The image width or height is invalid.";

        case ErrorCode.e_ErrImageUnsupportedFormat:
          return "The image pixel format is not supported by the SDK.";

        case ErrorCode.e_ErrImageInvalid:
          return "The image data is invalid or corrupted.";

        case ErrorCode.e_ErrExpire:
          return "A required resource, license, or token has expired.";

        case ErrorCode.e_ErrMissingArg:
          return "A required argument was not provided to the SDK.";

        case ErrorCode.e_ErrLicenseUnsupportedAPI:
          return "The current license does not permit this API.";

        case ErrorCode.e_ErrLicenseMismatch:
          return "The license does not match the current device, module, or SDK version.";

        case ErrorCode.e_ErrInvalidTable:
          return "The table data is invalid or missing.";

        case ErrorCode.e_ErrUnsupportedFeature:
          return "The source document uses a feature the SDK cannot lay out yet, so no output was produced.";

        case ErrorCode.e_ErrUnknown:
          return "An unknown SDK error occurred.";

        case ErrorCode.e_ErrLibraryNotLoad:
          return "The native SDK library has not been loaded.";

        default:
          return "The SDK returned an unrecognized error code.";
      }
    }

    #endregion

    // Word to PDF writes to the exact path it is given, so an existing file gets a numbered name.
    private static string UniqueFilePath(string path)
    {
      if (!File.Exists(path) && !Directory.Exists(path))
      {
        return path;
      }
      string folder = Path.GetDirectoryName(path);
      string name = Path.GetFileNameWithoutExtension(path);
      string extension = Path.GetExtension(path);
      for (int i = 1; ; i++)
      {
        string candidate = Path.Combine(folder, name + " (" + i + ")" + extension);
        if (!File.Exists(candidate) && !Directory.Exists(candidate))
        {
          return candidate;
        }
      }
    }
  }
}
