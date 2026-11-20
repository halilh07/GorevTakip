namespace GorevTakip.Models
{
    // "static class" = nesnesi oluşturulmayan, doğrudan KategoriAyarlari.XYZ diye
    // kullanılan sınıf. Bu bir ayar listesi olduğu için nesne üretmeye gerek yok.
    public static class KategoriAyarlari
    {
        // Dictionary = sözlük. "Anahtar → değer" çiftleri tutar.
        // Burada anahtar kategori adı (yazı), değer standart süre (TimeSpan).
        // TimeSpan = bir süre uzunluğu (örn. 48 saat). Tarih değil, "ne kadar" bilgisi.
        
        // static = tek kopya, herkes aynı listeyi kullanır.
        // readonly = liste program çalışırken başka bir listeyle değiştirilemez.
        // new() = boş sözlük oluştur, hemen altındaki süslü parantezle doldur.
        public static readonly Dictionary<string, TimeSpan> StandartSureler = new()
        {
            // Her satır bir kategori: { "Ad", süre },
            //
            // DEĞİŞTİRME İSTEĞİ:
            //  - Süreyi değiştirmek için sayıyı değiştir (48 → 72 gibi).
            //  - Birim değiştirmek için FromHours yerine FromMinutes veya FromDays yaz.
            //    Örnek: TimeSpan.FromMinutes(2) = 2 dakika.
            //    Ceza sistemini hızlıca test etmek için bir kategoriyi geçici olarak
            //    2 dakikaya çekebilirsin, sonra eski hâline döndür.
            //  - Yeni kategori eklemek için bir satır daha yaz, sonuna virgül koy.
            //  - Kategori adı arayüzdeki seçenekle HARF HARF aynı olmalı
            //    (büyük/küçük harf ve Türkçe karakterler dahil).
            { "Ders", TimeSpan.FromHours(48) },
            { "Spor", TimeSpan.FromHours(24) },
            { "İş", TimeSpan.FromHours(24) },
            { "Kişisel", TimeSpan.FromHours(72) },
            { "Alışveriş", TimeSpan.FromHours(24) }
        };
    }
}
