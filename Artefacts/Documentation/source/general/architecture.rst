Architecture
============

The project is divided into the following layers:

-  Tasks
-  Domain
-  Infrastructure
-  Presentation
-  Specs/Tests

Despite their divergent names, the layers and their functions should be
familiar to anyone with experience with the MVC pattern.

Tasks
-----

Previous known as "Application Services," the *Tasks Layer* serves to
tie together any non-business logic from a variety of third-party
services or persistence technologies. While setup and defining a service
such as Twitter would occur in the *Infrastructure Layer*, executing and
combining the results with, say, a local NHibernate database, would
occur in the *Tasks Layer*. The resulting viewModel or DTO would be sent
down to the *Presentation Layer*

Additional occupants
^^^^^^^^^^^^^^^^^^^^

-  EventHandlers
-  CommandHandlers
-  Commands

Presentation
------------

The *Presentation Layer* contains the familiar MVC project with views,
viewmodels and controllers, along with application setup tasks. Previous
iterations of Sharp Architecture spun out controllers to a separate
application, but as of 2.0 this is no longer the case.

Additional occupants
^^^^^^^^^^^^^^^^^^^^

-  ViewModels: These can live in the Presentation or in the Tasks project, depending on whether your tasks layer will be returning ViewModels which is not usually the case as ViewModels are tied to a specific view.
-  Query Objects: These can live in the Presentation or in the Tasks project, depending on whether you need the queries in your tasks layer.
-  Controllers
-  Views

Infrastructure
--------------

The *Infrastructure Layer* setups up third party data sources, along
with items such as NHibernate maps. You can extend the repository implementation
with additional methods to perform specific queries, but it is recommended 
to write your own Query Objects as shown in the `cookbook. <https://github.com/sharparchitecture/Sharp-Architecture-Cookbook/wiki/Using-Query-Objects>`_

Domain
------

The *Domain Layer* is where business entities and other business logic
resides. The domain layer should be persistence ignorant, with any
persistence existing in the *Tasks Layer* or in the *Presentation Layer*
(for populating viewModels).

Additional occupants
^^^^^^^^^^^^^^^^^^^^

-  Contracts for Tasks
-  Contracts for IQuery
-  Events that are emitted by your domain

