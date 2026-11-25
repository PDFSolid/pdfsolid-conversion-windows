using PDFSolid_Conversion_Demo;
using System.Windows;

namespace PDFSolid_Conversion_Demo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Load the light or dark palette before the main window resolves its colors.
            ThemeManager.Initialize();
            base.OnStartup(e);
        }
    }
}
