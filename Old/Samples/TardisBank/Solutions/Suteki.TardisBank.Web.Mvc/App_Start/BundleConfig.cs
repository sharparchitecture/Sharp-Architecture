namespace Suteki.TardisBank.Web.Mvc
{
    using System.Web.Optimization;

    /// <summary>
    /// Configures script and CSS bundles.
    /// </summary>
    public static class BundleConfig
    {
        /// <summary>
        /// News script bundle.
        /// </summary>
        public const string NewsScript = "~/bundles/news";
        /// <summary>
        /// Core scripts bundle.
        /// Includes jQuery, templates and other non-UI code.
        /// </summary>
        public const string CoreScript = "~/bundles/jquery";

        /// <summary>
        /// Register scripts.
        /// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        /// </summary>
        /// <param name="bundles"></param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle(CoreScript).Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js",
                "~/Scripts/jsrender.js",
                "~/Scripts/app/common.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                "~/Scripts/jquery-ui-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                //"~/Content/bootstrap.css",
                "~/Content/reset.css",
                "~/Content/text.css",
                "~/Content/960.css",
                "~/Content/Site.css",
                "~/Content/menu_style.css",
                "~/Content/themes/cupertino/jquery-ui.cupertino.css",
                "~/Content/font-awesome.css"
                ));

            bundles.Add(new ScriptBundle(NewsScript).Include(
                "~/Scripts/app/news.view.js",
                "~/Scripts/app/news.edit.js")
                );
        }
    }
}
