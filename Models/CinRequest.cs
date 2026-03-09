namespace examencsharp.Models
{
    public class CinRequest
    {
        public int Id { get; set; }

        public int CitoyenId { get; set; }
        public Citoyen? Citoyen { get; set; }

        public DateTime RequestDate { get; set; }

        public string Status { get; set; }
    }
}