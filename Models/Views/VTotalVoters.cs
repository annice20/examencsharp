using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VTotalVoters
    {
        public int ElectionId { get; set; }

        public int TotalVoters { get; set; }
    }
}