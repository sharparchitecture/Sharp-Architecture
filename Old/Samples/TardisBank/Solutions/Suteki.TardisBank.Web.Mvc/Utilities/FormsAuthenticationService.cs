namespace Suteki.TardisBank.Web.Mvc.Utilities
{
    using System;
    using System.Web;
    using System.Web.Security;

    public interface IFormsAuthenticationService
    {
        void SignOut();
        void SetAuthCookie(string userName, string[] roles, bool createPersistentCookie);
        string HashAndSalt(string userName, string password);
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public void SetAuthCookie(string userName, string[] roles, bool createPersistentCookie)
        {
            var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: userName,
                issueDate: DateTime.UtcNow,
                expiration: DateTime.UtcNow.Add(FormsAuthentication.Timeout),
                isPersistent: createPersistentCookie,
                userData: roles.Join(","),
                cookiePath: FormsAuthentication.FormsCookiePath
                );

            var hash = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);

            if (ticket.IsPersistent)
                cookie.Expires = ticket.Expiration;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public string HashAndSalt(string userName, string password)
        {
            var saltedPassword = userName + password;
            // hint: Use secure password hashing algorithms in real apps. 
            // E.g. https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing
            return FormsAuthentication.HashPasswordForStoringInConfigFile(saltedPassword, "SHA1");
        }
    }
}