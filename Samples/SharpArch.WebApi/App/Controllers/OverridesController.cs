using System.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SharpArch.AspNetCore.Transaction;

namespace SharpArch.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Transaction(IsolationLevel.ReadCommitted)]
    public class OverridesController : ControllerBase
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<OverridesController>();

        /// <summary>
        ///     Action-level override.
        /// </summary>
        /// <returns></returns>
        [HttpGet("local")]
        [Transaction(IsolationLevel.ReadUncommitted)]
        public ActionResult<string> LocalOverride()
        {
            Log.Information("local-override");
            return "ok";
        }

        [HttpGet("invalid-model")]
        public ActionResult InvalidModel()
        {
            ModelState.AddModelError("*", "Forced validation error.");
            Log.Information("invalid model");
            return BadRequest("forced model validation error");
        }

        [HttpGet("controller")]
        public ActionResult<string> ControllerLevel()
        {
            Log.Information("controller-level");
            return "ok";
        }
    }
}
