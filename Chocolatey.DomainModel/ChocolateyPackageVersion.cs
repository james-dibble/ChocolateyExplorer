using System;
namespace Chocolatey.DomainModel
{
    public class ChocolateyPackageVersion
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public string ReleaseNotes { get; set; }

        public string Author { get; set; }

        public Uri ChocolateyLink { get; set; }

        public int DownloadCount { get; set; }

        public Uri ProjectLink { get; set; }
    }
}
