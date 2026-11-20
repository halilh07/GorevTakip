// =====================================================
// BÖLÜM 1: ELEMANLARI BULMA
// Elemanları HTML'deki id'leriyle bulup değişkene koyuyoruz.
// ÖNEMLİ: Buradaki "..." içindeki adlar index.html'deki id'lerle birebir aynı olmalı.
// Birini değiştirirsen ikisini de değiştir, yoksa "Cannot read properties of null" hatası çıkar.
// =====================================================

let animasyonOynasin = true;
let yeniGorevId = null;

const kategoriSiniflari = {
    "Ders": "ders",
    "Spor": "spor",
    "İş": "is",
    "Kişisel": "kisisel",
    "Alışveriş": "alisveris"
};

// Giriş bölümü
const girisAlani = document.getElementById("girisAlani");
const epostaKutu = document.getElementById("eposta");
const sifreKutu = document.getElementById("sifre");
const girisBtn = document.getElementById("girisBtn");
const kayitBtn = document.getElementById("kayitBtn");
const mesaj = document.getElementById("mesaj");

// Uygulama bölümü
const uygulamaAlani = document.getElementById("uygulamaAlani");
const kullaniciYazi = document.getElementById("kullaniciYazi");
const cikisBtn = document.getElementById("cikisBtn");
const kutu = document.getElementById("baslik");
const kategoriKutu = document.getElementById("kategori");
const ekleBtn = document.getElementById("ekleBtn");
const ul = document.getElementById("liste");
const sayac = document.getElementById("sayac");
// Liste boşken gösterilen mesaj. Ad, index.html'deki id ile birebir aynı olmalı.
const bosMesaj = document.getElementById("bosMesaj");

// Profil kartındaki elemanlar. Adlar index.html'deki id'lerle birebir aynı olmalı.
const profilHarf = document.getElementById("profilHarf");
const profilSeviye = document.getElementById("profilSeviye");
const profilPuan = document.getElementById("profilPuan");
const profilTamamlanan = document.getElementById("profilTamamlanan");
// İlerleme çubuğunun dolgusu ve altındaki yazı.
// Adlar index.html'deki id'lerle birebir aynı olmalı.
const ilerlemeDolgu = document.getElementById("ilerlemeDolgu");
const ilerlemeYazi = document.getElementById("ilerlemeYazi");
const sonListe = document.getElementById("sonListe");
const sonBos = document.getElementById("sonBos");


// =====================================================
// BÖLÜM 2: EKRAN DEĞİŞTİRME
// "gizli" sınıfı (style.css'te display:none) eklenince eleman kaybolur,
// çıkarılınca görünür. Hangi bölümün görüneceğini bu iki fonksiyon belirler.
// =====================================================

// Giriş ekranını göster, uygulama ekranını gizle
function girisGoster() {
    girisAlani.classList.remove("gizli");
    uygulamaAlani.classList.add("gizli");
    sifreKutu.value = "";       // Şifre kutusunu temizle
    mesaj.textContent = "";     // Eski hata mesajını sil
    ul.innerHTML = "";          // Önceki kullanıcının görevleri ekranda kalmasın
}

// Uygulama ekranını göster, giriş ekranını gizle
function uygulamaGoster(eposta) {
    girisAlani.classList.add("gizli");
    uygulamaAlani.classList.remove("gizli");
    kullaniciYazi.textContent = eposta;   // Üst çubukta e-postayı göster
    animasyonOynasin = true;
    gorevleriGetir();                     // Bu kullanıcının görevlerini yükle
    profilGetir();                        // Profil kartını doldur

    // Yuvarlak simgeye e-postanın ilk harfini büyük harfle yaz.
    // charAt(0) = yazının ilk harfi (sayma 0'dan başlar).
    // toUpperCase() = büyük harfe çevirir (a → A).
    profilHarf.textContent = eposta.charAt(0).toUpperCase();
}

// =====================================================
// BÖLÜM 3: GİRİŞ, KAYIT, ÇIKIŞ
// =====================================================

async function girisYap() {
    mesaj.textContent = "";
    const eposta = epostaKutu.value.trim();
    const sifre = sifreKutu.value;

    if (eposta === "" || sifre === "") {
        mesaj.textContent = "E-posta ve şifre boş olamaz.";
        return;
    }

    // "?useCookies=true" çok önemli: sunucuya "giriş bilgisini cookie olarak ver" der.
    // Bunu yazmazsan sunucu cookie yerine başka türde bir anahtar verir
    // ve tarayıcı seni sonraki isteklerde tanımaz.
    // Gönderilen alan adları (email, password) sunucunun beklediği adlardır,
    // Türkçeleştirme, çalışmaz.
    const cevap = await fetch("/api/hesap/login?useCookies=true", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email: eposta, password: sifre })
    });

    if (cevap.ok) {                      // ok = 200 serisi başarılı cevap
        uygulamaGoster(eposta);
    } else {
        // DEĞİŞTİRME İSTEĞİ: Mesajı buradan değiştirebilirsin.
        mesaj.textContent = "E-posta veya şifre hatalı.";
    }
}

async function kayitOl() {
    mesaj.textContent = "";
    const eposta = epostaKutu.value.trim();
    const sifre = sifreKutu.value;

    if (eposta === "" || sifre === "") {
        mesaj.textContent = "E-posta ve şifre boş olamaz.";
        return;
    }

    const cevap = await fetch("/api/hesap/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email: eposta, password: sifre })
    });

    if (cevap.ok) {
        // Kayıt başarılı: kullanıcıyı bir de giriş yaptırıyoruz,
        // böylece tekrar şifre yazmak zorunda kalmaz.
        await girisYap();
    } else {
        // DEĞİŞTİRME İSTEĞİ: Şifre kurallarını Program.cs'ten değiştirirsen
        // bu mesajı da buna göre güncelle.
        mesaj.textContent =
            "Kayıt başarısız. E-posta geçerli olmalı. Şifre en az 6 karakter olmalı ve " +
            "büyük harf, küçük harf, rakam ve özel karakter (örn. !) içermeli. " +
            "Bu e-posta zaten kayıtlı da olabilir.";
    }
}

async function cikisYap() {
    // Sunucuya "cookie'mi sil" deriz (Program.cs'te yazdığımız /api/hesap/cikis adresi).
    await fetch("/api/hesap/cikis", { method: "POST" });
    girisGoster();
}

// Sayfa ilk açıldığında çalışır: "Zaten giriş yapmış mıyım?"
// manage/info giriş yapmışsa bilgi döner, yapmamışsa 401 hatası verir.
// Bu sayede sayfayı yenileyince tekrar giriş yapmak gerekmez (cookie hatırlıyor).
async function oturumKontrol() {
    const cevap = await fetch("/api/hesap/manage/info");
    if (cevap.ok) {
        const bilgi = await cevap.json();
        uygulamaGoster(bilgi.email);
    } else {
        girisGoster();
    }
}

// Sunucudan gelen tarihi (örn. "2026-11-15T11:20:00") okunur yazıya çevirir
// (örn. "15.11.2026 14:20").
// DEĞİŞTİRME İSTEĞİ: Gösterim biçimini aşağıdaki seçeneklerden değiştirirsin.
// Örneğin saati kaldırmak için hour ve minute satırlarını sil.
function tarihYaz(tarihYazisi) {
    // Sunucu tarihleri UTC (dünya saati) olarak tutuyor ama veritabanından
    // okunurken sonundaki "Z" harfi kaybolabiliyor. "Z" yoksa tarayıcı bu tarihi
    // yerel saat sanır ve 3 saat kayma olur. O yüzden "Z"yi biz ekliyoruz.
    const duzgun = tarihYazisi.endsWith("Z") ? tarihYazisi : tarihYazisi + "Z";

    // new Date(...) yazıyı tarih nesnesine çevirir. Tarayıcı UTC'yi
    // bilgisayarının saat dilimine (Türkiye'de +3) kendisi çevirir.
    const tarih = new Date(duzgun);

    // toLocaleString = tarihi belirtilen dilin biçiminde yazıya çevirir.
    // "tr-TR" = Türkçe biçim (gün.ay.yıl).
    return tarih.toLocaleString("tr-TR", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit"
    });
}

// Sunucudan profil bilgisini çeker ve karttaki yazıları günceller.
// Sunucu şu JSON'u verir: {"tamamlananSayisi": 1, "toplamPuan": 99, "seviye": 1}
async function profilGetir() {
    const cevap = await fetch("/api/profil");

    // cevap.ok = başarılı mı? Başarısızsa (örn. oturum düşmüş, 401) hiçbir şey yapma.
    // Oturum düşmesini zaten gorevleriGetir yakalayıp giriş ekranına döndürüyor.
    if (!cevap.ok) {
        return;
    }

    const profil = await cevap.json();

    // textContent = elemanın içindeki yazıyı değiştirir.
    // Sağdaki ad (profil.seviye vb.) sunucunun gönderdiği JSON'daki alan adıdır.
    // Sunucuda alan adını değiştirirsen buradaki adı da değiştirmen gerekir.
    profilSeviye.textContent = profil.seviye;
    profilPuan.textContent = profil.toplamPuan;
    profilTamamlanan.textContent = profil.tamamlananSayisi;
    // Çubuğun doluluk yüzdesi = bu seviyede toplanan puan / seviye için gereken puan.
    // Örn. 150 / 600 = 0.25 → çarpı 100 = %25.
    // Math.min(100, ...) = yüzde 100'ü geçemez (güvenlik önlemi, taşma olmasın).
    // Sunucu puanı zaten gereken puanın altında gönderiyor, yani normalde 100'e
    // ulaşmaz, ama olası bir hatada çubuk kutudan taşmasın diye koyduk.
    const yuzde = Math.min(100, (profil.seviyedekiPuan / profil.sonrakiSeviyePuani) * 100);

    // style.width = elemanın CSS genişliğini kodla değiştirir.
    // Yüzdenin sonuna "%" yazısını ekliyoruz: 25 → "25%".
    // CSS'teki "transition" sayesinde çubuk ani atlamaz, kayarak dolar.
    ilerlemeDolgu.style.width = yuzde + "%";

    // Çubuğun altındaki yazı: "150 / 600 puan".
    // DEĞİŞTİRME İSTEĞİ: Yazıyı değiştirmek için şablon yazısını düzenle,
    // örneğin `Sonraki seviyeye ${profil.sonrakiSeviyePuani - profil.seviyedekiPuan} puan kaldı`.
    ilerlemeYazi.textContent = `${profil.seviyedekiPuan} / ${profil.sonrakiSeviyePuani} puan`;
    sonListe.innerHTML = "";

    // Sunucudan gelen en fazla 5 görevi tek tek satıra çeviriyoruz.
    for (const s of profil.sonTamamlananlar) {
        const satir = document.createElement("div");
        satir.className = "son-satir";

        // Kategori rozeti: görev listesindekiyle aynı mantık ve aynı renk sınıfları.
        // kategoriSiniflari, daha aşağıda tanımlı eşleştirme listesi.
        const rozet = document.createElement("b");
        rozet.textContent = s.kategori;
        rozet.className = "rozet rozet-" + (kategoriSiniflari[s.kategori] || "diger");

        // Görev başlığı.
        const baslik = document.createElement("span");
        baslik.className = "son-baslik";
        baslik.textContent = s.baslik;

        // Kazanılan puan. Pozitifse başına + koyuyoruz (örn. +90),
        // negatifse eksi işareti sayıyla birlikte zaten geliyor.
        // DEĞİŞTİRME İSTEĞİ: "puan" kelimesi eklemek için sonuna + " puan" yaz.
        const puan = document.createElement("span");
        puan.className = "son-puan";
        puan.textContent = (s.puan > 0 ? "+" : "") + s.puan;
        if (s.puan < 0) {
            puan.classList.add("eksi");
        }

        // Üç parçayı satıra, satırı listeye ekle.
        satir.append(rozet, baslik, puan);
        sonListe.appendChild(satir);
    }

    // Liste boşsa "Henüz tamamlanan görev yok" yazısı görünsün, doluysa gizlensin.
    // (Adım 37'deki toggle ile aynı mantık.)
    sonBos.classList.toggle("gizli", profil.sonTamamlananlar.length > 0);
}

// =====================================================
// BÖLÜM 4: GÖREVLER
// Eskisiyle aynı. Tek yeni şey: 401 gelirse giriş ekranına dön.
// =====================================================

async function gorevleriGetir() {
    // Sağ taraf css'deki class adları, sol taraf ise KategoriAyarlari.cs dosyasındakiyle aynı addaki kategori adları
    
    const cevap = await fetch("/api/gorevler");

    // 401 = giriş yapılmamış ya da oturum süresi dolmuş
    if (cevap.status === 401) {
        girisGoster();
        return;
    }

    const gorevler = await cevap.json();
    // classList.toggle("gizli", şart) = şart DOĞRUYSA "gizli" sınıfını ekler
    bosMesaj.classList.toggle("gizli", gorevler.length > 0);

    ul.innerHTML = "";   // Listeyi boşalt, yoksa eskilerin üstüne eklenir
    ul.classList.toggle("animasyonlu", animasyonOynasin);

    for (const g of gorevler) {
        const li = document.createElement("li");
        li.classList.add("kagit-" + (kategoriSiniflari[g.kategori] || "diger"));
        if (g.id === yeniGorevId) {
            li.classList.add("yeni");
        }
        if (g.tamamlandi) {
            li.classList.add("bitti");   // CSS'teki "li.bitti span" kuralı üstünü çizer
        }

        // Onay kutusu
        const onay = document.createElement("input");
        onay.type = "checkbox";
        onay.checked = g.tamamlandi;
        onay.disabled = g.tamamlandi;
        onay.addEventListener("change", () => durumDegistir(g, onay.checked));

        // Görev yazısı (textContent güvenlidir, HTML olarak çalıştırmaz)
        const yazi = document.createElement("span");
        yazi.textContent = g.baslik;
        // Başlığın altına küçük bilgi satırı: kategori • eklenme tarihi.
        // <small> = küçük yazı elemanı. Başlığın (span) içine koyuyoruz.
        // `${...}` = yazının içine değişken gömme. • işareti sadece bir ayraç.
        // DEĞİŞTİRME İSTEĞİ: Ayracı veya sırayı değiştirebilirsin,
        // örneğin tarihi önce yazmak için iki parçanın yerini değiştir.
        const bilgi = document.createElement("small");

        // Bilgi satırının temel hâli: kategori • eklenme tarihi.
        // let = sonradan DEĞİŞTİRİLEBİLEN değişken (const değiştirilemez).
        // Metni aşağıda duruma göre uzatacağımız için let kullandık.
        let bilgiYazisi = tarihYaz(g.olusturmaTarihi);

        // Görev tamamlandıysa ve tamamlanma tarihi doluysa sona ek bilgi ekle.
        // && = "ve": iki şart da doğru olmalı.
        // (Tamamlanma tarihi boş olabilen bir alan, null ise tarihYaz hata verirdi.
        //  O yüzden önce dolu mu diye bakıyoruz.)
        if (g.tamamlandi && g.tamamlanmaTarihi) {
            // Puan pozitifse başına + koyuyoruz (örn. +90), negatifse zaten - ile gelir (örn. -50).
            // şart ? A : B  = "şart doğruysa A, değilse B" (kısa if-else).
            // DEĞİŞTİRME İSTEĞİ: "puan" kelimesini veya ✓ işaretini istediğin gibi değiştir.
            const isaret = g.puan > 0 ? "+" : "";
            bilgiYazisi += ` • ✓ ${tarihYaz(g.tamamlanmaTarihi)} • ${isaret}${g.puan} puan`;
        }

        // Kategori rozeti. Neden <span> değil <b>? style.css'te "li span" ve
        // "li.bitti span" kuralları var. <span> kullansaydık tamamlanan görevde
        // rozet de gri ve çizili olurdu. <b> bu kurallara girmiyor.
        const rozet = document.createElement("b");
        rozet.textContent = g.kategori;

        // className = elemanın class listesini tek seferde yazar.
        // Sonuç örneği: "rozet rozet-ders". İlki ortak görünüm, ikincisi kategorinin rengi.
        // kategoriSiniflari[g.kategori] = eşleştirme listesinden sınıf adını bulur.
        // || "diger" = listede olmayan bir kategori gelirse "diger" (gri) kullan.
        rozet.className = "rozet rozet-" + (kategoriSiniflari[g.kategori] || "diger");

        // append = önce rozeti, sonra yanına bilgi yazısını ekler.
        // Başındaki boşluk, rozetle yazı arasına küçük bir boşluk koyar.
        bilgi.append(rozet, " " + bilgiYazisi);

        yazi.appendChild(bilgi);

        // Sil düğmesi
        const silBtn = document.createElement("button");
        silBtn.textContent = "Sil";   // DEĞİŞTİRME İSTEĞİ: Düğme yazısını buradan değiştir
        silBtn.className = "sil";
        silBtn.addEventListener("click", () => gorevSil(g.id));

        li.append(onay, yazi, silBtn);
        ul.appendChild(li);
    }

    // Sayaç: tamamlananlar / toplam
    const biten = gorevler.filter(g => g.tamamlandi).length;
    sayac.textContent = `${biten} / ${gorevler.length} görev tamamlandı`;
    animasyonOynasin = false;
    yeniGorevId = null;
}

async function gorevEkle() {
    const baslik = kutu.value.trim();   // Baştaki/sondaki boşlukları siler
    if (baslik === "") {                // Boşsa ekleme
        kutu.focus();
        return;
    }
    // Seçilen kategoriyi oku. Kullanıcı "Kategori seç" seçeneğinde bıraktıysa
    // değer boş yazıdır (""). Bu durumda sunucuya hiç gitmeden uyarı veriyoruz.
    // DEĞİŞTİRME İSTEĞİ: Uyarı yazısını değiştirebilirsin.
    // alert geçici çözüm, tasarım aşamasında daha şık bir mesaja çevireceğiz.
    const kategori = kategoriKutu.value;
    if (kategori === "") {
        alert("Lütfen bir kategori seç.");
        kategoriKutu.focus();
        return;
    }

    // Sahip bilgisini (KullaniciId) göndermiyoruz, sunucu kendisi dolduruyor.
    const cevap = await fetch("/api/gorevler", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ baslik: baslik, kategori: kategori, tamamlandi: false })
    });
    if (cevap.ok) {
        const eklenen = await cevap.json();
        yeniGorevId = eklenen.id;
    }
    kutu.value = "";
    kutu.focus();
    gorevleriGetir();   // Listeyi yeniden çek
}

async function durumDegistir(gorev, yeniDurum) {
    await fetch(`/api/gorevler/${gorev.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            id: gorev.id,
            baslik: gorev.baslik,
            tamamlandi: yeniDurum
        })
    });
    gorevleriGetir();
    // Görev tamamlandı ya da geri alındı: puan ve tamamlanan sayısı değişti,
    // kartı yeniden çek.
    profilGetir();
}

async function gorevSil(id) {
    // Onay mesajı
    if (!confirm("Bu görevi silmek istediğine emin misin?")) {
        return;
    }
    await fetch(`/api/gorevler/${id}`, { method: "DELETE" });
    gorevleriGetir();
    // Tamamlanmış bir görev silinince toplam puan ve sayı değişir,
    // kartı yeniden çek.
    profilGetir();
}

// =====================================================
// BÖLÜM 5: DÜĞMELERİ VE TUŞLARI BAĞLAMA
// "Şu düğmeye tıklanınca şu fonksiyon çalışsın."
// Fonksiyon adının sonuna parantez koyulmaz: şimdi çalıştırmıyoruz, tanıtıyoruz.
// =====================================================

girisBtn.addEventListener("click", girisYap);
kayitBtn.addEventListener("click", kayitOl);
cikisBtn.addEventListener("click", cikisYap);
ekleBtn.addEventListener("click", gorevEkle);

// Enter tuşu: görev kutusundayken görev ekler
kutu.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
        gorevEkle();
    }
});

// Enter tuşu: şifre kutusundayken giriş yapar
sifreKutu.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
        girisYap();
    }
});

// =====================================================
// BAŞLANGIÇ: Sayfa açılınca ilk çalışan satır
// =====================================================
oturumKontrol();