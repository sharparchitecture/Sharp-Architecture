// ReSharper disable VirtualMemberCallInConstructor
namespace Tests.SharpArch.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.Domain.Validation;


    [HasUniqueDomainSignature]
    public class Contractor : Entity
    {
        [DomainSignature]
        public virtual string Name { get; set; }
    }


    [HasUniqueDomainSignatureWithGuidId]
    public class ObjectWithGuidId : EntityWithTypedId<Guid>
    {
        [DomainSignature]
        public virtual string Name { get; set; }
    }


    [HasUniqueDomainSignature]
    internal class ObjectWithStringIdAndValidatorForIntId : EntityWithTypedId<string>
    {
        [DomainSignature]
        public virtual string Name { get; set; }
    }


    [HasUniqueDomainSignatureWithStringId]
    public class User : EntityWithTypedId<string>
    {
        [DomainSignature]
        public virtual string Ssn { get; set; }

        public User(string id, string ssn)
        {
            Id = id;
            Ssn = ssn;
        }

        public User()
        {
        }
    }


    internal class EntityWithNoDomainSignatureProperties : EntityWithTypedId<int>
    {
        public virtual string Property1 { get; set; }

        public virtual int Property2 { get; set; }
    }


    internal class EntityWithAllPropertiesPartOfDomainSignature : EntityWithTypedId<int>
    {
        [DomainSignature]
        public virtual string Property1 { get; set; }

        [DomainSignature]
        public virtual int Property2 { get; set; }

        [DomainSignature]
        public virtual bool Property3 { get; set; }
    }


    internal class EntityWithSomePropertiesPartOfDomainSignature : EntityWithTypedId<int>
    {
        [DomainSignature]
        public virtual string Property1 { get; set; }

        public virtual int Property2 { get; set; }

        [DomainSignature]
        public virtual bool Property3 { get; set; }
    }


    internal class EntityWithAnotherEntityAsPartOfDomainSignature : EntityWithTypedId<int>
    {
        [DomainSignature]
        public virtual string Property1 { get; set; }

        [DomainSignature]
        public virtual EntityWithAllPropertiesPartOfDomainSignature Property2 { get; set; }

        public EntityWithAnotherEntityAsPartOfDomainSignature()
        {
            Property2 = new EntityWithAllPropertiesPartOfDomainSignature();
        }
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
