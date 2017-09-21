// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation

namespace Tests.SharpArch.Domain.Reflection
{
    using System;
    using FluentAssertions;
    using global::SharpArch.Domain.Reflection;
    using NUnit.Framework;

    [TestFixture]
    internal class TypePropertyDescriptorCacheTests
    {
        [SetUp]
        public void SetUp()
        {
            _cache = new TypePropertyDescriptorCache();
        }

        TypePropertyDescriptorCache _cache;

        [Test]
        public void Clear_Should_ClearTheCache()
        {
            _cache.GetOrAdd(GetType(), t => new TypePropertyDescriptor(t, null));
            _cache.Clear();
            _cache.Find(GetType()).Should().BeNull();
        }

        [Test]
        public void Find_Should_ReturnNullForMissingDescriptor()
        {
            _cache.Find(typeof(TypePropertyDescriptorCache)).Should().BeNull();
        }

        [Test]
        public void GetOrAdd_Should_AddMissingItemToCache()
        {
            Type type = GetType();
            var descriptor = new TypePropertyDescriptor(type, null);
            _cache.GetOrAdd(type, t => descriptor).Should().BeSameAs(descriptor);
        }
    }
}
