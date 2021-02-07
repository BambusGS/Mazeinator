using System.Windows;
using System;

namespace Mazeinator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();
            if (e.Args.Length > 0)
            {
                string startupArgs = String.Join("; ", e.Args);
                MessageBox.Show("Startup arguments: " + startupArgs, "Startup arguments", MessageBoxButton.OK, MessageBoxImage.Information); //had to change app.xaml starup
            }
            wnd.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An unhandled exception just occured: " + e.Exception.Message, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}