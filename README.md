# PDFSolid Conversion SDK for Windows (.NET)

High-performance .NET SDK for converting PDF to Word, Excel, PowerPoint, HTML, Image, TXT, RTF, CSV, JSON, Markdown, Searchable PDF, and OFD with AI-powered OCR, layout analysis, and table recognition.

## Features

- **PDF to Word** (.docx) — Flow and Box layout modes
- **PDF to Excel** (.xlsx) — per-table, per-page, or per-document worksheet options
- **PDF to PowerPoint** (.pptx)
- **PDF to HTML** (.html) — single/multi-page with optional bookmark navigation
- **PDF to CSV** (.csv)
- **PDF to Image** (.png, .jpg, .jpeg, .jpeg2000, .bmp, .tiff, .tga, .gif, .webp) — color/grayscale/binary, configurable scaling
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

### 2. Apply License and Initialize

```csharp
using PDFSolid_Conversion.Common;
using PDFSolid_Conversion.Conversion;

ErrorCode result = LibraryManager.LicenseVerify("LICENSE_KEY");
if (result != ErrorCode.e_ErrSuccess) {
    return;
}
LibraryManager.Initialize("path/to/resource");
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
LibraryManager.SetDocumentAIModel("path/model", -1);

WordOptions wordOptions = new WordOptions();
wordOptions.EnableOCR = true;
wordOptions.Languages = new List<OCRLanguage> { OCRLanguage.e_ENGLISH };
wordOptions.TransparentText = true;
ErrorCode error = CPDFConversion.StartPDFToWord("scan.pdf", "", "output.docx", wordOptions);
```

### PDF to JSON with Table Extraction

```csharp
JsonOptions jsonOptions = new JsonOptions();
jsonOptions.ContainTable = true;
ErrorCode error = CPDFConversion.StartPDFToJson("input.pdf", "", "output.json", jsonOptions);
```

### Custom AI Engine (SDK v1.1.0+)

```csharp
ConvertCallback callback = new ConvertCallback();
callback.OnOcr = (imagePath) => { /* your OCR */ return true; };
callback.GetOcrResult = () => { return ocrJson; };
callback.OnLayout = (imagePath) => { /* your layout */ return true; };
callback.GetLayoutResult = () => { return layoutJson; };
callback.OnTable = (imagePath) => { /* your table */ return true; };
callback.GetTableResult = () => { return tableJson; };

WordOptions wordOptions = new WordOptions();
wordOptions.EnableOCR = true;
wordOptions.EnableAiLayout = true;
CPDFConversion.StartPDFToWord("input.pdf", "", "output.docx", wordOptions, callback);
```

## Documentation

- [Developer Guide](doc/developer_guide_dotnet.md)
- [API Reference](doc/api_reference_dotnet.html)

## Contact

- Website: [https://www.pdfsolid.com](https://www.pdfsolid.com/)
- Sales: [sales@pdfsolid.com](mailto:sales@pdfsolid.com)
- Support: [support@pdfsolid.com](mailto:support@pdfsolid.com)
