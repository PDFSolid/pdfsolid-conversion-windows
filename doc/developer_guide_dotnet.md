# 1. Overview

PDFSolid Conversion SDK is a high-performance library designed for extracting and transforming the data within your PDF files, such as text, images, tables, links, and annotations, into various file formats. The Conversion SDK retains the original document layout and the properties of the file data, helping you build a reliable document conversion workflow in C# applications.

Effortlessly integrate the PDFSolid Conversion SDK into your C# projects in just a few steps, and enable the following file format conversions:

- Convert PDF to Word (.docx)
- Convert PDF to Excel (.xlsx)
- Convert PDF to PowerPoint (.pptx)
- Convert PDF to HTML (.html)
- Convert PDF to CSV (.csv)
- Convert PDF to Image (.png, .jpg, .jpeg, .jpeg2000, .bmp, .tiff, .tga, .gif, .webp)
- Convert PDF to Plain Text (.txt)
- Convert PDF to Rich Text Format (.rtf)
- Convert PDF to Searchable PDF (.pdf)
- Convert PDF to OFD (.ofd)
- Convert PDF to Structured Data (.json)
- Convert PDF to Markdown (.md)

To enhance format conversion results, PDFSolid also provides AI-powered document tools with the following capabilities:

- Optical Character Recognition (OCR)
- Layout Analysis
- Table Recognition

## 1.1 Why PDFSolid Conversion SDK

- Mature Technology

  With years of technology accumulation, PDFSolid has established a complete mechanism of product iteration to offer a continuous guarantee for product competitiveness.

- Complete PDF and Format Conversion Functionalities

  The comprehensive feature set can meet diverse conversion needs and is easy for customers to use without training costs.

- High-quality Service

  Professional service and technical support can quickly respond to users' feedback through onsite service or remote support such as telephone and email.

- Independent Intellectual Property Rights

  The technology is independent and compliant with ISO, helping enterprises conduct international business without copyright risks.

## 1.2 PDFSolid Conversion SDK for .NET

The PDFSolid Conversion SDK is designed to convert PDF files into many other formats while preserving the original layout and formatting of the documents. In this guide, we will demonstrate how to use the SDK in Windows C# projects.

## 1.3 License & Trial

The PDFSolid Conversion SDK is a commercial SDK that requires a license to grant developers the right to develop and distribute their applications. In development mode, each license is only valid for one device ID. PDFSolid provides flexible licensing models. Please contact [our marketing team](mailto:support@pdfsolid.com) for more information. Even if you have a license, it is prohibited to distribute any documents, sample code, or source code of the PDFSolid Conversion SDK to any third parties.

If you do not have a license, please contact the PDFSolid Team at support@pdfsolid.com to obtain a trial license for PDFSolid Conversion SDK.

# 2. Get Started

## 2.1 Requirements

Before starting, please make sure that you have met the following prerequisites.

### 2.1.1 Get PDFSolid License Key

PDFSolid provides two types of license key: 30-day free trial license and commercial license.

#### How to Get Free Trial License

Contact our sales team at support@pdfsolid.com and we will send you a 30-day free trial license for PDFSolid Conversion SDK.

#### How to Get Commercial License

PDFSolid Conversion SDK is a commercial SDK that requires a license for application release. Any documents, sample code, or source code distribution from the released package of PDFSolid to any third party is prohibited.

To get a commercial license for PDFSolid Conversion SDK, feel free to contact our sales team at support@pdfsolid.com.

For the .NET Conversion SDK, the commercial license must be bound to your developer device ID (How to find the developer device ID), and each license is only valid for one device ID in development mode.

### 2.1.2 Download Conversion SDK

Contact us at support@pdfsolid.com to obtain the PDFSolid .NET Conversion SDK.

### 2.1.3 System Requirements

| Development Platform | System Requirements | Development Environment | Notice |
| -------------------- | ------------------- | ----------------------- | ------ |
| Windows | Windows 7, 8, 10, and 11 (32-bit, 64-bit). | Visual Studio 2017 or higher. | .NET Framework 4.6.1 or higher. Samples have been tested on Windows 10 and 11. |

## 2.2 SDK Package Structure

You can contact us at support@pdfsolid.com to get the PDF format conversion SDK package. The PDFSolid Conversion .NET SDK contains the following files:

- ***"doc"*** - API reference and developer guide.
- ***"lib"*** - PDFSolid dynamic libraries for supported architectures.
- ***"samples"*** - Sample projects.
- ***"resource"*** - DocumentAI model resources.
- ***"legal.txt"*** - Legal and copyright information.
- ***"release_notes.txt"*** - Release information.

## 2.3 Apply the License Key

PDFSolid Conversion SDK currently supports offline authentication to verify license keys.

### 2.3.1 Copy the License Key

Accurately obtaining the license key is crucial for applying the license.

1. In the email you received, locate the XML file containing the license key.
2. Open the XML file and determine the license type based on the `<type>` field. If `<type>online</type>` is present, it indicates an online license. If `<type>offline</type>` is present or if the field is absent, it indicates an offline license.

**Online License:**

```xml
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<license version="1">
    <platform>windows</platform>
    <starttime>xxxxxxxx</starttime>
    <endtime>xxxxxxxx</endtime>
    <type>online</type>
    <key>LICENSE_KEY</key>
</license>
```

**Offline License:**

```xml
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<license version="1">
    <platform>windows</platform>
    <starttime>xxxxxxxx</starttime>
    <endtime>xxxxxxxx</endtime>
    <key>LICENSE_KEY</key>
</license>
```

3. Copy the value located at the `LICENSE_KEY` position within the `<key>LICENSE_KEY</key>` field. This is your license key.

### 2.3.2 Apply the License Key

You can perform offline authentication using the following method:

```csharp
using PDFSolid_Conversion.Common;

string license = "LICENSE_KEY";
ErrorCode result = LibraryManager.LicenseVerify(license);
```

Before calling any conversion API, initialize the SDK resource directory:

```csharp
using PDFSolid_Conversion.Common;

LibraryManager.Initialize("PDFSolid_Conversion_SDK/resource");
```

## 2.4 How to Run a Demo

### Windows

PDFSolid Conversion SDK provides a demo in the ***"samples"*** folder. You can follow these steps to run the demo:

1. Open the demo in Visual Studio: Find and double-click the ***"PDFSolid_Conversion_Demo.sln"*** in the ***"samples"*** folder.
2. Click **Start** to build and run the demo on a Windows device.

Output files such as Word, Excel, and PowerPoint files will be generated in the selected output folder.

## 2.5 How to Make a Windows Program in C# with PDFSolid Conversion SDK

### Create a New Windows Project

1. Open Visual Studio, choose ***File*** -> ***New*** -> ***Project...***, and then select ***Visual C#*** -> ***Windows Desktop*** -> ***Console App(.NET Framework)***.
2. Choose the options for your new project. Please make sure to choose .NET Framework 4.6.1 as the programming framework.
3. Place the project to the location as desired. Then, click **OK**.

### Add PDFSolid Conversion SDK Package

1. Copy all files in the ***"lib"*** folder to the project folder.

2. Add PDFSolid Conversion SDK dynamic library to **References**. In order to use PDFSolid Conversion SDK APIs in the project, you must add the reference to the project first.

   - In Solution Explorer, right-click the project and click **Add -> Reference...**
   - In the Add Reference dialog, click the **Browse** tab, navigate to the project folder, select ***"pdfsolidconversionsdk_dotnet.dll"*** dynamic library, and then click **OK**.

3. Add the ***"x64"*** and ***"x86"*** folders into the project. Please make sure to set the property **Copy to Output Directory** of ***"pdfsolidconversionsdk.dll"*** and ***"opencv_world4100.dll"*** to **Copy if newer**. Otherwise, you should copy them to the same folder as the executable file manually before running the project.

# 3. Conversion Guides

PDFSolid Conversion SDK allows developers to use simple C# APIs to convert PDFs to common formats such as Word, Excel, PowerPoint, HTML, CSV, PNG, JPEG, RTF, TXT, Searchable PDF, OFD, JSON, and Markdown. It also provides conversion options, such as whether to include images or annotations, whether to enable OCR, and whether to enable layout analysis.

## 3.1 Initialize Library Resources

### Overview

Initialize the necessary file and memory resources required by the PDFSolid Conversion SDK.

### Notes

- You must initialize SDK resources before calling any conversion interface.
- When using OCR, Layout Analysis, Table Recognition, PDF to Searchable PDF, or PDF to OFD, make sure the DocumentAI model resources in the `resource` directory are available.

### Example

```csharp
LibraryManager.Initialize("PDFSolid_Conversion_SDK/resource");
```

## 3.2 Set DocumentAI Model

### Overview

Before using OCR, Layout Analysis, Table Recognition, PDF to Searchable PDF, or PDF to OFD, set the DocumentAI model path first.

`SetDocumentAIModel` supports the `gpu_id` parameter to specify the GPU device index for the AI model. When `gpu_id` is `-1`, GPU acceleration is disabled.

### Example

```csharp
LibraryManager.SetDocumentAIModel("path/documentai.model", -1);
```

### Set AI Model Instance Count

If you need to control the number of Layout Analysis and Table Recognition model instances, call the following interface:

```csharp
LibraryManager.SetDocumentAIModelCount(1, 1);
```

The first parameter indicates the number of Layout Analysis model instances, and the second parameter indicates the number of Table Recognition model instances.

### Use Your Own AI Engine (SDK v4.1.0+)

This option is available only in SDK v4.1.0 or later. If you prefer to run OCR, Layout Analysis, or Table Recognition with your own model or a third-party service instead of the bundled DocumentAI model, the SDK exposes callback hooks on `ConvertCallback` that let you supply the results as JSON. See [3.11 Use Custom AI Models via Callbacks](#311-use-custom-ai-models-via-callbacks) for details. When all capabilities you need are covered by your own callbacks, `SetDocumentAIModel` does not have to be called.

## 3.3 Get Conversion Progress

PDFSolid Conversion SDK obtains conversion progress through the `progress` callback in `ConvertCallback`. The following example demonstrates how to get conversion progress while performing a PDF to Word task:

```csharp
OnProgress getProgress = (pageIndex, total) =>
{
    Console.WriteLine($"progress: {pageIndex} / {total}");
};

ConvertCallback callback = new ConvertCallback();
callback.progress = Marshal.GetFunctionPointerForDelegate(getProgress);

WordOptions wordOptions = new WordOptions();
CPDFConversion.StartPDFToWord("input.pdf", "password", "path/output.docx", wordOptions, callback);
```

## 3.4 Cancel Conversion Task

PDFSolid Conversion SDK supports interrupting an ongoing conversion task through the `cancel` callback in `ConvertCallback`. When the `cancel` callback returns `true`, the current conversion task stops as soon as possible.

```csharp
OnCancel getCancel = () =>
{
    return false;
};

ConvertCallback callback = new ConvertCallback();
callback.cancel = Marshal.GetFunctionPointerForDelegate(getCancel);

WordOptions wordOptions = new WordOptions();
CPDFConversion.StartPDFToWord("input.pdf", "password", "path/output.docx", wordOptions, callback);
```

If you need to cancel the conversion at a specific time, return `true` from the cancel callback based on external state.

## 3.5 Select Page Range for Conversion

PDFSolid Conversion SDK supports converting a specified page range. When an empty string is passed, all pages will be converted. If the page range exceeds one page, you can enable `OutputDocumentPerPage` to output each PDF page as a separate file.

```csharp
WordOptions wordOptions = new WordOptions();
wordOptions.OutputDocumentPerPage = true;
wordOptions.PageRanges = "1-3,5,7-9";
```

## 3.6 Contain Image and Annotation Options

### Overview

When converting PDF documents into various formats, PDFSolid Conversion SDK offers two common options: whether images are included in the generated document, and whether annotations from the PDF file are retained.

- When `ContainImage` is enabled, the SDK extracts images from the PDF document and embeds them in the corresponding pages and positions in the output file. For areas with overlapping images, the SDK merges these images into one image and embeds it at the correct location.
- When `ContainAnnotation` is enabled, most annotations are converted into raster images and embedded at the corresponding positions. Certain types of annotations, such as highlights, underlines, strikeouts, and squiggly lines, are converted into native formatting equivalents in Word, PowerPoint, and HTML documents when possible.

These options are commonly used in the following conversions:

- PDF to Word
- PDF to Excel
- PDF to PowerPoint
- PDF to HTML
- PDF to RTF
- Extract PDF to JSON
- Extract PDF to Markdown

### Sample

```csharp
WordOptions wordOptions = new WordOptions();
wordOptions.ContainImage = true;
wordOptions.ContainAnnotation = true;

CPDFConversion.StartPDFToWord("input.pdf", "password", "path/output.docx", wordOptions);
```

## 3.7 Page Layout Mode

In certain formats, the page layout mode plays a key role in the quality of the converted document. PDFSolid Conversion SDK supports two layout modes: Flow Layout and Box Layout.

- **Flow Layout:** This layout uses paragraph indentations, columns, and tab positions to adjust content. Its main advantage is flexibility. Content can flow automatically as the document is edited and can adapt to different screen sizes.
- **Box Layout:** This layout is based on the PDF fixed-page model and accurately positions text, images, and tables on the page using coordinates. It is useful for documents that require high-precision reproduction, such as contracts, design drafts, and academic papers.

Page layout modes are commonly used in the following conversions:

- PDF to Word
- PDF to HTML

### Sample

```csharp
WordOptions wordOptions = new WordOptions();

wordOptions.LayoutMode = PageLayoutMode.e_Flow;
CPDFConversion.StartPDFToWord("input.pdf", "password", "path/flow.docx", wordOptions);

wordOptions.LayoutMode = PageLayoutMode.e_Box;
CPDFConversion.StartPDFToWord("input.pdf", "password", "path/box.docx", wordOptions);
```

## 3.8 OCR

### Overview

OCR (Optical Character Recognition) converts images of typed, handwritten, or printed text into machine-encoded text.

OCR is commonly used for text recognition and extraction from the following types of documents:

- Non-editable scanned PDF files.
- Photographs of documents.
- Scene photos such as advertising layouts and signboards.
- Identification cards, passports, vehicle license plates, invoices, bills, and receipts.

The following features support OCR:

- PDF to Word
- PDF to Excel
- PDF to PowerPoint
- PDF to HTML
- PDF to RTF
- PDF to TXT
- PDF to CSV
- PDF to Searchable PDF
- PDF to OFD
- Extract PDF to JSON
- Extract PDF to Markdown

### OCR Language

Pass OCR languages through the `Languages` property for each conversion task.

```csharp
WordOptions wordOptions = new WordOptions();
wordOptions.EnableOCR = true;
wordOptions.Languages = new List<OCRLanguage> { OCRLanguage.e_ENGLISH };

CPDFConversion.StartPDFToWord("word.pdf", "password", "path/output.docx", wordOptions);
```

Supported enum values include:

| Enum | Description |
| ---- | ----------- |
| `OCRLanguage.e_CHINESE` | Chinese Simplified |
| `OCRLanguage.e_CHINESE_TRA` | Chinese Traditional |
| `OCRLanguage.e_ENGLISH` | English |
| `OCRLanguage.e_KOREAN` | Korean |
| `OCRLanguage.e_JAPANESE` | Japanese |
| `OCRLanguage.e_LATIN` | Latin script languages |
| `OCRLanguage.e_DEVANAGARI` | Devanagari |
| `OCRLanguage.e_CYRILLIC` | Cyrillic |
| `OCRLanguage.e_ARABIC` | Arabic |
| `OCRLanguage.e_TAMIL` | Tamil |
| `OCRLanguage.e_TELUGU` | Telugu |
| `OCRLanguage.e_KANNADA` | Kannada |
| `OCRLanguage.e_THAI` | Thai |
| `OCRLanguage.e_GREEK` | Greek |
| `OCRLanguage.e_ESLAV` | Eslav |
| `OCRLanguage.e_AUTO` | Automatically select language |

### OCR Options

Different OCR options can be selected according to actual needs:

- `OCROption.e_InvalidCharacter`: Recognizes invalid or garbled characters in the PDF document through OCR, while normal characters are not processed by OCR.
- `OCROption.e_ScanPage`: Recognizes scanned pages in the PDF document through OCR, while editable pages are not processed by OCR.
- `OCROption.e_InvalidCharacterAndScanPage`: Recognizes both invalid characters and scanned pages in the PDF document through OCR.
- `OCROption.e_All`: Recognizes all pages and characters in the PDF document through OCR.

### Preserve Page Background

When OCR is enabled, you can enable `ContainPageBackgroundImage` to preserve the original page background image of the PDF. If it is disabled, the image result detected during page layout analysis will be retained.

### Notice

- The quality of the OCR result depends on the quality of the input image. A good rule of thumb is that the more pixels in the character shapes, the better. The ideal image is a grayscale image with a resolution around 300 DPI.
- When performing OCR, make sure the OCR language setting matches the language in the PDF document to achieve the best OCR conversion quality.
- OCR functionality currently does not support operating systems lower than Windows 10.

### Converting Images to Other Document Formats

The OCR function also supports converting input images into Word, Excel, PowerPoint, HTML, CSV, RTF, TXT, JSON, and other formats.

```csharp
LibraryManager.SetDocumentAIModel("path/documentai.model", -1);

WordOptions wordOptions = new WordOptions();
wordOptions.EnableOCR = true;
wordOptions.Languages = new List<OCRLanguage> { OCRLanguage.e_ENGLISH };

CPDFConversion.StartPDFToWord("input.png", "", "path/output.docx", wordOptions);
```

## 3.9 Layout Analysis

### Overview

Layout analysis uses AI technology to parse and understand the structure of a document layout. It extracts text, images, tables, layers, and other data from input documents.

Features that support Layout Analysis:

- PDF to Word
- PDF to Excel
- PDF to PowerPoint
- PDF to HTML
- PDF to RTF
- PDF to TXT
- PDF to CSV
- Extract PDF to JSON
- Extract PDF to Markdown

### Notice

- You need to load the DocumentAI model before using layout analysis, or plug in your own AI engine via callbacks described in [3.11 Use Custom AI Models via Callbacks](#311-use-custom-ai-models-via-callbacks).
- When OCR is enabled, layout analysis is automatically enabled.
- AI table recognition is a separate stage controlled by `EnableAiTableRecognition`.

### Sample

```csharp
LibraryManager.SetDocumentAIModel("path/documentai.model", -1);

WordOptions wordOptions = new WordOptions();
wordOptions.EnableAiLayout = true;

CPDFConversion.StartPDFToWord("word.pdf", "password", "path/output.docx", wordOptions);
```

## 3.10 Table Recognition

### Overview

Table Recognition reconstructs the internal structure of tables detected during layout analysis, including rows, columns, merged cells, and cell boundaries, so that the converted document preserves the original tabular semantics instead of producing a flat grid of text fragments.

It is controlled by the independent option `EnableAiTableRecognition`, which is enabled by default. The table model is invoked for table regions detected by layout analysis.

Typical scenarios that benefit from Table Recognition:

- Borderless or partially bordered tables.
- Tables with merged header cells, multi-row headers, or spanning cells.
- Scanned tables processed by OCR.

### Notice

- Table Recognition runs only when layout analysis is active, either through `EnableAiLayout = true` or implicitly through `EnableOCR = true`.
- You need to load the DocumentAI model before using Table Recognition, or plug in your own table model via callbacks described in [3.11 Use Custom AI Models via Callbacks](#311-use-custom-ai-models-via-callbacks).
- Set `EnableAiTableRecognition = false` to disable the table model.
- The number of Table Recognition model instances can be tuned through the second parameter of `SetDocumentAIModelCount`.

### Sample

```csharp
LibraryManager.SetDocumentAIModel("path/documentai.model", -1);

WordOptions wordOptions = new WordOptions();
wordOptions.EnableAiLayout = true;
wordOptions.EnableAiTableRecognition = true;

CPDFConversion.StartPDFToWord("word.pdf", "password", "path/output.docx", wordOptions);
```

## 3.11 Use Custom AI Models via Callbacks

### Overview

Starting with SDK v4.1.0, PDFSolid Conversion SDK exposes a callback-based extension point that lets you plug in your own AI inference engine for OCR, Layout Analysis, and Table Recognition. Instead of relying on the built-in DocumentAI model loaded by `SetDocumentAIModel`, you can run inference with any model or service and return the result to the SDK as a JSON string.

When the relevant callback pair is registered on `ConvertCallback`, the SDK skips its built-in model invocation for that capability and consumes your JSON output instead. If a pair is left as `IntPtr.Zero`, the SDK falls back to the built-in DocumentAI model when available.

### Callback Pairs

Each AI capability uses two callbacks: a trigger callback invoked by the SDK with the path to a page image saved as PNG in a temporary directory, and a result getter invoked by the SDK immediately afterwards to retrieve the JSON string.

| Capability | Trigger callback | Result getter callback | Triggered when |
| ---------- | ---------------- | ---------------------- | -------------- |
| OCR | `ocr` | `get_ocr_result` | `EnableOCR = true` |
| Layout Analysis | `layout` | `get_layout_result` | `EnableAiLayout = true` or `EnableOCR = true` |
| Table Recognition | `table` | `get_table_result` | `EnableAiTableRecognition = true` and a table region is detected by layout analysis |

Rules:

- The trigger receives a UTF-8 path to a PNG file. Return `true` if inference succeeded, or `false` to make the SDK ignore the result for that page.
- The getter must return a UTF-8 JSON string pointer. The pointer must remain valid until the SDK has finished parsing it.
- Both callbacks for a capability must be set together. If only one is provided, the SDK falls back to the built-in path.
- Coordinates in your JSON must be in the pixel space of the image received by the trigger, with top-left origin, X to the right, and Y down.

### Sample

```csharp
using System.Runtime.InteropServices;
using PDFSolid_Conversion.Common;
using PDFSolid_Conversion.Conversion;

OnOCR ocrDelegate = (imagePath) =>
{
    // Run your OCR engine on imagePath, cache the JSON result.
    return true;
};
OnGetOCRResult getOcrResultDelegate = () =>
{
    return Marshal.StringToHGlobalAnsi(""); // UTF-8 JSON
};

OnLayout layoutDelegate = (imagePath) => true;
OnGetLayoutResult getLayoutResultDelegate = () =>
{
    return Marshal.StringToHGlobalAnsi("");
};

OnTable tableDelegate = (imagePath) => true;
OnGetTableResult getTableResultDelegate = () =>
{
    return Marshal.StringToHGlobalAnsi("");
};

ConvertCallback callback = new ConvertCallback();
callback.ocr               = Marshal.GetFunctionPointerForDelegate(ocrDelegate);
callback.get_ocr_result    = Marshal.GetFunctionPointerForDelegate(getOcrResultDelegate);
callback.layout            = Marshal.GetFunctionPointerForDelegate(layoutDelegate);
callback.get_layout_result = Marshal.GetFunctionPointerForDelegate(getLayoutResultDelegate);
callback.table             = Marshal.GetFunctionPointerForDelegate(tableDelegate);
callback.get_table_result  = Marshal.GetFunctionPointerForDelegate(getTableResultDelegate);

WordOptions wordOptions = new WordOptions();
wordOptions.EnableOCR = true;
wordOptions.EnableAiLayout = true;
wordOptions.EnableAiTableRecognition = true;
CPDFConversion.StartPDFToWord("input.pdf", "", "output.docx", wordOptions, callback);
```

You can register only the capabilities you want to override and leave others as `IntPtr.Zero` to keep the built-in behavior.

### Thread Safety and Lifetime

- Callbacks are invoked from the SDK conversion thread. Implement them in a thread-safe manner if your model engine is shared across calls.
- The PNG image at `imagePath` lives in the SDK temporary directory and may be deleted shortly after the trigger returns. Copy or process it before returning.
- The JSON pointer returned from a getter must remain valid until the same getter is called again or until the conversion task ends.

### OCR Result JSON Schema

Returned by `get_ocr_result`. The SDK populates each `text_spans[].chars[]` either from `words[]` if provided, or by uniformly splitting the span rect.

```json
{
  "text_spans": [
    {
      "text": "Hello World",
      "confidence": 0.98,
      "rotation": 0.0,
      "rect": { "left": 120, "top": 80, "right": 320, "bottom": 110 },
      "style": {
        "font_size": 18.0,
        "font_color": { "r": 0, "g": 0, "b": 0 }
      },
      "words": [
        { "text": "Hello", "rect": { "left": 120, "top": 80, "right": 200, "bottom": 110 } },
        { "text": "World", "rect": { "left": 210, "top": 80, "right": 320, "bottom": 110 } }
      ]
    }
  ]
}
```

| Field | Type | Required | Description |
| ----- | ---- | -------- | ----------- |
| `text_spans` | array | Yes | Recognized text spans on the page. |
| `text` | string | Yes | UTF-8 text content of the span. |
| `confidence` | number | No | 0.0 - 1.0. Spans below 0.1 are discarded. |
| `rotation` | number | No | Text rotation in degrees. Default 0. |
| `rect` | object | Yes | Bounding box in image pixels (`left`/`top`/`right`/`bottom`). |
| `style.font_size` | number | No | Estimated font size in pixels. |
| `style.font_color` | object | No | `{ r, g, b }` 0 - 255. |
| `words` | array | No | Per-word boxes. If omitted, the SDK splits the span rect evenly. Strongly recommended for CJK + Latin mixed lines for correct glyph spacing. |

### Layout Analysis Result JSON Schema

Returned by `get_layout_result`. Objects with `confidence < 0.45` are discarded.

```json
{
  "objects": [
    { "type": "title", "confidence": 0.95, "rect": { "left": 60, "top": 50, "right": 540, "bottom": 90 } },
    { "type": "paragraph", "confidence": 0.97, "rect": { "left": 60, "top": 100, "right": 540, "bottom": 220 } },
    { "type": "figure", "confidence": 0.92, "rect": { "left": 80, "top": 240, "right": 520, "bottom": 460 } },
    { "type": "table", "confidence": 0.93, "rect": { "left": 60, "top": 480, "right": 540, "bottom": 700 } }
  ]
}
```

Supported `type` values:

| Value | Meaning |
| ----- | ------- |
| `paragraph` | Body text paragraph |
| `title` | Heading |
| `figure` | Image or figure |
| `figure_title` | Figure caption header |
| `figure_caption` | Figure caption text |
| `table` | Table region. Whether the table is bordered or borderless is determined by the table recognition stage, not by the layout label. |
| `table_title` | Table caption header |
| `table_caption` | Table caption text |
| `ordered_list` | Ordered list |
| `unordered_list` | Unordered list |
| `catalogue` | Table of contents |
| `formula` | Math formula |
| `code` | Code block |
| `algorithm` | Algorithm block |
| `header` | Page header |
| `footer` | Page footer |
| `page_number` | Page number |
| `reference` | Reference or citation |

Objects with a `type` value that is not listed above are ignored. Use the values in this table as the canonical layout labels in your custom output.

### Table Recognition Result JSON Schema

Returned by `get_table_result` once per detected table region. Polygons use eight integers `[x0, y0, x1, y1, x2, y2, x3, y3]` in the order top-left, top-right, bottom-right, bottom-left.

```json
{
  "type": "table_with_line",
  "position": [60, 480, 540, 480, 540, 700, 60, 700],
  "rows": 3,
  "cols": 2,
  "angle": 0.0,
  "height_of_rows": [40, 60, 60],
  "width_of_cols": [200, 280],
  "table_cells": [
    {
      "start_row": 0,
      "end_row": 0,
      "start_col": 0,
      "end_col": 0,
      "cell_background_color_r": 240,
      "cell_background_color_g": 240,
      "cell_background_color_b": 240,
      "position": [60, 480, 260, 480, 260, 520, 60, 520]
    }
  ]
}
```

| Field | Type | Description |
| ----- | ---- | ----------- |
| `type` | string | `table_with_line` for bordered tables; any other value is treated as a non-standard (borderless) table. |
| `position` | int[8] | Table polygon in image pixels. |
| `rows` / `cols` | int | Row / column counts. |
| `angle` | number | Skew angle in degrees. |
| `height_of_rows` | int[] | Per-row pixel heights (length = `rows`). |
| `width_of_cols` | int[] | Per-column pixel widths (length = `cols`). |
| `table_cells[]` | array | One entry per merged cell. |
| `start_row` / `end_row` | int | Inclusive row span of the cell. |
| `start_col` / `end_col` | int | Inclusive column span of the cell. |
| `cell_background_color_*` | int | Cell background color components (0 - 255). |
| `position` | int[8] | Cell polygon in image pixels. |

### Tip: Validate Your JSON

If you need a reference output to compare against, run a conversion once with the built-in DocumentAI model. The SDK uses the same JSON shape internally, so your custom output should follow the same structure.

## 3.12 Output Font Option

### Overview

In some output formats, you can set the preferred font name to unify the default font style in the output document.

### Supported Formats

The `FontName` option currently applies to the following formats:

- PDF to Word
- PDF to Excel
- PDF to PowerPoint
- PDF to Searchable PDF
- PDF to OFD

For Searchable PDF and OFD, `FontName` controls the font used for the invisible or visible text layer that is overlaid on the page background. For Word, Excel, and PowerPoint, it sets the preferred default font of the generated document.

### Example

```csharp
WordOptions wordOptions = new WordOptions();
wordOptions.FontName = "Arial";

CPDFConversion.StartPDFToWord("word.pdf", "password", "path/output.docx", wordOptions);
```

## 3.13 Convert PDF to Word

### Overview

Converting PDF to Word converts a PDF file into an editable Word file. You can edit, modify, insert, or delete text and pictures, and adjust layout and properties.

### Sample

```csharp
WordOptions wordOptions = new WordOptions();

wordOptions.LayoutMode = PageLayoutMode.e_Box;
CPDFConversion.StartPDFToWord("word.pdf", "password", "path/output_box.docx", wordOptions);

wordOptions.LayoutMode = PageLayoutMode.e_Flow;
CPDFConversion.StartPDFToWord("word.pdf", "password", "path/output_flow.docx", wordOptions);
```

### Convert Formulas to Images

When a document contains complex formulas and you want to preserve visual consistency in the output document, enable `FormulaToImage`.

```csharp
WordOptions wordOptions = new WordOptions();
wordOptions.FormulaToImage = true;

CPDFConversion.StartPDFToWord("word.pdf", "password", "path/output.docx", wordOptions);
```

## 3.14 Convert PDF to Excel

### Overview

PDFSolid Conversion SDK supports converting PDF documents to Microsoft Excel format (.xlsx). By extracting, parsing, and importing data from PDF into Excel, users can further edit, analyze, or share Excel files.

### Excel Options

When converting PDF files to Excel files, pay attention to the following options:

- `AllContent`: If enabled, the converted XLSX file contains all content in the PDF.
- `WorksheetOption`: Controls how worksheets are created.

| Option | Description |
| ------ | ----------- |
| `ExcelWorksheetOption.e_ForTable` | Create one sheet for one table. |
| `ExcelWorksheetOption.e_ForPage` | Create one sheet for one PDF page. |
| `ExcelWorksheetOption.e_ForDocument` | Create one sheet for the entire PDF document. |

### Sample

```csharp
ExcelOptions excelOptions = new ExcelOptions();
CPDFConversion.StartPDFToExcel("excel.pdf", "password", "path/output.xlsx", excelOptions);

excelOptions.AllContent = true;
excelOptions.WorksheetOption = ExcelWorksheetOption.e_ForDocument;
CPDFConversion.StartPDFToExcel("excel.pdf", "password", "path/output_all.xlsx", excelOptions);
```

## 3.15 Convert PDF to PowerPoint

### Overview

PDFSolid Conversion SDK converts PDF files to PowerPoint files and restores the layout and format of the original document for presentation and editing in Microsoft PowerPoint.

### Sample

```csharp
PptOptions pptOptions = new PptOptions();
CPDFConversion.StartPDFToPpt("ppt.pdf", "password", "path/output.pptx", pptOptions);
```

## 3.16 Convert PDF to HTML

### Overview

PDFSolid Conversion SDK converts PDF files to HTML files while maintaining the layout and format of the original document, allowing users to browse and view the document on the web.

### HTML Options

| Option | Description |
| ------ | ----------- |
| `HtmlPageOption.e_SinglePage` | Convert the entire PDF file into a single HTML file. |
| `HtmlPageOption.e_SinglePageWithBookmark` | Convert the PDF file into a single HTML file with an outline for navigation at the beginning of the HTML page. |
| `HtmlPageOption.e_MultiPage` | Convert the PDF file into multiple HTML files. |
| `HtmlPageOption.e_MultiPageWithBookmark` | Convert the PDF file into multiple HTML files with an outline HTML file for navigation. |

### Sample

```csharp
HtmlOptions htmlOptions = new HtmlOptions();
CPDFConversion.StartPDFToHtml("html.pdf", "password", "path/output.html", htmlOptions);

htmlOptions.LayoutMode = PageLayoutMode.e_Box;
htmlOptions.HtmlOption = HtmlPageOption.e_MultiPageWithBookmark;
CPDFConversion.StartPDFToHtml("html.pdf", "password", "path/output_multi.html", htmlOptions);
```

## 3.17 Convert PDF to CSV

### Overview

PDFSolid Conversion SDK supports converting PDF documents to CSV (Comma-Separated Values). This is commonly used to extract tabular or structured data from PDF documents.

CSV conversion uses the Excel conversion API with `CsvFormat = true`.

### Automatically Create Folders

When multiple CSV files may be output, control whether to automatically create folders through `AutoCreateFolder`. When this option is enabled, a folder with the same name as the output file will be created in the output path to store the CSV files.

### Sample

```csharp
ExcelOptions excelOptions = new ExcelOptions();
excelOptions.CsvFormat = true;

CPDFConversion.StartPDFToExcel("csv.pdf", "password", "path/output.csv", excelOptions);

excelOptions.WorksheetOption = ExcelWorksheetOption.e_ForDocument;
CPDFConversion.StartPDFToExcel("csv.pdf", "password", "path/output_merged.csv", excelOptions);
```

## 3.18 Convert PDF to Image

### Overview

PDFSolid Conversion SDK provides an API for converting PDF to images.

### Setting Image Formats

Supported image formats include:

- JPG
- JPEG
- JPEG2000
- PNG
- BMP
- TIFF
- TGA
- GIF
- WEBP

### Setting Image Color Modes

Supported image color modes include:

- `ImageColorMode.Color`: Color mode, where the image effect is consistent with the original PDF page.
- `ImageColorMode.Gray`: Grayscale mode.
- `ImageColorMode.Binary`: Black and white mode.

### Setting Image Scaling

The SDK supports setting image scaling. If you want to double the image size, set `ImageScaling` to `2.0f`; to reduce the image size by half, set `ImageScaling` to `0.5f`.

### Enhancing Image Path Display

The SDK supports `PathEnhance` for enhancing the display of image paths. This option can be enabled when you want to improve the display effect of paths within the PDF page.

- Disable `PathEnhance` option:
  ![Disable PathEnhance](image/1.png)
- Enable `PathEnhance` option:
  ![Enable PathEnhance](image/2.png)

### Notice

- A higher `ImageScaling` value results in images with higher resolution, but it also increases memory usage and slows down conversion.
- A higher `ImageScaling` value does not necessarily mean higher clarity. The clarity also depends on the original image resolution in the document.

### Sample

```csharp
ImageOptions imageOptions = new ImageOptions();

imageOptions.ImageType = ImageType.JPEG;
CPDFConversion.StartPDFToImage("jpeg.pdf", "password", "path/output", imageOptions);

imageOptions.ImageType = ImageType.PNG;
imageOptions.ImageScaling = 2.0f;
CPDFConversion.StartPDFToImage("png.pdf", "password", "path/output", imageOptions);
```

## 3.19 Convert PDF to RTF

### Overview

RTF is a popular text format that can retain text format and style data and is convenient for most text readers to read and write.

### Sample

```csharp
RtfOptions rtfOptions = new RtfOptions();
CPDFConversion.StartPDFToRtf("rtf.pdf", "password", "path/output.rtf", rtfOptions);
```

## 3.20 Convert PDF to TXT

### Overview

When you need to extract text content from a PDF file for data analysis, text mining, or information retrieval, PDFSolid Conversion SDK can extract the text into a TXT file.

### Preserving Table Format

The SDK supports `TableFormat` to preserve the table format when writing the TXT file. It is generally recommended to enable this option, especially for data extraction scenarios.

### Sample

```csharp
TxtOptions txtOptions = new TxtOptions();
txtOptions.TableFormat = true;

CPDFConversion.StartPDFToTxt("txt.pdf", "password", "path/output.txt", txtOptions);
```

## 3.21 Convert PDF to Searchable PDF

### Overview

Searchable PDF conversion adds an invisible or visible text layer to an image-based PDF, such as a scanned document, using OCR.

### Set Transparent Text Layer

When outputting a searchable PDF, use `TransparentText` to control whether the text layer is transparent.

### Sample

```csharp
LibraryManager.SetDocumentAIModel("path/documentai.model", -1);

SearchablePdfOptions pdfOptions = new SearchablePdfOptions();
pdfOptions.EnableOCR = true;
pdfOptions.Languages = new List<OCRLanguage> { OCRLanguage.e_ENGLISH };
pdfOptions.TransparentText = true;

CPDFConversion.StartPDFToSearchablePDF("scan.pdf", "password", "path/output.pdf", pdfOptions);
```

## 3.22 Convert PDF to OFD

### Overview

PDFSolid Conversion SDK supports converting PDF documents to OFD documents. Similar to Searchable PDF, OFD conversion also supports OCR, page background preservation, and transparent text layers.

### Notice

- If you need to generate searchable OFD output, enable `TransparentText`.
- When `EnableOCR` is enabled, specify the OCR language through `Languages`.

### Sample

```csharp
LibraryManager.SetDocumentAIModel("path/documentai.model", -1);

OfdOptions ofdOptions = new OfdOptions();
ofdOptions.EnableOCR = true;
ofdOptions.Languages = new List<OCRLanguage> { OCRLanguage.e_ENGLISH };
ofdOptions.TransparentText = true;

CPDFConversion.StartPDFToOfd("scan.pdf", "password", "path/output.ofd", ofdOptions);
```

## 3.23 Releasing Library Resources

### Overview

Release the file and memory resources occupied by the PDFSolid Conversion SDK.

### Notice

- After calling `Release`, the PDFSolid Conversion SDK will no longer function properly and must be initialized again before reuse.
- If you only want to release resources occupied by the AI model rather than all SDK resources, call `ReleaseDocumentAIModel`.

### Sample

```csharp
LibraryManager.ReleaseDocumentAIModel();
LibraryManager.Release();
```

# 4. Data Extraction Guide

Unleash the power of data with PDFSolid Conversion SDK data extraction to detect, recognize, analyze, and extract PDF text, images, tables, and other content.

## 4.1 Extract PDF to JSON

### Overview

Extract text, tables, and images from PDF documents to a JSON file.

### Standard Table and Non-standard Table

Tables can commonly be divided into two categories:

- Standard table: The table border and inner lines are complete and clear. There is no need to manually add table lines to divide table content.
  ![Standard table example](image/3.png)
- Non-standard table: The table lacks borders or clear inner lines, requiring manual additions of table lines to separate content.
  ![Non-standard table example](image/4.png)

### Table Extraction Option

PDFSolid Conversion SDK supports `ContainTable`. When enabled, the SDK extracts table content from PDFs and outputs the table structure. Otherwise, table content is treated as regular text.

### Notice

- Without enabling AI layout analysis or OCR options, tables in the original PDF may not be extracted. It is recommended to enable AI layout analysis or OCR for high-precision table recognition.

### Sample

```csharp
JsonOptions jsonOptions = new JsonOptions();
jsonOptions.ContainTable = true;

CPDFConversion.StartPDFToJson("json.pdf", "password", "path/output.json", jsonOptions);
```

## 4.2 Extract PDF to Markdown

### Overview

Extract text, tables, and images from PDF documents to a Markdown file.

### Sample

```csharp
MarkdownOptions markdownOptions = new MarkdownOptions();

CPDFConversion.StartPDFToMarkdown("markdown.pdf", "password", "path/output.md", markdownOptions);
```

# 5. Support

## 5.1 FAQ

- Does OCR work on x86 architecture?

  Currently, OCR only works on x64 architecture.

- Why does my conversion fail with `e_ErrPDFPassword`?

  The PDF file requires a password. Pass the correct password as the second parameter to the conversion API. For PDFs without a password, pass an empty string `""`.

## 5.2 Contact Us

Thanks for your interest in PDFSolid Conversion SDK, the easy-to-use and powerful development solution. If you encounter technical questions or bug issues when using PDFSolid Conversion SDK, please submit the problem report to the [PDFSolid team](mailto:support@pdfsolid.com). The following information will help us solve your problem:

- PDFSolid Conversion SDK product and version.
- Your operating system and IDE version.
- Detailed descriptions of the problem.
- Any other related information, such as an error screenshot.

### Contact Information

- Home link: [https://www.pdfsolid.com](https://www.pdfsolid.com/)
- Email: [support@pdfsolid.com](mailto:support@pdfsolid.com)

Thanks,

The PDFSolid Team
