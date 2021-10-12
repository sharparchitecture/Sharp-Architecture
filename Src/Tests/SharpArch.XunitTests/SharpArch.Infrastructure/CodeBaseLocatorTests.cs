namespace Tests.SharpArch.Infrastructure
{
    using System;
    using System.IO;
    using System.Reflection;
    using FluentAssertions;
    using global::SharpArch.Infrastructure;
    using Xunit;
    using Xunit.Abstractions;


    public class CodeBaseLocatorTests
    {
        readonly ITestOutputHelper _output;

        public CodeBaseLocatorTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public void CanResolveAssemblyPath()
        {
            _output.WriteLine("Assembly path: '{0}'", CodeBaseLocator.GetAssemblyPath(Assembly.GetExecutingAssembly()));
            var path = CodeBaseLocator.GetAssemblyCodeBasePath(Assembly.GetExecutingAssembly());
            path.Should().NotBeNullOrEmpty();
            Directory.Exists(path).Should().BeTrue();
        }
    }
}
