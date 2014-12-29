Updating
========

Notes
-----

These are general notes on updating, for specfic notes on updating check the instructions for the version you are updating to below.

In Visual Studio, open the Package Manager Console, and run:

::

    Update-Package -Safe

Will only update packages where Major and Minor versions match what you have installed, which should work for patch updates (i.e. from 2.0.0 to 2.0.4) but will not update minor or major releases.

If any of the assemblies that have been updated are strongly named (most are unfortunatley), you would need to add assembly binding redirects to your applications config files (including the test projects). Luckily this can be done easily with nuget, in the Package Manager Console select run:

::

    foreach ($proj in get-project -all) {Add-BindingRedirect -ProjectName $proj.Name}


2.0 to 2.1
----------

You can pick and choose which of those packages you want updates, if you just want to update NHibernate, then just run the command with SharpArhc.NHibernate

::

    Update-Package SharpArch.Web.Mvc.Castle
    Update-Package SharpArch.NHibernate
    Update-Package SharpArch.Testing.NUnit
    Update-Package SharpArch.Domain

If you get problems with Iesi.Collections version mismatch, this is because versions of NHibernate < 3.3.1 didn't specify the highest version of Iesi.Collections, causing problems now that there is a major version change released. To fix this, add allowedVersions=(,4.0) as an attirbute to the Iesi.Collections element to any packages.config file that has it.
