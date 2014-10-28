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

        private IEnumerable<ChocolateyPackageVersion> _packageCache;

        public async Task<IEnumerable<ChocolateyPackageVersion>> RetrieveInstalledPackages()
        {
            return await this.RetrieveInstalledPackages(false);
        }
        
        public async Task<IEnumerable<ChocolateyPackageVersion>> RetrieveInstalledPackages(bool forceRefresh)
        {
            if(this._packageCache != null && !forceRefresh)
            {
                return this._packageCache;
            }

            using (var powershell = PowerShell.Create())
            {
                powershell.AddScript(listLocalCommand);

                var result = await Task.Factory.StartNew(() => powershell.Invoke());

                var installedPackages = result.Select(r => r.ToString()).Select(ParseFromOutput);

                this._packageCache = installedPackages;

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
