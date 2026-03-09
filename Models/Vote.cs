namespace examencsharp.Models
{
    public class Vote
    {
        public int Id { get; set; }

        public int CitoyenId { get; set; }
        public Citoyen? Citoyen { get; set; }

        public int CandidatId { get; set; }
        public Candidat? Candidat { get; set; }

        public int ElectionId { get; set; }
        public Election? Election { get; set; }

        public DateTime VoteDate { get; set; }

        public string QRCode { get; set; }
    }
}