namespace Suteki.TardisBank.Web.Mvc.Controllers.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ForgottenPasswordViewModel
    {
        [Required(ErrorMessage = "You must enter a User Name or Password")]
        public string UserName { get; set; }
    }
}