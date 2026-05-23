namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System.Web.Mvc;

    using Suteki.TardisBank.Domain;
    using Suteki.TardisBank.Tasks;
    using Suteki.TardisBank.Web.Mvc.Controllers.ViewModels;
    using Suteki.TardisBank.Web.Mvc.Utilities;

    public class AdminController : Controller
    {
        readonly IUserService userService;
        readonly IFormsAuthenticationService formsAuthenticationService;

        public AdminController(IUserService userService, IFormsAuthenticationService formsAuthenticationService)
        {
            this.userService = userService;
            this.formsAuthenticationService = formsAuthenticationService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        public ActionResult DeleteParentConfirm()
        {
            return this.View();
        }

        [HttpGet, SharpArch.Web.Mvc.Transaction]
        public ActionResult DeleteParent()
        {
            var parent = this.userService.CurrentUser as Parent;
            if (parent == null)
            {
                return StatusCode.NotFound;
            }

            foreach (var childProxy in parent.Children)
            {
                this.userService.DeleteUser(childProxy.Id);
            }
            this.userService.DeleteUser(parent.Id);

            return this.RedirectToAction("Logout", "User");
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return this.View();
        }

        [HttpPost, SharpArch.Web.Mvc.Transaction]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {

            var oldHashedPassword = this.GetHashedPassword(model.OldPassword);
            bool passwordIsOk = false;
            if (oldHashedPassword == this.userService.CurrentUser.Password)
            {
                passwordIsOk = true;
            }
            else
            {
                this.ModelState.AddModelError("OldPassword", "The password you provided is invalid");
            }
            if (this.ModelState.IsValid && passwordIsOk)
            {
                var newHashedPassword = this.GetHashedPassword(model.NewPassword);
                this.userService.CurrentUser.ResetPassword(newHashedPassword);
                // TODO: we should have also a flash message saying it's been successful
                return this.RedirectToAction("Index");
            }
            return this.View();
        }

        private string GetHashedPassword(string password)
        {
            return this.formsAuthenticationService.HashAndSalt(
                this.userService.CurrentUser.UserName,
                password);
        }

    }
}