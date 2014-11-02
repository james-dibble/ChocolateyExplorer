namespace Chocolatey.Manager
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Chocolatey.DomainModel;

    public class SetupScriptCreator : ISetupScriptCreator
    {
        private const string InstallChocolateyCommand = @"iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))";

        private readonly ISourcesManager _sources;

        public SetupScriptCreator(ISourcesManager sources)
        {
            this._sources = sources;
        }

        public void CreateSeuptScript(IEnumerable<ChocolateyPackageVersion> packages, string saveLocation)
        {
            var fileContents = new List<string>
            {
                InstallChocolateyCommand
            };

            fileContents.AddRange(this._sources.GetSources().Select(this.CreateAddSourceCommand));

            fileContents.AddRange(packages.Select(this.CreateInstallCommand));

            File.WriteAllLines(saveLocation, fileContents);
        }

        private string CreateInstallCommand(ChocolateyPackageVersion package)
        {
            const string installCommandTemplate = @"cinst '{0}' -version '{1}'";

            var installCommand = string.Format(installCommandTemplate, package.Id, package.Version);

            return installCommand;
        }

        private string CreateAddSourceCommand(ChocolateySource source)
        {
            const string addSourceCommandTemplate = @"choco sources add -name '{0}' -source '{1}'";

            var addSourceCommand = string.Format(addSourceCommandTemplate, source.Name, source.Location);

            return addSourceCommand;
        }
    }
}