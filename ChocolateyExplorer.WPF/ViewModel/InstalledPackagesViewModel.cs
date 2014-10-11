namespace ChocolateyExplorer.WPF.ViewModel
{
    using System.Collections.ObjectModel;
    using Chocolatey.DomainModel;
    using GalaSoft.MvvmLight;

    public class InstalledPackagesViewModel : ViewModelBase
    {
        public InstalledPackagesViewModel()
        {
            this.InstalledPackages = new ObservableCollection<ChocolateyPackageVersion>();
        }

        public ObservableCollection<ChocolateyPackageVersion> InstalledPackages { get; private set; }
    }
}