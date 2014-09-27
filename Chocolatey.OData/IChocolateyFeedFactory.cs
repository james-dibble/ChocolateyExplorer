namespace Chocolatey.OData
{
    using System;
    using System.Threading.Tasks;
    using Chocolatey.DomainModel;

    public interface IChocolateyFeedFactory
    {
        Task<bool> ValidateSource(Uri source);

        IChocolateyFeed Create(ChocolateySource source);
    }
}
