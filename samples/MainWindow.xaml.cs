using PDFSolid_Conversion.Common;
using PDFSolid_Conversion.Conversion;
using PDFSolid_Conversion.DocumentAI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using MessageBox = System.Windows.MessageBox;

namespace PDFSolid_Conversion_Demo
{
  public class ConvertOptions
  {
    public OCRLanguage OCRLanguage = OCRLanguage.e_ENGLISH;
    public bool ContainAnnotations = true;
    public bool CsvFormat = false;
    public bool AllContent = false;
    public bool OneTablePerSheet = true;
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
  }

  /// <summary>
  /// Copyright © 2014-2026 PDF Technologies, Inc. All Rights Reserved.
  ///
  /// THIS SOURCE CODE AND ANY ACCOMPANYING DOCUMENTATION ARE PROTECTED BY INTERNATIONAL COPYRIGHT LAW
/// AND MAY NOT BE RESOLD OR REDISTRIBUTED.USAGE IS BOUND TO THE PDFSolid LICENSE AGREEMENT.
  /// UNAUTHORIZED REPRODUCTION OR DISTRIBUTION IS SUBJECT TO CIVIL AND CRIMINAL PENALTIES.
  /// This notice may not be removed from this file.
  ///
  /// https://www.pdfsolid.com
  /// </summary>
  public partial class MainWindow : Window
  {
    private OnProgress getPorgress = null;
    private OnCancel getCancel = null;
    private ErrorCode err;
    private bool cancel = false;
    public ConvertOptions Options;
    private List<string> selectedFiles;
    public MainWindow()
    {
      InitializeComponent();
      getPorgress = GetProgress;
      getCancel = GetCancel;
      Options = new ConvertOptions();
    }

    #region Method
    private void GetProgress(int pageIndex, int total)
    {
      Dispatcher.Invoke(() =>
      {
        Progress.Text = pageIndex + "/" + total;
        if (pageIndex == total)
        {
          Progress.Text += " Conversion to complete.";
        }
      });
    }

    private ConvertCallback BuildCallback()
    {
      ConvertCallback callback = new ConvertCallback();
      callback.progress = Marshal.GetFunctionPointerForDelegate(getPorgress);
      callback.cancel = Marshal.GetFunctionPointerForDelegate(getCancel);
      callback.ocr = IntPtr.Zero;
      callback.get_ocr_result = IntPtr.Zero;
      return callback;
    }


    private bool GetCancel()
    {
      return cancel;
    }

    private bool ShouldEnableOCR()
    {
      return Options.EnableOCR;
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

    private void ShowErrorMessage(ErrorCode error, string operation = "Conversion")
    {
      string errorName = Enum.IsDefined(typeof(ErrorCode), error) ? error.ToString() : "Unrecognized";
      string message = operation + " failed.\n\nReason: " + GetErrorMessage(error)
        + "\n\nError code: " + (int)error + " (" + errorName + ")";

      MessageBox.Show(message, operation + " Failed", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async Task WordConvert()
    {
      try
      {
        WordOptions wordOptions = new WordOptions();
        wordOptions.ContainImage = Options.ContainImages;
        wordOptions.ContainAnnotation = Options.ContainAnnotations;
        wordOptions.FormulaToImage = Options.FormulaToImage;
        wordOptions.EnableAiLayout = Options.EnableAiLayout;
        wordOptions.EnableAiTableRecognition = Options.EnableAiTableRecognition;
        wordOptions.EnableOCR = ShouldEnableOCR();
        wordOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        wordOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        wordOptions.LayoutMode = Options.LayoutMode;
        wordOptions.ContainPageBackgroundImage = Options.ContainPageBackgroundImage;
        wordOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        wordOptions.OcrOption = Options.OcrOption;
        wordOptions.FontName = Options.FontName;
        wordOptions.PageRanges = Options.PageRanges;
        wordOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToWord(input, "", outputFolder, wordOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task ExcelConvert()
    {
      try
      {
        ExcelOptions excelOptions = new ExcelOptions();
        excelOptions.ContainImage = Options.ContainImages;
        excelOptions.ContainAnnotation = Options.ContainAnnotations;
        excelOptions.FormulaToImage = Options.FormulaToImage;
        excelOptions.AllContent = Options.AllContent;
        excelOptions.CsvFormat = Options.CsvFormat;
        excelOptions.EnableAiLayout = Options.EnableAiLayout;
        excelOptions.EnableAiTableRecognition = Options.EnableAiTableRecognition;
        excelOptions.EnableOCR = ShouldEnableOCR();
        excelOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        excelOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        excelOptions.WorksheetOption = Options.WorksheetOption;
        excelOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        excelOptions.AutoCreateFolder = Options.AutoCreateFolder;
        excelOptions.OcrOption = Options.OcrOption;
        excelOptions.FontName = Options.FontName;
        excelOptions.PageRanges = Options.PageRanges;
        excelOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToExcel(input, "", outputFolder, excelOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task PptConvert()
    {
      try
      {
        PptOptions pptOptions = new PptOptions();
        pptOptions.ContainImage = Options.ContainImages;
        pptOptions.ContainAnnotation = Options.ContainAnnotations;
        pptOptions.FormulaToImage = Options.FormulaToImage;
        pptOptions.EnableAiLayout = Options.EnableAiLayout;
        pptOptions.EnableAiTableRecognition = Options.EnableAiTableRecognition;
        pptOptions.EnableOCR = ShouldEnableOCR();
        pptOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        pptOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        pptOptions.ContainPageBackgroundImage = Options.ContainPageBackgroundImage;
        pptOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        pptOptions.OcrOption = Options.OcrOption;
        pptOptions.FontName = Options.FontName;
        pptOptions.PageRanges = Options.PageRanges;
        pptOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToPpt(input, "", outputFolder, pptOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task HtmlConvert()
    {
      try
      {
        HtmlOptions htmlOptions = new HtmlOptions();
        htmlOptions.ContainImage = Options.ContainImages;
        htmlOptions.ContainAnnotation = Options.ContainAnnotations;
        htmlOptions.FormulaToImage = Options.FormulaToImage;
        htmlOptions.EnableAiLayout = Options.EnableAiLayout;
        htmlOptions.EnableAiTableRecognition = Options.EnableAiTableRecognition;
        htmlOptions.EnableOCR = ShouldEnableOCR();
        htmlOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        htmlOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        htmlOptions.LayoutMode = Options.LayoutMode;
        htmlOptions.HtmlOption = Options.htmlOption;
        htmlOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        htmlOptions.OcrOption = Options.OcrOption;
        htmlOptions.PageRanges = Options.PageRanges;
        htmlOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToHtml(input, "", outputFolder, htmlOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task RtfConvert()
    {
      try
      {
        RtfOptions rtfOptions = new RtfOptions();
        rtfOptions.ContainImage = Options.ContainImages;
        rtfOptions.ContainAnnotation = Options.ContainAnnotations;
        rtfOptions.FormulaToImage = Options.FormulaToImage;
        rtfOptions.EnableAiLayout = Options.EnableAiLayout;
        rtfOptions.EnableAiTableRecognition = Options.EnableAiTableRecognition;
        rtfOptions.EnableOCR = ShouldEnableOCR();
        rtfOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        rtfOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        rtfOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        rtfOptions.OcrOption = Options.OcrOption;
        rtfOptions.PageRanges = Options.PageRanges;
        rtfOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToRtf(input, "", outputFolder, rtfOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task PdfConvert()
    {
      try
      {
        SearchablePdfOptions pdfOptions = new SearchablePdfOptions();
        pdfOptions.ContainImage = Options.ContainImages;
        pdfOptions.ContainPageBackgroundImage = Options.ContainPageBackgroundImage;
        pdfOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        pdfOptions.OcrOption = Options.OcrOption;
        pdfOptions.TransparentText = Options.TransparentText;
        pdfOptions.FormulaToImage = Options.FormulaToImage;
        pdfOptions.EnableOCR = true;
        pdfOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        pdfOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        pdfOptions.PageRanges = Options.PageRanges;
        pdfOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToSearchablePDF(input, "", outputFolder, pdfOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task TxtConvert()
    {
      try
      {
        TxtOptions txtOptions = new TxtOptions();
        txtOptions.TableFormat = Options.TxtTableFormat;
        txtOptions.EnableAiLayout = Options.EnableAiLayout;
        txtOptions.EnableAiTableRecognition = Options.EnableAiTableRecognition;
        txtOptions.EnableOCR = ShouldEnableOCR();
        txtOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        txtOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        txtOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        txtOptions.OcrOption = Options.OcrOption;
        txtOptions.PageRanges = Options.PageRanges;
        txtOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToTxt(input, "", outputFolder, txtOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task ImageConvert()
    {
      try
      {
        ImageOptions imageOptions = new ImageOptions();
        imageOptions.ImageScaling = Options.ImageRatio;
        imageOptions.PathEnhance = Options.ImagePathEnhance;
        imageOptions.ImageType = Options.ImageFormat;
        imageOptions.ImageColorMode = Options.ImageMode;
        imageOptions.PageRanges = Options.PageRanges;

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToImage(input, "", Path.Combine(outputFolder, outputFileName), imageOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task JsonConvert()
    {
      try
      {
        JsonOptions jsonOptions = new JsonOptions();
        jsonOptions.ContainImage = Options.ContainImages;
        jsonOptions.ContainTable = Options.ContainTables;
        jsonOptions.EnableAiLayout = Options.EnableAiLayout;
        jsonOptions.EnableAiTableRecognition = Options.EnableAiTableRecognition;
        jsonOptions.EnableOCR = ShouldEnableOCR();
        jsonOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        jsonOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        jsonOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        jsonOptions.OcrOption = Options.OcrOption;
        jsonOptions.PageRanges = Options.PageRanges;
        jsonOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToJson(input, "", outputFolder, jsonOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task MarkdownConvert()
    {
      try
      {
        MarkdownOptions markdownOptions = new MarkdownOptions();
        markdownOptions.ContainImage = Options.ContainImages;
        markdownOptions.ContainAnnotation = Options.ContainAnnotations;
        markdownOptions.EnableAiLayout = Options.EnableAiLayout;
        markdownOptions.EnableAiTableRecognition = Options.EnableAiTableRecognition;
        markdownOptions.EnableOCR = ShouldEnableOCR();
        markdownOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        markdownOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        markdownOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        markdownOptions.OcrOption = Options.OcrOption;
        markdownOptions.PageRanges = Options.PageRanges;
        markdownOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToMarkdown(input, "", outputFolder, markdownOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }

    private async Task OfdConvert()
    {
      try
      {
        OfdOptions ofdOptions = new OfdOptions();
        ofdOptions.TransparentText = Options.TransparentText;
        ofdOptions.OutputDocumentPerPage = Options.OutputDocumentPerPage;
        ofdOptions.OcrOption = Options.OcrOption;
        ofdOptions.EnableOCR = true;
        ofdOptions.EnableDocumentOrientationClassification = Options.EnableDocumentOrientationClassification;
        ofdOptions.EnableDocumentDewarp = Options.EnableDocumentDewarp;
        ofdOptions.PageRanges = Options.PageRanges;
        ofdOptions.Languages = new List<OCRLanguage> { Options.OCRLanguage };

        string outputFolder = OutputPath.Text;
        string outputFileName = Path.GetFileNameWithoutExtension(InputPath.Text);
        string input = InputPath.Text;
        ConvertCallback callback = BuildCallback();
        err = await Task.Run(() => CPDFConversion.StartPDFToOfd(input, "", outputFolder, ofdOptions, callback));
        if (err != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(err);
        }
      }
      catch (Exception ex)
      {
        return;
      }
    }
    #endregion

    #region Event
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      string exePath = Path.GetDirectoryName(typeof(MainWindow).Assembly.Location);
      string resPath = exePath + "\\";

      if (LibraryManager.InitLibrary(Path.Combine(resPath, "x64")))
      {
        LibraryManager.Initialize(Path.Combine(resPath, "resource"));
        ErrorCode result = LibraryManager.LicenseVerify(Path.Combine(resPath, "license.xml"));
        if (result != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(result, "SDK license verification");
          return;
        }

        ErrorCode modelResult = LibraryManager.SetDocumentAIModel(Path.Combine(resPath, "resource", "models", "documentai.model"));
        if (modelResult != ErrorCode.e_ErrSuccess)
        {
          ShowErrorMessage(modelResult, "Document AI model loading");
        }
      }
      else
      {
        ShowErrorMessage(ErrorCode.e_ErrLibraryNotLoad, "SDK initialization");
      }
    }

    private void Input_Click(object sender, RoutedEventArgs e)
    {
      var dlg = new Microsoft.Win32.OpenFileDialog();
      dlg.Filter = "PDF Image Files (*.pdf;*.bmp;*.jpg;*.jpeg;*.png;*.tiff;*.webp;*.jp2;*.tif)|*.pdf;*.bmp;*.jpg;*.jpeg;*.png;*.tiff;*.webp;*.jp2;*.tif";
      dlg.Multiselect = true;

      if (dlg.ShowDialog() == true)
      {
        selectedFiles = new List<string>(dlg.FileNames);

        if (selectedFiles.Count > 0)
        {
          InputPath.Text = selectedFiles[0];
        }
        Progress.Text = "";
      }
    }

    private void Output_Click(object sender, RoutedEventArgs e)
    {
      FolderSelectDialog dlg = new FolderSelectDialog();

      if (dlg.ShowDialog())
      {
        OutputPath.Text = dlg.FileName;
      }
    }

    private async void Convert_Click(object sender, RoutedEventArgs e)
    {
      cancel = false;

      if (selectedFiles == null || selectedFiles.Count == 0)
      {
        MessageBox.Show("Invalid input path!");
        return;
      }

      if (string.IsNullOrEmpty(OutputPath.Text))
      {
        MessageBox.Show("Invalid output path!");
        return;
      }

      Cancel.IsEnabled = true;
      Convert.IsEnabled = false;
      ConvertType.IsEnabled = false;
      ConverterOptions.IsEnabled = false;

      foreach (string filePath in selectedFiles)
      {
        InputPath.Text = filePath;
        switch ((ConvertType.SelectedItem as ComboBoxItem).Name)
        {
          case "Word":
            await WordConvert();
            break;

          case "Excel":
            await ExcelConvert();
            break;

          case "Ppt":
            await PptConvert();
            break;

          case "Html":
            await HtmlConvert();
            break;

          case "Rtf":
            await RtfConvert();
            break;

          case "SearchablePDF":
            await PdfConvert();
            break;

          case "Txt":
            await TxtConvert();
            break;

          case "Json":
            await JsonConvert();
            break;

          case "Image":
            await ImageConvert();
            break;

          case "Markdown":
            await MarkdownConvert();
            break;

          case "Ofd":
            await OfdConvert();
            break;

          default:
            break;
        }
      }

      if (err == ErrorCode.e_ErrSuccess)
        Process.Start(OutputPath.Text);

      Cancel.IsEnabled = false;
      Convert.IsEnabled = true;
      ConvertType.IsEnabled = true;
      ConverterOptions.IsEnabled = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
      cancel = true;
    }

    private void ConverterOptions_Click(object sender, RoutedEventArgs e)
    {
      ConverterOptionsWindow optionsWindow = new ConverterOptionsWindow(this, (ConvertType.SelectedItem as ComboBoxItem).Name);
      optionsWindow.ShowDialog();
    }
    #endregion
  }
}
