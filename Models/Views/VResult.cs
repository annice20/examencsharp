using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VResult
    {
        public int Id { get; set; }

        public string NomCandidat { get; set; }

        public string PrenomCandidat { get; set; }

        public int total_votes { get; set; }
    }
}