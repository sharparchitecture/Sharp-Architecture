namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System;
    using System.Web.Mvc;

    using Suteki.TardisBank.Tasks;
    using Suteki.TardisBank.Web.Mvc.Utilities;

    public class ScheduleRunnerController : Controller
    {
        readonly ISchedulerService schedulerService;
        readonly TardisConfiguration configuration;

        public ScheduleRunnerController(ISchedulerService schedulerService, TardisConfiguration configuration)
        {
            this.schedulerService = schedulerService;
            this.configuration = configuration;
        }

        [HttpGet, SharpArch.Web.Mvc.Transaction]
        public ActionResult Execute(string id)
        {
            if (id == null || this.configuration.ScheduleKey != id) return StatusCode.NotFound;

            this.schedulerService.ExecuteUpdates(DateTime.Now);
            return StatusCode.Ok;
        }
    }
}