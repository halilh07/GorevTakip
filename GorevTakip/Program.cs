// --- USING SATIRLARI ---
// Kırmızı çizgi çıkarsa ilk bakacağın yer: eksik using olabilir.
using GorevTakip.Data;                  // UygulamaDbContext
using Microsoft.AspNetCore.Identity;    // IdentityUser, SignInManager
using Microsoft.EntityFrameworkCore;    // UseSqlite

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// BÖLÜM 1: builder.Build()'DEN ÖNCE = "Hangi parçaları kullanacağım?"
// Yeni bir servis eklemek istersen buraya yazarsın.
// =====================================================

builder.Services.AddControllers();   // Controller sınıflarını kullanabilmek için
builder.Services.AddOpenApi();       // API tarif belgesi (şimdilik işimize yaramıyor, kalsın)

// Veritabanı bağlantısı.
// DEĞİŞTİRME İSTEĞİ: SQL Server'a geçersen UseSqlite yerine UseSqlServer yazarsın.
// Veritabanı dosyasının adını değiştirmek istersen appsettings.json'daki
// "Varsayilan" satırını düzenlersin.
builder.Services.AddDbContext<UygulamaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Varsayilan")));

// Yetkilendirme sistemini ekler ("bu kullanıcının izni var mı?" kontrolü için).
builder.Services.AddAuthorization();

// Kullanıcı sistemini (kayıt, giriş, şifre kontrolü) ekler.
// IdentityUser = hazır kullanıcı sınıfı.
// AddEntityFrameworkStores = "kullanıcıları UygulamaDbContext'teki veritabanında sakla".
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<UygulamaDbContext>();

var app = builder.Build();   // BU SATIRDAN SONRA servis eklenemez

// =====================================================
// BÖLÜM 2: builder.Build()'DEN SONRA = "İstekler hangi sırayla işlensin?"
// SIRA ÖNEMLİ, yerlerini değiştirme.
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();   // Sadece geliştirme sırasında çalışır
}

app.UseHttpsRedirection();   // http ile gelenleri https'e yönlendirir
app.UseDefaultFiles();       // Adres "/" ise index.html'i bulur
app.UseStaticFiles();        // wwwroot'taki html/css/js dosyalarını sunar

// KURAL: UseAuthentication her zaman UseAuthorization'dan ÖNCE gelir.
// Önce "sen kimsin?" (cookie'ye bakar), sonra "iznin var mı?"
app.UseAuthentication();
app.UseAuthorization();

// Hazır kayıt ve giriş adreslerini ekler. Hepsinin başına /api/hesap gelir:
//   POST /api/hesap/register        → yeni hesap aç
//   POST /api/hesap/login           → giriş yap
//   GET  /api/hesap/manage/info     → giriş yapanın bilgisi
// DEĞİŞTİRME İSTEĞİ: Adresin başını değiştirmek istersen "/api/hesap" yazısını değiştir.
app.MapGroup("/api/hesap").MapIdentityApi<IdentityUser>();

// Hazır sistemde çıkış adresi yok, onu biz yazıyoruz.
// POST /api/hesap/cikis → giriş cookie'sini siler.
// RequireAuthorization = sadece giriş yapmış kullanıcılar çağırabilir.
app.MapPost("/api/hesap/cikis", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization();

app.MapControllers();   // Controller adreslerini (/api/gorevler) haritaya bağlar

app.Run();   // Uygulamayı çalıştırır, istek beklemeye başlar
