Configuration cache
===================

During an application's startup NHibernate can take significant time
when configuring and validating it's mapping to a database. Caching the
NHibernate configuration data can reduce initial startup time by storing
the configuration to a file and avoiding the validation checks that run
when a configuration is created from scratch.

SharpArch's NHibernateSession class now has a ConfigurationCache
property that accepts an object reference that does the work of loading
and saving the NHibernate configuration to a file. This new feature is
based on an article by Oren Eini (aka Ayende Rahien) Building a Desktop
To-Do Application with NHibernate, and a big thanks to Sandor
Drieenhuizen who provided a lot of this code.

Cache Setup
-----------

To use the configuration cache simply set the
NHibernateSession.ConfigurationCache property to a new instance of the
NHibernateConfigurationFileCache class, before calling the
NHibernateSession.Init method. For example:

::

    private void InitializeNHibernateSession()
            {
                NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache(
                    new string[] { "Northwind.Domain" });
                NHibernateSession.Init(
                    webSessionStorage,
                    new string[] { Server.MapPath("~/bin/Northwind.Infrastructure.dll") },
                    new AutoPersistenceModelGenerator().Generate(),
                    Server.MapPath("~/NHibernate.config"));
            }
    }

The constructor of the NHibernateConfigurationFileCache class takes a
string array of file dependencies, typically this is the Domain.dll that
contains the web application's domain model objects. The string array
can contain just the assembly name (as above) or the file name (ex
"Northwind.Domain.dll") or the full path to the file (ex
"c:.Domain.dll").

Details
-------

::

    INHibernateConfigurationCache

Interface that defines two methods for loading and saving the
configuration cache.

NHibernate.Cfg.Configuration LoadConfiguration(string configKey, string
configPath, string[] mappingAssemblies) void SaveConfiguration(string
configKey, NHibernate.Cfg.Configuration config)

These methods are used by the NHibernateSession.AddConfiguration method
to load and save a configuration object to and from a cache file. This
interface allows others to implement their own file caching mechanism if
necessary.

::

    NHibernateConfigurationCache

This class implements the interface and does the work of caching the
configuration. Several methods are virtual so they can be overridden in
a derived class, as may be necessary to store the cache file in a
different location or to have different logic to invalidate the cache.

The constructor takes an string array of file dependencies which are
used to test if the cached NHibernate configuration is current or not.
If any of the dependent file's last modified time stamp is later than
that of the cached configuration, then the cached file is discarded and
a new configuration is created from scratch. This configuration is then
serialized and saved to a file.

Note: NHibernate's XML config file and the mapping assemblies (ex
"Northwind.Data.dll") are automatically included when testing if the
cached configuration is current.

FileCache
---------

The SharpArch.Domainproject now contains a FileCache class that can
serialize and deserialize an object to a file in binary format. This
class uses a generic type parameter to define the type being serialized
and deserialized. This makes the FileCache class useful for any other
object that you might want to serialize to a file.

ConfigurationCache property
---------------------------------------------

This property holds a reference to an INHibernateConfigurationCache
object, and if not null, will use the caching logic to load and save the
NHibernate configuration. You must set the ConfigurationCache property
before calling the NHibernateSession.Init method, and you cannot change
the property reference after calling the Init method.

Configuration Serialization
---------------------------

To cache the configuration to a file, all objects contained within the
NHibernate configuration must be serializable. All of the default data
types included with NHibernate will serialize, but if you have any
custom data types (i.e. classes that implement IUserType), they must
also be marked with the [Serializable] attribute and, if necessary,
implement ISerializable.
