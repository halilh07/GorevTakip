// --- USING SATIRLARI ---
// Kırmızı çizgi çıkarsa ilk bakacağın yer: eksik using olabilir.
using System.Security.Claims;               // Giriş yapan kullanıcının bilgisini okumak için
using GorevTakip.Data;                      // UygulamaDbContext
using GorevTakip.Models;                    // Gorev
using Microsoft.AspNetCore.Authorization;   // [Authorize] etiketi
using Microsoft.AspNetCore.Mvc;             // Controller sınıfları
using Microsoft.EntityFrameworkCore;        // ToListAsync, FirstOrDefaultAsync

namespace GorevTakip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // Adres: /api/gorevler (sınıf adından "Controller" atılır)

    // [Authorize] = "Bu controller'a SADECE giriş yapmış kullanıcılar girebilir."
    // Giriş yapmayan biri herhangi bir metodu çağırınca otomatik 401 hatası alır.
    // DEĞİŞTİRME İSTEĞİ: Herkese açık bir metot istersen o metodun üstüne
    // [AllowAnonymous] yaz.
    [Authorize]
    public class GorevlerController : ControllerBase
    {
        // Veritabanı kapısı. Constructor'dan gelir (Dependency Injection).
        private readonly UygulamaDbContext _db;

        public GorevlerController(UygulamaDbContext db)
        {
            _db = db;
        }

        // YARDIMCI METOT: Giriş yapan kullanıcının Id'sini verir.
        // "User" ASP.NET'in hazır verdiği, o isteği yapan kullanıcıdır.
        // Cookie'den okunur, kullanıcı bunu dışarıdan değiştiremez (güvenli).
        // Aşağıdaki her metot kimin istek yaptığını bu metotla öğrenir.
        private string KullaniciIdAl()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        // GET /api/gorevler  → sadece giriş yapan kullanıcının görevlerini getir
        [HttpGet]
        public async Task<IActionResult> Getir()
        {
            var kid = KullaniciIdAl();

            // Where = filtre (SQL'deki WHERE). Sadece bu kullanıcıya ait görevler gelir.
            // DEĞİŞTİRME İSTEĞİ: Sıralama için .OrderByDescending(g => g.Id) ekleyebilirsin
            // (Where'den sonra, ToListAsync'ten önce).
            var liste = await _db.Gorevler
                // Şart: görev bu kullanıcının olsun VE silinmemiş olsun.
                // && = "ve", ! = "değil". Yani "silinmemiş olanlar".
                // Silinen görevler veritabanında kalır ama listeye hiç gelmez.
                .Where(g => g.KullaniciId == kid && !g.Silindi)
                .ToListAsync();

            return Ok(liste);
        }

        // POST /api/gorevler  → yeni görev ekle
        [HttpPost]
        public async Task<IActionResult> Ekle(Gorev g)
        {   // Kategori kontrolü: kategori boşsa veya KategoriAyarlari listesinde
            // yoksa görevi kaydetmeden hata dön.
            // ContainsKey = "sözlükte bu anahtar var mı?" (var ise true).
            // Başındaki ! "yoksa" demek.
            // BadRequest = 400 hatası: "gönderilen veri hatalı". Mesaj arayüze gider,
            // ileride kullanıcıya gösterebiliriz.
            // DEĞİŞTİRME İSTEĞİ: Mesaj yazısını istediğin gibi değiştirebilirsin.
            // Yeni kategori eklemek için bu dosyaya değil KategoriAyarlari.cs'ye gidersin.
            if (!KategoriAyarlari.StandartSureler.ContainsKey(g.Kategori))
            {
                return BadRequest("Geçersiz veya boş kategori.");
            }

            // Id'yi 0 yapıyoruz: veritabanı kendisi numara versin.
            // Sahibi ise tarayıcıdan gelene GÜVENMEDEN kendimiz yazıyoruz.
            // Yoksa biri başkasının adına görev ekleyebilirdi.
            g.Id = 0;
            g.KullaniciId = KullaniciIdAl();
            // Aşağıdaki alanları tarayıcıdan gelen değerlere BIRAKMIYORUZ,
            // sunucu kendisi belirliyor. Aksi hâlde biri istek gönderirken
            // kendine hazır puan yazabilir ya da geçmiş tarih uydurup puanı şişirebilirdi.

            // Oluşturulma anı = şu an (sunucu saati, UTC).
            g.OlusturmaTarihi = DateTime.UtcNow;

            // Yeni görev tamamlanmamış başlar: tamamlanma tarihi boş, puan 0.
            g.Tamamlandi = false;
            g.TamamlanmaTarihi = null;
            g.Puan = 0;

            _db.Gorevler.Add(g);              // Sepete koy (henüz kayıt yok)
            await _db.SaveChangesAsync();     // Gerçekten kaydet (await'i unutma!)
            return Ok(g);
        }

        // PUT /api/gorevler/5  → 5 numaralı görevi güncelle
        // {id} adresin sonundan gelir, metot parametresindeki "id" ile aynı adda olmalı.
        [HttpPut("{id}")]
        public async Task<IActionResult> Guncelle(int id, Gorev yeni)
        {
            var kid = KullaniciIdAl();

            // Görevi hem Id'sine HEM sahibine göre arıyoruz.
            // Başkasının görevi bulunamaz, yani başkasının görevini değiştiremezsin.
            var gorev = await _db.Gorevler
                .FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == kid && !g.Silindi);

            // Bulunamadıysa 404 dön (var olmayan ya da sana ait olmayan görev).
            if (gorev == null)
            {
                return NotFound();
            }

            // Sadece değişmesine izin verdiğimiz alanları güncelliyoruz.
            // DEĞİŞTİRME İSTEĞİ: Gorev'e yeni bir alan eklersen (örneğin Aciklama)
            // güncellenmesini istiyorsan buraya bir satır eklersin:
            //     gorev.Aciklama = yeni.Aciklama;
            gorev.Baslik = yeni.Baslik;

            // Tamamlanma durumu değişti mi? İki durum var, ikisini ayrı ayrı ele alıyoruz.
            // Durum değişmediyse (örn. zaten tamamlı bir görev için tekrar "tamamlandı"
            // gelirse) HİÇBİR ŞEY yapmıyoruz. Böylece puan tekrar hesaplanıp değiştirilemez.

            // DURUM 1: Daha önce tamamlanmamıştı, şimdi tamamlandı olarak işaretlendi.
            if (yeni.Tamamlandi && !gorev.Tamamlandi)
            {
                gorev.Tamamlandi = true;

                // Tamamlanma anı = şu an. Tarayıcıdan gelene güvenmiyoruz.
                gorev.TamamlanmaTarihi = DateTime.UtcNow;

                // Puanı hesaplayıp görevin üzerine yazıyoruz.
                // Metot üç bilgi ister: kategori, oluşturma anı, tamamlanma anı.
                // ".Value": TamamlanmaTarihi boş olabilen bir alan (DateTime?).
                // Burada boş olmadığını biliyoruz (az önce doldurduk),
                // .Value onun içindeki gerçek tarihi çıkarır.
                gorev.Puan = PuanHesaplayici.Hesapla(
                    gorev.Kategori,
                    gorev.OlusturmaTarihi,
                    gorev.TamamlanmaTarihi.Value);
            }

            // Tamamlanmış bir görevin işaretini KALDIRMAYA izin vermiyoruz.
            // Aşağıdaki durumda ("tamamlandı" olan görev için "tamamlanmadı" gelirse)
            // hiçbir if çalışmaz, görev olduğu gibi kalır. Böylece puan veya ceza,
            // işareti kaldırıp tekrar işaretleyerek değiştirilemez.
            // DEĞİŞTİRME İSTEĞİ: Geri almaya izin vermek istersen Adım 14'teki
            // "else if (!yeni.Tamamlandi && gorev.Tamamlandi)" bloğunu geri ekle.

            await _db.SaveChangesAsync();
            return Ok(gorev);
        }

        // DELETE /api/gorevler/5  → 5 numaralı görevi sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> Sil(int id)
        {
            var kid = KullaniciIdAl();

            // Yine hem Id hem sahip kontrolü: başkasının görevi silinemez.
            var gorev = await _db.Gorevler
                // && !g.Silindi = "silinmemiş olanlar arasında ara".
                // ! "değil" demek. Zaten silinmiş bir görev bulunamaz, böylece
                // aynı göreve iki kez silme isteği gönderip ceza iki kez kesilemez.
                .FirstOrDefaultAsync(g => g.Id == id && g.KullaniciId == kid && !g.Silindi);

            if (gorev == null)
            {
                return NotFound();
            }

            // Görevi veritabanından atmıyoruz, sadece gizliyoruz (yumuşak silme).
            // Kayıt kalır, böylece puanı (özellikle ceza) toplam puandan kaybolmaz.
            gorev.Silindi = true;

            // Tamamlanmamış görev siliniyorsa ceza hesapla ve görevin Puan alanına yaz.
            //   Standart süre dolmadan silindiyse SilmeCezasi 0 döner, etkisi olmaz.
            //   Süre dolduktan sonra silindiyse -100 ile 0 arası ceza döner.
            // Tamamlanmış görevin Puan'ı tamamlanırken zaten yazıldı, ona dokunmuyoruz:
            // görev gizli kalsa bile puanı sayılmaya devam edecek.
            // DateTime.UtcNow = silme anı (sunucu saati, tarayıcıya güvenmiyoruz).
            if (!gorev.Tamamlandi)
            {
                gorev.Puan = PuanHesaplayici.SilmeCezasi(
                    gorev.Kategori,
                    gorev.OlusturmaTarihi,
                    DateTime.UtcNow);
            }
            await _db.SaveChangesAsync();     // Gerçekten sil

            return NoContent();               // 204: başarılı, geri gönderecek veri yok
        }
    }
}