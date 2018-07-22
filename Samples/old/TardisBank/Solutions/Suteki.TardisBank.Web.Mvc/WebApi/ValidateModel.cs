namespace Suteki.TardisBank.Web.Mvc.WebApi
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    /// <summary>
    ///     Automatically validate model.
    /// </summary>
    /// <remarks>
    ///     Taken from article http://www.asp.net/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api
    /// </remarks>
    public sealed class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }
    }
}