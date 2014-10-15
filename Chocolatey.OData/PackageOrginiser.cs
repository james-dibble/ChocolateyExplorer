namespace Chocolatey.OData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Chocolatey.DomainModel;

    public static class PackageOrginiser
    {
        public static IEnumerable<ChocolateyPackage> GroupPackages(this IEnumerable<ChocolateyPackageVersion> packageVersions)
        {
            var allPackages = packageVersions.ToList();

            foreach (var chocolateyPackageVersion in packageVersions.DistinctBy(pv => pv.Id).AsParallel())
            {
                var version = chocolateyPackageVersion;

                var package = new ChocolateyPackage
                              {
                                  Id = chocolateyPackageVersion.Id,
                                  Title = chocolateyPackageVersion.Title,
                                  Versions =
                                      allPackages.Where(pv => pv.Id == version.Id)
                              };

                yield return package;
            }
        }
    }
}