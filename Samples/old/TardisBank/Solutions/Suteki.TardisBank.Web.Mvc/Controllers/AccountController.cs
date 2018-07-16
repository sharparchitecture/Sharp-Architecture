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

    public class AccountController : Controller
    {
        readonly IMediator mediator;
        readonly IUserService userService;

        public AccountController(IUserService userService, IMediator mediator)
        {
            this.userService = userService;
            this.mediator = mediator;
        }

        [HttpGet, Transaction]
        public ActionResult MakePayment(int id)
        {
            // id is the child's user name
            if (id == 0)
            {
                throw new ArgumentNullException("id");
            }

            var parent = userService.CurrentUser as Parent;
            var child = userService.GetUser(id) as Child;

            if (userService.AreNullOrNotRelated(parent, child))
            {
                return StatusCode.NotFound;
            }

            return View("MakePayment", new MakePaymentViewModel
            {
                ChildId = child.Id,
                ChildName = child.Name,
                Description = "",
                Amount = 0M
            });
        }

        [HttpPost, Transaction]
        public ActionResult MakePayment(MakePaymentViewModel makePaymentViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("MakePayment", makePaymentViewModel);
            }
            if (makePaymentViewModel == null)
            {
                throw new ArgumentNullException("makePaymentViewModel");
            }

            if (makePaymentViewModel.Amount == 0M)
            {
                ModelState.AddModelError("Amount", "A payment of zero? That's not nice.");
                return View("MakePayment", makePaymentViewModel);
            }

            var parent = userService.CurrentUser as Parent;
            var child = userService.GetUser(makePaymentViewModel.ChildId) as Child;

            if (userService.AreNullOrNotRelated(parent, child))
            {
                return StatusCode.NotFound;
            }

            parent.MakePaymentTo(child, makePaymentViewModel.Amount, makePaymentViewModel.Description);

            return View("PaymentConfirmation", makePaymentViewModel);
        }


        [HttpGet, Transaction]
        public ActionResult ParentView(int id)
        {
            var parent = userService.CurrentUser as Parent;
            var child = userService.GetUser(id) as Child;

            if (userService.AreNullOrNotRelated(parent, child))
            {
                return StatusCode.NotFound;
            }

            return View("Summary", new AccountSummaryViewModel
            {
                Parent = parent,
                Child = child
            });
        }

        [HttpGet, Transaction]
        public ActionResult ChildView()
        {
            var child = userService.CurrentUser as Child;
            if (child == null)
            {
                return StatusCode.NotFound;
            }
            return View("Summary", new AccountSummaryViewModel
            {
                Child = child
            });
        }

        [HttpGet, Transaction]
        public ActionResult WithdrawCash()
        {
            var child = userService.CurrentUser as Child;
            if (child == null)
            {
                return StatusCode.NotFound;
            }

            return View("WithdrawCash", new WithdrawCashViewModel
            {
                Amount = 0M,
                Description = ""
            });
        }

        [HttpPost, Transaction]
        public ActionResult WithdrawCash(WithdrawCashViewModel withdrawCashViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("WithdrawCash", withdrawCashViewModel);
            }
            if (withdrawCashViewModel == null)
            {
                throw new ArgumentNullException("withdrawCashViewModel");
            }

            if (withdrawCashViewModel.Amount == 0M)
            {
                ModelState.AddModelError("Amount", "There's no point in asking for zero cash.");
                return View("WithdrawCash", withdrawCashViewModel);
            }

            var child = userService.CurrentUser as Child;
            if (child == null)
            {
                return StatusCode.NotFound;
            }
            var parent = userService.GetUser(child.ParentId) as Parent;
            if (parent == null)
            {
                throw new TardisBankException("Parent with id '{0}' not found", child.ParentId);
            }

            try
            {
                child.WithdrawCashFromParent(
                    parent,
                    withdrawCashViewModel.Amount,
                    withdrawCashViewModel.Description, mediator);
            }
            catch (CashWithdrawException cashWithdrawException)
            {
                ModelState.AddModelError("Amount", cashWithdrawException.Message);
                return View("WithdrawCash", withdrawCashViewModel);
            }

            return View("WithdrawCashConfirm", withdrawCashViewModel);
        }

        [HttpGet, Transaction]
        public ActionResult WithdrawCashForChild(int id)
        {
            var parent = userService.CurrentUser as Parent;
            var child = userService.GetUser(id) as Child;

            if (userService.AreNullOrNotRelated(parent, child))
            {
                return StatusCode.NotFound;
            }

            return View(new WithdrawCashForChildViewModel
            {
                ChildId = child.Id,
                ChildName = child.Name,
                Description = "",
                Amount = 0M
            });
        }

        [HttpPost, Transaction]
        public ActionResult WithdrawCashForChild(WithdrawCashForChildViewModel withdrawCashForChildViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(withdrawCashForChildViewModel);
            }
            if (withdrawCashForChildViewModel == null)
            {
                throw new ArgumentNullException("withdrawCashForChildViewModel");
            }

            if (withdrawCashForChildViewModel.Amount == 0M)
            {
                ModelState.AddModelError("Amount", "0.00 is not a valid amount.");
                return View(withdrawCashForChildViewModel);
            }

            var child = userService.GetUser(withdrawCashForChildViewModel.ChildId) as Child;
            var parent = userService.CurrentUser as Parent;
            if (userService.AreNullOrNotRelated(parent, child))
            {
                return StatusCode.NotFound;
            }

            try
            {
                child.AcceptCashFromParent(
                    parent,
                    withdrawCashForChildViewModel.Amount,
                    withdrawCashForChildViewModel.Description);
            }
            catch (CashWithdrawException cashWithdrawException)
            {
                ModelState.AddModelError("Amount", cashWithdrawException.Message);
                return View(withdrawCashForChildViewModel);
            }

            return View("WithdrawCashForChildConfirm", withdrawCashForChildViewModel);
        }
    }
}
