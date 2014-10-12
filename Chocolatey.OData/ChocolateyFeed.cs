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

            var allPackages = new List<FeedPackage>();
            
            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<FeedPackage>)query.Execute());

            foreach (var package in response)
            {
                allPackages.Add(package);
            }

            var nextQuery = response.GetContinuation();

            while (nextQuery != null)
            {
                var currentResponse = await Task.Factory.StartNew(() => (QueryOperationResponse<FeedPackage>)this._feedClient.Execute(nextQuery));

                foreach (var package in currentResponse)
                {
                    allPackages.Add(package);
                }

                if(this.PageOfPackagesLoaded != null)
                {
                    this.PageOfPackagesLoaded(ConvertToPackages(allPackages));
                }
                
                nextQuery = currentResponse.GetContinuation();
            }

            return ConvertToPackages(allPackages);
        }

        public async Task<IEnumerable<ChocolateyPackage>> SearchPackages(string criteria)
        {
            var query = this._feedClient.Packages.AddQueryOption("$filter", "substringof('" + criteria + "',Id) eq true");

            var allPackages = new List<FeedPackage>();

            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<FeedPackage>)query.Execute());

            foreach (var package in response)
            {
                allPackages.Add(package);
            }

            var nextQuery = response.GetContinuation();

            while (nextQuery != null)
            {
                var currentResponse = await Task.Factory.StartNew(() => (QueryOperationResponse<FeedPackage>)this._feedClient.Execute(nextQuery));

                foreach (var package in currentResponse)
                {
                    allPackages.Add(package);
                }

                if (this.PageOfPackagesLoaded != null)
                {
                    this.PageOfPackagesLoaded(ConvertToPackages(allPackages));
                }

                nextQuery = currentResponse.GetContinuation();
            }

            return ConvertToPackages(allPackages);
        }

        private static IEnumerable<ChocolateyPackage> ConvertToPackages(IEnumerable<FeedPackage> results)
        {
            var allPackages = results.ToList();

            var packages = results.OrderBy(p => p.Id)
                                  .ThenByDescending(p => p.Version)
                                  .DistinctBy(p => p.Id)
                                  .AsParallel()
                                  .Select(p => new ChocolateyPackage
                                  {
                                      Id = p.Id,
                                      Title = p.Title,
                                      Versions = allPackages.Where(pv => pv.Id == p.Id)
                                                            .OrderByDescending(pv => pv.Version)
                                                            .Select(pv => new ChocolateyPackageVersion 
                                                            {
                                                                Author = pv.Authors,
                                                                ChocolateyLink = new Uri(pv.GalleryDetailsUrl),
                                                                Id = p.Id,
                                                                Title = pv.Title,
                                                                Description = pv.Description,
                                                                Version = pv.Version,
                                                                ReleaseNotes = pv.ReleaseNotes,
                                                                DownloadCount = pv.DownloadCount,
                                                                ProjectLink = string.IsNullOrEmpty(pv.ProjectUrl) ? null : new Uri(pv.ProjectUrl)
                                                            })
                                  });

            return packages;
        }
    }
}