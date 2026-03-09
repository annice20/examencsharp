using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VCandidate
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Program { get; set; }

        public string Photo { get; set; }

        public bool Validated { get; set; }

        public string Fokontany { get; set; }

        public string Election { get; set; }
    }
}