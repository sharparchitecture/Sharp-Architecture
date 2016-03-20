This example tests project has been provided to demonstrate a number of unit tests being run 
against a "live" development database.  The preferred mechanism is to use an in-memory SqlLite
database, which gets built at test execution time, to run database unit tests.  See the 
Northwind.Tests samples project for examples.  Essentially, this project has been added primarily 
for demonstrating backward compatibility with the previous approach.