namespace DomainLayer.Entities
{
    using Persistence;
    using SharpArch.Domain.DomainModel;
    using SharpArch.Domain.PersistenceSupport.SharpArch.NHibernate;


    [UseDatabase(Databases.Main)]
    public class User : Entity<long>
    {
        public virtual string Login { get; set; }
    }
}
