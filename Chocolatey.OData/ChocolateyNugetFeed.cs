namespace Chocolatey.OData
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Linq;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;
    using Chocolatey.OData.Nuget;

    public class ChocolateyNugetFeed : IChocolateyFeed
    {
        private readonly PackageContext  _feedClient;
        private DataServiceQueryContinuation<Package> _nextPage;
        private IList<ChocolateyPackageVersion> _packageCache;

        public ChocolateyNugetFeed(PackageContext feedClient, ChocolateySource source)
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
            if (this._nextPage == null)
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
            var query = this._feedClient.Packages;

            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<Package>)query.Execute());

            var packages = response.Select(ConvertToPackage);

            this.MergePackagesIntoCache(packages);

            this._nextPage = response.GetContinuation();

            return this._packageCache.GroupPackages();
        }

        public async Task<IEnumerable<ChocolateyPackage>> GetAllPackages()
        {
            var query = this._feedClient.Packages;

            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<Package>)query.Execute());

            var allPackages = response.Select(ConvertToPackage).ToList();

            var nextQuery = response.GetContinuation();

            return (await this.RetrievePackagesInternal(nextQuery, allPackages)).GroupPackages();
        }

        public async Task<IEnumerable<ChocolateyPackage>> SearchPackages(string criteria)
        {
            var query = this._feedClient.Packages.AddQueryOption("$filter", "substringof('" + criteria + "',Id) eq true");

            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<Package>)query.Execute());

            var allPackages = response.Select(ConvertToPackage).ToList();

            var nextQuery = response.GetContinuation();

            this.RaisePageOfPackagesLoaded(allPackages.GroupPackages());

            return (await this.RetrievePackagesInternal(nextQuery, allPackages)).GroupPackages();
        }

        private async Task<IEnumerable<ChocolateyPackageVersion>> RetrievePackagesInternal(DataServiceQueryContinuation<Package> query, IList<ChocolateyPackageVersion> currentPackages)
        {
            if (query == null)
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

            return await this.RetrievePackagesInternal(nextQuery, currentPackages);
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
            if (this.PageOfPackagesLoaded != null)
            {
                this.PageOfPackagesLoaded(loadedPackages);
            }
        }

        private static ChocolateyPackageVersion ConvertToPackage(Package package)
        {
            var convertedPackage = new ChocolateyPackageVersion
            {
                Author = package.Authors,
                Id = package.Id,
                Title = package.Title,
                Description = package.Description,
                Version = package.Version,
                ReleaseNotes = package.ReleaseNotes,
                DownloadCount = package.DownloadCount,
                ProjectLink =
                    string.IsNullOrEmpty(package.ProjectUrl)
                        ? null
                        : new Uri(package.ProjectUrl)
            };

            return convertedPackage;
        }
    }
}
