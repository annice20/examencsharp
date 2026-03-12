using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VTotalVoters
    {
        public int election_id { get; set; }

        public int total_voters { get; set; }
    }
}