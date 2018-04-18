// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Tests.Helpers
{
    internal interface ITestService
    {
        void Do();
    }


    class TestService : ITestService
    {
        public void Do()
        {
        }
    }

}
