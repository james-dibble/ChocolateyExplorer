namespace Chocolatey.OData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    using Chocolatey.DomainModel;
    using Chocolatey.OData.Service;
    using System.Data.Services.Client;

    public class ChocolateyFeed : IChocolateyFeed
    {
        private readonly ChocolateyFeedClient _feedClient;

        public ChocolateyFeed(ChocolateyFeedClient feedClient, ChocolateySource source)
        {
            this._feedClient = feedClient;
            this.Source = source;
        }

        public event Action<IEnumerable<ChocolateyPackage>> PageOfPackagesLoaded;

        public ChocolateySource Source { get; private set; }

        public async Task<IEnumerable<ChocolateyPackage>> GetAllPackages()
        {
            var query = this._feedClient.Packages;
            
            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<FeedPackage>)query.Execute());

            var allPackages = response.Select(ConvertToPackage).ToList();

            var nextQuery = response.GetContinuation();

            return (await this.RetrievePackagesInternal(nextQuery, allPackages)).GroupPackages();
        }

        public async Task<IEnumerable<ChocolateyPackage>> SearchPackages(string criteria)
        {
            var query = this._feedClient.Packages.AddQueryOption("$filter", "substringof('" + criteria + "',Id) eq true");

            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<FeedPackage>)query.Execute());

            var allPackages = response.Select(ConvertToPackage).ToList();

            var nextQuery = response.GetContinuation();

            return (await this.RetrievePackagesInternal(nextQuery, allPackages)).GroupPackages();
        }

        private async Task<IEnumerable<ChocolateyPackageVersion>> RetrievePackagesInternal(DataServiceQueryContinuation<FeedPackage> query, IList<ChocolateyPackageVersion> currentPackages)
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

            return await this.RetrievePackagesInternal(nextQuery, currentPackages);
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
                ProjectLink =
                    string.IsNullOrEmpty(package.ProjectUrl)
                        ? null
                        : new Uri(package.ProjectUrl)
            };

            return convertedPackage;
        }
    }
}