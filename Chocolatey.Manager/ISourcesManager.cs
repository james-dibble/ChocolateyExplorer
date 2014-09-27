namespace Chocolatey.Manager
{
    using System.Collections.Generic;
    using Chocolatey.DomainModel;

    public interface ISourcesManager
    {
        IEnumerable<ChocolateySource> GetSources();

        void AddSource(ChocolateySource source);

        void RemoveSource(ChocolateySource source);
    }
}
