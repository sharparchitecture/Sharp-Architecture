namespace TransactionAttribute.Tests.Setup
{
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using WebApi;


    public class TestServerSetup : IDisposable
    {
        public HttpClient Client { get; }
        public TestServer Server { get; }

        /// <inheritdoc />
        public TestServerSetup()
        {
            Server = new TestServer(Program.CreateHostBuilder()
                    .UseStartup<Startup>()
                    .UseContentRoot("../../../../App")
                //.UseSolutionRelativeContentRoot("../Samples/SharpArch.WebApi/App/")
            );
            Client = Server.CreateClient();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Server?.Dispose();
            Client?.Dispose();
        }
    }
}
