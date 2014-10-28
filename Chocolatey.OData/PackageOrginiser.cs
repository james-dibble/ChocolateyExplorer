namespace Chocolatey.OData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Chocolatey.DomainModel;

    public static class PackageOrginiser
    {
        public static IEnumerable<ChocolateyPackage> GroupPackages(
            this IEnumerable<ChocolateyPackageVersion> packageVersions, 
            IEnumerable<ChocolateyPackageVersion> installedPackages)
        {
            var allPackages = packageVersions.ToList();

            foreach (var chocolateyPackageVersion in packageVersions.DistinctBy(pv => pv.Id).AsParallel())
            {
                var version = chocolateyPackageVersion;

                var package = new ChocolateyPackage
                              {
                                  Id = chocolateyPackageVersion.Id,
                                  Title = chocolateyPackageVersion.Title,
                                  IsInstalled = installedPackages.Any(p => p.Id == chocolateyPackageVersion.Id),
                                  Versions =
                                      allPackages.Where(pv => pv.Id == version.Id).OrderByDescending(pv => pv.Version)
                              };

                yield return package;
            }
        }
    }
}