using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VParticipationRate
    {
        public int Id { get; set; }

        public string TitreElection { get; set; }

        public double participation_rate { get; set; }
    }
}