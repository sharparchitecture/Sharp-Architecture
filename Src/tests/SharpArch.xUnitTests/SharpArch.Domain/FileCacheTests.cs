
#if NETFULL
namespace Tests.SharpArch.Domain
{
    using System;
    using System.IO;
    using FluentAssertions;
    using global::SharpArch.Domain;
    using NUnit.Framework;



    [TestFixture]
    internal class FileCacheTests
    {
        [SetUp]
        public void SetUp()
        {
            _tempFileName = $"{Path.GetTempPath()}\\dummy{Guid.NewGuid()}.txt";
        }


        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFileName))
                File.Delete(_tempFileName);
        }

        string _tempFileName;

        [Serializable]
        public class DummyType
        {
            public string FirstProperty { get; set; }
        }

        [Fact]
        public void Retrieve_Should_LoadObjectFromCache()
        {
            var original = new DummyType {FirstProperty = "1"};
            FileCache.StoreInCache(original, _tempFileName);

            var copy = FileCache.RetrieveFromCache<DummyType>(_tempFileName);
            copy.Should().BeOfType<DummyType>();
            copy.Should().BeEquivalentTo(original, opt => opt.IncludingProperties());
        }

        [Fact]
        public void Retrieve_ShouldThrow_WhenEmptyPathSpecified()
        {
            Action a = () => FileCache.RetrieveFromCache<DummyType>("");
            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Retrieve_ShouldThrow_WhenNullPathSpecified()
        {
            Action a = () => FileCache.RetrieveFromCache<DummyType>(null);
            a.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("path");
        }

        [Fact]
        public void Retrieve_ShouldReturnNull_WhenFileContentIsInvalid()
        {
            File.WriteAllText(_tempFileName, "&&&&");

            FileCache.RetrieveFromCache<DummyType>(_tempFileName)
                .Should().BeNull();
        }

        [Fact]
        public void Store_ShouldThrow_WhenTryingToStoreNull()
        {
            Action a = () => FileCache.StoreInCache<DummyType>(null, _tempFileName);
            a.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Store_ShouldThrow_WhenPathIsEmpty()
        {
            Action a = () => FileCache.StoreInCache(new DummyType(), "");
            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Store_Should_SerializeToFile()
        {
            FileCache.StoreInCache(new DummyType(), _tempFileName);
            File.Exists(_tempFileName).Should().BeTrue();
        }
    }
}
#endif
