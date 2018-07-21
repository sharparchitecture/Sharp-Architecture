namespace Tests.Helpers
{
    internal interface ITestService
    {
        void Do();
    }


    internal class TestService : ITestService
    {
        public void Do()
        {
        }
    }
}
