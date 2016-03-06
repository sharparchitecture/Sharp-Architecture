namespace Northwind.Domain.Contracts.Tasks
{
    using System.Collections.Generic;

    public interface IDashboardTasks
    {
        DashboardSummaryDto GetDashboardSummary();
    }

    /// <summary>
    ///   Arguably, this could go into a dedicated DTO layer.
    /// </summary>
    public class DashboardSummaryDto
    {
        public IList<Supplier> SuppliersCarryingFewestProducts { get; set; }

        public IList<Supplier> SuppliersCarryingMostProducts { get; set; }
    }
}
