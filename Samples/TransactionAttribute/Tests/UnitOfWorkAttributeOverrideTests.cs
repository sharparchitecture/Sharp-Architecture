using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using SharpArch.WebApi.Tests.Setup;
using Xunit;

namespace SharpArch.WebApi.Tests
{
    using Sample.Stubs;


    public class UnitOfWorkAttributeOverrideTests : IClassFixture<TestServerSetup>
    {
        private readonly TestServerSetup _setup;

        public UnitOfWorkAttributeOverrideTests([NotNull] TestServerSetup setup)
        {
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
                response.StatusCode.Should().Be(statusCode);
                response.Headers.GetValues(TransactionManagerStub.TransactionIsolationLevel)
                    .Should().Contain(isolationLevel);
                response.Headers.GetValues(TransactionManagerStub.TransactionState)
                    .Should().Contain(transactionState);
            }
        }

        private Task<HttpResponseMessage> GetAsync(string relativePath)
        {
            return _setup.Client.GetAsync(new Uri(_setup.Client.BaseAddress, relativePath));
        }
    }
}
