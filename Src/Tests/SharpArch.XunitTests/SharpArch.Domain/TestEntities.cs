// ReSharper disable VirtualMemberCallInConstructor
namespace Tests.SharpArch.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.Domain.Validation;


    [HasUniqueDomainSignature]
    public class Contractor : Entity<int>
    {
        [DomainSignature]
        public virtual string Name { get; set; }
    }


    [HasUniqueDomainSignature]
    public class ObjectWithGuidId : Entity<Guid>
    {
        [DomainSignature]
        public virtual string Name { get; set; }
    }


    [HasUniqueDomainSignature]
    internal class ObjectWithStringIdAndValidatorForIntId : Entity<string>
    {
        [DomainSignature]
        public virtual string Name { get; set; }
    }


    [HasUniqueDomainSignature]
    public class User : Entity<string>
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


    internal class EntityWithNoDomainSignatureProperties : Entity<int>
    {
        public virtual string Property1 { get; set; }

        public virtual int Property2 { get; set; }
    }


    internal class EntityWithAllPropertiesPartOfDomainSignature : Entity<int>
    {
        [DomainSignature]
        public virtual string Property1 { get; set; }

        [DomainSignature]
        public virtual int Property2 { get; set; }

        [DomainSignature]
        public virtual bool Property3 { get; set; }
    }


    internal class EntityWithSomePropertiesPartOfDomainSignature : Entity<int>
    {
        [DomainSignature]
        public virtual string Property1 { get; set; }

        public virtual int Property2 { get; set; }

        [DomainSignature]
        public virtual bool Property3 { get; set; }
    }


    internal class EntityWithAnotherEntityAsPartOfDomainSignature : Entity<int>
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
    public class Song : Entity<int>
    {
        [DomainSignature]
        public virtual string SongTitle { get; set; }

        [DomainSignature]
        public virtual Band Performer { get; set; }
    }


    [HasUniqueDomainSignature(ErrorMessage = "Band already exists")]
    public class Band : Entity<int>
    {
        [DomainSignature]
        [Required]
        public virtual string BandName { get; set; }

        public virtual DateTime DateFormed { get; set; }
    }


    [HasUniqueDomainSignature(ErrorMessage = "Album already exists")]
    public class Album : Entity<int>
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
    public class Customer : Entity<int>
    {
        [DomainSignature]
        public virtual string Name { get; set; }

        [DomainSignature]
        public virtual Address Address { get; set; }
    }
}
