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
        private readonly PackageContext _feedClient;

        public ChocolateyNugetFeed(PackageContext feedClient, ChocolateySource source)
        {
            this._feedClient = feedClient;
            this.Source = source;
        }
        
        public event Action<IEnumerable<ChocolateyPackage>> PageOfPackagesLoaded;

        public ChocolateySource Source
        {
            get; private set;
        }

        public async Task<IEnumerable<ChocolateyPackage>> GetAllPackages()
        {
            var query = this._feedClient.Packages;

            var allPackages = new List<Package>();

            var response = await Task.Factory.StartNew(() => (QueryOperationResponse<Package>)query.Execute());

            foreach (var package in response)
            {
                allPackages.Add(package);
            }

            var nextQuery = response.GetContinuation();

            while (nextQuery != null)
            {
                var currentResponse = await Task.Factory.StartNew(() => (QueryOperationResponse<Package>)this._feedClient.Execute(nextQuery));

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

        public async Task<IEnumerable<ChocolateyPackage>> SearchPackages(string criteria)
        {
            var results = (await Task.Factory.StartNew(() => this._feedClient.Packages.Where(p => p.Id.Contains(criteria)).ToList()));

            return ConvertToPackages(results);
        }

        private static IEnumerable<ChocolateyPackage> ConvertToPackages(IEnumerable<Package> results)
        {
            var allPackages = results.ToList();

            var packages = results.OrderBy(p => p.Id)
                                  .ThenByDescending(p => p.Version)
                                  .DistinctBy(p => p.Id)
                                  .Select(p => new ChocolateyPackage
                                  {
                                      Id = p.Id,
                                      Title = p.Title,
                                      Versions = allPackages.Where(pv => pv.Id == p.Id)
                                                            .OrderByDescending(pv => pv.Version)
                                                            .Select(pv => new ChocolateyPackageVersion
                                                            {
                                                                Author = pv.Authors,
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
