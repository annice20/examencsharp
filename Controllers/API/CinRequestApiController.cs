using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers.API
{
    [Route("api/cin-requests")]
    [ApiController]
    public class CinRequestApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CinRequestApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET api/cin-requests
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _db.CinRequests
                .Include(r => r.Citoyen)
                .Select(r => new {
                    r.Id, r.CitoyenId, r.Status, r.RequestDate,
                    citoyen = new {
                        r.Citoyen!.NomCitoyen,
                        r.Citoyen.PrenomCitoyen,
                        r.Citoyen.CIN
                    }
                }).ToListAsync();
            return Ok(requests);
        }

        // GET api/cin-requests/en-attente
        [HttpGet("en-attente")]
        public async Task<IActionResult> GetEnAttente()
        {
            var requests = await _db.CinRequests
                .Include(r => r.Citoyen)
                .Where(r => r.Status == "En attente")
                .Select(r => new {
                    r.Id, r.CitoyenId, r.Status, r.RequestDate,
                    citoyen = new {
                        r.Citoyen!.NomCitoyen,
                        r.Citoyen.PrenomCitoyen,
                        r.Citoyen.CIN
                    }
                }).ToListAsync();
            return Ok(requests);
        }

        // PUT api/cin-requests/5/approuver
        [HttpPut("{id}/approuver")]
        public async Task<IActionResult> Approuver(int id)
        {
            var request = await _db.CinRequests.FindAsync(id);
            if (request == null) return NotFound(new { message = "Demande introuvable" });

            request.Status = "Approuvé";
            await _db.SaveChangesAsync();
            return Ok(new { message = "CIN approuvée" });
        }

        // PUT api/cin-requests/5/refuser
        [HttpPut("{id}/refuser")]
        public async Task<IActionResult> Refuser(int id)
        {
            var request = await _db.CinRequests.FindAsync(id);
            if (request == null) return NotFound(new { message = "Demande introuvable" });

            request.Status = "Refusé";
            await _db.SaveChangesAsync();
            return Ok(new { message = "CIN refusée" });
        }
    }
}