using System.ComponentModel.DataAnnotations;

namespace examencsharp.Models
{
    public class Election
    {
        public int Id { get; set; }

        [Required]
        public string TitreElection { get; set; }

        public DateTime DateElection { get; set; }

        public int FokontanyId { get; set; }
        public Fokontany? Fokontany { get; set; }

        public bool IsActive { get; set; } = false;

        public ICollection<Candidat>? Candidats { get; set; }

        public ICollection<Vote>? Votes { get; set; }
    }
}