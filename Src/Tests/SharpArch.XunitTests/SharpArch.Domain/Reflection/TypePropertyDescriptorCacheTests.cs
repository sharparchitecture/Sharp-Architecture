namespace Tests.SharpArch.Domain.Reflection
{
    using System;
    using FluentAssertions;
    using global::SharpArch.Domain.Reflection;
    using Xunit;


    public class TypePropertyDescriptorCacheTests
    {
        readonly TypePropertyDescriptorCache _cache;

        public TypePropertyDescriptorCacheTests()
        {
            _cache = new TypePropertyDescriptorCache();
        }

        [Fact]
        public void Clear_Should_ClearTheCache()
        {
            _cache.GetOrAdd(GetType(), t => new TypePropertyDescriptor(t, null));
            _cache.Clear();
            _cache.Find(GetType()).Should().BeNull();
        }

        [Fact]
        public void Find_Should_ReturnNullForMissingDescriptor()
        {
            _cache.Find(typeof(TypePropertyDescriptorCache)).Should().BeNull();
        }

        [Fact]
        public void GetOrAdd_Should_AddMissingItemToCache()
        {
            Type type = GetType();
            var descriptor = new TypePropertyDescriptor(type, null);
            _cache.GetOrAdd(type, _ => descriptor).Should().BeSameAs(descriptor);
        }
    }
}
