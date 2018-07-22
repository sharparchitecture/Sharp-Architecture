namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System;
    using System.Web.Mvc;
    using MediatR;
    using SharpArch.Web.Mvc;
    using Suteki.TardisBank.Domain;
    using Suteki.TardisBank.Tasks;
    using Suteki.TardisBank.Web.Mvc.Controllers.ViewModels;
    using Suteki.TardisBank.Web.Mvc.Utilities;

    public class UserController : Controller
    {
        readonly TardisConfiguration configuration;
        readonly IFormsAuthenticationService formsAuthenticationService;
        readonly IMediator mediator;
        readonly IUserService userService;

        public UserController(IUserService userService, IFormsAuthenticationService formsAuthenticationService,
            TardisConfiguration configuration, IMediator mediator)
        {
            this.userService = userService;
            this.formsAuthenticationService = formsAuthenticationService;
            this.configuration = configuration;
            this.mediator = mediator;
        }

        [ChildActionOnly]
        public ActionResult DisplayGreeting()
        {
            var user = userService.CurrentUser;
            var userName = user == null ? "Hello Stranger!" : user.UserName;
            return this.PartialView("DisplayGreeting", new UserViewModel { UserName = userName, IsLoggedIn = user != null });
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View("Register", GetRegistrationViewModel());
        }

        static RegistrationViewModel GetRegistrationViewModel()
        {
            return new RegistrationViewModel
            {
                Email = "",
                Name = "",
                Password = ""
            };
        }

        [HttpPost, Transaction]
        public ActionResult Register(RegistrationViewModel registrationViewModel)
        {
            return RegisterInternal(registrationViewModel, "Sorry, that email address has already been registered.",
                pwd =>
                    new Parent(registrationViewModel.Name, registrationViewModel.Email, pwd).Initialise(mediator),
                () => RedirectToAction("Confirm"), () => View("Register", registrationViewModel)
                );
        }

        ActionResult RegisterInternal(
            RegistrationViewModel registrationViewModel,
            string usernameTakenMessage,
            Func<string, User> createUser,
            Func<ActionResult> confirmAction,
            Func<ActionResult> invalidModelStateAction,
            Action<User> afterUserCreated = null)
        {
            if (registrationViewModel == null)
            {
                throw new ArgumentNullException("registrationViewModel");
            }
            if (createUser == null)
            {
                throw new ArgumentNullException("createUser");
            }
            if (confirmAction == null)
            {
                throw new ArgumentNullException("confirmAction");
            }
            if (invalidModelStateAction == null)
            {
                throw new ArgumentNullException("invalidModelStateAction");
            }

            if (ModelState.IsValid)
            {
                var conflictedUser = userService.GetUserByUserName(registrationViewModel.Email);
                if (conflictedUser != null)
                {
                    ModelState.AddModelError("Email", usernameTakenMessage);
                    return invalidModelStateAction();
                }

                var hashedPassword = formsAuthenticationService.HashAndSalt(
                    registrationViewModel.Email,
                    registrationViewModel.Password);

                var user = createUser(hashedPassword);

                if (string.IsNullOrWhiteSpace(configuration.EmailSmtpServer))
                {
                    // if no smtp server configured, just activate user as no email is sent out.
                    user.Activate();
                }

                userService.SaveUser(user);

                if (afterUserCreated != null)
                {
                    afterUserCreated(user);
                }
                return confirmAction();
            }

            return invalidModelStateAction();
        }

        [HttpGet]
        public ActionResult Confirm()
        {
            return View("Confirm");
        }

        [HttpGet, Transaction]
        public ActionResult Activate(string id)
        {
            // id is the activation key
            var user = userService.GetUserByActivationKey(id);
            if (user == null)
            {
                return View("ActivationFailed");
            }
            user.Activate();
            return View("ActivateConfirm");
        }

        [HttpGet]
        public ActionResult Login()
        {
            var loginViewModel = new LoginViewModel
            {
                Name = "",
                Password = ""
            };
            return View("Login", loginViewModel);
        }

        [HttpPost, Transaction]
        public ActionResult Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = userService.GetUserByUserName(loginViewModel.Name);
                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(
                            "Name", "Please activate your account first by clicking on the link in your " +
                                "activation email.");
                        return View("Login", loginViewModel);
                    }

                    var hashedPassword = formsAuthenticationService.HashAndSalt(
                        loginViewModel.Name,
                        loginViewModel.Password);

                    if (hashedPassword == user.Password)
                    {
                        formsAuthenticationService.SetAuthCookie(user.UserName, GetRoles(user), false);
                        if (user is Child)
                        {
                            return RedirectToAction("ChildView", "Account");
                        }
                        return RedirectToAction("Index", "Child");
                    }
                    ModelState.AddModelError("Password", "Invalid Password");
                }
                else
                {
                    ModelState.AddModelError("Name", "Invalid Name");
                }
            }

            return View("Login", loginViewModel);
        }

        string[] GetRoles(User user)
        {
            return user.GetRoles();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            formsAuthenticationService.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet, Transaction]
        public ActionResult AddChild()
        {
            var parent = userService.CurrentUser as Parent;
            if (parent == null)
            {
                //throw new TardisBankException("You must be a parent in order to register a Child");
                return StatusCode.NotFound;
            }

            return View("AddChild", GetRegistrationViewModel());
        }

        [HttpPost, Transaction]
        public ActionResult AddChild(RegistrationViewModel registrationViewModel)
        {
            var parent = userService.CurrentUser as Parent;
            if (parent == null)
            {
                //throw new TardisBankException("You must be a parent in order to register a Child");
                return StatusCode.NotFound;
            }

            return RegisterInternal(registrationViewModel, "Sorry, that user name has already been taken",
                pwd => parent.CreateChild(registrationViewModel.Name, registrationViewModel.Email, pwd),
                () => RedirectToAction("Index", "Child"), () => View("AddChild", registrationViewModel)
                );
        }

        [HttpGet, Transaction]
        public ActionResult Messages()
        {
            var user = userService.CurrentUser;
            if (user == null)
            {
                return StatusCode.NotFound;
            }
            return View("Messages", user);
        }

        [HttpGet, Transaction]
        public ActionResult ReadMessage(int id)
        {
            var user = userService.CurrentUser;
            if (user == null)
            {
                return StatusCode.NotFound;
            }
            user.ReadMessage(id);
            return RedirectToAction("Messages");
        }
    }
}
