namespace Suteki.TardisBank.Web.Mvc.Utilities
{
    using System.Threading;

    public class Current
    {
        public static string CurrencySymbol
        {
            get { return Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol; }
        }
    }
}