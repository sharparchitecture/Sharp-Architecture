// ReSharper disable InternalMembersMustHaveComments

namespace Tests.Helpers
{
    using Moq;
    using NUnit.Framework;

    internal abstract class MockedTestsBase
    {
        protected MockRepository Mockery { get; private set; }


        [SetUp]
        public void SetUp()
        {
            this.Mockery = new MockRepository(MockBehavior.Default);
            DoSetUp();
        }

        protected abstract void DoSetUp();
    }
}
