using Microsoft.AspNetCore.Mvc;
using examencsharp.Models;

namespace examencsharp.Controllers.API
{
    [Route("api/electeurs")]
    [ApiController]
    public class ElecteurApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ElecteurApiController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        // 1. VERIFICATION ELIGIBILITE
        // GET api/electeurs/eligibilite/{citoyenId}
     
        [HttpGet("eligibilite/{citoyenId}")]
        public IActionResult VerifierEligibilite(int citoyenId)
        {
            // Condition 1 : Efa registered ve ?
            var citoyen = _context.Citoyens.Find(citoyenId);
            if (citoyen == null)
                return Ok(new {
                    eligible = false,
                    raison = "Citoyen non enregistré"
                });

            // Condition 2 : Misy CIN valide ve ?
            if (string.IsNullOrEmpty(citoyen.CIN))
                return Ok(new {
                    eligible = false,
                    raison = "CIN invalide ou manquante"
                });

            // Condition 3 : Rattaché au Fokontany ve ?
            if (citoyen.FokontanyId == 0)
                return Ok(new {
                    eligible = false,
                    raison = "Citoyen non rattaché à un Fokontany"
                });

            var fokontany = _context.Fokontany.Find(citoyen.FokontanyId);
            if (fokontany == null)
                return Ok(new {
                    eligible = false,
                    raison = "Fokontany introuvable"
                });

            return Ok(new {
                eligible = true,
                raison = "Citoyen éligible",
                citoyen = new {
                    citoyen.Id,
                    citoyen.NomCitoyen,
                    citoyen.PrenomCitoyen,
                    citoyen.CIN,
                    fokontany = fokontany.NomFokontany
                }
            });
        }

      
        // 2. VOTER (anti-double vote + journalisation)
        // POST api/electeurs/voter
       
        [HttpPost("voter")]
        public IActionResult Voter([FromBody] VoterDto dto)
        {
            // Condition 1 : Citoyen enregistré ?
            var citoyen = _context.Citoyens.Find(dto.CitoyenId);
            if (citoyen == null)
                return BadRequest(new { message = "Citoyen non enregistré" });

            // Condition 2 : CIN valide ?
            if (string.IsNullOrEmpty(citoyen.CIN))
                return BadRequest(new { message = "CIN invalide ou manquante" });

            // Condition 3 : Rattaché au Fokontany ?
            if (citoyen.FokontanyId == 0)
                return BadRequest(new { message = "Citoyen non rattaché à un Fokontany" });

            // Verification election active
            var election = _context.Elections.Find(dto.ElectionId);
            if (election == null)
                return BadRequest(new { message = "Election introuvable" });

            if (!election.IsActive)
                return BadRequest(new { message = "Cette élection n'est pas active" });

            // Empêcher le double vote
            bool dejaVote = _context.Votes.Any(v =>
                v.CitoyenId == dto.CitoyenId &&
                v.ElectionId == dto.ElectionId);

            if (dejaVote)
            {
                // Journalisation tentative double vote
                _context.VoteLogs.Add(new VoteLog {
                    CitoyenId = dto.CitoyenId,
                    ElectionId = dto.ElectionId,
                    Action = "TENTATIVE_DOUBLE_VOTE",
                    LogDate = DateTime.Now
                });
                _context.SaveChanges();
                return BadRequest(new { message = "Ce citoyen a déjà voté pour cette élection" });
            }

            // Verification candidat validé
            var candidat = _context.Candidats.Find(dto.CandidatId);
            if (candidat == null)
                return BadRequest(new { message = "Candidat introuvable" });

            if (!candidat.Validated)
                return BadRequest(new { message = "Ce candidat n'est pas validé" });

            // Enregistrement du vote
            var vote = new Vote {
                CitoyenId = dto.CitoyenId,
                CandidatId = dto.CandidatId,
                ElectionId = dto.ElectionId,
                VoteDate = DateTime.Now,
                QRCode = Guid.NewGuid().ToString()
            };
            _context.Votes.Add(vote);

            // Journalisation vote réussi
            _context.VoteLogs.Add(new VoteLog {
                CitoyenId = dto.CitoyenId,
                ElectionId = dto.ElectionId,
                Action = "VOTE_EFFECTUE",
                LogDate = DateTime.Now
            });

            _context.SaveChanges();

            return Ok(new {
                message = "Vote enregistré avec succès",
                qrCode = vote.QRCode,
                voteDate = vote.VoteDate
            });
        }

      
        // 3. VERIFICATION DOUBLE VOTE
        // GET api/electeurs/a-vote/{citoyenId}/{electionId}
      
        [HttpGet("a-vote/{citoyenId}/{electionId}")]
        public IActionResult ADejaVote(int citoyenId, int electionId)
        {
            bool dejaVote = _context.Votes.Any(v =>
                v.CitoyenId == citoyenId &&
                v.ElectionId == electionId);

            return Ok(new {
                aVote = dejaVote,
                message = dejaVote
                    ? "Ce citoyen a déjà voté"
                    : "Ce citoyen n'a pas encore voté"
            });
        }

        
        // 4. JOURNAL DES VOTES PAR ELECTION
        // GET api/electeurs/journal/{electionId}
      
        [HttpGet("journal/{electionId}")]
        public IActionResult GetJournal(int electionId)
        {
            var logs = _context.VoteLogs
                .Where(l => l.ElectionId == electionId)
                .Select(l => new {
                    l.Id,
                    l.CitoyenId,
                    l.Action,
                    l.LogDate
                })
                .OrderByDescending(l => l.LogDate)
                .ToList();

            return Ok(new {
                electionId = electionId,
                total = logs.Count,
                logs = logs
            });
        }

        
        // 5. JOURNAL PAR CITOYEN
        // GET api/electeurs/journal/citoyen/{citoyenId}
        
        [HttpGet("journal/citoyen/{citoyenId}")]
        public IActionResult GetJournalCitoyen(int citoyenId)
        {
            var logs = _context.VoteLogs
                .Where(l => l.CitoyenId == citoyenId)
                .Select(l => new {
                    l.Id,
                    l.ElectionId,
                    l.Action,
                    l.LogDate
                })
                .OrderByDescending(l => l.LogDate)
                .ToList();

            return Ok(new {
                citoyenId = citoyenId,
                totalActions = logs.Count,
                logs = logs
            });
        }
    }

    // DTO ho an'ny vote
    public class VoterDto
    {
        public int CitoyenId { get; set; }
        public int CandidatId { get; set; }
        public int ElectionId { get; set; }
    }
}