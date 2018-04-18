// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable MemberCanBePrivate.Global

namespace Tests.SharpArch.Web.Http
{
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Filters;
    using System.Web.Http.Hosting;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.Web.Http;
    using Moq;
    using Tests.Helpers;

    internal abstract class TransactionAttributeTestsBase : MockedTestsBase
    {
        private Mock<IDependencyScope> dependencyScope;
        protected Mock<ITransactionManager> TransactionManagerMock { get; set; }
        protected TransactionAttribute TransactionAttribute { get; set; }
        protected HttpRequestMessage RequestMessage { get; private set; }

        protected override void DoSetUp()
        {
            this.TransactionManagerMock = Mockery.Create<ITransactionManager>();
            this.dependencyScope = Mockery.Create<IDependencyScope>();
            this.dependencyScope.Setup(s => s.GetService(typeof(ITransactionManager)))
                .Returns(this.TransactionManagerMock.Object);

            this.RequestMessage = new HttpRequestMessage();
            this.RequestMessage.Properties.Add(HttpPropertyKeys.DependencyScope, this.dependencyScope.Object);

            this.TransactionAttribute = new TransactionAttribute();
        }

        protected HttpControllerContext CreateHttpControllerContext()
        {
            return new HttpControllerContext {Request = RequestMessage};
        }

        protected HttpActionContext CreateActionExecutingContext()
        {
            return new HttpActionContext {ControllerContext = CreateHttpControllerContext()};
        }

        protected HttpActionExecutedContext CreateActionExecutedContext()
        {
            return new HttpActionExecutedContext
            {
                ActionContext = new HttpActionContext
                {
                    ControllerContext = CreateHttpControllerContext()
                }
            };
        }
    }
}
