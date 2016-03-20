namespace Northwind.WcfServices.Dtos
{
    using Northwind.Domain.Organization;

    public class EmployeeDto
    {
        private EmployeeDto()
        {
        }

        public string FirstName { get; set; }

        public int Id { get; set; }

        public string LastName { get; set; }

        /// <summary>
        ///   Transfers the employee entity's property values to the DTO.
        ///   Strongly consider Jimmy Bogard's AutoMapper (http://automapper.codeplex.com/) 
        ///   for doing this kind of work in a more automated fashion.
        /// </summary>
        public static EmployeeDto Create(Employee employee)
        {
            if (employee == null)
            {
                return null;
            }

            return new EmployeeDto { Id = employee.Id, FirstName = employee.FirstName, LastName = employee.LastName };
        }
    }
}