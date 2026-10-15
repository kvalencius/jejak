using AbsensiSales.Core.Models;

namespace AbsensiSales.Core.Services;

/// <summary>
/// Menyimpan sesi pengguna yang sedang login (sales maupun HR).
/// Pada mockup, status login hanya berlaku selama aplikasi berjalan.
/// </summary>
public class SessionService
{
    public Sales? SalesAktif { get; set; }
    public bool HrLogin { get; set; }

    public string TanggalHariIni => DateTime.Now.ToString("yyyy-MM-dd");

    public void Logout()
    {
        SalesAktif = null;
        HrLogin = false;
    }
}
