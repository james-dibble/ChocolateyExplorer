namespace Chocolatey.OData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;
    using Chocolatey.OData.Nuget;
    using Chocolatey.OData.Service;

    public class ChocolateyFeedFactory : IChocolateyFeedFactory
    {
        private readonly IDictionary<ChocolateySource, IChocolateyFeed> _feedCache;

        public ChocolateyFeedFactory()
        {
            this._feedCache = new Dictionary<ChocolateySource, IChocolateyFeed>();
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

                feed = new ChocolateyNugetFeed(nugetClient, source);
            }
            else
            {
                var client = new ChocolateyFeedClient(source.Location);

                feed = new ChocolateyFeed(client, source);
            }

            this._feedCache.Add(source, feed);

            return feed;
        }
    }
}
