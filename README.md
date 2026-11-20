# 📝 Görev Takip

Görevlerini kategoriye göre ekleyip zamanında tamamladıkça puan kazandığın, seviye atladığın oyunlaştırılmış bir görev takip web uygulaması.

## Özellikler

- **Hesap sistemi:** Kayıt ol, giriş yap, çıkış yap. Her kullanıcı yalnızca kendi görevlerini görür.
- **Kategoriler:** Her görev bir kategoriye ait ve her kategorinin kendi standart süresi var.
- **Puan sistemi:** Görevi ne kadar erken bitirirsen o kadar çok puan alırsın, standart süreyi aşarsan ceza yersin.
- **Seviye sistemi:** Puan topladıkça seviye atlarsın. Seviyeler ilerledikçe zorlaşır.
- **Profil kartı:** Seviye, ilerleme çubuğu, toplam puan, tamamlanan görev sayısı ve son tamamlanan görevler.
- **Not kağıdı tasarımı:** Görevler kategoriye göre renkli, hafif eğik not kağıtları olarak görünür.
- **Mobil uyumlu:** Telefonda tek sütun, bilgisayarda iki sütunlu düzen.

## Puan kuralları

Puan, görev **eklendiği andan tamamlandığı ana kadar geçen süreye** göre hesaplanır.

| Durum | Puan |
|---|---|
| Hemen tamamlandı | 100 |
| Standart sürenin içinde tamamlandı | 100'den 50'ye doğru düzgün azalır |
| Standart süre aşıldıktan sonra tamamlandı | 50'den başlar, her ek standart süre için 100 puan düşer |
| Alt sınır | -100 |

Silme kuralları:

- Tamamlanmamış bir görev standart süresi dolmadan silinirse puana etkisi olmaz.
- Standart süresi dolduktan sonra silinirse silme anındaki ceza (0 ile -100 arası) toplam puana işlenir.
- Silinen görevler listeden kaybolur ama veritabanında kalır, bu sayede ceza silerek temizlenemez.
- Tamamlanan bir görevin işareti geri alınamaz.

Seviye: 1. seviyeden 2. seviyeye 200 puan gerekir, sonraki her seviye geçişi bir öncekinden 200 puan daha fazlasını ister (200, 400, 600, ...).

### Kategoriler ve standart süreler

| Kategori | Standart süre |
|---|---|
| Ders | 48 saat |
| Spor | 24 saat |
| İş | 24 saat |
| Kişisel | 72 saat |
| Alışveriş | 24 saat |

Süreler `Models/KategoriAyarlari.cs` dosyasında tanımlıdır.

## Kullanılan teknolojiler

- **Backend:** C#, ASP.NET Core Web API (.NET 10)
- **Veritabanı:** SQLite, Entity Framework Core (migration'larla)
- **Kimlik doğrulama:** ASP.NET Core Identity (cookie tabanlı oturum, şifreler hash'lenerek saklanır)
- **Frontend:** HTML, CSS (Grid, Flexbox, animasyonlar, media query), saf JavaScript (fetch API)
- **Araçlar:** Visual Studio, Git, GitHub

## Proje yapısı

```
GorevTakip/
├── Controllers/
│   ├── GorevlerController.cs     # Görev ekleme, güncelleme, silme, listeleme
│   └── ProfilController.cs       # Profil bilgisi (puan, seviye, son tamamlananlar)
├── Models/
│   ├── Gorev.cs                  # Görev modeli
│   ├── KategoriAyarlari.cs       # Kategoriler ve standart süreler
│   └── PuanHesaplayici.cs        # Puan ve silme cezası hesabı
├── Data/
│   └── UygulamaDbContext.cs      # Veritabanı bağlantısı
├── Migrations/                   # Veritabanı yapısının geçmişi
├── wwwroot/
│   ├── index.html
│   ├── style.css
│   └── script.js
└── Program.cs                    # Uygulamanın başlangıç ve ayar dosyası
```

## API adresleri

| Adres | Ne yapar |
|---|---|
| `POST /api/hesap/register` | Yeni hesap açar |
| `POST /api/hesap/login?useCookies=true` | Giriş yapar |
| `POST /api/hesap/cikis` | Çıkış yapar |
| `GET /api/hesap/manage/info` | Giriş yapan kullanıcının bilgisi |
| `GET /api/gorevler` | Kullanıcının görevlerini listeler |
| `POST /api/gorevler` | Yeni görev ekler |
| `PUT /api/gorevler/{id}` | Görevi günceller (tamamlandı olarak işaretler) |
| `DELETE /api/gorevler/{id}` | Görevi siler (gizler) |
| `GET /api/profil` | Profil bilgilerini verir |

## Güvenlik notları

- Görevlerin sahibi, tarihleri ve puanları sunucuda belirlenir, tarayıcıdan gelen değerlere güvenilmez.
- Her sorgu giriş yapan kullanıcının Id'siyle filtrelenir, kimse başkasının görevine erişemez.
- Şifreler düz yazı olarak saklanmaz.

## Çalıştırma

Gerekenler: **Visual Studio** (ASP.NET iş yükü kurulu) ve **.NET 10 SDK**.

1. Projeyi indir:

   ```
   git clone https://github.com/halilh07/GorevTakip.git
   ```

2. `GorevTakip.sln` dosyasını Visual Studio ile aç
3. **Görünüm → Diğer Pencereler → Paket Yöneticisi Konsolu**'nu aç ve şunu yaz (veritabanı dosyası repoda yok, bu komut onu oluşturur):

   ```
   Update-Database
   ```

4. **F5** ile çalıştır ve tarayıcıda kayıt ol

Şifre kuralları: en az 6 karakter, büyük harf, küçük harf, rakam ve özel karakter (örn. `Sifre123!`).

## Neler öğrendim

Bu proje boyunca REST API tasarımını, Entity Framework Core migration'larını, cookie tabanlı kimlik doğrulamayı, sunucu tarafı doğrulamanın önemini ve modern CSS düzen tekniklerini (Grid, Flexbox, animasyonlar, mobil uyum) uyguladım.
