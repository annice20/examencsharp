using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers.API
{
    [Route("api/votes")]
    [ApiController]
    public class VoteApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public VoteApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET api/votes/election/5
        [HttpGet("election/{electionId}")]
        public async Task<IActionResult> GetByElection(int electionId)
        {
            var votes = await _db.Votes
                .Where(v => v.ElectionId == electionId)
                .Select(v => new {
                    v.Id, v.CitoyenId, v.CandidatId,
                    v.ElectionId, v.VoteDate
                }).ToListAsync();
            return Ok(new { total = votes.Count, votes });
        }

        // POST api/votes/voter
        [HttpPost("voter")]
        public async Task<IActionResult> Voter([FromBody] VoteApiDto dto)
        {
            // Verification citoyen
            var citoyen = await _db.Citoyens.FindAsync(dto.CitoyenId);
            if (citoyen == null)
                return BadRequest(new { message = "Citoyen introuvable" });

            // Verification election active
            var election = await _db.Elections.FindAsync(dto.ElectionId);
            if (election == null)
                return BadRequest(new { message = "Election introuvable" });
            if (!election.IsActive)
                return BadRequest(new { message = "Election non active" });

            // Anti double vote
            bool dejaVote = await _db.Votes.AnyAsync(v =>
                v.CitoyenId == dto.CitoyenId &&
                v.ElectionId == dto.ElectionId);
            if (dejaVote)
                return BadRequest(new { message = "Ce citoyen a déjà voté" });

            // Verification candidat
            var candidat = await _db.Candidats.FindAsync(dto.CandidatId);
            if (candidat == null)
                return BadRequest(new { message = "Candidat introuvable" });
            if (!candidat.Validated)
                return BadRequest(new { message = "Candidat non validé" });

            // Enregistrement
            var vote = new Vote {
                CitoyenId  = dto.CitoyenId,
                CandidatId = dto.CandidatId,
                ElectionId = dto.ElectionId,
                VoteDate   = DateTime.UtcNow,
                QRCode     = Guid.NewGuid().ToString()
            };
            _db.Votes.Add(vote);

            // Journalisation
            _db.VoteLogs.Add(new VoteLog {
                CitoyenId  = dto.CitoyenId,
                ElectionId = dto.ElectionId,
                Action     = "VOTE_EFFECTUE",
                LogDate    = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return Ok(new {
                message = "Vote enregistré",
                qrCode  = vote.QRCode,
                voteDate = vote.VoteDate
            });
        }

        // GET api/votes/a-vote/{citoyenId}/{electionId}
        [HttpGet("a-vote/{citoyenId}/{electionId}")]
        public async Task<IActionResult> ADejaVote(int citoyenId, int electionId)
        {
            bool dejaVote = await _db.Votes.AnyAsync(v =>
                v.CitoyenId == citoyenId &&
                v.ElectionId == electionId);

            return Ok(new {
                aVote   = dejaVote,
                message = dejaVote
                    ? "Ce citoyen a déjà voté"
                    : "Ce citoyen n'a pas encore voté"
            });
        }
    }

    public class VoteApiDto
    {
        public int CitoyenId  { get; set; }
        public int CandidatId { get; set; }
        public int ElectionId { get; set; }
    }
}