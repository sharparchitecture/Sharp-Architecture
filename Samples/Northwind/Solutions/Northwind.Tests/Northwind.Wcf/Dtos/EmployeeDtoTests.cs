namespace Tests.Northwind.Wcf.Dtos
{
    using global::Northwind.Domain.Organization;
    using global::Northwind.WcfServices.Dtos;

    using NUnit.Framework;

    using SharpArch.Testing.NUnit;
    using SharpArch.Testing.NUnit.Helpers;

    [TestFixture]
    public class EmployeeDtoTests
    {
        [Test]
        public void CanCreateDtoWithEntity()
        {
            Employee employee = new Employee { FirstName = "Steven", LastName = "Buchanan" };
            EntityIdSetter.SetIdOf(employee, 5);

            EmployeeDto employeeDto = EmployeeDto.Create(employee);

            employeeDto.Id.ShouldEqual(5);
            employeeDto.FirstName.ShouldEqual("Steven");
            employeeDto.LastName.ShouldEqual("Buchanan");
        }
    }
}
