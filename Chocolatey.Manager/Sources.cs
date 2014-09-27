namespace Chocolatey.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Chocolatey.DomainModel;

    public class Sources : ISourcesManager
    {
        private static string ConfigurationLocation
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(@"%SystemDrive%\ProgramData\chocolatey\chocolateyinstall\chocolatey.config");
            }
        }

        public IEnumerable<ChocolateySource> GetSources()
        {
            var config = XDocument.Load(ConfigurationLocation);

            var sources = config.Descendants("source").Select(s => new ChocolateySource 
            { 
                Name = s.Attribute("id").Value, 
                Location = new Uri(s.Attribute("value").Value),
                IsNugetFeed = s.Attributes().Any(a => a.Name == "nuget") ? s.Attribute("nuget").Value == "true" : false
            });

            return sources;
        }


        public void AddSource(ChocolateySource source)
        {
            this.RemoveSource(source);

            var config = XDocument.Load(ConfigurationLocation);

            var newElement = new XElement("source", new XAttribute("id", source.Name), new XAttribute("value", source.Location.AbsoluteUri), new XAttribute("nuget", source.IsNugetFeed));

            config.Descendants("sources").FirstOrDefault().Add(newElement);

            config.Save(ConfigurationLocation);
        }


        public void RemoveSource(ChocolateySource source)
        {
            var config = XDocument.Load(ConfigurationLocation);

            config.Descendants("source").Where(e => e.Attribute("id").Value == source.Name).Remove();

            config.Save(ConfigurationLocation);
        }
    }
}
