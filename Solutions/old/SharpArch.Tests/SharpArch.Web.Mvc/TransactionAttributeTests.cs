// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident

namespace Tests.SharpArch.Web.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Web.Mvc;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.Web.Mvc;
    using Moq;
    using NUnit.Framework;
    using Tests.Helpers;

    [TestFixture]
    internal class TransactionAttribute_On_StartingAction_Tests : MockedTestsBase
    {
        private Mock<ITransactionManager> transactionManagerMock;
        private TransactionAttribute transactionAttribute;
        private Mock<ActionExecutingContext> filterContext;

        protected override void DoSetUp()
        {
            this.transactionManagerMock = Mockery.Create<ITransactionManager>();
            this.transactionAttribute = new TransactionAttribute
            {
                TransactionManager = this.transactionManagerMock.Object
            };
            this.filterContext = Mockery.Create<ActionExecutingContext>();
        }

        [Test]
        public void Should_IgnoreTransaction_on_ChildAction()
        {
            this.filterContext.SetupGet(c => c.IsChildAction).Returns(true);

            this.transactionAttribute.OnActionExecuting(filterContext.Object);

            this.transactionManagerMock.Verify(tm => tm.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Never,
                "nested transactions are not allowed");
        }

        [Test]
        public void Should_StartTransaction_on_TopLevelAction()
        {
            filterContext.SetupGet(c => c.IsChildAction).Returns(false);
            this.transactionAttribute.IsolationLevel = IsolationLevel.RepeatableRead;

            this.transactionAttribute.OnActionExecuting(filterContext.Object);

            this.transactionManagerMock.Verify(tm => tm.BeginTransaction(IsolationLevel.RepeatableRead),
                "should start transaction with given isolation level");
        }
    }

    [TestFixture]
    class TransactionAttribute_On_ActionCompleted_Tests : MockedTestsBase
    {
        private Mock<ITransactionManager> transactionManagerMock;
        private TransactionAttribute transactionAttribute;
        private Mock<ActionExecutedContext> filterContext;
        private FakeController fakeController;

        protected override void DoSetUp()
        {
            this.transactionManagerMock = Mockery.Create<ITransactionManager>();
            this.transactionAttribute = new TransactionAttribute
            {
                TransactionManager = this.transactionManagerMock.Object
            };

            this.fakeController = new FakeController
            {
                ViewData = new ViewDataDictionary(new Model {Required = "y"})
            };

            this.filterContext = Mockery.Create<ActionExecutedContext>();
            this.filterContext.SetupGet(c => c.Controller).Returns(this.fakeController);
            // most tests require top-level action
            this.filterContext.SetupGet(c => c.IsChildAction).Returns(false);
        }

        public class Model
        {
            [Required]
            public string Required { get; set; }
        }

        public class FakeController : ControllerBase
        {
            protected override void ExecuteCore()
            {
            }
        }

        private void Act()
        {
            this.transactionAttribute.OnActionExecuted(this.filterContext.Object);
        }

        [Test]
        public void Should_CommitTransaction_if_ErrorWasHandled()
        {
            this.filterContext.SetupGet(c => c.Exception).Returns(new Exception());
            this.filterContext.Object.ExceptionHandled = true;

            Act();

            this.transactionManagerMock.Verify(tm => tm.CommitTransaction());
        }

        [Test]
        public void Should_CommitTransaction_if_IgnoreModelError_Specified()
        {
            this.fakeController.ViewData.ModelState.AddModelError("*", "Error");
            this.transactionAttribute.RollbackOnModelValidationError = false;

            Act();

            this.transactionManagerMock.Verify(tm => tm.CommitTransaction(),
                "should commit transaction when RollbackOnModelStateError is false");
        }

        [Test]
        public void Should_CommitTransaction_on_SuccessfulActionCompletion()
        {
            Act();

            this.transactionManagerMock.Verify(tm => tm.CommitTransaction());
        }

        [Test]
        public void Should_IgnoreTransaction_on_ChildAction()
        {
            this.filterContext.SetupGet(c => c.IsChildAction).Returns(true);

            Act();

            this.transactionManagerMock.Verify(tm => tm.CommitTransaction(), Times.Never());
            this.transactionManagerMock.Verify(tm => tm.RollbackTransaction(), Times.Never());
        }

        [Test]
        public void Should_RollbackTransaction_on_ModelError()
        {
            this.fakeController.ViewData.ModelState.AddModelError("*", "Error");

            Act();

            this.transactionManagerMock.Verify(tm => tm.RollbackTransaction());
        }

        [Test]
        public void Should_RollbackTransaction_on_UnhandledError()
        {
            this.filterContext.SetupGet(c => c.Exception).Returns(new Exception());
            this.filterContext.Object.ExceptionHandled = false;

            Act();

            this.transactionManagerMock.Verify(tm => tm.RollbackTransaction());
        }
    }
}
