namespace Tests.SharpArch.Infrastructure
{
    using System.IO;
    using System.Reflection;
    using FluentAssertions;
    using global::SharpArch.Infrastructure.Caching;
    using Xunit;


    public class DependencyList_With_RealFileSystem_Tests
    {
        private readonly DependencyList _dependencyList;

        /// <inheritdoc />
        public DependencyList_With_RealFileSystem_Tests()
        {
            _dependencyList = DependencyList.WithBasePathOfAssembly(Assembly.GetExecutingAssembly());
        }

        [Fact]
        public void Should_Resolve_AssemblyName()
        {
            var list = _dependencyList.AddAssemblyOf<DependencyList_With_RealFileSystem_Tests>().Build();
            list.Should().HaveCount(1);
            var fullPath = list[0];
            fullPath.Should().EndWith("SharpArch.XunitTests.dll");
            File.Exists(fullPath).Should().BeTrue("file '{0}'cannot be found on disk", fullPath);
        }

        [Fact]
        public void Should_Resolve_Assembly_By_Name()
        {
            var list = _dependencyList.AddFile("SharpArch.Domain").Build();
            list.Should().HaveCount(1);
            var fullPath = list[0];
            fullPath.Should().EndWith("SharpArch.Domain.dll");
            File.Exists(fullPath).Should().BeTrue("file '{0}'cannot be found on disk", fullPath);
        }

        [Fact]
        public void Should_Resolve_Assembly_By_NameAndExtension()
        {
            var list = _dependencyList.AddFile("SharpArch.Domain.dll").Build();
            list.Should().HaveCount(1);
            var fullPath = list[0];
            fullPath.Should().EndWith("SharpArch.Domain.dll");
            File.Exists(fullPath).Should().BeTrue("file '{0}'cannot be found on disk", fullPath);
        }

        [Fact]
        public void Can_Get_LastModificationTime()
        {
            _dependencyList
                .AddAssemblyOf<DependencyList>()
                .AddAssemblyOf<DependencyList_With_RealFileSystem_Tests>();
            _dependencyList.GetLastModificationTime().Should().NotBeNull();
        }
    }
}
