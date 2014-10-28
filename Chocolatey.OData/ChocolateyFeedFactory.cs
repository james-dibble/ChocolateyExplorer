namespace Chocolatey.OData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;
    using Chocolatey.Manager;
    using Chocolatey.OData.Nuget;
    using Chocolatey.OData.Service;

    public class ChocolateyFeedFactory : IChocolateyFeedFactory
    {
        private readonly IDictionary<ChocolateySource, IChocolateyFeed> _feedCache;
        private readonly IInstalledPackagesManager _installedPackages;


        public ChocolateyFeedFactory(IInstalledPackagesManager installedPackages)
        {
            this._feedCache = new Dictionary<ChocolateySource, IChocolateyFeed>();
            this._installedPackages = installedPackages;
        }

        public async Task<bool> ValidateSource(Uri source)
        {
            try
            {
                var client = new ChocolateyFeedClient(source);

                var checkSource = Task.Factory.StartNew(() => client.Packages.Execute());

                checkSource.Wait();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public IChocolateyFeed Create(ChocolateySource source)
        {
            if(this._feedCache.ContainsKey(source))
            {
                return this._feedCache[source];
            }

            IChocolateyFeed feed;

            if (source.IsNugetFeed)
            {
                var nugetClient = new PackageContext(source.Location);

                feed = new ChocolateyNugetFeed(nugetClient, source, this._installedPackages);
            }
            else
            {
                var client = new ChocolateyFeedClient(source.Location);

                feed = new ChocolateyFeed(client, source, this._installedPackages);
            }

            this._feedCache.Add(source, feed);

            return feed;
        }
    }
}
