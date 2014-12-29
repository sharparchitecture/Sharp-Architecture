Simple CRUD application
=======================

Sharp Architecture makes it simple to do CRUD applications right out of
the box. While we recommend the use of another framework such as Dynamic
Data for CRUD heavy sites, take a look at how to do simple CRUD
operations in Sharp Architecture.

:ref:`Install<installation>` S#arp Architecture using your preferred method 

Create your first entity
------------------------

Let's jump back over to our Visual Studio solution. Navigate to your
Domain project and define a class called Product that inherits from
Entity. All domain objects that are persisted in Sharp inherit from this
base class. Entity does many things, but for the purposes of this
example we only care about the fact that:

-  The property Id is already set for us.
-  We are now inheriting a ValidatableObject, which gives us the
   IsValid() method.

Due to NHibernate, all properties must be marked virtual. We'll only
have one property for this class, a string titled Name. When you're
done, your class should look like this:

::

    namespace IceCreamYouScreamCorp.Domain
    {
        using System;
        using System.ComponentModel.DataAnnotations;

        using SharpArch.Domain.DomainModel;

        public class Product : Entity
        {
            [Required(ErrorMessage = "Must have a clever name!")]
            public virtual string Name { get; set; }
        }
    }

Configure NHibernate.config
---------------------------

You can skip this step if you are using RavenDB persistance.

Before we go any further, navigate to your Mvc project and find
NHibernate.config. I'm assuming you're running a local instance of SQL
Express with a database named IceCreamDb. You'll need to figure out your
own connection string, but once you do, find the
"connection.connection\_string" tags:

::

    <property name="connection.connection_string">Server=localhost\SQLEXPRESS;Initial Catalog=IceCreamDb;Integrated Security=SSPI;</property>

Prep your database environment
------------------------------

You can skip this step if you are using RavenDB persistance.

Before we start we'll need to wire up NHibernate to your database. If
you have not already done so, create a database. Let's assume you have
SQLExpress running on your local machine and you've created a database
called IceCreamDb.

Run the test
``IceCreamYouScreamCorp.Tests.SharpArchTemplate.Data.NHibernateMaps.CanGenerateDatabaseSchema``
which will generate a script for creating the database in the Database
folder under the folder you templified.

Alternatively, run the
``IceCreamYouScreamCorp.Tests.SharpArchTemplate.Data.NHibernateMaps.CanCreateDatabase``
test which will create the tables for you. This test is set to run
explictly to avoid overwriting existing tables.

Create a Controller
-------------------

Create a controller in your controllers folder named ProductsController.

    NB: In this example we're injecting repositories into our
    controllers. This is not recommended best practice. Please see the
    Cookbook example for abstracting this to your Tasks layer. I think
    for the purposes of this example, it is much more clear to inject
    the Repository.

    NB + 1: Sharp Architecture uses dependency injection, the "Don't
    call us, we'll call you," principle. Dependency injection is a bit
    out of the scope of the article. If this is new to you, I highly
    recommend looking at `Martin Fowler's
    article <http://martinfowler.com/articles/injection.html>`_.

One of the great things about Sharp Architecture is that it has created
a generic repository for you. Generally there's no need to worry about
the NHibernate session, or creating a specific repository each time you
need to talk to your database. As such, let's create a local field,
``private readonly INHibernateRepository<Product> productRepository;``
and inject it into our controller:

::

    public ProductsController(INHibernateRepository<Product> productRepository)
    {
       this.productRepository = productRepository;
    }

Create ActionResults for our Crud Operations
--------------------------------------------

Now we're all setup let's we'll need to do the following things:

-  Return a list of all products
-  Return a single product
-  Create/Update a single product
-  Delete a product

Returning all products on the ``Index`` ActionResult:

::

    public ActionResult Index()
    {
       var products = this.productRepository.GetAll();
       return View(products);
    }

Return a single product and display it on an editable form:

::

        [Transaction]
        [HttpGet]
        public ActionResult CreateOrUpdate(int id)
        {
            var product = this.productRepository.Get(id);
            return View(product);
        }

Post the result, return the object if it is invalid:

::

        [Transaction]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateOrUpdate(Product product)
        {
            if (ModelState.IsValid && product.IsValid())
            {
                this.productRepository.SaveOrUpdate(product);
                return this.RedirectToAction("Index");
            }

            return View(product);
        }

Delete a product, making sure we are posting as we are changing data.

::

        [Transaction]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var product = this.productRepository.Get(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            this.productRepository.Delete(product);
            return this.RedirectToAction("Index");
        }

Add the views
-------------

Now all we have to do is create our views for each action. Once this is
complete, you can run the application to see it in action.

Index.cshtml:

::

    @using IceCreamYouScreamCorp.Web.Mvc   
    @model IEnumerable<IceCreamYouScreamCorp.Domain.Product>

    @{
        ViewBag.Title = "Index";
    }

    <h2>Index</h2>

    <p>
        @Html.ActionLink((ProductsController c) => c.CreateOrUpdate(0),"Create New")
    </p>
    <table>
        <tr>
            <th>
                Name
            </th>
            <th></th>
            <th></th>
        </tr>

    @foreach (var item in Model) {
        <tr>
            <td>
                @Html.DisplayFor(modelItem => item.Name)
            </td>
            <td>
                @Html.ActionLink("Edit", "CreateOrUpdate", new { id=item.Id })
            </td>
            <td>
            @using (Html.BeginForm("Delete", "Products")) {
                @Html.Hidden("id", item.Id)
                <input type="submit" value="Delete" />
                @Html.AntiForgeryToken()
            }
            </td>
        </tr>
    }
    </table>

CreateOrUpdate.cshtml:

::

    @model IceCreamYouScreamCorp.Domain.Product

    @using (Html.BeginForm()) {
    @Html.ValidationSummary(true)
    <fieldset>
            <legend>Product</legend>

            <div class="editor-label">
                @Html.LabelFor(model => model.Name)
            </div>
            <div class="editor-field">
                @Html.EditorFor(model => model.Name)
                @Html.ValidationMessageFor(model => model.Name)
            </div>

            <p>
                <input type="submit" value="Save" />
            </p>
        </fieldset>

        @Html.AntiForgeryToken()
    }

    <div>
        @Html.ActionLink("Back to List", "Index")
    </div>

**Done!**

Start the web project go to /Products to marvel at your creation.

We've achieved the basics of a CRUD operation, without touching on TDD
or some other best practices, but this should get you going very quickly
on using Sharp Architecture in your project.
