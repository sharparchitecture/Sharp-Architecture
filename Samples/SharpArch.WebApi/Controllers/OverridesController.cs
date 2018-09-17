using System.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SharpArch.AspNetCore;

namespace SharpArch.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Transaction(IsolationLevel.ReadCommitted)]
    public class OverridesController : ControllerBase
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<OverridesController>();

        [HttpGet("local")]
        [Transaction(IsolationLevel.ReadUncommitted)]
        public ActionResult<string> LocalOverride()
        {
            Log.Information("local-override");
            return "ok";
        }

        [HttpGet("controller")]
        public ActionResult<string> ControllerLevel()
        {
            Log.Information("controller-level");
            return "ok";
        }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class GlobalController : ControllerBase
    {
        [HttpGet("default")]
        public ActionResult<string> Default()
        {
            Log.Information("default");
            return "ok";
        }
    }
}
