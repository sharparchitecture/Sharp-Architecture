using System;
using System.Linq;
using System.Web.Mvc;
using Castle.Core;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Castle.Windsor;
using NUnit.Framework;

namespace Suteki.TardisBank.Tests.Container
{
    using System.Web.Http;
    using global::Suteki.TardisBank.Web.Mvc.CastleWindsor;
    using global::Suteki.TardisBank.Web.Mvc.Controllers;

    [TestFixture]
    public class ControllerRegistrationTests
    {

        [SetUp]
        public void SetUp()
        {
            types = type.Assembly.GetExportedTypes();
            container = new WindsorContainer().Install(new MvcControllersInstaller());
        }

        private IWindsorContainer container;
        private readonly Type type = typeof (HomeController);
        private Type[] types;

        private IHandler[] ControllerHandlers()
        {
            return container.Kernel.GetAssignableHandlers(typeof (IController));
        }

        [Test]
        public void Controllers_are_transient()
        {
            var nonTransientControllers = ControllerHandlers()
                .Where(h => h.ComponentModel.LifestyleType != LifestyleType.Transient);

            Assert.IsEmpty(nonTransientControllers.ToArray());
        }

        [Test]
        public void Controllers_have_Controller_name_suffix()
        {
            var controllers = ControllerHandlers().Select(h => h.ComponentModel.Implementation).ToSet();
            var namedControllers = types.Where(t => t.Name.EndsWith("Controller") && IsMvcController(t)).ToSet();

            controllers.SymmetricExceptWith(namedControllers);

            Assert.IsEmpty(controllers.ToArray());
        }

        static bool IsMvcController(Type t)
        {
            return typeof(ApiController).IsAssignableFrom(t) == false;
        }


        [Test]
        public void MvcControllers_implement_IController()
        {
            var controllers = ControllerHandlers().Select(h => h.ComponentModel.Implementation).ToSet();
            var typedControllers = types.Where(t => t.Is<IController>()).ToSet();

            controllers.SymmetricExceptWith(typedControllers);

            Assert.IsEmpty(controllers.ToArray());
        }

        [Test]
        public void MvcControllers_live_in_controllers_namespace()
        {
            var controllers = ControllerHandlers().Select(h => h.ComponentModel.Implementation).ToSet();
            var typesInControllersNamespace = types.Where(t => t.Namespace == type.Namespace && IsMvcController(t)).ToSet();

            controllers.SymmetricExceptWith(typesInControllersNamespace);

            Assert.IsEmpty(controllers.ToArray());
        }

        [Test]
        public void Controllers_use_full_impl_name_as_id()
        {
            var improperlyNamedControllers = ControllerHandlers()
                .Where(h => h.ComponentModel.Implementation.FullName != h.ComponentModel.Name);

            Assert.IsEmpty(improperlyNamedControllers.ToArray());
        }
    }
}