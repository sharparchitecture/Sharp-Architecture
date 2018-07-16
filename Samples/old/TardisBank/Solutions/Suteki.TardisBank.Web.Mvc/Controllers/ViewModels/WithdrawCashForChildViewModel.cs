namespace Suteki.TardisBank.Web.Mvc.Controllers.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class WithdrawCashForChildViewModel
    {
        public int ChildId{ get; set; }
        public string ChildName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal Amount { get; set; }
    }
}