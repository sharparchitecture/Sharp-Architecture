namespace Suteki.TardisBank.Tests.Functional.Setup
{
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Hosting;
    using WebApi;


    public class TestServerSetup : IDisposable
    {
        readonly IHost _host;
        public HttpClient Client { get; }
        public TestServer Server { get; }

        public TestServerSetup()
        {
            var hostBuilder = Program.CreateHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseTestServer();
                    webHostBuilder.UseSolutionRelativeContentRoot("TardisBank/Src/Suteki.TardisBank.WebApi/");
                }
            );
            _host = hostBuilder.Build();
            _host.Start();
            Server = _host.GetTestServer();
            Client = Server.CreateClient();
            Client.BaseAddress = Server.BaseAddress;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Server?.Dispose();
            Client?.Dispose();
            _host?.Dispose();
        }
    }
}
