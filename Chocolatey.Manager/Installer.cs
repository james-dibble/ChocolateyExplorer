namespace Chocolatey.Manager
{
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Linq;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;
    using System;

    public class Installer : IChocolateyInstaller
    {
        public event Action<string> OutputReceived;

        public async Task Install(ChocolateyPackageVersion package, string arguments)
        {
            var args = string.IsNullOrEmpty(arguments) ? string.Empty : " -installArguments " + arguments;

            using (var powershell = PowerShell.Create())
            {
                var command = string.Format(
                    "cinst {0} -version {1}{2}",
                    package.Id,
                    package.Version,
                    args);

                powershell.AddScript(command);

                var outputCollection = new PSDataCollection<PSObject>();
                outputCollection.DataAdded += this.SendOutput;

                await Task.Factory.StartNew(() => powershell.Invoke(null, outputCollection, null));
            }
        }

        public async Task Install(ChocolateyPackageVersion package)
        {
            await this.Install(package, string.Empty);
        }


        public async Task Uninstall(ChocolateyPackageVersion package)
        {
            using (var powershell = PowerShell.Create())
            {
                var command = string.Format(
                    "cuninst {0} -version {1}",
                    package.Id,
                    package.Version);

                powershell.AddScript(command);

                var outputCollection = new PSDataCollection<PSObject>();
                outputCollection.DataAdded += this.SendOutput;

                await Task.Factory.StartNew(() => powershell.Invoke(null, outputCollection, null));
            }
        }


        public async Task Update(ChocolateyPackageVersion package)
        {
            using (var powershell = PowerShell.Create())
            {
                var command = string.Format("cup {0}", package.Id);

                powershell.AddScript(command);
                powershell.Streams.Error.DataAdded += ErrorDataAdded;
                powershell.Streams.Warning.DataAdded += WarningDataAdded;

                var outputCollection = new PSDataCollection<PSObject>();
                outputCollection.DataAdded += this.SendOutput;
                
                await Task.Factory.StartNew(() => powershell.Invoke(null, outputCollection, null));
            }
        }

        void WarningDataAdded(object sender, DataAddedEventArgs args)
        {
            var outputCollection = sender as PSDataCollection<PSObject>;

            var output = outputCollection[args.Index].ToString();

            this.RaiseOutputReceiced(output);
        }

        void ErrorDataAdded(object sender, DataAddedEventArgs args)
        {
            var outputCollection = sender as PSDataCollection<PSObject>;

            var output = outputCollection[args.Index].ToString();

            this.RaiseOutputReceiced(output);
        }

        private void SendOutput(object sender, DataAddedEventArgs args)
        {
            var outputCollection = sender as PSDataCollection<PSObject>;

            var output = outputCollection[args.Index].ToString();

            this.RaiseOutputReceiced(output);
        }

        private void RaiseOutputReceiced(string output)
        {
            if (this.OutputReceived != null)
            {
                this.OutputReceived(output);
            }
        }
    }
}