using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace examencsharp.Controllers
{
    public class VotersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VotersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Vérifier éligibilité
        public async Task<IActionResult> CheckEligibility(int citoyenId, int electionId)
        {
            var citizen = await _context.Citoyens
                .FirstOrDefaultAsync(c => c.Id == citoyenId);

            if (citizen == null)
                return Content("Citoyen non trouvé");

            var alreadyVoted = await _context.Votes
                .AnyAsync(v => v.CitoyenId == citoyenId && v.ElectionId == electionId);

            if (alreadyVoted)
                return Content("Vous avez déjà voté");

            return Content("Éligible pour voter");
        }
    }
}