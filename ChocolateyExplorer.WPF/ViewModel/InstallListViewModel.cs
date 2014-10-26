namespace ChocolateyExplorer.WPF.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;

    using Chocolatey.DomainModel;
    
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    public class InstallListViewModel : ViewModelBase
    {
        private ChocolateyPackageVersion _selectedPackage;

        public InstallListViewModel()
        {
            this.Packages = new ObservableCollection<ChocolateyPackageVersion>();

            this.AddPackageToInstallListCommand = new RelayCommand<ChocolateyPackageVersion>(this.AddPackageToInstallList);
            this.RemovePackageFromInstallListCommand = 
                new RelayCommand<ChocolateyPackageVersion>(this.RemovePackageToInstallList, package => package != null);
            this.ClearInstallListCommand = new RelayCommand(() => this.Packages.Clear(), () => this.Packages.Any());
        }

        public ObservableCollection<ChocolateyPackageVersion> Packages { get; private set; }

        public ICommand AddPackageToInstallListCommand { get; private set; }

        public RelayCommand<ChocolateyPackageVersion> RemovePackageFromInstallListCommand { get; private set; }

        public RelayCommand ClearInstallListCommand { get; private set; }

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

        public void AddPackageToInstallList(ChocolateyPackageVersion package)
        {
            this.Packages.Add(package);

            this.ClearInstallListCommand.RaiseCanExecuteChanged();
        }

        public void RemovePackageToInstallList(ChocolateyPackageVersion package)
        {
            this.Packages.Remove(package);

            this.ClearInstallListCommand.RaiseCanExecuteChanged();
        }
    }
}