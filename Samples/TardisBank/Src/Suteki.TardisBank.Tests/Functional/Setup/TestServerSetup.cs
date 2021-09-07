namespace Suteki.TardisBank.Tests.Functional.Setup
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

        public TestServerSetup()
        {
            Server = new TestServer(Program.CreateHostBuilder(Array.Empty<string>())
#if NET3UP
                .UseTestServer()
#endif
                .UseStartup<Startup>()
                .UseSolutionRelativeContentRoot("TardisBank/Src/Suteki.TardisBank.WebApi/")
            );
            Client = Server.CreateClient();
            Client.BaseAddress = Server.BaseAddress;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Server?.Dispose();
            Client?.Dispose();
        }
    }
}
