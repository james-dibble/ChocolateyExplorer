namespace Chocolatey.Manager
{
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Linq;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;

    public class Installer : IChocolateyInstaller
    {
        public async Task<IEnumerable<string>> Install(ChocolateyPackageVersion package)
        {
            using (var powershell = PowerShell.Create())
            {
                var command = string.Format(
                    "cinst {0} -version {1}",
                    package.Id,
                    package.Version);

                powershell.AddScript(command);

                var result = await Task.Factory.StartNew(() => powershell.Invoke());

                return result.Select(r => r.ToString());
            }
        }


        public async Task<IEnumerable<string>> Uninstall(ChocolateyPackageVersion package)
        {
            using (var powershell = PowerShell.Create())
            {
                var command = string.Format(
                    "cuninst {0} -version {1}",
                    package.Id,
                    package.Version);

                powershell.AddScript(command);

                var result = await Task.Factory.StartNew(() => powershell.Invoke());

                return result.Select(r => r.ToString());
            }
        }


        public async Task<IEnumerable<string>> Update(ChocolateyPackageVersion package)
        {
            using (var powershell = PowerShell.Create())
            {
                var command = string.Format("cup {0}", package.Id);

                powershell.AddScript(command);

                var result = await Task.Factory.StartNew(() => powershell.Invoke());

                return result.Select(r => r.ToString());
            }
        }
    }
}
