using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VDashboard
    {
        public int total_citoyens { get; set; }

        public int cin_delivered { get; set; }

        public int pending_requests { get; set; }

        public int total_votes { get; set; }
    }
}