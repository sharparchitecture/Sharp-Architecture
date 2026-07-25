using Xunit;

#if NET8_0_OR_GREATER
[assembly: Trait("Category", "Samples")]
#else
[assembly: Xunit.AssemblyTrait("Category", "Samples")]
#endif

namespace TransactionAttribute.Tests;

using System.Data;
using System.Net;
using Setup;
using Shouldly;
using WebApi.Stubs;
using Xunit;


public class UnitOfWorkAttributeOverrideTests : IClassFixture<TestServerSetup>
{
    readonly TestServerSetup _setup;

    public UnitOfWorkAttributeOverrideTests(TestServerSetup setup)
    {
        if (setup == null)
            throw new ArgumentNullException(nameof(setup));
        _setup = setup ?? throw new ArgumentNullException(nameof(setup));
    }

    [Theory]
    [InlineData("api/overrides/local", HttpStatusCode.OK,
        nameof(IsolationLevel.ReadUncommitted), "committed")]
    [InlineData("api/overrides/invalid-model", HttpStatusCode.BadRequest,
        nameof(IsolationLevel.ReadCommitted), "rolled-back")]
    [InlineData("api/overrides/controller", HttpStatusCode.OK,
        nameof(IsolationLevel.ReadCommitted), "committed")]
    [InlineData("api/global/default", HttpStatusCode.OK,
        nameof(IsolationLevel.Chaos), "committed")]
    public async Task CanUseLocalOverride(string method, HttpStatusCode statusCode, string isolationLevel, string transactionState)
    {
        using (var response = await GetAsync(method))
        {
            response.StatusCode.ShouldBe(statusCode);
            response.Headers.GetValues(TransactionManagerStub.TransactionIsolationLevel)
                .ShouldContain(isolationLevel);
            response.Headers.GetValues(TransactionManagerStub.TransactionState)
                .ShouldContain(transactionState);
        }
    }

    Task<HttpResponseMessage> GetAsync(string relativePath)
        => _setup.Client.GetAsync(new Uri(_setup.Client.BaseAddress!, relativePath));
}
