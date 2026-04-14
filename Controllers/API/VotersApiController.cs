using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace examencsharp.Controllers.API
{
    [Route("api/voters")]
    [ApiController]
    public class VotersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VotersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/voters/eligibilite/{citoyenId}/{electionId}
        [HttpGet("eligibilite/{citoyenId}/{electionId}")]
        public async Task<IActionResult> CheckEligibility(int citoyenId, int electionId)
        {
            var citoyen = await _context.Citoyens
                .Include(c => c.Fokontany)
                .FirstOrDefaultAsync(c => c.Id == citoyenId);

            if (citoyen == null)
                return Ok(new { eligible = false, raison = "Citoyen introuvable" });

            if (string.IsNullOrEmpty(citoyen.CIN))
                return Ok(new { eligible = false, raison = "CIN manquante" });

            var dejaVote = await _context.Votes.AnyAsync(v =>
                v.CitoyenId == citoyenId &&
                v.ElectionId == electionId);

            if (dejaVote)
                return Ok(new { eligible = false, raison = "A déjà voté" });

            return Ok(new {
                eligible = true,
                raison   = "Éligible pour voter",
                citoyen  = new {
                    citoyen.Id,
                    citoyen.NomCitoyen,
                    citoyen.PrenomCitoyen,
                    citoyen.CIN,
                    fokontany = citoyen.Fokontany?.NomFokontany
                }
            });
        }

        // GET api/voters/journal/{electionId}
        [HttpGet("journal/{electionId}")]
        public async Task<IActionResult> GetJournal(int electionId)
        {
            var logs = await _context.VoteLogs
                .Where(l => l.ElectionId == electionId)
                .OrderByDescending(l => l.LogDate)
                .Select(l => new {
                    l.Id, l.CitoyenId,
                    l.Action, l.LogDate
                }).ToListAsync();

            return Ok(new { total = logs.Count, logs });
        }
    }
}