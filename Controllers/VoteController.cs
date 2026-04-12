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

    var election = await _db.Elections
        .Include(e => e.Fokontany)
        .FirstOrDefaultAsync(e => e.IsActive);

    if (election == null)
        return View("AucuneElection");

    // ← Mitady citoyen aloha
    var userId = HttpContext.Session.GetInt32("userId");
    var citoyen = await _db.Citoyens
        .FirstOrDefaultAsync(c => c.UtilisateurId == userId);

    if (citoyen == null)
        return View("NonEligible");

    // ← Mampiasa citoyen.Id marina
    var dejaVote = await _db.Votes
        .AnyAsync(v => v.CitoyenId == citoyen.Id && v.ElectionId == election.Id);

    if (dejaVote)
        return View("DejaVote");

    var candidats = await _db.Candidats
        .Where(c => c.ElectionId == election.Id)
        .ToListAsync();

    ViewBag.Election = election;
    return View(candidats);
}

       [HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Voter(int candidatId, int electionId)
{
    var userEmail = HttpContext.Session.GetString("user");
    if (string.IsNullOrEmpty(userEmail))
        return RedirectToAction("Login", "Account");

    var userId = HttpContext.Session.GetInt32("userId");
    if (userId == null)
        return RedirectToAction("Login", "Account");

    // Mitady citoyen linked amin'ny utilisateur
    var citoyen = await _db.Citoyens
        .FirstOrDefaultAsync(c => c.UtilisateurId == userId);

    // Raha tsy misy citoyen → mamorona vote amin'ny utilisateur.Id mivantana
    // amin'ny alalan'ny foreign key disable — fa tsara kokoa ny mitady citoyen
    int citoyenId;
    if (citoyen != null)
        citoyenId = citoyen.Id;
    else
    {
        // Tsy misy citoyen linked — tsy afaka mifidy
        ViewBag.Error = "Votre compte n'est pas lié à un citoyen. Contactez l'administrateur.";
        return RedirectToAction("Index");
    }

    // Anti double-vote
    var dejaVote = await _db.Votes
        .AnyAsync(v => v.CitoyenId == citoyenId && v.ElectionId == electionId);
    if (dejaVote)
        return View("DejaVote");

    _db.Votes.Add(new Vote
    {
        CitoyenId  = citoyenId,  // ← citoyen.Id marina
        CandidatId = candidatId,
        ElectionId = electionId,
        VoteDate   = DateTime.UtcNow,
        QRCode     = Guid.NewGuid().ToString()
    });

    await _db.SaveChangesAsync();
    return RedirectToAction("Confirmation");
}
    }
}