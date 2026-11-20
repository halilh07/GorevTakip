namespace GorevTakip.Models
{
    public class Gorev
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = "";
        public bool Tamamlandi { get; set; }
        public string KullaniciId { get; set; } = "";
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
        public DateTime? TamamlanmaTarihi { get; set; }
        public string Kategori { get; set; } = "";
        public int Puan { get; set; }
        public bool Silindi { get; set; }
    }
}
