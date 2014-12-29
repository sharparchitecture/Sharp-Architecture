Multiple Databases
==================

To add support for an additional database to your project:

Create an NHibernateForOtherDb.config file which contains the connection
string to the new database

Within YourProject.Web.Mvc/Global.asax.cs, immediately under the
NHibernateSession.Init() to initialize the first session factory, an
additional call to NHibernateSession.AddConfiguration(). While the first
NHibernate initialization assumes a default factory key, you'll need to
provide an explicit factory key for the 2nd initialization. This factory
key will be referred to by repositories which are tied to the new
database as well; accordingly, I'd recommend making it a globally
accessible string to make it easier to refere to in various locations.
E.g.,

::

     NHibernateSession.AddConfiguration(Northwind.Infrastructure.DataGlobals.OTHER_DB_FACTORY_KEY, 
          new string[] { Server.MapPath("~/bin/Northwind.Data.dll") }, 
          new AutoPersistenceModelGenerator().Generate(),
          Server.MapPath("~/NHibernateForOtherDb.config"), null, null, null);

    // In DataGlobals.cs:
    public const string OTHER_DB_FACTORY_KEY = "nhibernate.other_db";

Create an Entity class which you intend to be tied to the new database;
e.g.,

::

    public class Village : Entity
    {
        public virtual string Name { get; set; }
    }

Within YourProject.Core/DataInterfaces, add a new repository interface
for the Entity class and simply inherit from the base repository
interface. Since you'll need to decorate the concrete repository
implementation with the factory session key, you need to have the
explicit interface to then have the associated concrete implementation;
e.g.,

::

    using SharpArch.Core.PersistenceSupport;

    namespace Northwind.Domain.DataInterfaces
    {
        public interface IVillageRepository : IRepository<Village> { }
    }

Within YourProject.Data, add a new repository implementation for the
repository interface just defined; e.g.,

::

    using Northwind.Core.DataInterfaces;
    using SharpArch.Data.NHibernate;
    using Northwind.Core;

    namespace Northwind.Infrastructure
    {
        [SessionFactory(DataGlobals.OTHER_DB_FACTORY_KEY)]
        public class VillageRepository : 
            Repository<Village>, IVillageRepository { }
    }

Within YourProject.Web.Mvc.Controllers, add a new controller which will
use the repository (or which will accept an application service which
then uses the repository); e.g.,

::

    using System.Web.Mvc;
    using SharpArch.Web.NHibernate;
    using Northwind.Core.DataInterfaces;
    using SharpArch.Core;
    using Northwind.Core;
    using System.Collections.Generic;

    namespace Northwind.Web.Mvc.Controllers
    {
        public class VillagesController : Controller
        {
            public VillagesController(
                IVillageRepository villageRepository) {
                Check.Require(villageRepository != null,
                    "villageRepository may not be null");
                this.villageRepository = villageRepository;
            }

            [Transaction(DataGlobals.OTHER_DB_FACTORY_KEY)]
            public ActionResult Index() {
                IList<Village> villages = villageRepository.GetAll();
                return View(villages);
            }

            private IVillageRepository villageRepository;
        }
    }

Create an index page to list the villages given to it from the
controller.

Note that the WCF support built into the SharpArch libraries does NOT
support multiple databases at this time. WCF will always use the
"default" database; the one which is configured without an explicit
session factory key.
