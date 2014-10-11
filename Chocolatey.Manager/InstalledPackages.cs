namespace Chocolatey.Manager
{
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Linq;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;

    public class InstalledPackages : IInstalledPackagesManager
    {
        private const string listLocalCommand = @"clist -lo";

        public async Task<IEnumerable<ChocolateyPackageVersion>> RetrieveInstalledPackages()
        {
            using (var powershell = PowerShell.Create())
            {
                powershell.AddScript(listLocalCommand);

                var result = await Task.Factory.StartNew(() => powershell.Invoke());

                var installedPackages = result.Select(r => r.ToString()).Select(ParseFromOutput);

                return installedPackages;
            }
        }

        private static ChocolateyPackageVersion ParseFromOutput(string output)
        {
            var split = output.Split(' ');

            var packageName = split.ElementAt(0);
            var packageVersion = split.ElementAt(1);

            return new ChocolateyPackageVersion { Id = packageName, Version = packageVersion };
        }
    }
}
