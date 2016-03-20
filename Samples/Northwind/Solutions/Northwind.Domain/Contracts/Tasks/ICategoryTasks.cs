namespace Northwind.Domain.Contracts.Tasks
{
    using System.Collections.Generic;

    public interface ICategoryTasks
    {
        Category Create(string categoryName);

        List<Category> GetAllCategories();

        Category GetCategoryById(int id);
    }
}