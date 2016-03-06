namespace Northwind.Tasks
{
    using System.Collections.Generic;

    using Northwind.Domain;
    using Northwind.Domain.Contracts.Tasks;

    using SharpArch.Domain.PersistenceSupport;

    public class TerritoryTasks : ITerritoryTasks
    {
        private readonly IRepository<Territory> territoryRepository;

        public TerritoryTasks(IRepository<Territory> territoryRepository)
        {
            this.territoryRepository = territoryRepository;
        }

        public IList<Territory> GetTerritories()
        {
            var territories = this.territoryRepository.GetAll();
            return territories;
        }
    }
}