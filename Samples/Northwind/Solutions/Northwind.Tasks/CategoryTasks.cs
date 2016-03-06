namespace Northwind.Tasks
{
    using System.Collections.Generic;
    using System.Linq;

    using Northwind.Domain;
    using Northwind.Domain.Contracts.Tasks;

    using SharpArch.Domain.PersistenceSupport;

    public class CategoryTasks : ICategoryTasks
    {
        private readonly IRepository<Category> categoryRepository;

        public CategoryTasks(IRepository<Category> categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public Category Create(string categoryName)
        {
            var category = new Category(categoryName);
            this.categoryRepository.DbContext.BeginTransaction();
            category = this.categoryRepository.SaveOrUpdate(category);
            this.categoryRepository.DbContext.CommitTransaction();

            return category;
        }

        public List<Category> GetAllCategories()
        {
            this.categoryRepository.DbContext.BeginTransaction();
            var categories = this.categoryRepository.GetAll();
            this.categoryRepository.DbContext.CommitTransaction();

            return categories.ToList();
        }

        public Category GetCategoryById(int id)
        {
            this.categoryRepository.DbContext.BeginTransaction();
            var category = this.categoryRepository.Get(id);
            this.categoryRepository.DbContext.CommitTransaction();

            return category;
        }
    }
}