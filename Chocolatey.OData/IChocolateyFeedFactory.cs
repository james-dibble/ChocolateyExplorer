namespace Chocolatey.OData
{
    using System;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;

    public interface IChocolateyFeedFactory
    {
        IChocolateyFeed Create(ChocolateySource source);
    }
}
