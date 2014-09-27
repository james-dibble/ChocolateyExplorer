using System.Collections.Generic;
namespace Chocolatey.DomainModel
{
    public class ChocolateyPackage
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public IEnumerable<ChocolateyPackageVersion> Versions { get; set; }
    }
}
