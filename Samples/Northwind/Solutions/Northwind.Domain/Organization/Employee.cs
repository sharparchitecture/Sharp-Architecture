namespace Northwind.Domain.Organization
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using NHibernate.Validator.Constraints;

    using SharpArch.Domain.DomainModel;
    using SharpArch.NHibernate.NHibernateValidator;

    /// <summary>
    ///   The domain signature of this object isn't very realistic as you'll likely have same named 
    ///   people in a large company.  Regardless, the Northwind DB doesn't provide a great domain 
    ///   identifier, so the full name will have to do.  Alternatively, you don't have to have 
    ///   domain signature properties.  If you don't, then Equals will use it's default behavior and
    ///   compare the object references themselves.
    /// </summary>
    [Serializable]
    [HasUniqueDomainSignature(ErrorMessage = "An employee already exists with the same first and last name")]
    public class Employee : Entity
    {
        public Employee()
        {
            this.InitMembers();
        }

        [DomainSignature]
        [NotNullNotEmpty(Message = "First name must be provided")]
        [JsonProperty]
        public virtual string FirstName { get; set; }

        public virtual string FullName
        {
            get
            {
                return this.LastName + ", " + this.FirstName;
            }
        }

        [DomainSignature]
        [NotNullNotEmpty(Message = "Last name must be provided")]
        [JsonProperty]
        public virtual string LastName { get; set; }

        [Range(1, 9999, Message = "Phone extension must be between 1 and 9999")]
        [JsonProperty]
        public virtual int? PhoneExtension { get; set; }

        /// <summary>
        ///   Note the protected set...only the ORM should set the collection reference directly
        ///   after it's been initialized in <see cref = "InitMembers" />
        /// </summary>
        [JsonProperty]
        public virtual IList<Territory> Territories { get; protected set; }

        [JsonProperty]
        public virtual bool Valid
        {
            get { return IsValid(); }
        }
        
        private void InitMembers()
        {
            // Init the collection so it's never null
            this.Territories = new List<Territory>();
        }
    }
}