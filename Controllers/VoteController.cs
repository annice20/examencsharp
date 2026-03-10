using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers
{
    public class VoteController : Controller
    {
        private readonly ApplicationDbContext _db;
        public VoteController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var utilisateur = await _db.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == userEmail);
            if (utilisateur == null) return RedirectToAction("Login", "Account");

            var citoyen = await _db.Citoyens
                .FirstOrDefaultAsync(c => c.UtilisateurId == utilisateur.Id);
            if (citoyen == null) return View("NonEligible");

            // CIN approuvée ?
            var cinOk = await _db.CinRequests
                .AnyAsync(r => r.CitoyenId == citoyen.Id && r.Status == "Approved");
            if (!cinOk) return View("NonEligible");

            // Élection active dans son fokontany
            var election = await _db.Elections.Include(e => e.Fokontany)
                .FirstOrDefaultAsync(e => e.FokontanyId == citoyen.FokontanyId && e.IsActive);
            if (election == null) return View("AucuneElection");

            // Déjà voté ?
            var dejaVote = await _db.Votes
                .AnyAsync(v => v.CitoyenId == citoyen.Id && v.ElectionId == election.Id);
            if (dejaVote) return View("DejaVote");

            var candidats = await _db.Candidats
                .Where(c => c.ElectionId == election.Id && c.Validated)
                .ToListAsync();

            ViewBag.Election = election;
            ViewBag.Citoyen = citoyen;
            return View(candidats);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Voter(int candidatId, int electionId)
        {
            var userEmail = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var utilisateur = await _db.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == userEmail);
            var citoyen = await _db.Citoyens
                .FirstOrDefaultAsync(c => c.UtilisateurId == utilisateur!.Id);
            if (citoyen == null) return Forbid();

            // Anti double-vote
            var dejaVote = await _db.Votes
                .AnyAsync(v => v.CitoyenId == citoyen.Id && v.ElectionId == electionId);
            if (dejaVote) return View("DejaVote");

            var qrCode = Guid.NewGuid().ToString();

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Votes.Add(new Vote
                {
                    CitoyenId  = citoyen.Id,
                    CandidatId = candidatId,
                    ElectionId = electionId,
                    VoteDate   = DateTime.UtcNow,
                    QRCode     = qrCode
                });
                _db.VoteLogs.Add(new VoteLog
                {
                    CitoyenId  = citoyen.Id,
                    ElectionId = electionId,
                    Action     = "VOTE",
                    LogDate    = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            TempData["QRCode"] = qrCode;
            return RedirectToAction("Confirmation");
        }

        public IActionResult Confirmation()
        {
            ViewBag.QRCode = TempData["QRCode"];
            return View();
        }
    }
}