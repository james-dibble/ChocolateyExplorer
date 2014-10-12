namespace ChocolateyExplorer.WPF.ViewModel
{
    using System;
    using System.Deployment.Application;
    using System.Reflection;
    using System.Windows;
    using GalaSoft.MvvmLight;

    public class AboutViewModel : ViewModelBase
    {
        public Version ApplicationVersion
        {
            get
            {
                if(ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.CurrentVersion;
                }

                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
    }
}
