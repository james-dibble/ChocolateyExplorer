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

        public ChocolateySourcesViewModel(ISourcesManager sourcesManager, IChocolateyFeedFactory feedFactory)
        {
            this._sourcesManager = sourcesManager;
            this._feedFactory = feedFactory;

            var sources = this._sourcesManager.GetSources();

            this.Sources = new ObservableCollection<ChocolateySource>(sources);
            this.AddSourceCommand = new RelayCommand(this.AddSource, this.ValidateNewSource);
            this.RemoveSourceCommand = new RelayCommand<ChocolateySource>(this.RemoveSource);
        }

        public event Action<ChocolateySource> SelectedSourceChanged;

        public ObservableCollection<ChocolateySource> Sources { get; private set; }

        public ICommand AddSourceCommand { get; private set; }

        public ICommand RemoveSourceCommand { get; private set; }

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

                if(value != null)
                {
                    this.NewSourceId = value.Name;
                    this.NewSourceLocation = value.Location;
                    this.IsNewSourceNuget = value.IsNugetFeed;
                }

                if (this.SelectedSourceChanged != null)
                {
                    this.SelectedSourceChanged(value);
                }
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

            this.Sources.Remove(source);
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