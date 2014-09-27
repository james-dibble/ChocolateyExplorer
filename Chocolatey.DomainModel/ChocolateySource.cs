namespace Chocolatey.DomainModel
{
    using System;

    public class ChocolateySource
    {
        public string Name { get; set; }

        public Uri Location { get; set; }

        public bool IsNugetFeed { get; set; }
    }
}
