namespace Chocolatey.DomainModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ChocolateyPackage
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public Uri IconLink { get { return this.LatestVersion.IconLink; } }

        public IEnumerable<ChocolateyPackageVersion> Versions { get; set; }

        public ChocolateyPackageVersion LatestVersion
        {
            get
            {
                return this.Versions.OrderByDescending(pv => pv.Version).First();
            }
        }
    }
}
