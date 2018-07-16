namespace Suteki.TardisBank.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using SharpArch.Domain.DomainModel;

    public class Announcement: Entity
    {
        public virtual DateTime Date { get; set; }

        [MaxLength(120)]
        public virtual string Title { get; set; }

        [MaxLength(2000)]
        public virtual string Content { get; set; }

        public virtual DateTime LastModifiedUtc { get; set; }


    }
}