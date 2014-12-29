Frequently Asked Questions
==========================

**Q: Is there a continuous integration available for downloading built
S#arp Architecture artifacts?**

A: Great question! In fact, S#arp Architecture is part of the CI
environment hosted at http://teamcity.codebetter.com.

**Q: How do I register an NHibernate IInterceptor?**

A: To register an IInterceptor, simply invoke the registration after
initializing NHibernateSession in Global.asax.cs; e.g.,

::

    NHibernateSession.Init(new WebSessionStorage(this),

        new string[] { Server.MapPath("~/bin/MyProject.Data.dll") });

    NHibernateSession.RegisterInterceptor(new MyAuditLogger()); 

**Q: How do I “downgrade” my S#arp Architecture project to not use Fluent
NHibernate Auto Mapping?**

A: Starting with S#arp Architecture RC, Fluent NHibernate’s auto
persistence mapping is supported and included by default. If you have no
idea what I’m talking about, take a quick look at
http://www.chrisvandesteeg.nl/2008/12/02/fluent-nhibernates-autopersistencemodel-i-love-it/.
If you don’t want to use the Auto Mapping capabilities, you can use
Fluent NHibernate ClassMaps or HBMs as well.

-  Delete
   YourProject.Infrastruction/NHibernateMaps/AutoPersistenceModelGenerator.cs
   from your project

-  Modify YourProject.Web.Mvc/Global.asax.cs to no longer load the auto
   persistence model and feed it to the NHibernate initialization
   process. The following example code demonstrates how this may be
   accomplished:

   AutoPersistenceModel autoPersistenceModel = new
   AutoPersistenceModelGenerator().Generate();

   NHibernateSession.Init(new WebSessionStorage(this), new string[] {
   Server.MapPath("~/bin/Northwind.Infrastructure.dll") });

Likewise, within
YourProject.Tests/YourProject.Infrastructure/NHibernateMaps/MappingIntegrationTests
class, remove the passing of the auto-persistence model to the
NHibernate initialization process:

::

      [TestFixture]
      [Category("DB Tests")]
      public class MappingIntegrationTests
      {
          [SetUp]
          public virtual void SetUp() {
              string[] mappingAssemblies =
                  RepositoryTestsHelper.GetMappingAssemblies();

              AutoPersistenceModel autoPersistenceModel =
                  new AutoPersistenceModelGenerator().Generate(); 

              NHibernateSession.Init(new SimpleSessionStorage(),
                  mappingAssemblies,
                  "../../../../app/Northwind.Web.Mvc/NHibernate.config");
          }

Compile and hope for the best. ;)

Note that Fluent NHibernate Auto Mapper can live in happy coexistence
with Fluent NHibernate ClassMaps and HBMs. Simply exclude the applicable
classes from the Auto Mapper and add their mapping definition,
accordingly. Alternatively, it’s very simple to provide mapping
overrides as well, which is the approach I personally prefer to take.

**Q: How do I run my S#arp Architecture project in a medium trust
environment?**

A: If you must run in a Medium Trust environment, the following
modifications must be made to the NHibernate configuration:

 \… managed\_web \… Additionally, all class mappings need to be set to lazy="false".

**Q: How do I run my S#arp Architecture project in 64 bit (x64)
environment?**

A: There are a couple of options for running in a 64 bit environment.
The first is to switch IIS7 to have the website run in a classic .NET
application pool. Alternatively, you can create a separate app pool and
ensure that the "Enable 32 bit application" is checked under the
advanced settings for the app pool.

In addition to modifying IIS for a 64 bit environment, also note that
that you should modify YourProject.Tests to build as an x86 assembly, as
the included SQLite assembly will only work as x86. Alternatively, you
could download an x64 compliant version of SQLite.

**Q: Because the Entity’s Id property has a protected setter, the Id
property isn’t being included during XML serialization? What can we do
to include it?**

A: Having the Id setter being protected is a fundamental principle of
S#arp Architecture. But there are times when the Id is needed to be
included during XML serialization. The following base class can be added
to YourProject.Core to facilitate the Id being included with XML
serialized objects:

::

    public class EntityWithSerializableId : Entity
    {
        public virtual int ItemId {
            get { return Id; }
            set { ; }
        }
    }
