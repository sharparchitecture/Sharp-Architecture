namespace TransactionAttribute.Tests.Setup;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using WebApi;


public sealed class TestServerSetup : IDisposable
{
    readonly IHost _host;
    public HttpClient Client { get; }
    public TestServer Server { get; }

    public TestServerSetup()
    {
        var hostBuilder = Program.CreateHostBuilder(webHostBuilder =>
        {
            webHostBuilder.UseTestServer();
            webHostBuilder.UseContentRoot("../../../../App");
            //.UseSolutionRelativeContentRoot("../Samples/SharpArch.WebApi/App/")
        });
        _host = hostBuilder.Build();
        _host.Start();
        Server = _host.GetTestServer();
        Client = Server.CreateClient();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Server.Dispose();
        Client.Dispose();
        _host.Dispose();
    }
}
