using PDFSolid_Conversion.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace PDFSolid_Conversion_Demo
{
  /// <summary>
  /// The right-hand options panel. Every option row is built once; picking a tool only shows the
  /// rows that tool uses (docs/demo_ui/design_spec.md §5), so values carry over between tools.
  /// Edits are written straight into the shared <see cref="ConvertOptions"/>.
  /// </summary>
  public class OptionsPanel : StackPanel
  {
    private enum Kind { Switch, Segmented, Select, Text, Number }

    private sealed class Option
    {
      public string Key;
      public string Section;
      public string Label;
      public string Hint;
      public string Placeholder;
      public Kind Kind;
      public string[] Choices;
      public Func<ConvertOptions, bool> GetBool;
      public Action<ConvertOptions, bool> SetBool;
      public Func<ConvertOptions, int> GetIndex;
      public Action<ConvertOptions, int> SetIndex;
      public Func<ConvertOptions, string> GetText;
      public Action<ConvertOptions, string> SetText;
    }

    private sealed class Row
    {
      public Option Option;
      public Grid Root;
      public StackPanel Labels;
      public TextBlock Label;
      public TextBlock Hint;
      public FrameworkElement Control;
      public CheckBox Switch;
      public RadioButton[] Segments;
      public ComboBox Select;
      public TextBox Field;
    }

    private static readonly string[] SectionOrder = { "Content", "Layout", "Recognition", "Image", "Pages and output", "Image input" };
    private static readonly string[] CommonOrder = { "layout", "worksheet", "csv", "htmlPages", "imgFormat", "ocr", "ocrLang", "pages" };

    private static readonly Dictionary<string, string> SectionNotes = new Dictionary<string, string>
    {
      { "Image input", "Only used when the input file is an image." },
    };

    private static readonly string[] Languages =
    {
      "Chinese (Simplified)", "Chinese (Traditional)", "English", "Korean", "Japanese", "Latin", "Devanagari",
      "Cyrillic", "Arabic", "Tamil", "Telugu", "Kannada", "Thai", "Greek", "Eslav"
    };

    private readonly Dictionary<string, Row> rows = new Dictionary<string, Row>();
    private readonly List<Row> ordered = new List<Row>();
    private readonly Dictionary<string, StackPanel> sections = new Dictionary<string, StackPanel>();
    private readonly Border commonSection;
    private readonly Expander advancedSettings;
    private readonly StackPanel emptyState;
    private ConvertOptions options = new ConvertOptions();
    private ConversionTool tool;
    private bool syncing;

    public OptionsPanel()
    {
      var common = new StackPanel();
      common.Children.Add(new TextBlock { Text = "Common options", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
      commonSection = new Border { Padding = new Thickness(0, 14, 0, 6), Child = common };
      Children.Add(commonSection);

      var advancedContent = new StackPanel();
      advancedSettings = new Expander
      {
        Header = "Advanced settings", Content = advancedContent, Style = BuildDisclosureStyle(),
        Visibility = Visibility.Collapsed,
      };
      AutomationName(advancedSettings, "Advanced settings");
      advancedSettings.PreviewKeyDown += (sender, args) =>
      {
        // Leave arrows in option controls alone; only the header handles disclosure keys.
        if (advancedSettings.IsEnabled && args.OriginalSource is ToggleButton
            && args.OriginalSource == advancedSettings.Template.FindName("HeaderSite", advancedSettings)
            && (args.Key == Key.Left || args.Key == Key.Right || args.Key == Key.Enter))
        {
          advancedSettings.IsExpanded = args.Key == Key.Enter ? !advancedSettings.IsExpanded : args.Key == Key.Right;
          args.Handled = true;
        }
      };
      Children.Add(advancedSettings);

      foreach (string section in SectionOrder)
      {
        var content = new StackPanel();
        var heading = new TextBlock { Text = section, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 14, 0, 4) };
        heading.SetResourceReference(TextBlock.ForegroundProperty, "Ink2Brush");
        content.Children.Add(heading);
        if (SectionNotes.TryGetValue(section, out string note))
        {
          content.Children.Add(new TextBlock { Text = note, Style = Caption, Margin = new Thickness(0, 4, 0, 4) });
        }
        sections[section] = content;
        advancedContent.Children.Add(content);
      }

      foreach (Option option in Definitions())
      {
        Row row = BuildRow(option);
        rows[option.Key] = row;
        ordered.Add(row);
        if (!CommonOrder.Contains(option.Key))
        {
          sections[option.Section].Children.Add(row.Root);
        }
      }
      foreach (string key in CommonOrder)
      {
        common.Children.Add(rows[key].Root);
      }

      emptyState = new StackPanel { Margin = new Thickness(0, 18, 0, 0), Visibility = Visibility.Collapsed };
      var emptyTitle = new TextBlock { Text = "This conversion has no options.", TextWrapping = TextWrapping.Wrap };
      emptyTitle.SetResourceReference(TextBlock.ForegroundProperty, "Ink2Brush");
      emptyState.Children.Add(emptyTitle);
      emptyState.Children.Add(new TextBlock { Text = "Each document is converted as it is.", Style = Caption, Margin = new Thickness(0, 2, 0, 0) });
      Children.Add(emptyState);
    }

    /// <summary>The option values being edited. Assigning reloads controls and resets disclosure state.</summary>
    public ConvertOptions Options
    {
      get { return options; }
      set
      {
        options = value;
        // MainWindow assigns a fresh options instance on Reset; tool switches keep this instance.
        advancedSettings.IsExpanded = false;
        Load();
        Refresh();
      }
    }

    public bool HasOptions => tool != null && tool.Options.Length > 0;

    public void ShowTool(ConversionTool nextTool)
    {
      tool = nextTool;
      commonSection.Visibility = HasOptions ? Visibility.Visible : Visibility.Collapsed;
      foreach (string name in SectionOrder)
      {
        // Common rows retain their original section metadata, but do not create empty groups.
        bool visible = tool != null && ordered.Any(r => r.Option.Section == name && !CommonOrder.Contains(r.Option.Key) && tool.Options.Contains(r.Option.Key));
        sections[name].Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
      }
      advancedSettings.Visibility = sections.Values.Any(section => section.Visibility == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
      emptyState.Visibility = HasOptions ? Visibility.Collapsed : Visibility.Visible;
      Refresh();
    }

    /// <summary>All supported options, including hidden rows, for the activity log.</summary>
    public string Summary()
    {
      var parts = new List<string>();
      foreach (Row row in ordered)
      {
        if (tool == null || !tool.Options.Contains(row.Option.Key))
        {
          continue;
        }
        Option option = row.Option;
        string value;
        switch (option.Kind)
        {
          case Kind.Switch:
            value = row.Switch.IsChecked == true ? "on" : "off";
            break;
          case Kind.Segmented:
          case Kind.Select:
            value = option.Choices[option.GetIndex(options)];
            break;
          default:
            value = option.GetText(options);
            if (value.Length == 0)
            {
              value = "none";
            }
            break;
        }
        parts.Add(option.Label + " " + value);
      }
      return parts.Count == 0 ? "no options" : string.Join(", ", parts);
    }

    // Copies the option values into the controls without echoing the change back.
    private void Load()
    {
      syncing = true;
      foreach (Row row in rows.Values)
      {
        Option option = row.Option;
        switch (option.Kind)
        {
          case Kind.Switch:
            row.Switch.IsChecked = option.GetBool(options);
            break;
          case Kind.Segmented:
            row.Segments[option.GetIndex(options)].IsChecked = true;
            break;
          case Kind.Select:
            row.Select.SelectedIndex = option.GetIndex(options);
            break;
          default:
            row.Field.Text = option.GetText(options);
            break;
        }
      }
      syncing = false;
    }

    // Applies the dependencies between options (design_spec.md §5.4).
    private void Refresh()
    {
      if (tool == null)
      {
        return;
      }
      bool ocrOn = tool.OcrAlwaysOn || options.EnableOCR;
      foreach (Row row in ordered)
      {
        bool visible = tool.Options.Contains(row.Option.Key) && (row.Option.Key != "ocrLang" || ocrOn);
        row.Root.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
      }

      Row ocr = rows["ocr"];
      syncing = true;
      ocr.Switch.IsChecked = ocrOn;
      syncing = false;
      SetEnabled(ocr, !tool.OcrAlwaysOn);
      ocr.Hint.Text = tool.OcrAlwaysOn ? "Always on for this format" : "Read text from scans and images";

      SetEnabled(rows["ocrLang"], ocrOn);
      SetEnabled(rows["ocrScope"], ocrOn);
      SetEnabled(rows["background"], ocrOn);
      rows["background"].Hint.Text = ocrOn ? "Keep the scanned page image" : "Turn on OCR to use";

      // OCR brings its own layout and table analysis, so the AI switches rest while it is on.
      SetEnabled(rows["aiLayout"], !ocrOn);
      SetEnabled(rows["aiTable"], !ocrOn);
    }

    private void OnSwitchChanged(Row row)
    {
      if (syncing)
      {
        return;
      }
      bool value = row.Switch.IsChecked == true;
      row.Option.SetBool(options, value);
      if (row.Option.Key == "ocr")
      {
        // Word keeps the page background off under OCR so the recognized text stays editable.
        options.ContainPageBackgroundImage = value && tool.Kind != ToolKind.Word;
        syncing = true;
        rows["background"].Switch.IsChecked = options.ContainPageBackgroundImage;
        syncing = false;
      }
      Refresh();
    }

    private static void SetEnabled(Row row, bool enabled)
    {
      row.Control.IsEnabled = enabled;
      double opacity = enabled ? 1 : 0.45;
      row.Labels.Opacity = opacity;
      row.Control.Opacity = opacity;
    }

    private Row BuildRow(Option option)
    {
      var row = new Row { Option = option };
      row.Root = new Grid { MinHeight = 36, Margin = new Thickness(0, 5, 0, 5) };
      row.Root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
      row.Root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
      row.Root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

      var labels = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
      row.Labels = labels;
      row.Label = new TextBlock { Text = option.Label, TextWrapping = TextWrapping.Wrap };
      row.Hint = new TextBlock { Text = option.Hint ?? "", Style = Caption };
      labels.Children.Add(row.Label);
      labels.Children.Add(row.Hint);
      if (string.IsNullOrEmpty(option.Hint) && option.Key != "ocr" && option.Key != "background")
      {
        row.Hint.Visibility = Visibility.Collapsed;
      }
      row.Root.Children.Add(labels);

      switch (option.Kind)
      {
        case Kind.Switch:
          row.Switch = new CheckBox { Style = FindStyle("Switch") };
          AutomationName(row.Switch, option.Label);
          row.Switch.Checked += (sender, args) => OnSwitchChanged(row);
          row.Switch.Unchecked += (sender, args) => OnSwitchChanged(row);
          row.Control = row.Switch;
          break;

        case Kind.Segmented:
          var track = new Border { Style = FindStyle("SegmentTrack") };
          var segments = new StackPanel { Orientation = Orientation.Horizontal };
          row.Segments = new RadioButton[option.Choices.Length];
          for (int i = 0; i < option.Choices.Length; i++)
          {
            int index = i;
            var segment = new RadioButton
            {
              Content = option.Choices[i],
              Style = FindStyle("SegmentButton"),
              GroupName = "segment-" + option.Key,
              Margin = new Thickness(i == 0 ? 0 : 2, 0, 0, 0),
            };
            segment.Checked += (sender, args) =>
            {
              if (!syncing)
              {
                option.SetIndex(options, index);
              }
            };
            row.Segments[i] = segment;
            segments.Children.Add(segment);
          }
          track.Child = segments;
          row.Control = track;
          break;

        case Kind.Select:
          row.Select = new ComboBox { Style = FindStyle("SelectBox"), ItemsSource = option.Choices };
          AutomationName(row.Select, option.Label);
          row.Select.SelectionChanged += (sender, args) =>
          {
            if (!syncing && row.Select.SelectedIndex >= 0)
            {
              option.SetIndex(options, row.Select.SelectedIndex);
            }
          };
          row.Control = row.Select;
          break;

        default:
          row.Field = new TextBox { Style = FindStyle("Field"), Tag = option.Placeholder ?? "" };
          AutomationName(row.Field, option.Label);
          if (option.Kind == Kind.Number)
          {
            row.Field.Width = 72;
            row.Field.TextAlignment = TextAlignment.Right;
            row.Field.LostKeyboardFocus += (sender, args) => CommitNumber(row);
          }
          else
          {
            row.Field.TextChanged += (sender, args) =>
            {
              if (!syncing)
              {
                option.SetText(options, row.Field.Text);
              }
            };
          }
          row.Control = row.Field;
          break;
      }

      Grid.SetColumn(row.Control, 2);
      row.Control.VerticalAlignment = VerticalAlignment.Center;
      row.Root.Children.Add(row.Control);
      return row;
    }

    // The scale must be a positive number; anything else puts the last good value back.
    private void CommitNumber(Row row)
    {
      if (TryParseNumber(row.Field.Text, out float value) && value > 0)
      {
        row.Option.SetText(options, Math.Min(value, 100f).ToString(CultureInfo.InvariantCulture));
      }
      syncing = true;
      row.Field.Text = row.Option.GetText(options);
      syncing = false;
    }

    private static bool TryParseNumber(string text, out float value)
    {
      return float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
          || float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static Style Caption => (Style)Application.Current.FindResource("Caption");

    private static Style BuildDisclosureStyle()
    {
      // Native Expander and ToggleButton peers retain accessibility and Space-key behavior.
      return (Style)XamlReader.Parse(@"
<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='Expander'>
  <Setter Property='Foreground' Value='{DynamicResource InkBrush}'/>
  <Setter Property='Focusable' Value='False'/>
  <Setter Property='Margin' Value='0,0,0,6'/>
  <Setter Property='Template'>
    <Setter.Value>
      <ControlTemplate TargetType='Expander'>
        <StackPanel>
          <ToggleButton x:Name='HeaderSite' Content='{TemplateBinding Header}'
                        AutomationProperties.Name='{Binding Header, RelativeSource={RelativeSource TemplatedParent}}'
                        IsChecked='{Binding IsExpanded, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'
                        Style='{DynamicResource ButtonBase}' FontWeight='SemiBold' Height='32'>
            <ToggleButton.Template>
              <ControlTemplate TargetType='ToggleButton'>
                <Border x:Name='HeaderBorder' Background='Transparent' CornerRadius='8' Padding='8,0'>
                  <Grid>
                    <Grid.ColumnDefinitions>
                      <ColumnDefinition Width='*'/>
                      <ColumnDefinition Width='16'/>
                    </Grid.ColumnDefinitions>
                    <ContentPresenter VerticalAlignment='Center'/>
                    <Path x:Name='Arrow' Grid.Column='1' Data='M6 4.5 L9.5 8 L6 11.5' Width='16' Height='16'
                          VerticalAlignment='Center' Stroke='{TemplateBinding Foreground}' StrokeThickness='1.5'
                          StrokeStartLineCap='Round' StrokeEndLineCap='Round' StrokeLineJoin='Round'
                          RenderTransformOrigin='0.5,0.5'/>
                  </Grid>
                </Border>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Arrow' Property='RenderTransform'>
                      <Setter.Value><RotateTransform Angle='90'/></Setter.Value>
                    </Setter>
                  </Trigger>
                  <Trigger Property='IsMouseOver' Value='True'>
                    <Setter TargetName='HeaderBorder' Property='Background' Value='{DynamicResource RaisedBrush}'/>
                  </Trigger>
                  <Trigger Property='IsPressed' Value='True'>
                    <Setter TargetName='HeaderBorder' Property='Background' Value='{DynamicResource RuleBrush}'/>
                  </Trigger>
                  <Trigger Property='IsEnabled' Value='False'>
                    <Setter TargetName='HeaderBorder' Property='Opacity' Value='0.4'/>
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </ToggleButton.Template>
          </ToggleButton>
          <ContentPresenter x:Name='OptionsContent' ContentSource='Content' Visibility='Collapsed'/>
        </StackPanel>
        <ControlTemplate.Triggers>
          <Trigger Property='IsExpanded' Value='True'>
            <Setter TargetName='OptionsContent' Property='Visibility' Value='Visible'/>
          </Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>");
    }

    private static Style FindStyle(string key)
    {
      return (Style)Application.Current.FindResource(key);
    }

    private static void AutomationName(DependencyObject element, string name)
    {
      System.Windows.Automation.AutomationProperties.SetName(element, name);
    }

    // The option table: labels, controls and the ConvertOptions field each one edits.
    private static IEnumerable<Option> Definitions()
    {
      yield return Switch("images", "Content", "Images", null, o => o.ContainImages, (o, v) => o.ContainImages = v);
      yield return Switch("background", "Content", "Page background", "Keep the scanned page image",
                          o => o.ContainPageBackgroundImage, (o, v) => o.ContainPageBackgroundImage = v);
      yield return Switch("annotations", "Content", "Annotations", null, o => o.ContainAnnotations, (o, v) => o.ContainAnnotations = v);
      yield return Switch("tables", "Content", "Tables", null, o => o.ContainTables, (o, v) => o.ContainTables = v);
      yield return Switch("formulas", "Content", "Formulas as images", null, o => o.FormulaToImage, (o, v) => o.FormulaToImage = v);
      yield return Switch("transparent", "Content", "Transparent text", "Keep the recognized text invisible",
                          o => o.TransparentText, (o, v) => o.TransparentText = v);
      yield return Switch("allContent", "Content", "All content", "Export text outside tables too", o => o.AllContent, (o, v) => o.AllContent = v);
      yield return Switch("tableFormat", "Content", "Format tables", "Line table cells up in columns",
                          o => o.TxtTableFormat, (o, v) => o.TxtTableFormat = v);

      // Hybrid is the SDK's flow layout; Fixed is its box layout.
      yield return new Option
      {
        Key = "layout", Section = "Layout", Label = "Page layout", Kind = Kind.Segmented, Choices = new[] { "Hybrid", "Fixed" },
        GetIndex = o => o.LayoutMode == PageLayoutMode.e_Flow ? 0 : 1,
        SetIndex = (o, i) => o.LayoutMode = i == 0 ? PageLayoutMode.e_Flow : PageLayoutMode.e_Box,
      };
      yield return Select("htmlPages", "Layout", "HTML files", new[] { "Single file", "Single file with outline", "Multiple files", "Multiple files with links" },
                          o => (int)o.htmlOption, (o, i) => o.htmlOption = (HtmlPageOption)i);
      yield return Select("worksheet", "Layout", "Worksheets", new[] { "One per table", "One per page", "One per document" },
                          o => (int)o.WorksheetOption, (o, i) => o.WorksheetOption = (ExcelWorksheetOption)i);
      yield return Text("font", "Layout", "Font name", null, "e.g. Arial", o => o.FontName, (o, v) => o.FontName = v);

      yield return Switch("aiLayout", "Recognition", "AI layout analysis", null, o => o.EnableAiLayout, (o, v) => o.EnableAiLayout = v);
      yield return Switch("aiTable", "Recognition", "AI table recognition", null, o => o.EnableAiTableRecognition, (o, v) => o.EnableAiTableRecognition = v);
      yield return Switch("ocr", "Recognition", "OCR", "Read text from scans and images", o => o.EnableOCR, (o, v) => o.EnableOCR = v);
      // The language list starts at the SDK's value 1; 0 is "unknown" and is not offered.
      yield return Select("ocrLang", "Recognition", "Language", Languages,
                          o => (int)o.OCRLanguage - 1, (o, i) => o.OCRLanguage = (OCRLanguage)(i + 1));
      yield return Select("ocrScope", "Recognition", "Recognize", new[] { "Garbled text", "Scanned pages", "Scans and garbled text", "Everything" },
                          o => (int)o.OcrOption, (o, i) => o.OcrOption = (OCROption)i);

      yield return Select("imgFormat", "Image", "Format", new[] { "JPG", "JPEG", "JPEG 2000", "PNG", "BMP", "TIFF", "TGA", "GIF", "WEBP" },
                          o => (int)o.ImageFormat, (o, i) => o.ImageFormat = (ImageType)i);
      yield return new Option
      {
        Key = "colorMode", Section = "Image", Label = "Color", Kind = Kind.Segmented, Choices = new[] { "Color", "Gray", "B&W" },
        GetIndex = o => (int)o.ImageMode, SetIndex = (o, i) => o.ImageMode = (ImageColorMode)i,
      };
      yield return new Option
      {
        Key = "scale", Section = "Image", Label = "Scale", Hint = "Higher is sharper and larger", Kind = Kind.Number,
        GetText = o => o.ImageRatio.ToString(CultureInfo.InvariantCulture),
        SetText = (o, v) => o.ImageRatio = float.Parse(v, CultureInfo.InvariantCulture),
      };
      yield return Switch("pathEnhance", "Image", "Enhance vector paths", null, o => o.ImagePathEnhance, (o, v) => o.ImagePathEnhance = v);

      yield return Text("pages", "Pages and output", "Page range", "e.g. 1-3, 5, 7-9", "All pages", o => o.PageRanges, (o, v) => o.PageRanges = v);
      yield return Switch("perPage", "Pages and output", "One file per page", null, o => o.OutputDocumentPerPage, (o, v) => o.OutputDocumentPerPage = v);
      yield return Switch("csv", "Pages and output", "Save tables as CSV", null, o => o.CsvFormat, (o, v) => o.CsvFormat = v);
      yield return Switch("autoFolder", "Pages and output", "Folder for multiple files", null, o => o.AutoCreateFolder, (o, v) => o.AutoCreateFolder = v);
      yield return Select("wordBookmarks", "Pages and output", "PDF bookmarks", new[] { "None", "Word headings", "Word bookmarks" },
                          o => (int)o.WordBookmarkMode, (o, i) => o.WordBookmarkMode = (WordPdfBookmarkMode)i);

      yield return Select("pageSize", "Image input", "Page size",
                          new[] { "Original", "Auto (nearest)", "A3", "A4", "A5", "B4 (JIS)", "B5 (JIS)", "B6 (JIS)", "Letter", "Legal", "Tabloid" },
                          o => (int)o.ImagePageSize, (o, i) => o.ImagePageSize = (ImagePageSize)i);
    }

    private static Option Switch(string key, string section, string label, string hint,
                                 Func<ConvertOptions, bool> get, Action<ConvertOptions, bool> set)
    {
      return new Option { Key = key, Section = section, Label = label, Hint = hint, Kind = Kind.Switch, GetBool = get, SetBool = set };
    }

    private static Option Select(string key, string section, string label, string[] choices,
                                 Func<ConvertOptions, int> get, Action<ConvertOptions, int> set)
    {
      return new Option { Key = key, Section = section, Label = label, Kind = Kind.Select, Choices = choices, GetIndex = get, SetIndex = set };
    }

    private static Option Text(string key, string section, string label, string hint, string placeholder,
                               Func<ConvertOptions, string> get, Action<ConvertOptions, string> set)
    {
      return new Option { Key = key, Section = section, Label = label, Hint = hint, Placeholder = placeholder, Kind = Kind.Text, GetText = get, SetText = set };
    }
  }
}
