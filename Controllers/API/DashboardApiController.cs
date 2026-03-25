using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models.Views;

namespace examencsharp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DashboardApiController(ApplicationDbContext db) => _db = db;

        // GET api/dashboard
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var dashboard = new VDashboard
            {
                total_citoyens   = await _db.Citoyens.CountAsync(),
                cin_delivered    = await _db.CinRequests.CountAsync(r => r.Status == "Approuvé"),
                pending_requests = await _db.CinRequests.CountAsync(r => r.Status == "En attente"),
                total_votes      = await _db.Votes.CountAsync()
            };

            var electionActive = await _db.Elections.FirstOrDefaultAsync(e => e.IsActive);

            List<VResult>? resultats = null;
            string? electionTitre = null;

            if (electionActive != null)
            {
                electionTitre = electionActive.TitreElection;

                resultats = await _db.Votes
                    .Where(v => v.ElectionId == electionActive.Id)
                    .Include(v => v.Candidat)
                    .GroupBy(v => new { v.Candidat!.NomCandidat, v.Candidat.PrenomCandidat })
                    .Select(g => new VResult
                    {
                        NomCandidat    = g.Key.NomCandidat,
                        PrenomCandidat = g.Key.PrenomCandidat,
                        total_votes    = g.Count()
                    })
                    .OrderByDescending(r => r.total_votes)
                    .ToListAsync();
            }

            return Ok(new DashboardResponse
            {
                Stats          = dashboard,
                ElectionTitre  = electionTitre,
                Resultats      = resultats
            });
        }
    }

    // ─── Response DTO ────────────────────────────────────────────────────────────
    public class DashboardResponse
    {
        public VDashboard       Stats         { get; set; } = null!;
        public string?          ElectionTitre { get; set; }
        public List<VResult>?   Resultats     { get; set; }
    }
}