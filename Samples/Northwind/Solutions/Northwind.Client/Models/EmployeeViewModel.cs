// Employee.cs
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using jQueryApi;
using jQueryApi.Templating;

namespace Northwind.Client.Models
{
    public class EmployeeViewModel
    {
        [PreserveCase]
        public Employee[] Employees;

        [PreserveCase]
        public Territory[] AvailableTerritories;
    }
}
