namespace MultiDatabase.Sample.Models
{
    using System.ComponentModel.DataAnnotations;


    public class NewUser
    {
        [Required]
        public string Login { get; set; }
    }
}
