namespace ChocolateyExplorer.WPF
{
    using System;
    using System.Deployment.Application;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Principal;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Startup += this.Application_Startup;
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
                    MessageBox.Show("Sorry, this application must be run as Administrator.");
                }

                this.Shutdown();
            }
        }
    }
}
