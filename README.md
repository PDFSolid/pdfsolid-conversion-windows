# PDFSolid Conversion SDK for Windows (.NET)

High-performance .NET SDK for converting PDF to Word, Excel, PowerPoint, HTML, Image, TXT, RTF, CSV, JSON, Markdown, Searchable PDF, and OFD with AI-powered OCR, layout analysis, and table recognition. SDK v1.3.0 also converts Word documents (`.docx`) to PDF.

## Features

- **PDF to Word** (.docx) — Flow and Box layout modes
- **Word to PDF** (.docx → .pdf) — optional page ranges and PDF bookmarks
- **PDF to Excel** (.xlsx) — per-table, per-page, or per-document worksheet options
- **PDF to PowerPoint** (.pptx)
- **PDF to HTML** (.html) — single/multi-page with optional bookmark navigation
- **PDF to CSV** (.csv)
- **PDF to Image** (.png, .jpg, .jpeg, .jpeg2000, .bmp, .tiff, .tga, .gif, .webp) — color/grayscale/binary, configurable scaling
- **Image to Image** — supported image inputs, including individual frames of multi-page TIFF files
- **PDF to Plain Text** (.txt) — optional table format preservation
- **PDF to RTF** (.rtf)
- **PDF to Searchable PDF** (.pdf) — OCR with transparent text layer
- **PDF to OFD** (.ofd) — OCR, page background preservation, transparent text layer
- **PDF to JSON** (.json) — structured data with table extraction
- **PDF to Markdown** (.md)

### AI-Powered Document Tools

- **OCR** — Optical Character Recognition for scanned documents and images
- **Layout Analysis** — AI-based document structure parsing
- **Table Recognition** — AI-based table structure reconstruction
- **Custom AI Models** — plug in your own OCR, layout, or table engine via callbacks (SDK v1.1.0+)

## Requirements

| Platform | System Requirements | Development Environment |
| -------- | ------------------- | ----------------------- |
| Windows | Windows 7, 8, 10, 11 (32/64-bit) | Visual Studio 2017+, .NET Framework 4.6.1+ |

## Quick Start

### 1. Get a License

Contact [sales@pdfsolid.com](mailto:sales@pdfsolid.com) for a 30-day free trial or commercial license.

### 2. Load the Native SDK, Initialize and Apply the License

Use the 1.3.0 managed assembly with the matching native DLLs for your process (x86 or x64). See [Getting Started](doc/html/getting-started.html) for deployment.

```csharp
using PDFSolid_Conversion.Common;
using PDFSolid_Conversion.Conversion;

if (!LibraryManager.InitLibrary(@"path\to\x64") ||
    !LibraryManager.Initialize(@"path\to\resource"))
{
    return;
}
ErrorCode result = LibraryManager.LicenseVerify("LICENSE_KEY");
if (result != ErrorCode.e_ErrSuccess)
{
    LibraryManager.Release();
    return;
}
```

### 3. Convert

```csharp
WordOptions wordOptions = new WordOptions();
ErrorCode error = CPDFConversion.StartPDFToWord("input.pdf", "", "output.docx", wordOptions);
```

### Release Resources

```csharp
LibraryManager.ReleaseDocumentAIModel();
LibraryManager.Release();
```

## Conversion Examples

### Word (DOCX) to PDF

Use the 1.3.0 managed assembly with the matching native SDK and dependencies for your process architecture (x86 or x64). After applying the license and initializing the SDK, check that the loaded native library provides Word to PDF:

```csharp
using PDFSolid_Conversion.Common;
using PDFSolid_Conversion.Conversion;

if (!CPDFConversion.IsWordToPDFSupported())
{
    throw new System.NotSupportedException("The loaded native SDK does not expose Word to PDF.");
}

ErrorCode result = CPDFConversion.StartWordToPDF("input.docx", "output.pdf");
```

The default converts all pages without PDF bookmarks. With the `WordOptions` overload, set `WordOptions.PageRanges` to select pages (for example, `1-2`) and `WordOptions.WordPdfBookmarkMode` to generate bookmarks from headings or Word bookmarks. Check the returned status before using the PDF. `ErrorCode.e_ErrUnsupportedFeature` means the input contains features that may affect fidelity; a PDF may still have been written.

For the overload with `out string diagnostics`, see the [.NET API reference](doc/html/api/PDFSolid_Conversion.Conversion.CPDFConversion.html).

### PDF to Excel

```csharp
ExcelOptions excelOptions = new ExcelOptions();
excelOptions.WorksheetOption = ExcelWorksheetOption.e_ForTable;
ErrorCode error = CPDFConversion.StartPDFToExcel("input.pdf", "", "output.xlsx", excelOptions);
```

### PDF to Image

```csharp
ImageOptions imageOptions = new ImageOptions();
imageOptions.ImageType = ImageType.e_PNG;
imageOptions.ImageScaling = 2.0;
ErrorCode error = CPDFConversion.StartPDFToImage("input.pdf", "", "output", imageOptions);
```

### PDF to Searchable PDF (OCR)

```csharp
LibraryManager.SetDocumentAIModel("path/to/resource/models/documentai.model", -1);

var options = new SearchablePdfOptions();
options.EnableOCR = true;
options.Languages = new System.Collections.Generic.List<OCRLanguage> { OCRLanguage.e_ENGLISH };
options.TransparentText = true;
ErrorCode error = CPDFConversion.StartPDFToSearchablePDF("scan.pdf", "", "output.pdf", options);
```

### PDF to JSON with Table Extraction

```csharp
JsonOptions jsonOptions = new JsonOptions();
jsonOptions.ContainTable = true;
ErrorCode error = CPDFConversion.StartPDFToJson("input.pdf", "", "output.json", jsonOptions);
```

### Custom AI Engine (SDK v1.1.0+)

See [Use Custom AI Models via Callbacks](doc/developer_guide_dotnet.md#312-use-custom-ai-models-via-callbacks) for the callback delegates, JSON results, and native pointer lifetime requirements.

## Documentation

- [Developer Guide](doc/developer_guide_dotnet.md)
- [API Reference](doc/api_reference_dotnet.html)

## Contact

- Website: [https://www.pdfsolid.com](https://www.pdfsolid.com/)
- Sales: [sales@pdfsolid.com](mailto:sales@pdfsolid.com)
- Support: [support@pdfsolid.com](mailto:support@pdfsolid.com)
