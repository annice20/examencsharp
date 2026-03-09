using Microsoft.EntityFrameworkCore;

namespace examencsharp.Models.Views
{
    [Keyless]
    public class VParticipationRate
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public double ParticipationRate { get; set; }
    }
}