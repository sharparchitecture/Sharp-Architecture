// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable PublicMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation.Evident
namespace Tests.Helpers
{
    using Castle.Windsor;
    using NUnit.Framework;

    internal abstract class WindsorTestsBase
    {
        protected WindsorContainer Container { get; private set; }

        [SetUp]
        public void SetUp()
        {
            this.Container = new WindsorContainer();
            Initialize(Container);
        }

        [TearDown]
        public void TearDown()
        {
            Container?.Dispose();
         }

        protected abstract void Initialize(WindsorContainer container);
    }
}
