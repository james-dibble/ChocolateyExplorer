namespace ChocolateyExplorer.WPF.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Chocolatey.DomainModel;
    using Chocolatey.Manager;
    using Chocolatey.OData;
    using System.Windows;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    public class ChocolateyPackagesViewModel : ViewModelBase
    {
        private readonly ChocolateySourcesViewModel _sourcesViewModel;
        private readonly InstalledPackagesViewModel _installedPackagesViewModel;
        private readonly ConsoleViewModel _consoleViewModel;
        private readonly IChocolateyFeedFactory _feedFactory;
        private readonly IChocolateyInstaller _installer;

        private IChocolateyFeed _feed;

        private bool _isWorking;
        private bool _canSelectPackage;
        private string _statusMessage;
        private string _searchTerm;
        private ChocolateyPackageVersion _selectedPackage;
        private bool _hasSearchResults;

        public ChocolateyPackagesViewModel(
            IChocolateyFeedFactory feedFactory,
            ChocolateySourcesViewModel sourcesViewModel,
            ConsoleViewModel consoleViewModel,
            IChocolateyInstaller installer,
            InstalledPackagesViewModel installedPackagesViewModel)
        {
            this._sourcesViewModel = sourcesViewModel;
            this._feedFactory = feedFactory;
            this._consoleViewModel = consoleViewModel;
            this._installer = installer;
            this._installedPackagesViewModel = installedPackagesViewModel;

            this._sourcesViewModel.SelectedSourceChanged += async newSource => await this.HandleSelectedSourceChanged(newSource);

            this.Packages = new ObservableCollection<ChocolateyPackage>();
            this.InstallPackageCommand = new RelayCommand<ChocolateyPackageVersion>(
                async package => await this.InstallPackage(package),
                package => package != null && !this.IsWorking);
            this.SearchPackagesCommand = new RelayCommand(
                async () => await this.SearchPackages(), 
                () => this._feed != null && !this.IsWorking && !string.IsNullOrEmpty(this.SearchTerm) && this.SearchTerm.Length > 2);
            this.SetSelectedPackageCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(
                package =>
                {
                    this.SelectedPackage = package.NewValue as ChocolateyPackageVersion;
                });
            this.LoadAllPackagesCommand = new RelayCommand(async () => await this.LoadPackages(), () => this._feed != null && !this.IsWorking);
            this.LoadMorePackagesCommand = new RelayCommand(
                async () => await this.LoadMorePackages(), 
                () => this._feed != null && this._feed.IsAnotherPageAvailable && !this.IsWorking && !this.HasSearchResults);
            this.ClearSearchResultsCommand = new RelayCommand(async () => await this.ClearSearchResults());

            this.IsWorking = false;
            this.CanSelectPackage = false;
            this.StatusMessage = "Ready";
        }

        public ObservableCollection<ChocolateyPackage> Packages { get; private set; }

        public RelayCommand<ChocolateyPackageVersion> InstallPackageCommand { get; private set; }

        public RelayCommand SearchPackagesCommand { get; private set; }

        public ICommand SetSelectedPackageCommand { get; private set; }

        public RelayCommand LoadAllPackagesCommand { get; private set; }

        public RelayCommand LoadMorePackagesCommand { get; private set; }

        public ICommand ClearSearchResultsCommand { get; private set; }

        public bool IsWorking
        {
            get
            {
                return this._isWorking;
            }
            set
            {
                this._isWorking = value;

                if (value)
                {
                    this.CanSelectPackage = false;
                }

                this.RaisePropertyChanged(() => this.IsWorking);
                this.LoadMorePackagesCommand.RaiseCanExecuteChanged();
                this.SearchPackagesCommand.RaiseCanExecuteChanged();
                this.RaisePropertyChanged(() => this.CanSearchPackages);
                this.InstallPackageCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanSelectPackage
        {
            get
            {
                return this._canSelectPackage;
            }
            set
            {
                this._canSelectPackage = value;

                this.RaisePropertyChanged(() => this.CanSelectPackage);
            }
        }

        public bool CanSearchPackages
        {
            get
            {
                return this._feed != null && !this.IsWorking;
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

        public string SearchTerm
        {
            get
            {
                return this._searchTerm;
            }
            set
            {
                this._searchTerm = value;

                this.RaisePropertyChanged(() => this.SearchTerm);
            }
        }

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

        public bool HasSearchResults
        {
            get
            {
                return this._hasSearchResults;
            }
            set
            {
                this._hasSearchResults = value;

                this.RaisePropertyChanged(() => this.HasSearchResults);
                this.LoadMorePackagesCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task InstallPackage(ChocolateyPackageVersion package)
        {
            this.IsWorking = true;
            this.StatusMessage = "Installing";

            this._consoleViewModel.AddConsoleLine(
                    "Installing package {0} from {1}",
                    package.Id,
                    this._feed.Source.Location);

            try
            {
                this._installer.OutputReceived += OutputReceived;

                await this._installer.Install(package);

                this._installer.OutputReceived -= OutputReceived;
            }
            catch (Exception ex)
            {
                this._consoleViewModel.AddConsoleLine(
                    "Failed to install package {0} from {1}",
                    package.Id,
                    this._feed.Source.Location);
                this._consoleViewModel.AddConsoleLine(ex.Message);
            }

            this.IsWorking = false;
            this.CanSelectPackage = true;
            this.StatusMessage = "Ready";

            await this._installedPackagesViewModel.RefreshPackages();
        }

        private async Task HandleSelectedSourceChanged(ChocolateySource source)
        {
            this.Packages.Clear();
            this.HasSearchResults = false;
            this.SearchTerm = string.Empty;
            
            this._feed = null;

            if (source != null)
            {
                this._feed = this._feedFactory.Create(source);
                await this.LoadPackages();
            }
            
            this.LoadMorePackagesCommand.RaiseCanExecuteChanged();
            this.SearchPackagesCommand.RaiseCanExecuteChanged();
            this.RaisePropertyChanged(() => this.CanSearchPackages);
        }

        private async Task SearchPackages()
        {
            this.IsWorking = true;
            this.StatusMessage = "Searching";
            this._consoleViewModel.AddConsoleLine(
                @"Searching for packages with criteria ""{0}"" from {1}",
                this.SearchTerm,
                this._feed.Source.Location);

            this.Packages.Clear();
            this.HasSearchResults = true;

            try
            {
                this._feed.PageOfPackagesLoaded += this.OnPageOfPackagesLoaded;

                var packages = await this._feed.SearchPackages(this.SearchTerm);

                this.Packages = new ObservableCollection<ChocolateyPackage>(packages);

                this._consoleViewModel.AddConsoleLine("Packages loaded.");
            }
            catch (Exception ex)
            {
                this._consoleViewModel.AddConsoleLine("Failed to serach for packages from {0}", this._feed.Source.Location);
                this._consoleViewModel.AddConsoleLine(ex.Message);
            }
            finally
            {
                this._feed.PageOfPackagesLoaded += this.OnPageOfPackagesLoaded;
            }

            this.RaisePropertyChanged(() => this.Packages);

            this.IsWorking = false;
            this.CanSelectPackage = true;
            this.StatusMessage = "Ready";
        }

        private async Task LoadPackages()
        {
            this.IsWorking = true;
            this.StatusMessage = "Loading";
            this._consoleViewModel.AddConsoleLine("Loading packages from {0}", this._feed.Source.Location);

            this.Packages.Clear();

            try
            {
                var packages = await this._feed.LoadFirstPage();

                this.Packages = new ObservableCollection<ChocolateyPackage>(packages);

                this._consoleViewModel.AddConsoleLine("Packages loaded.");
            }
            catch (Exception ex)
            {
                this._consoleViewModel.AddConsoleLine("Failed to load packages from {0}", this._feed.Source.Location);
                this._consoleViewModel.AddConsoleLine(ex.Message);
            }

            this.RaisePropertyChanged(() => this.Packages);

            this.IsWorking = false;
            this.CanSelectPackage = true;
            this.StatusMessage = "Ready";
        }

        private async Task LoadMorePackages()
        {
            this.IsWorking = true;
            this.StatusMessage = "Loading";
            this._consoleViewModel.AddConsoleLine("Loading packages from {0}", this._feed.Source.Location);

            this.Packages.Clear();

            try
            {
                var packages = await this._feed.GetNextPage();

                this.Packages = new ObservableCollection<ChocolateyPackage>(packages);

                this._consoleViewModel.AddConsoleLine("Packages loaded.");
            }
            catch (Exception ex)
            {
                this._consoleViewModel.AddConsoleLine("Failed to load packages from {0}", this._feed.Source.Location);
                this._consoleViewModel.AddConsoleLine(ex.Message);
            }

            this.RaisePropertyChanged(() => this.Packages);

            this.IsWorking = false;
            this.CanSelectPackage = true;
            this.StatusMessage = "Ready";
        }

        private async Task ClearSearchResults()
        {
            await this.LoadPackages();

            this.HasSearchResults = false;
            this.SearchTerm = string.Empty;
        }

        private void OnPageOfPackagesLoaded(IEnumerable<ChocolateyPackage> packages)
        {
            this.Packages = new ObservableCollection<ChocolateyPackage>(packages);

            this.RaisePropertyChanged(() => this.Packages);
        }
                
        private void OutputReceived(string obj)
        {
            this._consoleViewModel.AddConsoleLine(obj);
        }
    }
}
