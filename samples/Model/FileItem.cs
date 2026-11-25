using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace PDFSolid_Conversion_Demo
{
  public enum FileStatus
  {
    Ready,
    Waiting,
    Converting,
    Done,
    Failed,
    Stopped,
    /// <summary>The selected tool cannot take this file type; it is skipped.</summary>
    Unsupported
  }

  /// <summary>
  /// A row in the file list: the input file and what happened to it in the last run.
  /// </summary>
  public sealed class FileItem : INotifyPropertyChanged
  {
    private FileStatus status = FileStatus.Ready;
    private double progress;
    private string progressText = "";
    private string statusText = "Ready";
    private string errorText = "";
    private string errorDetail = "";
    private bool canRemove = true;

    public FileItem(string path)
    {
      FilePath = path;
      Name = Path.GetFileName(path);
      Folder = Path.GetDirectoryName(path);
      string extension = Path.GetExtension(path) ?? "";
      Format = DocFormat.ForInputExtension(extension);
      ChipLabel = extension.TrimStart('.').ToUpperInvariant();
      if (ChipLabel.Length > 4)
      {
        ChipLabel = ChipLabel.Substring(0, 4);
      }
      SizeText = FormatSize(new FileInfo(path).Length);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string FilePath { get; }

    public string Name { get; }

    public string Folder { get; }

    public string SizeText { get; }

    public DocFormat Format { get; }

    public Brush ChipBrush => Format.BaseBrush;

    public string ChipLabel { get; }

    /// <summary>Where the converted result was written, used by Show.</summary>
    public string OutputPath { get; set; }

    public FileStatus Status
    {
      get { return status; }
      private set { status = value; Notify(nameof(Status)); }
    }

    /// <summary>The state in words; the row shows it for Ready, Waiting, Stopped and Unsupported.</summary>
    public string StatusText
    {
      get { return statusText; }
      private set { statusText = value; Notify(nameof(StatusText)); }
    }

    /// <summary>Converted share of the pages, 0 to 1.</summary>
    public double Progress
    {
      get { return progress; }
      private set { progress = value; Notify(nameof(Progress)); }
    }

    /// <summary>Pages done out of pages total, such as "7 / 12".</summary>
    public string ProgressText
    {
      get { return progressText; }
      private set { progressText = value; Notify(nameof(ProgressText)); }
    }

    /// <summary>Short reason shown on the row when the conversion failed.</summary>
    public string ErrorText
    {
      get { return errorText; }
      private set { errorText = value; Notify(nameof(ErrorText)); }
    }

    /// <summary>Full explanation with the error code, shown as the row's tooltip.</summary>
    public string ErrorDetail
    {
      get { return errorDetail; }
      private set { errorDetail = value; Notify(nameof(ErrorDetail)); }
    }

    /// <summary>False while a conversion runs, which hides the remove button.</summary>
    public bool CanRemove
    {
      get { return canRemove; }
      set { canRemove = value; Notify(nameof(CanRemove)); }
    }

    public void SetReady()
    {
      SetQuiet(FileStatus.Ready, "Ready");
    }

    public void SetWaiting()
    {
      SetQuiet(FileStatus.Waiting, "Waiting");
    }

    public void SetStopped()
    {
      SetQuiet(FileStatus.Stopped, "Stopped");
    }

    public void SetUnsupported(string reason)
    {
      SetQuiet(FileStatus.Unsupported, reason);
    }

    public void SetConverting()
    {
      Progress = 0;
      ProgressText = "";
      StatusText = "Converting";
      Status = FileStatus.Converting;
    }

    public void SetProgress(int page, int total)
    {
      if (total <= 0)
      {
        return;
      }
      Progress = (double)page / total;
      ProgressText = page.ToString(CultureInfo.InvariantCulture) + " / " + total.ToString(CultureInfo.InvariantCulture);
    }

    public void SetDone(string outputPath)
    {
      OutputPath = outputPath;
      StatusText = "Done";
      Status = FileStatus.Done;
    }

    public void SetFailed(string reason, string detail)
    {
      ErrorText = reason;
      ErrorDetail = detail;
      StatusText = reason;
      Status = FileStatus.Failed;
    }

    private void SetQuiet(FileStatus quietStatus, string text)
    {
      StatusText = text;
      Status = quietStatus;
    }

    private void Notify(string property)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    private static string FormatSize(long bytes)
    {
      double megabytes = bytes / 1048576.0;
      if (megabytes >= 1)
      {
        return megabytes.ToString("0.0", CultureInfo.InvariantCulture) + " MB";
      }
      return System.Math.Max(1, (long)System.Math.Round(bytes / 1024.0)).ToString(CultureInfo.InvariantCulture) + " KB";
    }
  }
}
