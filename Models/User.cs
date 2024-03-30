using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace LoginAPI.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string firstName { get; set; }
        [Required]
        public string lastName { get; set; }
        public DateTime dateCreated { get; set; } = DateTime.UtcNow;
    }
}
