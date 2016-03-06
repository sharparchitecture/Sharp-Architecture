namespace Northwind.Domain.Contracts.Tasks
{
    using System.Collections.Generic;

    public interface ITerritoryTasks
    {
        IList<Territory> GetTerritories();
    }
}