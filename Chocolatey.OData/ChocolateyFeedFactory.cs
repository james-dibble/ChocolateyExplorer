namespace Chocolatey.OData
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
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

        public IChocolateyFeed Create(ChocolateySource source)
        {
            if(this._feedCache.ContainsKey(source))
            {
                return this._feedCache[source];
            }

            var client = new ChocolateyFeedClient(source.Location)
            {
                IgnoreMissingProperties = true,
                MergeOption = MergeOption.NoTracking
            };

            var feed = new ChocolateyFeed(client, source, this._installedPackages);

            this._feedCache.Add(source, feed);

            return feed;
        }
    }
}
