namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System;
    using System.Web.Mvc;

    using Suteki.TardisBank.Domain;
    using Suteki.TardisBank.Tasks;
    using Suteki.TardisBank.Web.Mvc.Controllers.ViewModels;
    using Suteki.TardisBank.Web.Mvc.Utilities;

    public class PasswordController : Controller
    {
        readonly IUserService userService;
        readonly IEmailService emailService;
        readonly IFormsAuthenticationService formsAuthenticationService;

        public PasswordController(IUserService userService, IEmailService emailService, IFormsAuthenticationService formsAuthenticationService)
        {
            this.userService = userService;
            this.formsAuthenticationService = formsAuthenticationService;
            this.emailService = emailService;
        }

        [HttpGet, SharpArch.Web.Mvc.Transaction]
        public ActionResult Forgot()
        {
            return this.View("Forgot", new ForgottenPasswordViewModel
            {
                UserName = "",
            });
        }

        [HttpPost, SharpArch.Web.Mvc.Transaction]
        public ActionResult Forgot(ForgottenPasswordViewModel forgottenPasswordViewModel)
        {
            if (!this.ModelState.IsValid) return this.View("Forgot", forgottenPasswordViewModel);
            if (forgottenPasswordViewModel == null)
            {
                throw new ArgumentNullException("forgottenPasswordViewModel");
            }

            var user = this.userService.GetUserByUserName(forgottenPasswordViewModel.UserName);
            if (user == null)
            {
                this.ModelState.AddModelError("UserName", "We don't have a record of that email or user name.");
                return this.View("Forgot", forgottenPasswordViewModel);
            }

            if (user is Child)
            {
                var parent = this.userService.GetUser(((Child)user).ParentId);
                if (parent == null)
                {
                    throw new TardisBankException("Missing parent: {0}", ((Child)user).ParentId);
                }

                this.SendPasswordResetEmail(user, toAddress: parent.UserName);
                return this.RedirectToAction("ChildConfirm");
            }

            if (user is Parent)
            {
                this.SendPasswordResetEmail(user, toAddress: user.UserName);
                return this.RedirectToAction("ParentConfirm");
            }

            throw new TardisBankException("unknown User subtype");
        }

        string GetNewPasswordFor(User user)
        {
            var newPassword = Guid.NewGuid().ToString().Substring(0, 5);
            var hashedPassword = this.formsAuthenticationService.HashAndSalt(user.UserName, newPassword);
            user.ResetPassword(hashedPassword);
            return newPassword;
        }

        void SendPasswordResetEmail(User user, string toAddress)
        {
            var isChildString = user is Child ? user.Name + "'s" : "Your";
            var subject = string.Format("Tardis Bank: {0} reset password", isChildString);
            var body = string.Format("Here is {0} new password: {1}", isChildString, this.GetNewPasswordFor(user));
            this.emailService.SendEmail(toAddress, subject, body);
        }

        [HttpGet]
        public ViewResult ChildConfirm()
        {
            return this.View();
        }

        [HttpGet]
        public ViewResult ParentConfirm()
        {
            return this.View();
        }
    }
}