// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident

namespace Tests.SharpArch.Web.Http
{
    using System;
    using System.Data;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using NUnit.Framework;

    [TestFixture]
    internal class TransactionAttribute_On_StartingAction_Tests : TransactionAttributeTestsBase
    {
        private HttpActionContext filterContext;

        protected override void DoSetUp()
        {
            base.DoSetUp();

            this.filterContext = CreateActionExecutingContext();
        }


        [Test]
        public void Should_StartTransaction_on_TopLevelAction()
        {
            this.TransactionAttribute.IsolationLevel = IsolationLevel.RepeatableRead;

            this.TransactionAttribute.OnActionExecuting(filterContext);

            this.TransactionManagerMock.Verify(tm => tm.BeginTransaction(IsolationLevel.RepeatableRead),
                "should start transaction with given isolation level");
        }
    }


    [TestFixture]
    class TransactionAttribute_On_ActionCompleted_Tests : TransactionAttributeTestsBase
    {
        private HttpActionExecutedContext filterContext;


        protected override void DoSetUp()
        {
            base.DoSetUp();

            this.filterContext = CreateActionExecutedContext();
        }


        private void Act()
        {
            TransactionAttribute.OnActionExecuted(this.filterContext);
        }


        [Test]
        public void Should_CommitTransaction_if_IgnoreModelError_Specified()
        {
            this.filterContext.ActionContext.ModelState.AddModelError("*", "error");
            this.TransactionAttribute.RollbackOnModelValidationError = false;

            Act();

            TransactionManagerMock.Verify(tm => tm.CommitTransaction(),
                "should commit transaction when RollbackOnModelValidationError is false");
        }

        [Test]
        public void Should_CommitTransaction_on_SuccessfulActionCompletion()
        {
            Act();

            this.TransactionManagerMock.Verify(tm => tm.CommitTransaction());
        }


        [Test]
        public void Should_RollbackTransaction_on_ModelError()
        {
            this.filterContext.ActionContext.ModelState.AddModelError("*", "error");

            Act();

            this.TransactionManagerMock.Verify(tm => tm.RollbackTransaction());
        }

        [Test]
        public void Should_RollbackTransaction_on_UnhandledError()
        {
            this.filterContext.Exception = new Exception();

            Act();

            this.TransactionManagerMock.Verify(tm => tm.RollbackTransaction());
        }
    }
}
