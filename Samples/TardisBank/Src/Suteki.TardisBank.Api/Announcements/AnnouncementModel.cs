namespace Suteki.TardisBank.Api.Announcements;

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;


/// <summary>
///     Announcement summary model.
/// </summary>
public class AnnouncementSummary
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("date")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    [JsonProperty("title")]
    public string Title { get; set; } = null!;
}


/// <summary>
///     Full announcement details.
/// </summary>
public class AnnouncementModel : AnnouncementSummary
{
    [JsonProperty("content")]
    [Required]
    public string Content { get; set; } = null!;
}
