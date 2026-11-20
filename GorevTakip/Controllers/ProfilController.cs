// Eksik using olursa kırmızı çizgi çıkar, Ctrl + . ile eklettirebilirsin.
using System.Security.Claims;               // Giriş yapan kullanıcının Id'sini okumak için
using GorevTakip.Data;                      // UygulamaDbContext
using Microsoft.AspNetCore.Authorization;   // [Authorize]
using Microsoft.AspNetCore.Mvc;             // Controller sınıfları
using Microsoft.EntityFrameworkCore;        // CountAsync

namespace GorevTakip.Controllers
{
    [ApiController]
    // Adres: /api/profil (sınıf adından "Controller" atılır, küçük harfe çevrilir).
    [Route("api/[controller]")]
    // Giriş yapmayan kimse profile erişemesin.
    [Authorize]
    public class ProfilController : ControllerBase
    {
        // Veritabanı kapısı. Constructor'dan gelir (Dependency Injection).
        private readonly UygulamaDbContext _db;

        public ProfilController(UygulamaDbContext db)
        {
            _db = db;
        }

        // GET /api/profil → giriş yapan kullanıcının profil bilgilerini ver
        [HttpGet]
        public async Task<IActionResult> Getir()
        {
            // Giriş yapan kullanıcının Id'si. Cookie'den okunur,
            // kullanıcı bunu dışarıdan değiştiremez.
            var kid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // CountAsync = şartı sağlayan kayıtların SAYISINI verir.
            // Şart: görev bu kullanıcıya ait VE tamamlanmış.
            // Arkada şuna benzer bir SQL çalışır: SELECT COUNT(*) ... WHERE ...
            // DEĞİŞTİRME İSTEĞİ: Sadece belirli bir kategorideki tamamlananları
            // saymak istersen şarta "&& g.Kategori == "Ders"" ekleyebilirsin.
            var tamamlananSayisi = await _db.Gorevler
                .CountAsync(g => g.KullaniciId == kid && g.Tamamlandi);


            // Toplam puan = bu kullanıcının tamamlanmış görevlerinin puanlarının toplamı.
            // Where  = önce şarta uyan görevleri seç (bu kullanıcının, tamamlanmış olanlar).
            // SumAsync = seçilen görevlerin Puan alanlarını toplar.
            //   (g => g.Puan) "toplanacak alan Puan olsun" demek.
            // Ceza alan görevlerin puanı eksi olduğu için toplam da eksiye inebilir.
            // Hiç tamamlanmış görev yoksa toplam 0 çıkar.
            // DEĞİŞTİRME İSTEĞİ: Sadece belirli bir kategorinin puanını toplamak
            // istersen Where içindeki şarta "&& g.Kategori == "Ders"" ekle.
            var toplamPuan = await _db.Gorevler
                .Where(g => g.KullaniciId == kid)
                .SumAsync(g => g.Puan);


            // SEVİYE HESABI: seviye atladıkça bir sonraki seviye için gereken puan artıyor.
            // const = sabit: değeri program çalışırken değişmez, ayarı buradan yaparız.
            //
            // DEĞİŞTİRME İSTEĞİ:
            //  - ilkSeviyePuani: 1. seviyeden 2. seviyeye geçmek için gereken puan.
            //  - seviyeArtisi: her seviyede gereken puanın ne kadar artacağı.
            //  Daha zor bir sistem için ikisini de büyüt (örn. 300 ve 300),
            //  seviyeler hep aynı zorlukta kalsın istersen seviyeArtisi'ni 0 yap.
            const int ilkSeviyePuani = 200;
            const int seviyeArtisi = 200;

            // Eksi puanı 0 say: seviye 1'in altına inemez.
            var kalanPuan = Math.Max(toplamPuan, 0);

            var seviye = 1;                    // Herkes seviye 1'de başlar.
            var gereken = ilkSeviyePuani;      // Şu anki seviyeyi geçmek için gereken puan.

            // while = "şart doğru olduğu sürece tekrarla".
            // Elimizdeki puan bir sonraki seviyeye yetiyorsa seviye atla,
            // kullandığımız puanı düş, bir sonraki seviye için gereken puanı artır.
            // Puan yetmeyene kadar döngü döner, sonra durur.
            while (kalanPuan >= gereken)
            {
                kalanPuan -= gereken;   // -= : "kalanPuan = kalanPuan - gereken" demek
                seviye++;               // ++ : 1 artır
                gereken += seviyeArtisi; // += : "gereken = gereken + seviyeArtisi" demek
            }



            // new { ... } = adı olmayan geçici bir nesne. Ok() bunu JSON'a çevirir.
            // Gidecek JSON: {"tamamlananSayisi": 3}
            // Yeni bilgi eklemek için süslü parantezin içine virgülle yeni
            // alan yazacağız (sonraki adımlarda).
            // Süslü parantezin içine yeni alan eklemek için araya virgül koyuyoruz.
            // Gidecek JSON: {"tamamlananSayisi": 3, "toplamPuan": 215}
            // Gidecek JSON:
            // {"tamamlananSayisi": 1, "toplamPuan": 750, "seviye": 3,
            //  "seviyedekiPuan": 150, "sonrakiSeviyePuani": 600}
            //
            // "seviyedekiPuan = kalanPuan" biçimi: alanın JSON'daki adını biz veriyoruz,
            // değerini sağdaki değişkenden alıyor. (Değişkenle alan adı aynıysa
            // sadece değişkeni yazmak yeterliydi, burada adlar farklı.)
            //   seviyedekiPuan     = şu seviyede topladığın puan (döngüden kalan)
            //   sonrakiSeviyePuani = bir sonraki seviyeye geçmek için gereken puan
            // Arayüzde "150 / 600" gibi bir ilerleme çubuğu bunlarla çizilecek.
            // DEĞİŞTİRME İSTEĞİ: Alan adlarını değiştirirsen script.js'te
            // profil.seviyedekiPuan ve profil.sonrakiSeviyePuani kullandığımız
            // yerlerde de aynı adları yazman gerekir.
            var sonTamamlananlar = await _db.Gorevler
                .Where(g => g.KullaniciId == kid && g.Tamamlandi && !g.Silindi)
                .OrderByDescending(g => g.TamamlanmaTarihi)
                .Take(5)
                .Select(g => new { g.Baslik, g.Kategori, g.Puan, g.TamamlanmaTarihi })
                .ToListAsync();

            return Ok(new
            {
                tamamlananSayisi,
                toplamPuan,
                seviye,
                seviyedekiPuan = kalanPuan,
                sonrakiSeviyePuani = gereken,
                sonTamamlananlar
            });
        }
    }
}
