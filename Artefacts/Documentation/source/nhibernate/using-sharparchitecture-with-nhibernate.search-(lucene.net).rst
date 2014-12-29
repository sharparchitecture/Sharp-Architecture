S#arp Architecture with NHibernate.Search
=========================================

.. note::
    *Do* **not** *use this with NHibernate Configuration Cache:*

    NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache();

NuGet Install
-------------

Install-Package NHibernate.Search

(This used to be a big deal, building from source, etc.)

Configuring and Building the Index
----------------------------------

Maintaining an index in Lucene is easy. There's a configuration setting
to keep any deletes, inserts or updates current in the index, and a way
to build the index from an existing dataset from scratch. You usually
need both unless you're starting on a project from scratch, but even so,
it is nice to have a way to build an index in case the existing index
gets corrupted.

This example will assume you've created a folder in the root of your MVC
project, and have given the ASP.NET security guy full permissions.

Open up your root Web.Config, add the following right above :

Right below add the following:

::

    <nhs-configuration xmlns='urn:nhs-configuration-1.0'>
        <search-factory>
            <property  name="hibernate.search.default.indexBase">~\LuceneIndex</property>
        </search-factory>
    </nhs-configuration>

Navigate to your NHibernate.Config, before add:

::

        <listener class='NHibernate.Search.Event.FullTextIndexEventListener, NHibernate.Search' type='post-insert'/>
        <listener class='NHibernate.Search.Event.FullTextIndexEventListener, NHibernate.Search' type='post-update'/>
        <listener class='NHibernate.Search.Event.FullTextIndexEventListener, NHibernate.Search' type='post-delete'/>

Add a Search Repository
-----------------------

In "Northwind.Infrastructure" add a repository called
"ContractRepository.cs" (the example assumes you have an object you want
to search over called contract):

::

    public interface IContractRepository
    {
        IList<Contract> Query(string q)
        void BuildSearchIndex();
    }

Let's add a method to this repository that will create the initial
index, if an index already exists, it will be deleted. We'll iterate
through all the Suppliers to accomplish this:

::

        public void BuildSearchIndex()
        {
            FSDirectory entityDirectory = null;
            IndexWriter writer = null;

            var entityType = typeof(Contract);

            var indexDirectory = new DirectoryInfo(GetIndexDirectory());

            if (indexDirectory.Exists)
            {
                indexDirectory.Delete(true);
            }

            try
            {
                var dir = new DirectoryInfo(Path.Combine(indexDirectory.FullName, entityType.Name));
                entityDirectory = FSDirectory.Open(dir);
                writer = new IndexWriter(entityDirectory, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), true, IndexWriter.MaxFieldLength.UNLIMITED);
            }
            finally
            {
                if (entityDirectory != null)
                {
                    entityDirectory.Close();
                }

                if (writer != null)
                {
                    writer.Close();
                }
            }

            var fullTextSession = Search.CreateFullTextSession(this.Session);

            // Iterate through contracts and add them to Lucene's index
            foreach (var instance in Session.CreateCriteria(typeof(Contract)).List<Contract>())
            {
                fullTextSession.Index(instance);
            }
        }

        private static string GetIndexDirectory()
        {
            INHSConfigCollection nhsConfigCollection = CfgHelper.LoadConfiguration();
            string property = nhsConfigCollection.DefaultConfiguration.Properties["hibernate.search.default.indexBase"];
            var fi = new FileInfo(property);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fi.Name); 
        }

Finally, we'll add a method to query the index:

::

        private static string GetIndexDirectory()
        {
            INHSConfigCollection nhsConfigCollection = CfgHelper.LoadConfiguration();
            string property = nhsConfigCollection.DefaultConfiguration.Properties["hibernate.search.default.indexBase"];
            var fi = new FileInfo(property);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fi.Name); 
        }

Add Search Controller
---------------------
::

	namespace Northwind.Web.Controllers 
	{
		private readonly IContractRepositorycontractRepository;
		
		public class SearchController :	Controller
		{
			public LuceneSupplierController(IContractRepositorycontractRepository)
			{
				this.contractRepository= contractRepository;
			}

			public ActionResult BuildSearchIndex()
			{
				contractRepository.BuildSearchIndex();
				return RedirectToAction("Index", "Home");            
			}

			public ActionResult Search(string query)
			{
				List<Contract> Contracts = contractRepository.Query(query).ToList();
				return View(Contracts );
			}
        }
    }

-  Wire up a view to display the search results
-  Navigate to localhost:portnumber/ContractController/BuildSearchIndex
-  This will (quickly) build your index, it would be beneficial to pass
   status messages here
-  You should see a Suppliers folder in the LuceneIndex folder of the
   project
-  To verify the index, download Luke and point it to the LuceneIndex

Pre-Requisite Reading
---------------------
I really recommend Hibernate Search in Action, you can really make
queries do some neat things that aren't covered in this tutorial. It
will, however, get you up and running quickly.
