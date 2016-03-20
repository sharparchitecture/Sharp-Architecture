namespace Northwind.WcfServices
{
    using System.Collections.Generic;

    using Northwind.Domain;
    using Northwind.WcfServices.Dtos;

    using SharpArch.Domain;
    using SharpArch.Domain.PersistenceSupport;

    /// <summary>
    ///   Concrete implementation of the service.
    /// </summary>
    public class TerritoriesWcfService : ITerritoriesWcfService
    {
        private readonly IRepository<Territory> territoryRepository;

        public TerritoriesWcfService(IRepository<Territory> territoryRepository)
        {
            Check.Require(territoryRepository != null, "territoryRepository may not be null");

            this.territoryRepository = territoryRepository;
        }

        /// <summary>
        ///   Doesn't do anything because it only exists to be interchangeable with WCF client 
        ///   proxies, such as <see cref = "TerritoriesWcfServiceClient" />
        /// </summary>
        public void Abort()
        {
        }

        /// <summary>
        ///   Doesn't do anything because it only exists to be interchangeable with WCF client 
        ///   proxies, such as <see cref = "TerritoriesWcfServiceClient" />
        /// </summary>
        public void Close()
        {
        }

        public IList<TerritoryDto> GetTerritories()
        {
            // I'd rather have the transaction begun via an attribute, like with a controller action, 
            // or within a service object, but this works for the current example.
            this.territoryRepository.DbContext.BeginTransaction();

            var territories = this.territoryRepository.GetAll();
            var territoryDtos = new List<TerritoryDto>();

            foreach (var territory in territories)
            {
                territoryDtos.Add(TerritoryDto.Create(territory));
            }

            // Since we're certainly not going to require lazy loading, commit the transcation
            // before returning the data.
            this.territoryRepository.DbContext.CommitTransaction();

            return territoryDtos;
        }
    }
}