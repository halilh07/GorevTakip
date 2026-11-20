namespace GorevTakip.Models
{
    // Puan hesabını tek bir yerde topluyoruz.
    // static class: nesne üretmeden PuanHesaplayici.Hesapla(...) diye çağıracağız.
    // Kuralları değiştirmek istediğinde sadece bu dosyaya bakman yeterli olacak.
    public static class PuanHesaplayici
    {
        // Parametreler (metoda dışarıdan verilen bilgiler):
        //   kategori   = görevin kategorisi (örn. "Ders")
        //   olusturma  = görevin eklendiği an
        //   tamamlanma = görevin tamamlandığı an
        // Dönüş: int = hesaplanan puan.
        public static int Hesapla(string kategori, DateTime olusturma, DateTime tamamlanma)
        {
            // 1) Kategorinin standart süresini sözlükten bul.
            // TryGetValue = "bu anahtar sözlükte var mı bak, varsa değerini ver".
            //   Varsa true döner ve değer 'standart' değişkenine yazılır.
            //   Yoksa false döner. Başına ! koyduk: "yoksa" demek.
            // "out TimeSpan standart" = metodun sonucu bu değişkene bırakmasının yolu.
            //
            // DEĞİŞTİRME İSTEĞİ: Tanımsız kategori için varsayılan süreyi
            // değiştirmek istersen aşağıdaki 24 sayısını değiştir.
            if (!KategoriAyarlari.StandartSureler.TryGetValue(kategori, out TimeSpan standart))
            {
                standart = TimeSpan.FromHours(24);
            }

            // 2) Geçen süre = tamamlanma anı - oluşturma anı.
            // İki tarihi çıkarınca TimeSpan (süre) çıkar. Örn. 30 saat.
            TimeSpan gecen = tamamlanma - olusturma;

            // 3) Standart süre DOLMADAN tamamlandıysa:
            // puan 100'den başlar, süre ilerledikçe 50'ye doğru düzgün azalır.
            if (gecen <= standart)
            {
                // oran = geçen süre / standart süre. Her zaman 0 ile 1 arasında çıkar.
                //   0 = hemen tamamlandı, 1 = tam standart süre doldu, 0.5 = yarısı geçti.
                // TimeSpan'ı TimeSpan'a bölünce sonuç double (ondalıklı sayı) çıkar.
                double oran = gecen / standart;

                // Formül: 100 - 50 * oran
                //   oran 0   → 100 - 0  = 100
                //   oran 0.5 → 100 - 25 = 75
                //   oran 1   → 100 - 50 = 50
                // DEĞİŞTİRME İSTEĞİ: Başlangıç puanını değiştirmek için 100'ü,
                // standart süre dolunca kalacak puanı değiştirmek için 50'yi değiştir.
                // (İkisi birbirine bağlı: 50, "100 eksi standart süre sonundaki puan"dır.
                // Örneğin standart süre sonunda 60 puan kalsın istersen 50 yerine 40 yaz.)
                double puan = 100 - 50 * oran;

                // Math.Round = en yakın tam sayıya yuvarlar (89.58 → 90).
                // (int) = double'ı int'e çevirir. Puan alanımız tam sayı olduğu için gerekli.
                return (int)Math.Round(puan);
            }

            // 4) Buraya geldiysek standart süre AŞILMIŞ demektir.
            // (Süre aşılmasaydı yukarıdaki if çalışır ve metot orada biterdi.)

            // Standart süreyi ne kadar aştık? Örn. 48 saat standart, 72 saatte bitti → 24 saat.
            TimeSpan asim = gecen - standart;

            // Aşım oranı = aşılan süre / standart süre.
            //   0.5 = standart sürenin yarısı kadar geciktik
            //   1   = standart süre kadar daha geciktik (toplam süre iki katı oldu)
            double asimOrani = asim / standart;

            // Formül: 50 - 100 * asimOrani
            //   Aşım yok (0)  → 50 puandan başlar (if bloğunun bittiği yer)
            //   asimOrani 0.5 → 50 - 50  = 0
            //   asimOrani 1   → 50 - 100 = -50
            //   asimOrani 2   → 50 - 200 = -150 (alt sınır devreye girecek)
            // DEĞİŞTİRME İSTEĞİ: Cezayı hafifletmek için 100'ü küçült (örn. 50),
            // ağırlaştırmak için büyüt. 50 ise ceza başlarken kalan puandır,
            // bir önceki adımdaki 50 ile AYNI olmalı, yoksa puan tabloda zıplar.
            double cezaliPuan = 50 - 100 * asimOrani;

            // Math.Max(a, b) = ikisinden büyük olanı verir.
            // cezaliPuan -150 çıksa bile Math.Max(-150, -100) = -100 olur,
            // yani puan -100'ün altına inemez (alt sınır).
            // DEĞİŞTİRME İSTEĞİ: Alt sınırı değiştirmek için -100'ü değiştir.
            return (int)Math.Round(Math.Max(cezaliPuan, -100));
        }

        // Tamamlanmamış bir görev SİLİNİRKEN uygulanacak cezayı hesaplar.
        // Dönüş her zaman 0 ya da negatiftir (0 ile -100 arası): silmek puan kazandırmaz,
        // sadece ceza kesebilir.
        // Parametreler: kategori, görevin eklendiği an, silinme anı.
        public static int SilmeCezasi(string kategori, DateTime olusturma, DateTime silme)
        {
            // Standart süreyi bul (Hesapla'daki aynı kod: yoksa 24 saat varsayılan).
            if (!KategoriAyarlari.StandartSureler.TryGetValue(kategori, out TimeSpan standart))
            {
                standart = TimeSpan.FromHours(24);
            }

            // Görev eklendiğinden silinene kadar geçen süre.
            TimeSpan gecen = silme - olusturma;

            // Standart süre DOLMADAN silindiyse ceza yok, puana etkisi sıfır.
            if (gecen <= standart)
            {
                return 0;
            }

            // Süre aşıldıysa ne kadar aşıldığına bak.
            // asimOrani: 0.5 = standart sürenin yarısı kadar geciktik,
            //            1   = standart süre kadar daha geciktik.
            TimeSpan asim = gecen - standart;
            double asimOrani = asim / standart;

            // Ceza = -100 * aşım oranı. Aşım büyüdükçe ceza büyür.
            // Math.Max(ceza, -100) = ceza -100'ün altına inemez (alt sınır).
            // DEĞİŞTİRME İSTEĞİ: Cezayı hafifletmek için 100'ü küçült (örn. 50),
            // alt sınırı değiştirmek için -100'ü değiştir.
            double ceza = -100 * asimOrani;
            return (int)Math.Round(Math.Max(ceza, -100));
        }
    }
}
