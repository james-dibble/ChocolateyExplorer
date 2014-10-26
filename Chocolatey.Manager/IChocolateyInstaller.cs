namespace Chocolatey.Manager
{
    using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chocolatey.DomainModel;

    public interface IChocolateyInstaller
    {
        event Action<string> OutputReceived;

        Task Install(ChocolateyPackageVersion package);

        Task Install(ChocolateyPackageVersion package, string arguments, CancellationToken cancellationToken);

        Task Install(ChocolateyPackageVersion package, string arguments);

        Task Uninstall(ChocolateyPackageVersion package);

        Task Update(ChocolateyPackageVersion package);
    }
}
