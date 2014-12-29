namespace Tests.SharpArch.Domain.Commands
{
    using System.ComponentModel.DataAnnotations;

    using global::SharpArch.Domain.Commands;

    public class InvalidCommand : CommandBase
    {
        [Required]
        public bool? Invalid { get; set; }

        [Range(100, 199)]
        public int InvalidInt { get; set; }
    }
}