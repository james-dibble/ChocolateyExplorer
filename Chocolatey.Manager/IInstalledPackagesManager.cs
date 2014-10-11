namespace Chocolatey.Manager
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;

    public interface IInstalledPackagesManager
    {
        IEnumerable<ChocolateyPackageVersion> RetrieveInstalledPackages();
    }
}
