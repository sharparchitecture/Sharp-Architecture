namespace Suteki.TardisBank.Tests
{
    using log4net.Config;
    using NUnit.Framework;

    [SetUpFixture]
    public class AssemblySetup
    {
        [OneTimeSetUp]
        public void RunBeforeAllTests()
        {
            XmlConfigurator.Configure();
            ServiceLocatorInitializer.Init();
        }
    }
}