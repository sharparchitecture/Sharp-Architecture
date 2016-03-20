namespace Northwind.Domain
{
    using System;

    using SharpArch.Domain;
    using SharpArch.Domain.DomainModel;

    [Serializable]
    public class Region : Entity, IHasAssignedId<int>
    {
        public Region(string description)
        {
            Check.Require(!string.IsNullOrEmpty(description));
            this.Description = description;
        }

        /// <summary>
        ///   The Northwind DB doesn't make the Id of this object an identity field; 
        ///   not using an identity setting on the DB was a bad design decision for 
        ///   Northwind - learn from their mistakes!
        /// </summary>
        protected Region()
        {
        }

        [DomainSignature]
        public virtual string Description { get; protected set; }

        public virtual void SetAssignedIdTo(int assignedId)
        {
            this.Id = assignedId;
        }
    }
}