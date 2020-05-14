namespace Tests.SharpArch.NHibernate.MultiDb
{
    using FluentAssertions;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.Domain.PersistenceSupport.SharpArch.NHibernate;
    using Xunit;


    public class DefaultDatabaseIdentifierProviderTests
    {
        [UseDatabase("db1")]
        class BaseRepo

        {
        }


        [UseDatabase("db2")]
        class OverrideRepo : BaseRepo
        {
        }


        class UndecoratedRepo
        {
        }


        public class MultipleDatabase_Mode
        {
            readonly AttributeBasedDatabaseIdentifierProvider _provider = new AttributeBasedDatabaseIdentifierProvider();

            [Fact]
            public void Can_Retrieve_DatabaseIdentifier_From_Instance()
            {
                _provider.GetFromInstance(new BaseRepo()).Should().Be("db1");
            }

            [Fact]
            public void Can_Retrieve_DatabaseIdentifier_From_Type()
            {
                _provider.GetFromType(typeof(BaseRepo)).Should().Be("db1");
            }

            [Fact]
            public void Should_Return_DefaultDatabase_When_Attribute_Is_Not_Applied()
            {
                _provider.GetFromType(typeof(UndecoratedRepo)).Should().Be(DatabaseIdentifier.Default);
            }

            [Fact]
            public void Should_Use_Override_From_Subclass()
            {
                _provider.GetFromType(typeof(OverrideRepo)).Should().Be("db2", "subclass has it's own attribute");
            }
        }


        public class SingleDatabase_Mode
        {
            readonly IDatabaseIdentifierProvider _provider = DefaultDatabaseIdentifierProvider.Instance;

            [Fact]
            public void Should_Return_DefaultDatabase_For_Decorated_Instance()
            {
                _provider.GetFromInstance(new BaseRepo()).Should().Be(DatabaseIdentifier.Default);
            }

            [Fact]
            public void Should_Return_DefaultDatabase_For_Decorated_Type()
            {
                _provider.GetFromType(typeof(BaseRepo)).Should().Be(DatabaseIdentifier.Default);
            }

            [Fact]
            public void Should_Return_DefaultDatabase_For_Undecorated_Type()
            {
                _provider.GetFromType(typeof(UndecoratedRepo)).Should().Be(DatabaseIdentifier.Default);
            }
        }
    }
}
