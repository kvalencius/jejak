using AbsensiSales.HR.Services;

namespace AbsensiSales.HR;

public partial class App : Application
{
    public App(AppShell shell)
    {
        InitializeComponent();
        MainPage = shell;

        // Seluruh isi halaman diskalakan 80% begitu tampil.
        PageAppearing += (_, page) =>
        {
            Diagnosa.Catat("SEBELUM", page);
            UiScale.Terapkan(page);
            Diagnosa.Catat("SESUDAH", page);
        };
    }
}
