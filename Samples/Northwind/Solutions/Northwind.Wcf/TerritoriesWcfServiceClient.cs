namespace Northwind.WcfServices
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Northwind.WcfServices.Dtos;

    /// <summary>
    ///   Provides a strongly typed client proxy to use the WCF service without having to configure 
    ///   it via WCF configuration.
    /// </summary>
    public class TerritoriesWcfServiceClient : ClientBase<ITerritoriesWcfService>, ITerritoriesWcfService
    {
        public TerritoriesWcfServiceClient()
        {
        }

        public TerritoriesWcfServiceClient(string endpointName)
            : base(endpointName)
        {
        }

        public TerritoriesWcfServiceClient(Binding binding, EndpointAddress address)
            : base(binding, address)
        {
        }

        public IList<TerritoryDto> GetTerritories()
        {
            return this.Channel.GetTerritories();
        }
    }
}