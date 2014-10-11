namespace Chocolatey.Manager
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;

    public interface IChocolateyInstaller
    {
        Task<IEnumerable<string>> Install(ChocolateyPackageVersion package);

        Task<IEnumerable<string>> Uninstall(ChocolateyPackageVersion package);

        Task<IEnumerable<string>> Update(ChocolateyPackageVersion package);
    }
}
