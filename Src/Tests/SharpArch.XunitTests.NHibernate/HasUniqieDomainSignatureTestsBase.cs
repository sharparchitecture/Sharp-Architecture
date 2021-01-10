﻿using SharpArch.NHibernate.Impl;

namespace Tests.SharpArch.NHibernate
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.NHibernate;
    using global::SharpArch.Testing.Xunit.NHibernate;
    using Moq;


    public abstract class HasUniqueDomainSignatureTestsBase : TransientDatabaseTests<NHibernateTestsSetup>
    {
        protected Mock<IServiceProvider> ServiceProviderMock;
        protected ValidationContext ValidationContext;

        public HasUniqueDomainSignatureTestsBase()
            : base(new NHibernateTestsSetup())
        {
            ServiceProviderMock = new Mock<IServiceProvider>();
            ServiceProviderMock.Setup(sp => sp.GetService(typeof(IEntityDuplicateChecker)))
                .Returns(new EntityDuplicateChecker(Session));
        }

        /// <summary>
        ///     Create validation context for given object.
        /// </summary>
        /// <param name="objectToValidate"></param>
        /// <returns></returns>
        protected ValidationContext ValidationContextFor(object objectToValidate)
        {
            return new ValidationContext(objectToValidate, ServiceProviderMock.Object, null);
        }
    }
}
