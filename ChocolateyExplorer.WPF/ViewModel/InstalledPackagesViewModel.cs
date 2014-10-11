namespace ChocolateyExplorer.WPF.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;
    using Chocolatey.Manager;
    using GalaSoft.MvvmLight;

    public class InstalledPackagesViewModel : ViewModelBase
    {
        private readonly IInstalledPackagesManager _packagesManager;

        private bool _isLoadingPackages;
        private string _statusMessage;

        public InstalledPackagesViewModel(IInstalledPackagesManager packagesManager)
        {
            this._packagesManager = packagesManager;

            this.IsLoadingPackages = false;
            this.StatusMessage = "Ready";

            this.InstalledPackages = new ObservableCollection<ChocolateyPackageVersion>();
        }

        public ObservableCollection<ChocolateyPackageVersion> InstalledPackages { get; private set; }

        public bool IsLoadingPackages
        {
            get
            {
                return this._isLoadingPackages;
            }
            set
            {
                this._isLoadingPackages = value;

                this.StatusMessage = "Loading";

                this.RaisePropertyChanged(() => this.IsLoadingPackages);
            }
        }

        public string StatusMessage
        {
            get
            {
                return this._statusMessage;
            }
            set
            {
                this._statusMessage = value;

                this.RaisePropertyChanged(() => this.StatusMessage);
            }
        }

        public async Task RefreshPackages()
        {
            this.IsLoadingPackages = true;

            var installedPackages = await this._packagesManager.RetrieveInstalledPackages();

            this.InstalledPackages.Clear();

            foreach (var package in installedPackages)
            {
                this.InstalledPackages.Add(package);
            }

            this.IsLoadingPackages = false;
            this.StatusMessage = "Ready";
        }
    }
}