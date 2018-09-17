Overview
========

TransactionAttribute is a `MVC ActionFilter <https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1#action-filters>`_ 
which is used to wrap Controller action in transaction. Transaction will be committed on successful action execution or rolled back in case of unhandled exception
or validation error.

Implementation
--------------


