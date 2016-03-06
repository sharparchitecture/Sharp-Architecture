namespace Northwind.Web.Mvc.WcfServices
{
    using System.Configuration;
    using System.ServiceModel;

    using Northwind.WcfServices;

    public class TerritoriesWcfServiceFactory
    {
        public ITerritoriesWcfService Create()
        {
            // I see the below as a magic string; I typically like to move these to a 
            // web.config reader to consolidate the app setting names
            var address = new EndpointAddress(ConfigurationManager.AppSettings["territoryWcfServiceUri"]);
            var binding = new WSHttpBinding();

            return new TerritoriesWcfServiceClient(binding, address);
        }
    }
}