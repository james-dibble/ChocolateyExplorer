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

    public class ChocolateyFeed : IChocolateyFeed
    {
        private readonly ChocolateyFeedClient _feedClient;
        private DataServiceQueryContinuation<FeedPackage> _nextPage;
        private IList<ChocolateyPackageVersion> _packageCache;

        public ChocolateyFeed(ChocolateyFeedClient feedClient, ChocolateySource source)
        {
            this._feedClient = feedClient;
            this.Source = source;

            this._packageCache = new List<ChocolateyPackageVersion>();
        }

        public event Action<IEnumerable<ChocolateyPackage>> PageOfPackagesLoaded;

        public ChocolateySource Source { get; private set; }

        public bool IsAnotherPageAvailable
        {
            get { return this._nextPage != null; }
        }

        public async Task<IEnumerable<ChocolateyPackage>> GetNextPage()
        {
            if(this._nextPage == null)
            {
                throw new InvalidOperationException("Tried to retrieve a page from a query that was complete.");
            }

            var response = await Task.Factory.StartNew(() => this._feedClient.Execute(this._nextPage));

            var packages = response.Select(ConvertToPackage);

            this.MergePackagesIntoCache(packages);
            
            this._nextPage = response.GetContinuation();

            return this._packageCache.GroupPackages();
        }

        public async Task<IEnumerable<ChocolateyPackage>> LoadFirstPage()
        {
            if(this._packageCache.Any())
            {
                return this._packageCache.GroupPackages();
            }

            var query = this._feedClient.Packages;
            
            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<FeedPackage>)query.Execute());

            var packages = response.Select(ConvertToPackage);

            this.MergePackagesIntoCache(packages);
            
            this._nextPage = response.GetContinuation();
            
            return this._packageCache.GroupPackages();
        }

        public async Task<IEnumerable<ChocolateyPackage>> SearchPackages(string criteria, CancellationToken cancellationToken)
        {
            var searchOptionTemplate = @"(substringof('{0}',tolower(Id)) eq true) or (substringof('{0}',tolower(Title)) eq true) or (substringof('{0}',tolower(Description)) eq true)";

            var query = this._feedClient.Packages.AddQueryOption("$filter", string.Format(searchOptionTemplate, criteria.ToLower()));

            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<FeedPackage>)query.Execute(), cancellationToken);

            if(cancellationToken.IsCancellationRequested)
            {
                return new List<ChocolateyPackage>();
            }

            var allPackages = response.Select(ConvertToPackage).ToList();
            
            var nextQuery = response.GetContinuation();

            this.RaisePageOfPackagesLoaded(allPackages.GroupPackages());

            return (await this.RetrievePackagesInternal(nextQuery, allPackages, cancellationToken)).GroupPackages();
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
                currentPackages.Add(ConvertToPackage(feedPackage));
            }

            var nextQuery = currentResponse.GetContinuation();

            this.RaisePageOfPackagesLoaded(currentPackages.GroupPackages());

            return await this.RetrievePackagesInternal(nextQuery, currentPackages, cancellationToken);
        }

        private void MergePackagesIntoCache(IEnumerable<ChocolateyPackageVersion> newPackages)
        {
            foreach (var newPackage in newPackages)
            {
                this._packageCache.Add(newPackage);
            }
        }

        private void RaisePageOfPackagesLoaded(IEnumerable<ChocolateyPackage> loadedPackages)
        {
            if(this.PageOfPackagesLoaded != null)
            {
                this.PageOfPackagesLoaded(loadedPackages);
            }
        }
        
        private static ChocolateyPackageVersion ConvertToPackage(FeedPackage package)
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