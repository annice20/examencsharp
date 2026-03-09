namespace examencsharp.Models
{
    public class Candidat
    {
        public int Id { get; set; }

        public int ElectionId { get; set; }
        public Election? Election { get; set; }

        public string NomCandidat { get; set; }

        public string PrenomCandidat { get; set; }

        public string Programme { get; set; }

        public string Photo { get; set; }

        public bool Validated { get; set; } = false;

        public int FokontanyId { get; set; }
        public Fokontany? Fokontany { get; set; }

        public ICollection<Vote>? Votes { get; set; }
    }
}