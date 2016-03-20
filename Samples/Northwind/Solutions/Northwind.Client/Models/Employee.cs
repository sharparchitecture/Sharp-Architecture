// Employee.cs
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using jQueryApi;
using jQueryApi.Templating;

namespace Northwind.Client.Models
{
    public class Employee : Entity 
    {
        [PreserveCase]
        public string FirstName;

        [PreserveCase]
        public string FullName;

        [PreserveCase]
        public string LastName;

        [PreserveCase]
        public int PhoneExtension;

        [PreserveCase]
        public Territory[] Territories;

        [PreserveCase]
        public string TerritoriesString;

        public Employee()
        {
            this.Id = 0;
        }

        public IEnumerator GetEnumerator() 
        {
            return (this.Territories as IEnumerable).GetEnumerator();
        }

    }

    public interface IEmployeeService
    {
        void DisplayForm(Employee employee);

        void ShowValidationResults(Employee employee);
    }

    public sealed class EmployeeService : IEmployeeService
    {
        // See: EmployeeController.Details for Script.Literal() explanation
        public void DisplayForm(Employee employee)
        {
            jQueryObject employeeForm = jQueryTemplating.RenderTemplate(jQuery.Select("#employee-form-tmpl").GetHtml(), employee);

            jQuery.Select("#main").Append(employeeForm);
            jQuery.Select("#employee-form").Cast<Dialogs>()
                                           .Dialog(Script.Literal(@"{minWidth:500,close: function(event, ui){$(this).dialog('destroy').remove();}}"));
        }

        public void ShowValidationResults(Employee employee) 
        {
            foreach (ValidationResult validationResult in employee.ValidationResultsJson) 
            {
                if (validationResult.PropertyName == "FirstName") 
                {
                    jQuery.Select("#FirstName").Append("<div class=\"error-message\">" + validationResult.Message + "</div>");
                }

                if (validationResult.PropertyName == "LastName") 
                {
                    jQuery.Select("#LastName").Append("<div class=\"error-message\">" + validationResult.Message + "</div>");
                }

                if (validationResult.PropertyName == "PhoneExtension") 
                {
                    jQuery.Select("#PhoneExtension").Append("<div class=\"error-message\">" + validationResult.Message + "</div>");
                }
            }

            jQuery.Select("#employee-save").Attribute("disabled", string.Empty);
        }
    }
}
