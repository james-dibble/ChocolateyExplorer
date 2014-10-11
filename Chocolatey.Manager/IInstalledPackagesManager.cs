namespace Chocolatey.Manager
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;

    public interface IInstalledPackagesManager
    {
        Task<IEnumerable<ChocolateyPackageVersion>> RetrieveInstalledPackages();
    }
}
