NHibernate HiLo Generator
=========================

Why use HiLo
------------

HiLo identity generators allow NHibernate to generate identity values to map child objects to their parent without having to hit the database as opposed to using the native identity generation causes nhibernate to hit the database on every save, which affects performance and running of batch statements.

For more information on generator behaviours refer to `this <http://fabiomaulo.blogspot.co.uk/2009/02/nh210-generators-behavior-explained.html>`_ blog post by Fabio Maulo.

Using HiLo Id generation
------------------------
In your Infrastructure project, under NHibernateMaps:

.. code-block:: C#

    public class PrimaryKeyConvention : IIdConvention
    {
        public void Apply(FluentNHibernate.Conventions.Instances.IIdentityInstance instance)
        {
            instance.Column(instance.EntityType.Name + "Id");
            instance.UnsavedValue("0");
            instance.GeneratedBy.HiLo("1000");
        }
    }

Create the following table:

.. code-block:: sql

    CREATE TABLE [dbo].[hibernate_unique_key](
           [next_hi] [int] NOT NULL
            ) ON [PRIMARY]

Populate the column with a number to seed and you're done.
