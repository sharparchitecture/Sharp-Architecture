namespace Suteki.TardisBank.Api.Announcements;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


public class NewAnnouncement
{
    [JsonPropertyName("date")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("content")]
    [Required]
    public string Content { get; set; } = null!;
}
