namespace Tests.SharpArch.Infrastructure
{
    using System;
    using System.IO;
    using FluentAssertions;
    using global::SharpArch.Infrastructure.Abstractions;
    using global::SharpArch.Infrastructure.Caching;
    using Moq;
    using Xunit;


    public class DependencyListTests
    {
        private const string BasePath = "d:\\";
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly DependencyList _dependencyList;

        /// <inheritdoc />
        public DependencyListTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _dependencyList = new DependencyList(_fileSystemMock.Object, BasePath);
        }

        [Fact]
        public void AddFile_NonExistingFile_Should_Throw_FileNotFoundException()
        {
            const string fileName = "e:\\path\\file.txt";
            _fileSystemMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            Action addFile = () => _dependencyList.AddFile(fileName);
            addFile.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void AddFile_WithAbsolutePath_Should_AddFile_AsIs()
        {
            const string fileName = "e:\\path\\file.txt";
            _fileSystemMock.Setup(f => f.FileExists(fileName)).Returns(true);

            _dependencyList.AddFiles(new[] {fileName});
            _dependencyList.Build().Should().OnlyContain(x => x == fileName);
        }

        [Fact]
        public void AddFile_WithRelativePath_Should_Use_BasePath()
        {
            const string fileName = "file.txt";
            const string fileWithBasePath = BasePath + fileName;
            _fileSystemMock.Setup(f => f.FileExists(fileWithBasePath)).Returns(true);

            _dependencyList.AddFile(fileName);
            _dependencyList.Build()
                .Should().OnlyContain(x => x == fileWithBasePath);
        }

        [Fact]
        public void AddFile_Should_Try_FileName_With_DllExtensionAdded()
        {
            const string fileName = "file";
            const string fileWithDllExt = "file.dll";
            _fileSystemMock.Setup(f => f.FileExists(fileWithDllExt)).Returns(true);

            _dependencyList.AddFile(fileName);
            _dependencyList.Build()
                .Should().OnlyContain(x => x == fileWithDllExt);
        }

        [Fact]
        public void AddFile_Should_Try_FileName_With_BasePath_And_DllExtensionAdded()
        {
            const string fileName = "file";
            const string fullPath = BasePath + "file.dll";
            _fileSystemMock.Setup(f => f.FileExists(fullPath)).Returns(true);

            _dependencyList.AddFile(fileName);
            _dependencyList.Build()
                .Should().OnlyContain(x => x == fullPath);
        }

        [Fact]
        public void Can_Calculate_Maximum_ModificationDate()
        {
            const string fileName1 = "file1";
            const string fileName2 = "file2";

            _fileSystemMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(f => f.GetLastWriteTimeUtc(BasePath + fileName1)).Returns(new DateTime(2018, 01, 01, 00, 00, 00, DateTimeKind.Utc));
            _fileSystemMock.Setup(f => f.GetLastWriteTimeUtc(BasePath + fileName2)).Returns(new DateTime(2019, 01, 01, 00, 00, 00, DateTimeKind.Utc));

            _dependencyList.AddFiles(new[] {fileName1, fileName2});
            _dependencyList.GetLastModificationTime().Should().Be(new DateTime(2019, 01, 01, 00, 00, 00, DateTimeKind.Utc));
        }
    }
}
