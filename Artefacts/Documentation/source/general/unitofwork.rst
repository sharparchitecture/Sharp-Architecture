Unit of Work
============

`Unit of work <https://martinfowler.com/eaaCatalog/unitOfWork.html>`_ pattern for ASP.NET MVC is implemented
using combination of TransactionAttribute and UnitOfWorkHandler.

TransactionAttribute is used to configure unit of work for controller action, no actual transaction management
is implemented by it.

It configures following parameters:

- isolation level;
- whether transaction is committed or rolled back in case of model validation error;

UnitOfWorkHandler is a `MVC ActionFilter <https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1#action-filters>`_ 
which is used to wrap Controller action in transaction. 

Best practices
--------------

- Apply TransactionAttribute at *controller* or *action* level.
  Applying it globally will cause transaction open on every request.
- Register UnitOfWorkHandler globally.
  This class is stateless, using it as a singleton will reduce memory allocation count and improve performance.

