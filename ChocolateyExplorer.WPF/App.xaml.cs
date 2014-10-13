namespace ChocolateyExplorer.WPF
{
    using System;
    using System.Deployment.Application;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Principal;
    using System.Windows;

    using ChocolateyExplorer.WPF.Views;

    using FirstFloor.ModernUI.Windows.Controls;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            this.Startup += this.Application_Startup;
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var messageBox = new ModernDialog
                             {
                                 Title = "Woops",
                                 Content = new ErrorDialog(e.Exception)
                             };

            messageBox.Show();

            e.Handled = true;
        }

        private bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!IsRunAsAdministrator())
            {
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };

                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    var messageBox = new ModernDialog
                    {
                        Title = "Woops",
                        Content = new ErrorDialog(new InvalidOperationException("Chocolatey Explorer must be run as an administrator."))
                    };

                    messageBox.Show();
                }

                this.Shutdown();
            }
        }
    }
}
