namespace Suteki.TardisBank.Api.Announcements;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


/// <summary>
///     Announcement summary model.
/// </summary>
public class AnnouncementSummary
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("date")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;
}


/// <summary>
///     Full announcement details.
/// </summary>
public class AnnouncementModel : AnnouncementSummary
{
    [JsonPropertyName("content")]
    [Required]
    public string Content { get; set; } = null!;
}
