Although this DTO (Data Transfer Object) folder resides in Northwind.Wcf, a better design might be 
to create a Northwind.Dto project which could be leveraged by application services to transfer data 
to the presentation layer as well as being used by the WCF service.