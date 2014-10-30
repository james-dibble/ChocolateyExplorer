namespace Chocolatey.OData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    using Chocolatey.DomainModel;
    using Chocolatey.OData.Service;
    using System.Data.Services.Client;
    using System.Threading;
    using Chocolatey.Manager;

    public class ChocolateyFeed : IChocolateyFeed
    {
        private readonly ChocolateyFeedClient _feedClient;
        private readonly IInstalledPackagesManager _installedPackages;
        private DataServiceQueryContinuation<FeedPackage> _nextPage;
        private IList<ChocolateyPackage> _packageCache;

        public ChocolateyFeed(ChocolateyFeedClient feedClient, ChocolateySource source, IInstalledPackagesManager installedPackages)
        {
            this._feedClient = feedClient;
            this._installedPackages = installedPackages;
            this.Source = source;

            this._packageCache = new List<ChocolateyPackage>();
        }

        public event Action<IEnumerable<ChocolateyPackage>> PageOfPackagesLoaded;

        public ChocolateySource Source { get; private set; }

        public bool IsAnotherPageAvailable
        {
            get { return this._nextPage != null; }
        }

        public async Task<IEnumerable<ChocolateyPackage>> LoadFirstPage()
        {
            var query = this._feedClient.Packages.Where(p => p.IsLatestVersion) as DataServiceQuery<FeedPackage>;

            var retrievePackagesTask = Task.Factory.FromAsync(query.BeginExecute, result => query.EndExecute(result), TaskCreationOptions.None);

            var installedPackages = await this._installedPackages.RetrieveInstalledPackages();

            var queryOperation = await retrievePackagesTask as QueryOperationResponse<FeedPackage>;

            var convertedPackages = queryOperation.Select(package => this.ConvertToPackage(package, installedPackages)).ToList();

            this.MergePackagesIntoCache(convertedPackages);

            this._nextPage = queryOperation.GetContinuation();

            return convertedPackages;
        }

        public async Task<IEnumerable<ChocolateyPackage>> GetNextPage()
        {
            if (this._nextPage == null)
            {
                throw new InvalidOperationException("No page to retireve packages from.");
            }

            var retrievePackagesTask = Task.Factory.FromAsync<DataServiceQueryContinuation<FeedPackage>, OperationResponse>(
                this._feedClient.BeginExecute,
                this._feedClient.EndExecute,
                this._nextPage,
                null);

            var installedPackages = await this._installedPackages.RetrieveInstalledPackages();

            var queryOperation = await retrievePackagesTask as QueryOperationResponse<FeedPackage>;

            var convertedPackages = queryOperation.Select(package => this.ConvertToPackage(package, installedPackages)).ToList();

            this.MergePackagesIntoCache(convertedPackages);

            this._nextPage = queryOperation.GetContinuation();

            return this._packageCache;
        }

        public async Task<IEnumerable<ChocolateyPackage>> SearchPackages(string criteria, CancellationToken cancellationToken)
        {
            var searchOptionTemplate = @"(substringof('{0}',tolower(Id)) eq true) or (substringof('{0}',tolower(Title)) eq true) or (substringof('{0}',tolower(Description)) eq true)";

            var query = this._feedClient.Packages.AddQueryOption("$filter", string.Format(searchOptionTemplate, criteria.ToLower()));

            return null;
        }

        public Task<IEnumerable<ChocolateyPackageVersion>> GetPackageVersions(ChocolateyPackage package)
        {
            throw new NotImplementedException();
        }


        private async Task<IEnumerable<ChocolateyPackageVersion>> RetrievePackagesInternal(DataServiceQueryContinuation<FeedPackage> query, IList<ChocolateyPackageVersion> currentPackages, CancellationToken cancellationToken)
        {
            if (query == null || cancellationToken.IsCancellationRequested)
            {
                return currentPackages;
            }

            var currentResponse = await Task.Factory.StartNew(() => this._feedClient.Execute(query));

            foreach (var feedPackage in currentResponse)
            {
                currentPackages.Add(ConvertToPackageVersion(feedPackage));
            }

            var nextQuery = currentResponse.GetContinuation();

            this.RaisePageOfPackagesLoaded(currentPackages.GroupPackages(await this._installedPackages.RetrieveInstalledPackages()));

            return await this.RetrievePackagesInternal(nextQuery, currentPackages, cancellationToken);
        }

        private void MergePackagesIntoCache(IEnumerable<ChocolateyPackage> newPackages)
        {
            foreach (var newPackage in newPackages)
            {
                this._packageCache.Add(newPackage);
            }
        }

        private void RaisePageOfPackagesLoaded(IEnumerable<ChocolateyPackage> loadedPackages)
        {
            if (this.PageOfPackagesLoaded != null)
            {
                this.PageOfPackagesLoaded(loadedPackages);
            }
        }

        private ChocolateyPackage ConvertToPackage(FeedPackage package, IEnumerable<ChocolateyPackageVersion> installedPackages)
        {
            var convertedPackage = new ChocolateyPackage
            {
                Id = package.Id,
                IconLink = string.IsNullOrEmpty(package.IconUrl) ? null : new Uri(package.IconUrl),
                Title = package.Title,
                IsInstalled = installedPackages.Any(p => p.Id == package.Id)
            };

            return convertedPackage;
        }

        private static ChocolateyPackageVersion ConvertToPackageVersion(FeedPackage package)
        {
            var convertedPackage = new ChocolateyPackageVersion
            {
                Author = package.Authors,
                ChocolateyLink = new Uri(package.GalleryDetailsUrl),
                Id = package.Id,
                Title = package.Title,
                Description = package.Description,
                Version = package.Version,
                ReleaseNotes = package.ReleaseNotes,
                DownloadCount = package.DownloadCount,
                IconLink = string.IsNullOrEmpty(package.IconUrl)
                        ? null
                        : new Uri(package.IconUrl),
                ProjectLink =
                    string.IsNullOrEmpty(package.ProjectUrl)
                        ? null
                        : new Uri(package.ProjectUrl)
            };

            return convertedPackage;
        }
    }
}