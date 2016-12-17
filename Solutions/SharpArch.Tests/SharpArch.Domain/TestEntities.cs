// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MissingAnnotation
// ReSharper disable VirtualMemberCallInConstructor
namespace Tests.SharpArch.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.NHibernate.NHibernateValidator;

    [HasUniqueDomainSignature]
    class Contractor : Entity
    {
        [DomainSignature]
        public virtual string Name { get; set; }
    }

    [HasUniqueDomainSignatureWithGuidId]
    class ObjectWithGuidId : EntityWithTypedId<Guid>
    {
        [DomainSignature]
        public virtual string Name { get; set; }
    }

    [HasUniqueDomainSignature]
    class ObjectWithStringIdAndValidatorForIntId : EntityWithTypedId<string>
    {
        [DomainSignature]
        public virtual string Name { get; set; }
    }

    [HasUniqueDomainSignatureWithStringId]
    public class User : EntityWithTypedId<string>
    {
        public User(string id, string ssn)
        {
            Id = id;
            Ssn = ssn;
        }

        public User()
        {
        }

        [DomainSignature]
        public virtual string Ssn { get; set; }
    }

    class EntityWithNoDomainSignatureProperties : EntityWithTypedId<int>
    {
        public virtual string Property1 { get; set; }

        public virtual int Property2 { get; set; }
    }

    class EntityWithAllPropertiesPartOfDomainSignature : EntityWithTypedId<int>
    {
        [DomainSignature]
        public virtual string Property1 { get; set; }

        [DomainSignature]
        public virtual int Property2 { get; set; }

        [DomainSignature]
        public virtual bool Property3 { get; set; }
    }

    class EntityWithSomePropertiesPartOfDomainSignature : EntityWithTypedId<int>
    {
        [DomainSignature]
        public virtual string Property1 { get; set; }

        public virtual int Property2 { get; set; }

        [DomainSignature]
        public virtual bool Property3 { get; set; }
    }

    class EntityWithAnotherEntityAsPartOfDomainSignature : EntityWithTypedId<int>
    {
        public EntityWithAnotherEntityAsPartOfDomainSignature()
        {
            Property2 = new EntityWithAllPropertiesPartOfDomainSignature();
        }

        [DomainSignature]
        public virtual string Property1 { get; set; }

        [DomainSignature]
        public virtual EntityWithAllPropertiesPartOfDomainSignature Property2 { get; set; }
    }


    [HasUniqueDomainSignature]
    public class Song : Entity
    {
        [DomainSignature]
        public virtual string SongTitle { get; set; }

        [DomainSignature]
        public virtual Band Performer { get; set; }
    }

    [HasUniqueDomainSignature(ErrorMessage = "Band already exists")]
    public class Band : Entity
    {
        [DomainSignature]
        [Required]
        public virtual string BandName { get; set; }
        public virtual DateTime DateFormed { get; set; }
    }

    [HasUniqueDomainSignature(ErrorMessage = "Album already exists")]
    public class Album : Entity
    {
        [DomainSignature]
        [Required]
        public virtual string Title { get; set; }

        public virtual Band Author { get; set; }
    }

    public class Address : ValueObject
    {
        public virtual string StreetAddress { get; set; }

        public virtual string PostCode { get; set; }
    }


    [HasUniqueDomainSignature]
    public class Customer : Entity
    {
        [DomainSignature]
        public virtual string Name { get; set; }

        [DomainSignature]
        public virtual Address Address { get; set; }
    }

}