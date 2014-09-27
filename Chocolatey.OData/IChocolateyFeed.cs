namespace Chocolatey.OData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;

    public interface IChocolateyFeed
    {
        event Action<IEnumerable<ChocolateyPackage>> PageOfPackagesLoaded;

        ChocolateySource Source { get; }

        Task<IEnumerable<ChocolateyPackage>> GetAllPackages();

        Task<IEnumerable<ChocolateyPackage>> SearchPackages(string criteria);
    }
}
