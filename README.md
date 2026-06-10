# PDFSolid Conversion SDK for Windows (.NET)

A high-performance .NET library for extracting and transforming PDF content — text, images, tables, links, and annotations — into various file formats while preserving the original document layout.

## Supported Conversions

| Output Format | Extension |
| ------------- | --------- |
| Word | .docx |
| Excel | .xlsx |
| PowerPoint | .pptx |
| HTML | .html |
| CSV | .csv |
| Image | .png, .jpg, .jpeg, .jpeg2000, .bmp, .tiff, .tga, .gif, .webp |
| Plain Text | .txt |
| Rich Text Format | .rtf |
| Searchable PDF | .pdf |
| OFD | .ofd |
| Structured Data | .json |
| Markdown | .md |

## AI-Powered Document Tools

- **Optical Character Recognition (OCR)** — Recognize text from scanned documents and images.
- **Layout Analysis** — AI-based document structure parsing (paragraphs, tables, figures, etc.).
- **Table Recognition** — Reconstruct table structure including merged cells and borderless tables.
- **Custom AI Engine** — Plug in your own OCR/Layout/Table model via callbacks (SDK v4.1.0+).

## Requirements

| Platform | System Requirements | IDE | Framework |
| -------- | ------------------- | --- | --------- |
| Windows | Windows 7, 8, 10, 11 (32/64-bit) | Visual Studio 2017+ | .NET Framework 4.6.1+ |

## Quick Start

### 1. Initialize the SDK

```csharp
using PDFSolid_Conversion.Common;
using PDFSolid_Conversion.Conversion;

LibraryManager.Initialize("path/to/resource");
```

### 2. Apply License

```csharp
ErrorCode result = LibraryManager.LicenseVerify("LICENSE_KEY");
```

### 3. Convert PDF to Word

```csharp
WordOptions wordOptions = new WordOptions();
wordOptions.LayoutMode = PageLayoutMode.e_Flow;
wordOptions.ContainImage = true;
wordOptions.ContainAnnotation = true;

ErrorCode error = CPDFConversion.StartPDFToWord("input.pdf", "", "output.docx", wordOptions);
```

### 4. Convert PDF to Excel

```csharp
ExcelOptions excelOptions = new ExcelOptions();
excelOptions.WorksheetOption = ExcelWorksheetOption.e_ForTable;

ErrorCode error = CPDFConversion.StartPDFToExcel("input.pdf", "", "output.xlsx", excelOptions);
```

### 5. Convert PDF with OCR

```csharp
LibraryManager.SetDocumentAIModel("path/documentai.model", -1);

WordOptions wordOptions = new WordOptions();
wordOptions.EnableOCR = true;
wordOptions.Languages = new List<OCRLanguage> { OCRLanguage.e_ENGLISH };

ErrorCode error = CPDFConversion.StartPDFToWord("scanned.pdf", "", "output.docx", wordOptions);
```

### 6. Release Resources

```csharp
LibraryManager.ReleaseDocumentAIModel();
LibraryManager.Release();
```

## Running the Demo

1. Open `samples/PDFSolid_Conversion_Demo.sln` in Visual Studio.
2. Click **Start** to build and run the WPF demo application.
3. Select an input file, choose an output folder, pick a conversion type, and click **Convert**.

## Package Structure

```
├── doc/           # API reference and developer guide
├── lib/           # SDK dynamic libraries
├── samples/       # Demo project (WPF)
├── resource/      # DocumentAI model resources
├── legal.txt      # Legal and copyright information
└── release_notes.txt
```

## Key Conversion Options

| Option | Description | Applies To |
| ------ | ----------- | ---------- |
| `ContainImage` | Include images in output | Word, Excel, PPT, HTML, RTF, JSON, Markdown |
| `ContainAnnotation` | Retain PDF annotations | Word, Excel, PPT, HTML, RTF, JSON, Markdown |
| `LayoutMode` | Flow or Box layout | Word, HTML |
| `EnableOCR` | Enable OCR for scanned documents | All text-based formats |
| `EnableAiLayout` | Enable AI layout analysis | All text-based formats |
| `EnableAiTableRecognition` | Enable AI table recognition | All text-based formats |
| `PageRanges` | Select specific pages (e.g. "1-3,5,7-9") | All formats |
| `OutputDocumentPerPage` | Output one file per PDF page | All formats |
| `FontName` | Set preferred output font | Word, Excel, PPT, Searchable PDF, OFD |
| `FormulaToImage` | Convert formulas to images | Word, Excel, PPT, RTF |

## Documentation

- [Developer Guide](doc/developer_guide_dotnet.md) — Comprehensive guide covering all APIs and options.
- [API Reference](doc/api_reference_dotnet.html) — Full API reference documentation.

## License

PDFSolid Conversion SDK is a commercial SDK. A license is required for development and distribution. Contact [support@pdfsolid.com](mailto:support@pdfsolid.com) for licensing information.

## Support

- Website: [https://www.pdfsolid.com](https://www.pdfsolid.com/)
- Email: [support@pdfsolid.com](mailto:support@pdfsolid.com)
