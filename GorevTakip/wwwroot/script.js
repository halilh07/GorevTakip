const kutu = document.getElementById("baslik");
const ekleBtn = document.getElementById("ekleBtn");
const ul = document.getElementById("liste");
const sayac = document.getElementById("sayac");

async function gorevleriGetir() {
    const cevap = await fetch("/api/gorevler");
    const gorevler = await cevap.json();

    ul.innerHTML = "";

    for (const g of gorevler) {
        const li = document.createElement("li");
        if (g.tamamlandi) {
            li.classList.add("bitti");
        }

        const onay = document.createElement("input");
        onay.type = "checkbox";
        onay.checked = g.tamamlandi;
        onay.addEventListener("change", () => durumDegistir(g, onay.checked));

        const yazi = document.createElement("span");
        yazi.textContent = g.baslik;

        const silBtn = document.createElement("button");
        silBtn.textContent = "Sil";
        silBtn.className = "sil";
        silBtn.addEventListener("click", () => gorevSil(g.id));

        li.append(onay, yazi, silBtn);
        ul.appendChild(li);
    }

    const biten = gorevler.filter(g => g.tamamlandi).length;
    sayac.textContent = `${biten} / ${gorevler.length} görev tamamlandı`;
}

async function gorevEkle() {
    const baslik = kutu.value.trim();
    if (baslik === "") {
        kutu.focus();
        return;
    }

    await fetch("/api/gorevler", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ baslik: baslik, tamamlandi: false })
    });

    kutu.value = "";
    kutu.focus();
    gorevleriGetir();
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
}

async function gorevSil(id) {
    await fetch(`/api/gorevler/${id}`, { method: "DELETE" });
    gorevleriGetir();
}

ekleBtn.addEventListener("click", gorevEkle);
kutu.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
        gorevEkle();
    }
});

gorevleriGetir();