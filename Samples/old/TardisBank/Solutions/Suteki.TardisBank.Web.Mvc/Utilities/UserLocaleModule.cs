namespace Suteki.TardisBank.Web.Mvc.Utilities
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Web;

    public class UserLocaleModule : IHttpModule
    {
        public void Init(HttpApplication httpApplication)
        {
            httpApplication.BeginRequest += (sender, eventArgs) =>
            {
                var app = sender as HttpApplication;
                if (app == null)
                {
                    throw new ApplicationException("Sender is null or not an HttpApplication");
                }
                var request = app.Context.Request;
                if (request.UserLanguages == null || request.UserLanguages.Length == 0) return;

                var language = request.UserLanguages[0];
                if (language == null) return;

                try
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
                }
                catch
                {}
            };
        }

        public void Dispose()
        {
            
        }
    }
}