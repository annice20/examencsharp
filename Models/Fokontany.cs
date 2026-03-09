namespace examencsharp.Models
{
    public class Fokontany
    {
        public int Id { get; set; }

        public string NomFokontany { get; set; }

        public string District { get; set; }

        public string Commune { get; set; }

        public ICollection<Citoyen>? Citoyens { get; set; }

        public ICollection<Election>? Elections { get; set; }

        public ICollection<Candidat>? Candidats { get; set; }
    }
}