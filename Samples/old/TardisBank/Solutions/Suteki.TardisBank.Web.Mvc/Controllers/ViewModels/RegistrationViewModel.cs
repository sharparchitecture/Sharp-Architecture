namespace Suteki.TardisBank.Web.Mvc.Controllers.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "You must supply an email address")]
        [StringLength(140, ErrorMessage = "Sorry maximum of 140 chars, just like Twitter :)")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must supply a name")]
        [StringLength(140, ErrorMessage = "Sorry maximum of 140 chars, just like Twitter :)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "You must supply a password")]
        public string Password { get; set; }
    }
}