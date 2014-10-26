namespace ChocolateyExplorer.WPF.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Chocolatey.DomainModel;
    using Chocolatey.Manager;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    public class InstallListViewModel : ViewModelBase
    {
        private readonly IChocolateyInstaller _installer;
        private readonly ConsoleViewModel _console;
        private readonly InstalledPackagesViewModel _installedPackages;
        private ChocolateyPackageVersion _selectedPackage;
        private bool _isWorking;
        private string _statusMessage;

        public InstallListViewModel(IChocolateyInstaller installer, ConsoleViewModel console, InstalledPackagesViewModel installedPackages)
        {
            this._installer = installer;
            this._console = console;
            this._installedPackages = installedPackages;

            this.Packages = new ObservableCollection<ChocolateyPackageVersion>();
            this.IsWorking = false;
            this.StatusMessage = "Ready";

            this.AddPackageToInstallListCommand = new RelayCommand<ChocolateyPackageVersion>(this.AddPackageToInstallList);
            this.RemovePackageFromInstallListCommand = 
                new RelayCommand<ChocolateyPackageVersion>(this.RemovePackageToInstallList, package => package != null);
            this.ClearInstallListCommand = new RelayCommand(() => this.Packages.Clear(), () => this.Packages.Any() && !this.IsWorking);
            this.InstallPackagesCommand = new RelayCommand(async () => await this.InstallPackages(), () => this.Packages.Any() && !this.IsWorking);
        }

        public ObservableCollection<ChocolateyPackageVersion> Packages { get; private set; }

        public ICommand AddPackageToInstallListCommand { get; private set; }

        public RelayCommand<ChocolateyPackageVersion> RemovePackageFromInstallListCommand { get; private set; }

        public RelayCommand ClearInstallListCommand { get; private set; }

        public RelayCommand InstallPackagesCommand { get; private set; }

        public ChocolateyPackageVersion SelectedPackage
        {
            get
            {
                return this._selectedPackage;
            }
            set
            {
                this._selectedPackage = value;

                this.RaisePropertyChanged(() => this.SelectedPackage);
            }
        }

        public bool IsWorking
        {
            get
            {
                return this._isWorking;
            }
            set
            {
                this._isWorking = value;

                this.RaisePropertyChanged(() => this.IsWorking);
                this.RaisePropertyChanged(() => this.CanSelectPackage);
            }
        }

        public bool CanSelectPackage
        {
            get
            {
                return !this.IsWorking;
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

        public void AddPackageToInstallList(ChocolateyPackageVersion package)
        {
            this.Packages.Add(package);

            this.ClearInstallListCommand.RaiseCanExecuteChanged();
            this.InstallPackagesCommand.RaiseCanExecuteChanged();
        }

        public void RemovePackageToInstallList(ChocolateyPackageVersion package)
        {
            this.Packages.Remove(package);

            this.ClearInstallListCommand.RaiseCanExecuteChanged();
            this.InstallPackagesCommand.RaiseCanExecuteChanged();
        }

        public async Task InstallPackages()
        {
            this.StatusMessage = "Installing Packages";
            this.IsWorking = true;

            this._console.AddConsoleLine("Installing {0} packages from list", this.Packages.Count);

            this._installer.OutputReceived += this.OutputReceived;

            foreach (var package in this.Packages)
            {
                this._console.AddConsoleLine("Installing package {0}", package.Id);

                await this._installer.Install(package);

                this._console.AddConsoleLine("Package {0} installed", package.Id);
            }

            this._installer.OutputReceived -= this.OutputReceived;

            this._console.AddConsoleLine("Installed all packages from list");

            this.Packages.Clear();
            this.SelectedPackage = null;

            this.ClearInstallListCommand.RaiseCanExecuteChanged();
            this.InstallPackagesCommand.RaiseCanExecuteChanged();

            await this._installedPackages.RefreshPackages();

            this.StatusMessage = "Ready";
            this.IsWorking = false;
        }

        private void OutputReceived(string obj)
        {
            this._console.AddConsoleLine(obj);
        }
    }
}