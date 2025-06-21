namespace Suteki.TardisBank.Web.Mvc.Utilities
{
    public static class DateFormatter
    {
        public static string CurrentJQuery
        {
            get
            {
                var systemPattern = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                // change to a style that jQuery UI understands
                return systemPattern.Replace("M", "m").Replace("yyyy", "yy");
            }
        }
    }
}