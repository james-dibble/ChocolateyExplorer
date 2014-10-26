namespace ChocolateyExplorer.WPF.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Chocolatey.DomainModel;
    using Chocolatey.Manager;
    using Chocolatey.OData;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    public class ChocolateySourcesViewModel : ViewModelBase
    {
        private readonly ISourcesManager _sourcesManager;
        private readonly IChocolateyFeedFactory _feedFactory;

        private ChocolateySource _selectedChocolateySource;
        private string _newSourceId;
        private Uri _newSourceLocation;
        private bool _isNewSourceNuget;
        private bool _canEditSource;

        public ChocolateySourcesViewModel(ISourcesManager sourcesManager, IChocolateyFeedFactory feedFactory)
        {
            this._sourcesManager = sourcesManager;
            this._feedFactory = feedFactory;
                        
            this.Sources = new ObservableCollection<ChocolateySource>();
            this.AddSourceCommand = new RelayCommand(this.AddSource, this.ValidateNewSource);
            this.RemoveSourceCommand = new RelayCommand<ChocolateySource>(this.RemoveSource, source => source != null);
            this.AddNewSourceCommand = new RelayCommand(() => this.SelectedChocolateySource = null);
        }

        public event Action<ChocolateySource> SelectedSourceChanged;

        public ObservableCollection<ChocolateySource> Sources { get; private set; }

        public ICommand AddSourceCommand { get; private set; }

        public ICommand RemoveSourceCommand { get; private set; }

        public ICommand AddNewSourceCommand { get; private set; }

        public string NewSourceId
        {
            get
            {
                return this._newSourceId;
            }
            set
            {
                this._newSourceId = value;

                this.RaisePropertyChanged(() => this.NewSourceId);
            }
        }

        public Uri NewSourceLocation
        {
            get
            {
                return this._newSourceLocation;
            }
            set
            {
                this._newSourceLocation = value;

                this.RaisePropertyChanged(() => this.NewSourceLocation);
            }
        }

        public bool IsNewSourceNuget
        {
            get
            {
                return this._isNewSourceNuget;
            }
            set
            {
                this._isNewSourceNuget = value;

                this.RaisePropertyChanged(() => this.IsNewSourceNuget);
            }
        }

        public bool IsAddingSource
        {
            get
            {
                return !this._canEditSource;
            }
        }

        public bool CanEditSource
        {
            get
            {
                return this._canEditSource;
            }
            set
            {
                this._canEditSource = value;

                this.RaisePropertyChanged(() => this.CanEditSource);
                this.RaisePropertyChanged(() => this.IsAddingSource);
            }
        }

        public ChocolateySource SelectedChocolateySource
        {
            get
            {
                return this._selectedChocolateySource;
            }
            set
            {
                this._selectedChocolateySource = value;

                this.RaisePropertyChanged(() => this.SelectedChocolateySource);

                if (value != null)
                {
                    this.NewSourceId = value.Name;
                    this.NewSourceLocation = value.Location;
                    this.IsNewSourceNuget = value.IsNugetFeed;
                    this.CanEditSource = true;
                }
                else
                {
                    this.NewSourceId = null;
                    this.NewSourceLocation = null;
                    this.IsNewSourceNuget = false;
                    this.CanEditSource = false;
                }

                if (this.SelectedSourceChanged != null)
                {
                    this.SelectedSourceChanged(value);
                }
            }
        }

        public void PopulateSources()
        {
            var sources = this._sourcesManager.GetSources();

            this.Sources = new ObservableCollection<ChocolateySource>(sources);

            this.RaisePropertyChanged(() => this.Sources);

            if(this.Sources.Any())
            {
                this.SelectedChocolateySource = this.Sources.First();
            }
        }

        private void AddSource()
        {
            var newSource = new ChocolateySource
            {
                Name = this.NewSourceId,
                Location = this.NewSourceLocation,
                IsNugetFeed = this.IsNewSourceNuget
            };

            this.RemoveSource(newSource);

            this._sourcesManager.AddSource(newSource);

            this.Sources.Add(newSource);

            this.NewSourceId = string.Empty;
            this.NewSourceLocation = null;
            this.IsNewSourceNuget = false;
        }

        private void RemoveSource(ChocolateySource source)
        {
            if (this.SelectedChocolateySource == source)
            {
                if (this.SelectedSourceChanged != null)
                {
                    this.SelectedSourceChanged(null);
                }
            }

            this._sourcesManager.RemoveSource(source);

            this.Sources = new ObservableCollection<ChocolateySource>(this.Sources.Where(s => s.Name != source.Name));
            this.RaisePropertyChanged(() => this.Sources);
        }

        private bool ValidateNewSource()
        {
            if (string.IsNullOrEmpty(this.NewSourceId))
            {
                return false;
            }

            if (this.NewSourceLocation == null)
            {
                return false;
            }

            ////if(!this._feedFactory.ValidateSource(this.NewSourceLocation).Result)
            ////{
            ////    return false;
            ////}

            return true;
        }
    }
}