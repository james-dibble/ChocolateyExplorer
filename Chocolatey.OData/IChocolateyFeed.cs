namespace Chocolatey.OData
{
    using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chocolatey.DomainModel;

    public interface IChocolateyFeed
    {
        event Action<IEnumerable<ChocolateyPackage>> PageOfPackagesLoaded;

        ChocolateySource Source { get; }

        bool IsAnotherPageAvailable { get; }

        Task<IEnumerable<ChocolateyPackage>> LoadFirstPage();

        Task<IEnumerable<ChocolateyPackage>> GetNextPage();

        Task<IEnumerable<ChocolateyPackage>> SearchPackages(string criteria, CancellationToken cancellationToken);
    }
}
