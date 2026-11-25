# PDFSolid Conversion SDK for Windows 1.3.0 Upgrade Warnings

This document contains a list of considerations that developers should be aware of when upgrading from previous versions to PDFSolid Conversion SDK for Windows 1.3.0. Changes in the code may be necessary to ensure your project compiles and runs properly with the new version.

**Upgrading to version 1.3.0 from version 1.2.0**

If you are upgrading from version 1.2.0 to version 1.3.0, please note the changes between version 1.2.0 and version 1.3.0.

**Important: update the SDK files together and rebuild your project.** The native structure behind the conversion options has gained members, so the managed assembly `pdfsolidconversionsdk_dotnet.dll` and the native library `pdfsolidconversionsdk.dll` must come from the same version. Replace the existing files with all the files in the ***"lib"*** folder of the 1.3.0 SDK package; do not replace only some of them. This upgrade does not remove or rename any existing public interface, so 1.2.0 code keeps compiling.

**Important: the default table recognition model profile has changed.** Version 1.2.0 uses the high-accuracy model by default, while version 1.3.0 uses the balanced model by default. Without any code change, the table recognition results and resource usage after upgrading differ from 1.2.0. To keep the behavior of 1.2.0, explicitly select the high-accuracy profile and load the table extension package `table_legacy.model` (see item 1 under "In the conversion option classes" below).

**Important: upgrade the model files as well.** The `documentai.model` of 1.2.0 and earlier does not contain the resources required by the balanced profile, while the model file name is unchanged in 1.3.0, so **continuing to use the old file causes model loading to fail. Overwrite the old file in your deployment directory with the `documentai.model` shipped in the 1.3.0 SDK package.** When you need extended features such as high-accuracy tables, high-accuracy OCR, or orientation correction, download the corresponding extension model packages separately. The composition and purpose of the model packages are described in "Model Package Reference" in the "Set DocumentAI Model" chapter of the developer guide.

In the LibraryManager class:

1. The `SetDocumentAIModel` interface now supports incremental loading of split model packages. The base package `documentai.model` provides layout analysis, balanced table recognition, and basic OCR; table, OCR, and orientation correction capabilities are provided by extension packages. Each call loads only the capabilities contained in that file and merges them with the capabilities already loaded; it does not replace them. Sample code:

   ```c#
   LibraryManager.SetDocumentAIModel("path/documentai.model", -1);
   LibraryManager.SetDocumentAIModel("path/table_legacy.model", -1);
   ```

   If a package fails to load, the capabilities already loaded are kept, and a specific error code is returned.

2. The timing of release and configuration has changed. If conversion tasks are still running when `Release` is called, the release is postponed until the last task returns, and new conversion tasks are rejected in the meantime. While a task is running or a release has not completed, setting the DocumentAI model again is rejected and returns `ErrorCode.e_ErrConverting`. Unload the SDK only after all conversion interfaces have returned.

3. The new `LibraryManager.GetLicenseType` interface returns the type of the currently verified license: `LicenseType.e_Unknown`, `LicenseType.e_Trial`, or `LicenseType.e_Formal`. It does not verify the license again and does not consume page quota.

In the CPDFConversion class:

1. The new Word to PDF interface `StartWordToPDF` is added. You can choose to generate PDF bookmarks, specify the output page range, and get diagnostic information through `out string diagnostics`. Its options reuse `WordOptions`: only `WordPdfBookmarkMode` (new) and `PageRanges` take effect for Word to PDF.

   ```c#
   // Basic usage.
   ErrorCode error = CPDFConversion.StartWordToPDF("input.docx", "output.pdf");

   // Set bookmarks and page range, and get diagnostics.
   WordOptions options = new WordOptions();
   options.WordPdfBookmarkMode = WordPdfBookmarkMode.e_Headings;
   options.PageRanges = "1-3,5,7-9";
   string diagnostics;
   error = CPDFConversion.StartWordToPDF("input.docx", "output.pdf", options, out diagnostics);
   ```

   When the document contains features that may affect fidelity, `ErrorCode.e_ErrUnsupportedFeature` (95) is returned. The PDF is usually still generated in this case; use `output=produced` or `output=not_produced` on the first line of the diagnostic text to tell. If the diagnostics do not fit, `ErrorCode.e_ErrOutOfMemory` is returned. See the "Convert Word to PDF" chapter of the developer guide for details.

2. The behavior for image input has changed:
   - When an image is converted to TXT or to CSV-format Excel without OCR enabled, `ErrorCode.e_ErrUnsupportedFeature` (95) is now returned. Please enable OCR.
   - When an image is converted to a document format that can carry images, disabling OCR (the default) outputs the complete source image and does not generate recognized text.
   - `StartPDFToImage` accepts both PDF files and supported image files as input. Converting between images does not run OCR or layout analysis. For a multi-page TIFF, each selected frame produces one image, and `PageRanges` selects the frames.

In the conversion option classes:

1. `WordOptions`, `ExcelOptions`, `PptOptions`, `HtmlOptions`, `RtfOptions`, `TxtOptions`, `JsonOptions`, and `MarkdownOptions` gain the `TableRecognitionAccuracy` option to select the table recognition accuracy. **The default is `TableRecognitionAccuracy.e_Balanced`, which differs from 1.2.0, where the high-accuracy profile was the default.** Selecting `TableRecognitionAccuracy.e_HighAccuracy` requires `table_legacy.model` to be loaded. If it is not loaded, the conversion still succeeds, but tables are recognized with the rule-based method instead, so the result differs from the high-accuracy result. Image, searchable PDF, and OFD conversions do not run table recognition, so their option classes do not have this option; Word to PDF ignores it. To keep the behavior of 1.2.0, update the original code:

   ```c#
   WordOptions options = new WordOptions();
   CPDFConversion.StartPDFToWord("input.pdf", "", "output.docx", options);
   ```

   to:

   ```c#
   LibraryManager.SetDocumentAIModel("path/documentai.model", -1);
   LibraryManager.SetDocumentAIModel("path/table_legacy.model", -1);

   WordOptions options = new WordOptions();
   options.TableRecognitionAccuracy = TableRecognitionAccuracy.e_HighAccuracy;
   CPDFConversion.StartPDFToWord("input.pdf", "", "output.docx", options);
   ```

2. The new `EnableAutoPageSize` option (default `false`): when enabled, the page size of the output for image conversion is matched to the nearest standard paper according to the image resolution. It only takes effect for image input.

3. The new `CustomPageWidth` and `CustomPageHeight` options (default `0`), in points (1 point = 1/72 inch). When both are set, they specify the output page size directly and take precedence over `EnableAutoPageSize`. They only take effect for image input.

   ```c#
   WordOptions options = new WordOptions();
   options.CustomPageWidth = 595.28f;    // A4
   options.CustomPageHeight = 841.89f;
   ```

   The `EnableAutoPageSize`, `CustomPageWidth`, and `CustomPageHeight` options are available in `WordOptions`, `ExcelOptions`, `PptOptions`, `HtmlOptions`, `RtfOptions`, `TxtOptions`, `JsonOptions`, `MarkdownOptions`, `SearchablePdfOptions`, and `OfdOptions`.
