using System;
using System.ComponentModel.DataAnnotations;

namespace examencsharp.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string MotDePasse { get; set; }

        public string? Code2FA { get; set; }
        public DateTime? Expiration2FA { get; set; }
    }
}