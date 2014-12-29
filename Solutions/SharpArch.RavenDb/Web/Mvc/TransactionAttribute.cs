namespace SharpArch.RavenDb.Web.Mvc
{
    using System.Web.Mvc;

    using Raven.Client;

    using SharpArch.Domain;
    
    /// <summary>
    /// Transasction attribute that ensures RavenDbSession.SaveChanges is called if no errors occured.
    /// The transaction attribute DOES NOT create a new TransasctionScope, it only calls SaveChanges on the session.
    /// If you have multiple operations that you want to enlist in a transaction scope, then take a look at the SharpArchContrib System.Transacion attribute.
    /// </summary>
    public class TransactionAttribute : ActionFilterAttribute
    {
        public bool RollbackOnModelStateError { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            IDocumentSession session = SafeServiceLocator<IDocumentSession>.GetService();
        
            if (((filterContext.Exception != null) && filterContext.ExceptionHandled) ||
                this.ShouldRollback(filterContext))
            {
                return;
            }

            session.SaveChanges();
        }

        private bool ShouldRollback(ControllerContext filterContext)
        {
            return this.RollbackOnModelStateError && !filterContext.Controller.ViewData.ModelState.IsValid;
        }
    }
}