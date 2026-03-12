using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VCandidate
    {
        public int Id { get; set; }

        public string NomCandidat { get; set; }

        public string PrenomCandidat { get; set; }

        public string Programme { get; set; }

        public string Photo { get; set; }

        public bool Validated { get; set; }

        public string fokontany { get; set; }

        public string election { get; set; }
    }
}