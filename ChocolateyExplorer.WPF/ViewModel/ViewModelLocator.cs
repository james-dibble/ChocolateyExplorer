/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:ChocolateyExplorer.WPF"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using Chocolatey.Manager;
using Chocolatey.OData;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace ChocolateyExplorer.WPF.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ChocolateySourcesViewModel>();
            SimpleIoc.Default.Register<ChocolateyPackagesViewModel>();
            SimpleIoc.Default.Register<InstalledPackagesViewModel>();
            SimpleIoc.Default.Register<ConsoleViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
            SimpleIoc.Default.Register<InstallListViewModel>();

            SimpleIoc.Default.Register<IChocolateyFeedFactory, ChocolateyFeedFactory>();
            SimpleIoc.Default.Register<ISourcesManager, Sources>();
            SimpleIoc.Default.Register<IChocolateyInstaller, Installer>();
            SimpleIoc.Default.Register<IInstalledPackagesManager, InstalledPackages>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public ChocolateySourcesViewModel Sources
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ChocolateySourcesViewModel>();
            }
        }

        public ChocolateyPackagesViewModel Packages
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ChocolateyPackagesViewModel>();
            }
        }

        public ConsoleViewModel Console
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ConsoleViewModel>();
            }
        }

        public InstalledPackagesViewModel InstalledPackages
        {
            get
            {
                return ServiceLocator.Current.GetInstance<InstalledPackagesViewModel>();
            }
        }

        public AboutViewModel About
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AboutViewModel>();
            }
        }

        public InstallListViewModel InstallList
        {
            get
            {
                return ServiceLocator.Current.GetInstance<InstallListViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}