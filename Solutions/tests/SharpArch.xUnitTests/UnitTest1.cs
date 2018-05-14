using System;
using Xunit;

namespace tests
{
    using FluentAssertions;
    using SharpArch.Domain.Reflection;


    public class TypePropertyDescriptorCacheTests
    {
        public TypePropertyDescriptorCacheTests()
        {
            _cache = new TypePropertyDescriptorCache();
        }

        TypePropertyDescriptorCache _cache;

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
            _cache.GetOrAdd(type, t => descriptor).Should().BeSameAs(descriptor);
        }
    }
}
