// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation

namespace Tests.SharpArch.Castle
{
    using System;
    using FluentAssertions;
    using global::Castle.MicroKernel.Registration;
    using global::Castle.Windsor;
    using global::SharpArch.Castle.Extensions;
    using global::SharpArch.Domain.Reflection;
    using NUnit.Framework;
    using Tests.Helpers;

    [TestFixture]
    internal class WindsorPropertyInjectionTests : WindsorTestsBase
    {
        private TypePropertyDescriptorCache propertyCache;

        protected override void Initialize(WindsorContainer container)
        {
            this.propertyCache = new TypePropertyDescriptorCache();
        }

        class PropertyInjection
        {
            public ITestService Service { get; set; }
        }



        [Test]
        public void InjectProperties_Should_Inject_ResolvableDependencies()
        {
            Container.Register(Component.For<ITestService>().ImplementedBy<TestService>());

            var obj = new PropertyInjection();
            Container.Kernel.InjectProperties(obj, this.propertyCache);
            obj.Service.Should().NotBeNull();
        }


        [Test]
        public void InjectProperties_Should_Skip_UnresolvableDependencies()
        {
            var customDependency = new TestService();
            var obj = new PropertyInjection {Service = customDependency};

            Container.Kernel.InjectProperties(obj, this.propertyCache);
            obj.Service.Should().BeSameAs(customDependency);
        }


        [Test]
        public void InjectProperties_Should_ValidateProperties_WhenConstructingPropertyCache()
        {
            Container.Register(Component.For<ITestService>().ImplementedBy<TestService>());

            Action inject = () => Container.Kernel.InjectProperties(new PropertyInjection(), this.propertyCache,
                (prop, component) => { throw new InvalidOperationException(); });

            inject.ShouldThrow<InvalidOperationException>();
            this.propertyCache.Count.Should().Be(0, "should not cache property descriptors if invalid dependencies found");
        }


        [Test]
        public void CleanupInjectableProperties_Should_NullProperties()
        {
            Container.Register(Component.For<ITestService>().ImplementedBy<TestService>());

            var obj = new PropertyInjection();
            Container.Kernel.InjectProperties(obj, this.propertyCache);

            Container.Kernel.CleanupInjectableProperties(obj, this.propertyCache);
            obj.Service.Should().BeNull();
        }

        [Test]
        public void CleanupInjectableProperties_ShouldNot_ModifyPropertyCache()
        {
            Container.Kernel.CleanupInjectableProperties(new PropertyInjection(), this.propertyCache);
            this.propertyCache.Count.Should().Be(0);
        }
    }
}
