using System.Windows;

namespace TcpIpProxy
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string exceptionName = e.Exception.GetType().Name;

            MessageBox.Show("An " + exceptionName + " has occurred:\n" + e.Exception.Message + "\n\nStacktrace:\n" + e.Exception.StackTrace, "TCP/IP-Proxy - " + exceptionName, MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;

            if (this.MainWindow != null)
            {
                this.MainWindow.Close();
            }
        }
    }
}
