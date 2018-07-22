namespace Suteki.TardisBank.Web.Mvc.Controllers.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class WithdrawCashViewModel
    {
        [Required]
        [StringLength(140, ErrorMessage = "Sorry maximum of 140 chars, just like Twitter :)")]
        public string Description { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal Amount { get; set; }
    }
}