namespace AbsensiSales.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("pengisian-toko", typeof(Views.PengisianTokoPage));
        Routing.RegisterRoute("verifikasi-wajah", typeof(Views.VerifikasiWajahPage));
        Routing.RegisterRoute("foto-toko", typeof(Views.FotoTokoPage));
        Routing.RegisterRoute("verifikasi-data", typeof(Views.VerifikasiDataPage));
        Routing.RegisterRoute("pengajuan-izin", typeof(Views.PengajuanIzinPage));
        Routing.RegisterRoute("status-permohonan", typeof(Views.StatusPermohonanPage));
        Routing.RegisterRoute("register", typeof(Views.RegisterPage));
    }
}
