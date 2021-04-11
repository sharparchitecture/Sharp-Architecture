namespace Suteki.TardisBank.Api.Announcements
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;


    public class NewAnnouncement
    {
        [JsonProperty("date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [JsonProperty("title")]
        public string Title { get; set; } = null!;

        [JsonProperty("content")]
        [Required]
        public string Content { get; set; } = null!;
    }
}
