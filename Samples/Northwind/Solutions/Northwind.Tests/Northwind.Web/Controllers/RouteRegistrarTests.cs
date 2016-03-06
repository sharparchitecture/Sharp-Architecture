namespace Tests.Northwind.Web.Controllers
{
    using System.Web.Routing;

    using MvcContrib.TestHelper;

    using NUnit.Framework;

    [TestFixture]
    public class RouteRegistrarTests
    {
        [SetUp]
        public void SetUp()
        {
            RouteTable.Routes.Clear();
            //RouteRegistrar.RegisterRoutesTo(RouteTable.Routes);
        }

        [Test]
        public void CanVerifyRouteMaps()
        {
            //"~/".Route().ShouldMapTo<HomeController>(x => x.Index());
        }
    }
}
