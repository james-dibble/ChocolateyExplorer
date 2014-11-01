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

            var retrievePackagesTask = this.ExecuteQuery(query);

            var installedPackages = await this._installedPackages.RetrieveInstalledPackages();

            var queryResult = await retrievePackagesTask as QueryOperationResponse<FeedPackage>;

            var convertedPackages = queryResult.Select(package => this.ConvertToPackage(package, installedPackages)).ToList();

            this.MergePackagesIntoCache(convertedPackages);

            this._nextPage = queryResult.GetContinuation();

            return convertedPackages;
        }

        public async Task<IEnumerable<ChocolateyPackage>> GetNextPage()
        {
            if (this._nextPage == null)
            {
                throw new InvalidOperationException("No page to retireve packages from.");
            }

            var retrievePackagesTask = Task.Factory.FromAsync<IEnumerable<FeedPackage>>(
                this._feedClient.BeginExecute<FeedPackage>(this._nextPage, null, null),
                queryAsyncResult => this._feedClient.EndExecute<FeedPackage>(queryAsyncResult));

            var installedPackages = await this._installedPackages.RetrieveInstalledPackages();

            var queryOperation = await retrievePackagesTask as QueryOperationResponse<FeedPackage>;

            var convertedPackages = queryOperation.Select(package => this.ConvertToPackage(package, installedPackages)).ToList();

            this.MergePackagesIntoCache(convertedPackages);

            this._nextPage = queryOperation.GetContinuation();

            return this._packageCache;
        }

        public async Task<IEnumerable<ChocolateyPackage>> SearchPackages(string criteria, CancellationToken cancellationToken)
        {
            var searchOptionTemplate = @"IsLatestVersion and ((substringof('{0}',tolower(Id)) eq true) or (substringof('{0}',tolower(Title)) eq true) or (substringof('{0}',tolower(Description)) eq true))";

            var query = this._feedClient.Packages.AddQueryOption("$filter", string.Format(searchOptionTemplate, criteria.ToLower())) as DataServiceQuery<FeedPackage>;

            var queryResultTask = this.ExecuteQuery(query);

            var installedPackages = await this._installedPackages.RetrieveInstalledPackages();

            var queryResult = await queryResultTask;

            var convertedPackages = queryResult.Select(package => this.ConvertToPackage(package, installedPackages)).ToList();

            this.RaisePageOfPackagesLoaded(convertedPackages);

            var nextQuery = queryResult.GetContinuation();

            return await this.RetrievePackagesInternal(nextQuery, convertedPackages, cancellationToken);
        }

        public async Task<IEnumerable<ChocolateyPackageVersion>> GetPackageVersions(ChocolateyPackage package)
        {
            var query = this._feedClient.Packages.Where(p => p.Id == package.Id) as DataServiceQuery<FeedPackage>;

            var queryResultTask = this.ExecuteQuery(query);

            var installedPackages = await this._installedPackages.RetrieveInstalledPackages();

            var queryResult = await queryResultTask;

            var convertedPackages = queryResult.Select(p => ConvertToPackageVersion(p)).ToList();
            
            var nextQuery = queryResult.GetContinuation();

            return await this.RetrievePackagesInternal(nextQuery, convertedPackages);
        }
        
        private async Task<IEnumerable<ChocolateyPackage>> RetrievePackagesInternal(
            DataServiceQueryContinuation<FeedPackage> query,
            IList<ChocolateyPackage> currentPackages,
            CancellationToken cancellationToken)
        {
            if (query == null || cancellationToken.IsCancellationRequested)
            {
                return currentPackages;
            }

            var retrievePackagesTask = new TaskFactory(cancellationToken).FromAsync<IEnumerable<FeedPackage>>(
                this._feedClient.BeginExecute<FeedPackage>(this._nextPage, null, null),
                queryAsyncResult => this._feedClient.EndExecute<FeedPackage>(queryAsyncResult));

            if (cancellationToken.IsCancellationRequested)
            {
                return currentPackages;
            }

            var installedPackages = await this._installedPackages.RetrieveInstalledPackages();

            var queryOperation = await retrievePackagesTask as QueryOperationResponse<FeedPackage>;

            foreach (var feedPackage in queryOperation)
            {
                currentPackages.Add(ConvertToPackage(feedPackage, installedPackages));
            }

            this.RaisePageOfPackagesLoaded(currentPackages);

            var nextQuery = queryOperation.GetContinuation();

            return await this.RetrievePackagesInternal(nextQuery, currentPackages, cancellationToken);
        }

        private async Task<IEnumerable<ChocolateyPackageVersion>> RetrievePackagesInternal(
            DataServiceQueryContinuation<FeedPackage> query,
            IList<ChocolateyPackageVersion> currentPackages)
        {
            if (query == null)
            {
                return currentPackages;
            }

            var retrievePackagesTask = Task.Factory.FromAsync<IEnumerable<FeedPackage>>(
                this._feedClient.BeginExecute<FeedPackage>(this._nextPage, null, null),
                queryAsyncResult => this._feedClient.EndExecute<FeedPackage>(queryAsyncResult));
            
            var installedPackages = await this._installedPackages.RetrieveInstalledPackages();

            var queryOperation = await retrievePackagesTask as QueryOperationResponse<FeedPackage>;

            foreach (var feedPackage in queryOperation)
            {
                currentPackages.Add(ConvertToPackageVersion(feedPackage));
            }

            var nextQuery = queryOperation.GetContinuation();

            return await this.RetrievePackagesInternal(nextQuery, currentPackages);
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

        private async Task<QueryOperationResponse<FeedPackage>> ExecuteQuery(DataServiceQuery<FeedPackage> query)
        {
            var retrievePackagesTask = Task.Factory.FromAsync(query.BeginExecute, result => query.EndExecute(result), TaskCreationOptions.None);

            var queryOperation = await retrievePackagesTask as QueryOperationResponse<FeedPackage>;

            return queryOperation;
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