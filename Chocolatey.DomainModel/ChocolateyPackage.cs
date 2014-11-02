namespace Chocolatey.DomainModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ChocolateyPackage
    {
        public ChocolateyPackage()
        {
            this.Versions = new ObservableCollection<ChocolateyPackageVersion>();
        }

        public string Id { get; set; }

        public string Title { get; set; }

        public Uri IconLink { get; set; }

        public bool IsInstalled { get; set; }

        public ObservableCollection<ChocolateyPackageVersion> Versions { get; set; }

        public ChocolateyPackageVersion LatestVersion
        {
            get
            {
                return this.Versions.OrderByDescending(pv => pv.Version).First();
            }
        }
    }
}
