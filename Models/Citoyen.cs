using System.ComponentModel.DataAnnotations;

namespace examencsharp.Models
{
    public class Citoyen
    {
        public int Id { get; set; }

        public int UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }

        public string NomCitoyen { get; set; }

        public string PrenomCitoyen { get; set; }

        [Required]
        public string CIN { get; set; }

        public DateTime DateNaissance { get; set; }

        public string Address { get; set; }

        public int FokontanyId { get; set; }
        public Fokontany? Fokontany { get; set; }

        public string QRCodeToken { get; set; } = Guid.NewGuid().ToString();
        public bool ADejaVote { get; set; } = false;

        public ICollection<CinRequest>? CinRequests { get; set; }

        public ICollection<Vote>? Votes { get; set; }
    }
}