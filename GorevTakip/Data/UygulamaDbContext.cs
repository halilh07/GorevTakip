// "using" satırları: başka klasörlerdeki sınıfları bu dosyada kullanabilmek için.
// Bir sınıf kırmızı çizgi olursa ilk bakacağın yer buradır, eksik using olabilir.
using GorevTakip.Models;                              // Gorev sınıfı buradan geliyor
using Microsoft.AspNetCore.Identity;                  // IdentityUser (hazır kullanıcı sınıfı)
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // IdentityDbContext (kullanıcı tabloları)
using Microsoft.EntityFrameworkCore;                  // DbContext, DbSet, ModelBuilder

namespace GorevTakip.Data
{
    // Veritabanı kapımız. "IdentityDbContext<IdentityUser>" sayesinde
    // kullanıcı, şifre, rol tabloları kendiliğinden gelir.
    // DEĞİŞTİRME İSTEĞİ: Kullanıcıya "Ad Soyad" gibi alan eklemek istersen
    // kendi kullanıcı sınıfını yazıp (IdentityUser'dan türeterek)
    // aşağıdaki iki yerdeki "IdentityUser" yerine onu yazarsın:
    // bu satır ve OnModelCreating içindeki HasOne<...>.
    public class UygulamaDbContext : IdentityDbContext<IdentityUser>
    {
        // Constructor: veritabanı ayarlarını (SQLite, bağlantı yazısı) Program.cs'ten alır.
        // Buraya dokunma, ayarlar Program.cs ve appsettings.json'da yapılır.
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options)
            : base(options)
        {
        }

        // Her DbSet bir TABLO demektir. Tablo adı = özelliğin adı (Gorevler).
        // DEĞİŞTİRME İSTEĞİ: Yeni bir tablo istersen (örneğin Kategori sınıfı yazdın)
        // buraya şu satırı eklersin:
        //     public DbSet<Kategori> Kategoriler { get; set; } = null!;
        // Sonra Paket Yöneticisi Konsolu'nda Add-Migration ve Update-Database yaz.
        public DbSet<Gorev> Gorevler { get; set; } = null!;

        // EF Core tabloları kurarken bu metodu çalıştırır.
        // Varsayılan kuralların dışında özel ayarlar burada yapılır.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity'nin kendi tablo ayarlarını çalıştırır.
            // BU SATIRI SİLME ve hep en üstte tut, silersen kullanıcı sistemi bozulur.
            base.OnModelCreating(builder);

            // Görev ile kullanıcıyı bağlayan ilişki. Cümle gibi oku:
            // "Bir Gorev'in bir kullanıcısı var (HasOne),
            //  bir kullanıcının birçok görevi olabilir (WithMany),
            //  bağlantıyı Gorev'deki KullaniciId alanı tutar (HasForeignKey)."
            // Sonuç: var olmayan bir kullanıcıya görev eklenemez.
            // DEĞİŞTİRME İSTEĞİ: Kullanıcı silinince görevleri kalsın istersen
            // en sona .OnDelete(DeleteBehavior.Restrict) ekleyebilirsin
            // (sonra migration gerekir).
            builder.Entity<Gorev>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(g => g.KullaniciId);
        }
    }
}
