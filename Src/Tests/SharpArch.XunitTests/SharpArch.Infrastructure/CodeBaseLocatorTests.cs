namespace Tests.SharpArch.Infrastructure;

using System.Reflection;
using global::SharpArch.Infrastructure;
using Shouldly;
using Xunit;


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
        var path = CodeBaseLocator.GetAssemblyCodeBasePath(Assembly.GetExecutingAssembly());
        _output.WriteLine("Assembly path: '{0}'", path);
        path.ShouldNotBeNullOrEmpty();
        Directory.Exists(path).ShouldBeTrue();
    }
}
