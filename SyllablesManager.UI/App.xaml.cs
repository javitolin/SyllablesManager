using System.Windows;

namespace SyllablesManager.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();
            if (e.Args.Length == 1)
            {
                wnd.WantedFileToLoad = e.Args[0];
            }

            wnd.Show();
        }
    }
}
