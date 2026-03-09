using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VDashboard
    {
        public int TotalCitizens { get; set; }

        public int CinDelivered { get; set; }

        public int PendingRequests { get; set; }

        public int TotalVotes { get; set; }
    }
}