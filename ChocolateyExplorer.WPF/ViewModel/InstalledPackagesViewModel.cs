namespace ChocolateyExplorer.WPF.ViewModel
{
    using System.Collections.ObjectModel;
using Chocolatey.DomainModel;
using Chocolatey.Manager;
using GalaSoft.MvvmLight;

    public class InstalledPackagesViewModel : ViewModelBase
    {
        private readonly IInstalledPackagesManager _packagesManager;

        public InstalledPackagesViewModel(IInstalledPackagesManager packagesManager)
        {
            this._packagesManager = packagesManager;

            this.InstalledPackages = new ObservableCollection<ChocolateyPackageVersion>(this._packagesManager.RetrieveInstalledPackages());
        }

        public ObservableCollection<ChocolateyPackageVersion> InstalledPackages { get; private set; }
    }
}