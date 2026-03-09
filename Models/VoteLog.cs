namespace examencsharp.Models
{
    public class VoteLog
    {
        public int Id { get; set; }

        public int CitoyenId { get; set; }
        public Citoyen? Citoyen { get; set; }

        public int ElectionId { get; set; }
        public Election? Election { get; set; }

        public string Action { get; set; }

        public DateTime LogDate { get; set; }
    }
}