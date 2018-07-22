namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System;
    using System.Web.Mvc;

    using Domain;
    using Tasks;
    using ViewModels;
    using Utilities;

    public class ScheduleController : Controller
    {
        readonly IUserService userService;

        public ScheduleController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet, SharpArch.Web.Mvc.Transaction]
        public ActionResult AddSchedule(int id)
        {
            // id is the child's username
            var child = this.userService.GetUser(id) as Child;
            if (this.userService.IsNotChildOfCurrentUser(child)) return StatusCode.NotFound;

            // give the user some defaults
            var addScheduleViewModel = new AddScheduleViewModel
            {
                ChildId = child.Id,
                Amount = 1.0M,
                Description = "Pocket Money",
                Interval = Interval.Week,
                StartDate = DateTime.Now
            };

            return this.View("AddSchedule", addScheduleViewModel);
        }

        [HttpPost, SharpArch.Web.Mvc.Transaction]
        public ActionResult AddSchedule(AddScheduleViewModel addScheduleViewModel)
        {
            if (!this.ModelState.IsValid) return this.View("AddSchedule", addScheduleViewModel);

            if (addScheduleViewModel.StartDate < DateTime.Now.Date)
            {
                this.ModelState.AddModelError("StartDate", "The start date can not be in the past.");
                return this.View("AddSchedule", addScheduleViewModel);
            }

            var child = this.userService.GetUser(addScheduleViewModel.ChildId) as Child;
            if (this.userService.IsNotChildOfCurrentUser(child)) return StatusCode.NotFound;

            child.Account.AddPaymentSchedule(
                addScheduleViewModel.StartDate,
                addScheduleViewModel.Interval,
                addScheduleViewModel.Amount,
                addScheduleViewModel.Description
                );

            return this.View("AddScheduleConfirm", addScheduleViewModel);
        }

        [HttpGet, SharpArch.Web.Mvc.Transaction]
        public ActionResult RemoveSchedule(int id, int scheduleId)
        {
            // id is the child user name
            var child = this.userService.GetUser(id) as Child;
            if (this.userService.IsNotChildOfCurrentUser(child)) return StatusCode.NotFound;

            child.Account.RemovePaymentSchedule(scheduleId);

            return this.Redirect(this.Request.UrlReferrer.OriginalString);
        }
    }
}