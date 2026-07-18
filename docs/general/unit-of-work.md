# Unit of Work

The [unit of work](https://martinfowler.com/eaaCatalog/unitOfWork.html) pattern for
ASP.NET Core is implemented using a combination of `TransactionAttribute` and
`AutoTransactionHandler`.

`TransactionAttribute` is a marker attribute used to configure the unit of work for a
controller action; it does not implement any transaction management itself. It
configures the following parameters:

- isolation level;
- whether the transaction is committed or rolled back in case of a model validation
  error.

`AutoTransactionHandler` is an
[MVC action filter](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)
that wraps a controller action in a transaction, using the settings from the
`[Transaction]` attribute applied to that action or its controller. It commits the
transaction on success and rolls it back on an unhandled exception (and, optionally, on
a model-validation error).

## Best practices

- Apply `[Transaction]` at the *controller* or *action* level. Applying it globally
  will cause a transaction to be opened on every request, including requests that don't
  need one.
- Register `AutoTransactionHandler` globally, as an MVC filter. It is stateless, so
  registering it as a singleton reduces memory allocations and improves performance.
