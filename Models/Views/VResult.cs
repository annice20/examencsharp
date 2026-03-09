using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VResult
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int TotalVotes { get; set; }
    }
}