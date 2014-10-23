namespace ChocolateyExplorer.WPF.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using Chocolatey.DomainModel;
    
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    public class InstallListViewModel : ViewModelBase
    {
        public InstallListViewModel()
        {
            this.Packages = new ObservableCollection<ChocolateyPackageVersion>();

            this.AddPackageToInstallListCommand = new RelayCommand<ChocolateyPackageVersion>(this.AddPackageToInstallList);
            this.RemovePackageFromInstallListCommand = new RelayCommand<ChocolateyPackageVersion>(this.RemovePackageToInstallList);
            this.ClearInstallListCommand = new RelayCommand(() => this.Packages.Clear());
        }

        public ObservableCollection<ChocolateyPackageVersion> Packages { get; private set; }

        public ICommand AddPackageToInstallListCommand { get; private set; }

        public ICommand RemovePackageFromInstallListCommand { get; private set; }

        public ICommand ClearInstallListCommand { get; private set; }

        public void AddPackageToInstallList(ChocolateyPackageVersion package)
        {
            this.Packages.Add(package);
        }

        public void RemovePackageToInstallList(ChocolateyPackageVersion package)
        {
            this.Packages.Remove(package);
        }
    }
}