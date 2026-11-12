using GorevTakip.Data;
using GorevTakip.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GorevlerController : ControllerBase
    {
        private readonly UygulamaDbContext _db;

        public GorevlerController(UygulamaDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Getir()
        {
            var liste = await _db.Gorevler.OrderByDescending(g => g.Id).ToListAsync();
            return Ok(liste);
        }

        [HttpPost]
        public async Task<IActionResult> Ekle(Gorev g)
        {
            if (string.IsNullOrWhiteSpace(g.Baslik))
            {
                return BadRequest("Başlık boş olamaz.");
            }

            _db.Gorevler.Add(g);
            await _db.SaveChangesAsync();
            return Ok(g);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Guncelle(int id, Gorev yeni)
        {
            var gorev = await _db.Gorevler.FindAsync(id);
            if (gorev == null)
            {
                return NotFound();
            }

            gorev.Baslik = yeni.Baslik;
            gorev.Tamamlandi = yeni.Tamamlandi;
            await _db.SaveChangesAsync();
            return Ok(gorev);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Sil(int id)
        {
            var gorev = await _db.Gorevler.FindAsync(id);
            if (gorev == null)
            {
                return NotFound();
            }

            _db.Gorevler.Remove(gorev);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
