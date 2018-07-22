namespace Suteki.TardisBank.Web.Mvc.Controllers.ViewModels
{
    using Suteki.TardisBank.Domain;

    public class AccountSummaryViewModel
    {
        public Child Child { get; set; }
        public Parent Parent { get; set; }

        public bool IsParentView
        {
            get { return this.Parent != null;  }
        }
    }
}