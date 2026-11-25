using PDFSolid_Conversion.Common;
using PDFSolid_Conversion.Conversion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PDFSolid_Conversion_Demo
{
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
    private readonly ObservableCollection<FileItem> files = new ObservableCollection<FileItem>();
    private readonly Dictionary<ConversionTool, RadioButton> navItems = new Dictionary<ConversionTool, RadioButton>();

    private ConvertOptions options = new ConvertOptions();
    private ConversionTool tool;
    private string outputFolder = "";
    private string lastRunFolder = "";
    private bool running;
    private volatile bool cancelRequested;
    private readonly SemaphoreSlim conversionGate = new SemaphoreSlim(4, 4);

    private bool sdkChecked;
    private bool sdkLoaded;
    private ErrorCode licenseResult = ErrorCode.e_ErrLicenseUninitialized;
    private bool wordToPdfSupported;

    public MainWindow()
    {
      InitializeComponent();
      ThemeManager.Attach(this);

      FileList.ItemsSource = files;
      OptionsView.Options = options;
      BuildNav();
      SelectTool(ConversionTool.All[0], false);
      UpdateFileArea();
      UpdateOutputField();
    }

    #region SDK

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      string exePath = Path.GetDirectoryName(typeof(MainWindow).Assembly.Location);
      string libraryPath = Path.Combine(exePath, "x64");
      sdkChecked = true;

      sdkLoaded = LibraryManager.InitLibrary(libraryPath);
      if (!sdkLoaded)
      {
        Log("The SDK library could not be loaded from " + libraryPath + ".");
        SetSdkStatus(false, "SDK not loaded", "Check the x64 folder");
        UpdateNotice();
        UpdateActions();
        return;
      }

      LibraryManager.Initialize(Path.Combine(exePath, "resource"));
      string version = LibraryManager.GetVersion();
      Log("SDK " + version + " loaded from " + libraryPath);

      licenseResult = LibraryManager.LicenseVerify(Path.Combine(exePath, "license.xml"));
      if (licenseResult != ErrorCode.e_ErrSuccess)
      {
        Log("License verification failed. " + ConversionRunner.Describe(licenseResult));
        SetSdkStatus(false, "License invalid", "SDK " + version);
      }
      else
      {
        Log("License verified");
        SetSdkStatus(true, "License active", "SDK " + version);

        ErrorCode modelResult = LibraryManager.SetDocumentAIModel(Path.Combine(exePath, "resource", "models", "documentai.model"));
        Log(modelResult == ErrorCode.e_ErrSuccess
          ? "Document AI model loaded"
          : "Document AI model not loaded. " + ConversionRunner.Describe(modelResult));
      }

      wordToPdfSupported = CPDFConversion.IsWordToPDFSupported();
      Log(wordToPdfSupported ? "Word to PDF is available" : "This SDK build doesn't include Word to PDF");
      UpdateNotice();
      UpdateActions();
    }

    private void SetSdkStatus(bool healthy, string status, string detail)
    {
      SdkDot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, healthy ? "OkBrush" : "BadBrush");
      SdkStatus.Text = status;
      SdkVersion.Text = detail;
      if (!healthy)
      {
        ShowLog(true);
      }
    }

    private ConvertCallback BuildCallback(FileItem file, out OnProgress progress, out OnCancel cancel)
    {
      progress = (pageIndex, total) => Dispatcher.BeginInvoke(new Action(() =>
      {
        if (file.Status == FileStatus.Converting)
          file.SetProgress(pageIndex, total);
      }));
      cancel = () => cancelRequested;
      ConvertCallback callback = new ConvertCallback();
      callback.progress = Marshal.GetFunctionPointerForDelegate(progress);
      callback.cancel = Marshal.GetFunctionPointerForDelegate(cancel);
      callback.ocr = IntPtr.Zero;
      callback.get_ocr_result = IntPtr.Zero;
      return callback;
    }

    #endregion

    #region Tools

    private void BuildNav()
    {
      var groups = new[]
      {
        new { Group = ToolGroup.FromPdf, Title = "Convert PDF to" },
        new { Group = ToolGroup.MakeSearchable, Title = "Make searchable" },
        new { Group = ToolGroup.CreatePdf, Title = "Create PDF" },
      };

      foreach (var group in groups)
      {
        var title = new TextBlock { Text = group.Title, FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(20, 16, 20, 6) };
        title.SetResourceReference(TextBlock.ForegroundProperty, "Ink3Brush");
        NavPanel.Children.Add(title);

        foreach (ConversionTool entry in ConversionTool.All.Where(t => t.Group == group.Group))
        {
          var content = new DockPanel();
          var chip = new DocChip { Width = 16, Height = 20, ShowLines = true, Fill = entry.Chip.BaseBrush, Margin = new Thickness(0, 0, 10, 0) };
          content.Children.Add(chip);
          if (entry.Badge != null)
          {
            var badge = new Border { Style = (Style)FindResource("Pill"), Child = new TextBlock { Text = entry.Badge } };
            DockPanel.SetDock(badge, Dock.Right);
            content.Children.Add(badge);
          }
          content.Children.Add(new TextBlock { Text = entry.Name, VerticalAlignment = VerticalAlignment.Center });

          var item = new RadioButton { Style = (Style)FindResource("NavItem"), Content = content, GroupName = "tools" };
          System.Windows.Automation.AutomationProperties.SetName(item, entry.Title);
          ConversionTool target = entry;
          item.Checked += (sender, args) => SelectTool(target, true);
          navItems[entry] = item;
          NavPanel.Children.Add(item);
        }
      }
    }

    private void SelectTool(ConversionTool next, bool animate)
    {
      if (next == tool)
      {
        return;
      }
      tool = next;
      navItems[next].IsChecked = true;
      ThemeManager.SetAccent(next.Target);

      SourceChip.Fill = next.Source.BaseBrush;
      SourceChip.Label = next.Source.Label;
      SwapTargetChip(next.Target, animate);
      ToolTitle.Text = next.Title;
      ToolDescription.Text = next.Description;
      DropTitle.Text = "Drop " + next.InputNoun + " here";
      DropTypes.Text = next.InputSummary;
      DropBarText.Text = "Drop more " + next.InputNoun + " here, or";

      OptionsView.ShowTool(next);
      ResetButton.Visibility = next.Options.Length > 0 ? Visibility.Visible : Visibility.Hidden;
      OptionsScroller.ScrollToTop();

      // Results belong to the tool that produced them, so a new tool starts every row afresh.
      foreach (FileItem file in files)
      {
        MarkForTool(file);
      }
      ClearRunSummary();
      UpdateNotice();
      UpdateActions();
    }

    // The only animation in the window: the new output chip slides in as the old one leaves.
    private void SwapTargetChip(DocFormat format, bool animate)
    {
      var chip = new DocChip { Width = 40, Height = 50, Fill = format.BaseBrush, Label = format.Label };
      if (!animate || ThemeManager.ReduceMotion || TargetChipHost.Children.Count == 0)
      {
        TargetChipHost.Children.Clear();
        TargetChipHost.Children.Add(chip);
        return;
      }

      var old = (UIElement)TargetChipHost.Children[TargetChipHost.Children.Count - 1];
      TargetChipHost.Children.Add(chip);
      Slide(old, 1, 0, 0, -8, 140, new QuadraticEase { EasingMode = EasingMode.EaseIn }, () => TargetChipHost.Children.Remove(old));
      Slide(chip, 0, 1, 8, 0, 180, new QuadraticEase { EasingMode = EasingMode.EaseOut }, null);
    }

    private static void Slide(UIElement element, double fromOpacity, double toOpacity, double fromY, double toY,
                              int milliseconds, IEasingFunction easing, Action completed)
    {
      var shift = new TranslateTransform(0, fromY);
      element.RenderTransform = shift;
      var duration = TimeSpan.FromMilliseconds(milliseconds);
      var fade = new DoubleAnimation(fromOpacity, toOpacity, duration) { EasingFunction = easing };
      if (completed != null)
      {
        fade.Completed += (sender, args) => completed();
      }
      element.BeginAnimation(OpacityProperty, fade);
      shift.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(fromY, toY, duration) { EasingFunction = easing });
    }

    private void UpdateNotice()
    {
      string title = null;
      string text = null;
      if (!sdkChecked)
      {
        // Nothing to say until Window_Loaded has tried the library.
      }
      else if (!sdkLoaded)
      {
        title = "The SDK library didn't load.";
        text = "Put pdfsolidconversionsdk.dll and the libraries it needs in the x64 folder next to the app, then start it again.";
      }
      else if (licenseResult != ErrorCode.e_ErrSuccess)
      {
        title = "The license didn't verify.";
        text = ConversionRunner.Describe(licenseResult) + " Conversions fail until license.xml is valid.";
      }
      else if (tool.Kind == ToolKind.WordToPdf && !wordToPdfSupported)
      {
        title = "This SDK build doesn't include Word to PDF.";
        text = "You can still add files. Convert turns on once the SDK library supports it.";
      }

      Notice.Visibility = title == null ? Visibility.Collapsed : Visibility.Visible;
      NoticeTitle.Text = title ?? "";
      NoticeText.Text = text ?? "";
    }

    private void ResetOptions_Click(object sender, RoutedEventArgs e)
    {
      options = new ConvertOptions();
      OptionsView.Options = options;
    }

    #endregion

    #region Files

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
      BrowseForFiles();
    }

    private void BrowseForFiles()
    {
      if (running)
      {
        return;
      }
      var dialog = new Microsoft.Win32.OpenFileDialog { Filter = tool.OpenFileFilter, Multiselect = true };
      if (dialog.ShowDialog(this) == true)
      {
        AddFiles(dialog.FileNames);
      }
    }

    // Folders are opened one level deep and only contribute files the current tool can take.
    private void AddFiles(IEnumerable<string> paths)
    {
      var known = new HashSet<string>(files.Select(f => f.FilePath), StringComparer.OrdinalIgnoreCase);
      var added = new List<string>();
      foreach (string path in paths)
      {
        if (Directory.Exists(path))
        {
          try
          {
            added.AddRange(Directory.GetFiles(path).Where(tool.Accepts).OrderBy(p => p, StringComparer.OrdinalIgnoreCase));
          }
          catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
          {
            Log("Can't read the folder " + path + ": " + ex.Message);
          }
        }
        else if (File.Exists(path))
        {
          added.Add(path);
        }
      }

      foreach (string path in added.Where(known.Add))
      {
        var file = new FileItem(path);
        MarkForTool(file);
        files.Add(file);
      }

      if (string.IsNullOrEmpty(outputFolder) && files.Count > 0)
      {
        outputFolder = files[0].Folder;
        UpdateOutputField();
      }
      ClearRunSummary();
      UpdateFileArea();
      UpdateActions();
    }

    private void MarkForTool(FileItem file)
    {
      if (tool.Accepts(file.FilePath))
      {
        file.SetReady();
      }
      else
      {
        file.SetUnsupported(tool.TakesWordFiles ? "Not a Word file" : "Not a PDF or image");
      }
    }

    private void RemoveFile_Click(object sender, RoutedEventArgs e)
    {
      if (((FrameworkElement)sender).DataContext is FileItem file && !running)
      {
        files.Remove(file);
        UpdateFileArea();
        UpdateActions();
      }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
      files.Clear();
      ClearRunSummary();
      UpdateFileArea();
      UpdateActions();
    }

    private void UpdateFileArea()
    {
      bool any = files.Count > 0;
      DropZone.Visibility = any ? Visibility.Collapsed : Visibility.Visible;
      FilesArea.Visibility = any ? Visibility.Visible : Visibility.Collapsed;
      FileCount.Text = files.Count.ToString(CultureInfo.InvariantCulture);
    }

    private void Workspace_DragOver(object sender, DragEventArgs e)
    {
      bool accept = !running && e.Data.GetDataPresent(DataFormats.FileDrop);
      e.Effects = accept ? DragDropEffects.Copy : DragDropEffects.None;
      SetDropHighlight(accept);
      e.Handled = true;
    }

    private void Workspace_DragLeave(object sender, DragEventArgs e)
    {
      SetDropHighlight(false);
    }

    private void Workspace_Drop(object sender, DragEventArgs e)
    {
      SetDropHighlight(false);
      if (!running && e.Data.GetData(DataFormats.FileDrop) is string[] paths)
      {
        AddFiles(paths);
      }
    }

    private void SetDropHighlight(bool on)
    {
      foreach (System.Windows.Shapes.Rectangle frame in new[] { DropZoneBorder, DropBarBorder })
      {
        frame.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty, on ? "AccentHiBrush" : "RuleStrongBrush");
        if (on)
        {
          frame.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, "SelSoftBrush");
        }
        else
        {
          frame.Fill = Brushes.Transparent;
        }
      }
    }

    private void ShowFile_Click(object sender, RoutedEventArgs e)
    {
      if (!(((FrameworkElement)sender).DataContext is FileItem file))
      {
        return;
      }
      if (File.Exists(file.OutputPath))
      {
        Process.Start("explorer.exe", "/select,\"" + file.OutputPath + "\"");
      }
      else if (Directory.Exists(file.OutputPath))
      {
        Process.Start("explorer.exe", "\"" + file.OutputPath + "\"");
      }
      else
      {
        OpenFolder(lastRunFolder);
      }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
      OpenFolder(lastRunFolder);
    }

    private static void OpenFolder(string folder)
    {
      if (Directory.Exists(folder))
      {
        Process.Start("explorer.exe", "\"" + folder + "\"");
      }
    }

    #endregion

    #region Output folder

    private void ChangeOutput_Click(object sender, RoutedEventArgs e)
    {
      ChooseOutputFolder();
    }

    private void OutputField_Click(object sender, MouseButtonEventArgs e)
    {
      ChooseOutputFolder();
    }

    private void ChooseOutputFolder()
    {
      if (running)
      {
        return;
      }
      var dialog = new FolderSelectDialog { Title = "Save converted files to" };
      if (Directory.Exists(outputFolder))
      {
        dialog.InitialDirectory = outputFolder;
      }
      if (dialog.ShowDialog(new WindowInteropHelper(this).Handle))
      {
        outputFolder = dialog.FileName;
        UpdateOutputField();
        UpdateActions();
      }
    }

    private void UpdateOutputField()
    {
      bool empty = string.IsNullOrEmpty(outputFolder);
      OutputPath.Text = empty ? "Choose a folder" : outputFolder;
      OutputPath.SetResourceReference(TextBlock.ForegroundProperty, empty ? "Ink3Brush" : "InkBrush");
      OutputField.ToolTip = empty ? null : outputFolder;
    }

    #endregion

    #region Convert

    private void UpdateActions()
    {
      int count = files.Count(f => tool.Accepts(f.FilePath));
      bool blocked = tool.Kind == ToolKind.WordToPdf && !wordToPdfSupported;
      ConvertButton.Content = count == 0 ? "Convert" : "Convert " + count + (count == 1 ? " file" : " files");
      ConvertButton.IsEnabled = !running && count > 0 && !string.IsNullOrEmpty(outputFolder) && !blocked;
    }

    private async void Convert_Click(object sender, RoutedEventArgs e)
    {
      if (running || !ConvertButton.IsEnabled)
      {
        return;
      }
      List<FileItem> queue = files.Where(f => tool.Accepts(f.FilePath)).ToList();
      ConversionTool runTool = tool;
      ConvertOptions runOptions = options.Copy();
      string runFolder = outputFolder;

      try
      {
        Directory.CreateDirectory(runFolder);
      }
      catch (Exception ex)
      {
        Log("Can't use the output folder " + runFolder + ": " + ex.Message);
        ShowLog(true);
        return;
      }

      SetRunning(true);
      lastRunFolder = runFolder;
      foreach (FileItem file in queue)
      {
        file.SetWaiting();
      }
      Log(runTool.Title + ": " + queue.Count + (queue.Count == 1 ? " file" : " files") + " to " + runFolder);
      Log("  " + OptionsView.Summary());

      int converted = 0, failed = 0, stopped = 0;
      try
      {
        // SDK suffix selection is not atomic. Serialize inputs whose output names can collide,
        // before taking a worker slot so other names can still use all four workers.
        var nameGates = queue.Select(file => ConversionRunner.OutputNameKey(file.FilePath))
          .Distinct(StringComparer.OrdinalIgnoreCase)
          .ToDictionary(name => name, name => new SemaphoreSlim(1, 1), StringComparer.OrdinalIgnoreCase);
        Task<ConversionResult>[] tasks = queue.Select((file, index) =>
          ConvertOneAsync(file, index, queue.Count, runTool, runFolder, runOptions,
            nameGates[ConversionRunner.OutputNameKey(file.FilePath)])).ToArray();
        ConversionResult[] results = await Task.WhenAll(tasks);
        converted = results.Count(r => r.Code == ErrorCode.e_ErrSuccess);
        failed = results.Count(r => r.Code != ErrorCode.e_ErrSuccess && r.Code != ErrorCode.e_ErrCancel);
        stopped = results.Count(r => r.Code == ErrorCode.e_ErrCancel);
      }
      catch (Exception ex)
      {
        Log("Batch failed: " + ex.Message);
        failed = queue.Count(f => f.Status == FileStatus.Failed);
        converted = queue.Count(f => f.Status == FileStatus.Done);
        stopped = queue.Count(f => f.Status == FileStatus.Stopped);
      }
      finally
      {
        SetRunning(false);
      }
      var parts = new List<string> { converted + " converted" };
      if (failed > 0)
      {
        parts.Add(failed + " failed");
      }
      if (stopped > 0)
      {
        parts.Add(stopped + " stopped");
      }
      RunSummary.Text = string.Join(", ", parts);
      OpenFolderButton.Visibility = converted > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private sealed class ConversionResult
    {
      public ErrorCode Code;
    }

    private async Task<ConversionResult> ConvertOneAsync(FileItem file, int index, int total,
                                                          ConversionTool runTool, string runFolder,
                                                          ConvertOptions runOptions, SemaphoreSlim nameGate)
    {
      await nameGate.WaitAsync();
      try
      {
        await conversionGate.WaitAsync();
        try
        {
          return await ConvertOneCoreAsync(file, index, total, runTool, runFolder, runOptions);
        }
        finally
        {
          conversionGate.Release();
        }
      }
      finally
      {
        nameGate.Release();
      }
    }

    private async Task<ConversionResult> ConvertOneCoreAsync(FileItem file, int index, int total,
                                                              ConversionTool runTool, string runFolder,
                                                              ConvertOptions runOptions)
    {
      if (cancelRequested)
      {
        await Dispatcher.InvokeAsync(file.SetStopped);
        return new ConversionResult { Code = ErrorCode.e_ErrCancel };
      }

      await Dispatcher.InvokeAsync(() =>
      {
        RunSummary.Text = "Up to 4 conversions at once · " + total + " files";
        file.SetConverting();
        Log(file.Name);
      });

      Stopwatch watch = Stopwatch.StartNew();
      ISet<string> existingOutputs = null;
      string revealPath = null;
      ErrorCode result = ErrorCode.e_ErrUnknown;
      string exceptionText = null;
      OnProgress progress = null;
      OnCancel cancel = null;
      try
      {
        ConvertCallback callback = BuildCallback(file, out progress, out cancel);
        string input = file.FilePath;
        existingOutputs = ConversionRunner.SnapshotOutput(runFolder);
        result = await Task.Run(() => cancelRequested ? ErrorCode.e_ErrCancel
          : ConversionRunner.Run(runTool, input, runFolder, runOptions.Copy(), callback, out revealPath));
      }
      catch (Exception ex)
      {
        exceptionText = ex.GetType().Name + ": " + ex.Message;
      }

      finally
      {
        GC.KeepAlive(progress);
        GC.KeepAlive(cancel);
      }

      string output = null;
      if (result == ErrorCode.e_ErrSuccess && exceptionText == null)
      {
        output = ConversionRunner.FindOutput(runTool, runOptions, runFolder, file.FilePath, revealPath, existingOutputs);
        if (string.IsNullOrEmpty(output) || !File.Exists(output) && !Directory.Exists(output))
        {
          exceptionText = "The SDK reported success but produced no output. Results folder: " + runFolder;
          result = ErrorCode.e_ErrUnknown;
        }
      }
      await Dispatcher.InvokeAsync(() =>
      {
        if (exceptionText != null)
        {
          file.SetFailed("Conversion failed", exceptionText);
          Log("  " + exceptionText);
        }
        else if (result == ErrorCode.e_ErrSuccess)
        {
          file.SetDone(output);
          Log("  Done in " + watch.Elapsed.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " s");
        }
        else if (result == ErrorCode.e_ErrCancel)
        {
          file.SetStopped();
          Log("  Stopped");
        }
        else
        {
          file.SetFailed(ConversionRunner.ShortReason(result), ConversionRunner.Describe(result));
          Log("  Failed. " + ConversionRunner.Describe(result));
        }
      });
      return new ConversionResult { Code = exceptionText == null ? result : ErrorCode.e_ErrUnknown };
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
      if (!running || cancelRequested)
      {
        return;
      }
      cancelRequested = true;
      StopButton.IsEnabled = false;
      Log("Stopping at the next page");
    }

    private void SetRunning(bool value)
    {
      running = value;
      cancelRequested = false;
      ConvertButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
      StopButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
      StopButton.IsEnabled = true;
      NavPanel.IsEnabled = !value;
      OptionsView.IsEnabled = !value;
      OptionsView.Opacity = value ? 0.6 : 1;
      ResetButton.IsEnabled = !value;
      ChangeButton.IsEnabled = !value;
      BrowseButton.IsEnabled = !value;
      BrowseLink.IsEnabled = !value;
      ClearButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
      OpenFolderButton.Visibility = Visibility.Collapsed;
      foreach (FileItem file in files)
      {
        file.CanRemove = !value;
      }
      UpdateActions();
    }

    private void ClearRunSummary()
    {
      RunSummary.Text = "";
      OpenFolderButton.Visibility = Visibility.Collapsed;
    }

    #endregion

    #region Activity log

    private void Log(string message)
    {
      string line = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "  " + message;
      LogText.AppendText(LogText.Text.Length == 0 ? line : Environment.NewLine + line);
      LogText.ScrollToEnd();
    }

    private void ShowLog(bool show)
    {
      LogPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
      LogButton.IsChecked = show;
      if (show)
      {
        LogText.ScrollToEnd();
      }
    }

    private void LogButton_Click(object sender, RoutedEventArgs e)
    {
      ShowLog(LogButton.IsChecked == true);
    }

    private void CloseLog_Click(object sender, RoutedEventArgs e)
    {
      ShowLog(false);
    }

    private void CopyLog_Click(object sender, RoutedEventArgs e)
    {
      if (LogText.Text.Length > 0)
      {
        Clipboard.SetText(LogText.Text);
      }
    }

    private void SdkStatus_Click(object sender, RoutedEventArgs e)
    {
      ShowLog(true);
    }

    #endregion

    // Ctrl+O adds files, Ctrl+Enter converts, Esc stops a running conversion.
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
      if (ctrl && e.Key == Key.O)
      {
        BrowseForFiles();
        e.Handled = true;
      }
      else if (ctrl && e.Key == Key.Enter && ConvertButton.IsEnabled && ConvertButton.Visibility == Visibility.Visible)
      {
        Convert_Click(ConvertButton, new RoutedEventArgs());
        e.Handled = true;
      }
      else if (e.Key == Key.Escape && running)
      {
        Stop_Click(StopButton, new RoutedEventArgs());
        e.Handled = true;
      }
    }
  }
}
