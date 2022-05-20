// ReSharper disable MissingXmlDoc
namespace Suteki.TardisBank.Domain;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using SharpArch.Domain.DomainModel;


[DebuggerDisplay("{Id}: {Title}")]
public class Announcement : Entity<int>
{
    public virtual DateTime Date { get; set; }

    [MaxLength(120)]
    public virtual string Title { get; set; } = null!;

    [MaxLength(2000)]
    public virtual string? Content { get; set; }

    public virtual DateTime LastModifiedUtc { get; set; }
}
