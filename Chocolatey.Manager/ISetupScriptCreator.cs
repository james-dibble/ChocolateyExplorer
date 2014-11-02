using System.Collections.Generic;
using Chocolatey.DomainModel;
namespace Chocolatey.Manager
{
    public interface ISetupScriptCreator
    {
        void CreateSeuptScript(IEnumerable<ChocolateyPackageVersion> packages, string saveLocation);
    }
}
