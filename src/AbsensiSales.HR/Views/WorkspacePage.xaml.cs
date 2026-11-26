using AbsensiSales.Core.Services;
using AbsensiSales.HR.Controls;
using AbsensiSales.HR.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AbsensiSales.HR.Views;

public partial class WorkspacePage : ContentPage
{
    private readonly HrNavigationService _nav;
    private readonly SessionService _session;
    private readonly IServiceProvider _services;

    private bool? _tataLetakSempit;

    public WorkspacePage(HrNavigationService nav, SessionService session, IServiceProvider services)
    {
        InitializeComponent();
        _nav = nav;
        _session = session;
        _services = services;

        _nav.Navigated += OnNavigated;
    }

    /// <summary>
    /// Di layar sempit (HP) sidebar diganti navigasi ringkas di atas konten,
    /// sehingga halaman tetap memakai seluruh lebar layar.
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0)
            return;

        bool sempit = width < 900;

        if (_tataLetakSempit == sempit)
            return;

        _tataLetakSempit = sempit;

        Sidebar.IsVisible = !sempit;
        NavRingkas.IsVisible = sempit;
        TopbarInfo.IsVisible = !sempit;

        RootGrid.ColumnDefinitions[0].Width = sempit ? new GridLength(0) : new GridLength(280);
        RootGrid.ColumnSpacing = sempit ? 0 : 14;
        RootGrid.Padding = new Thickness(sempit ? 8 : 14);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_session.HrLogin)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        if (ContentFrame.Content is null)
            BukaDashboard();
    }

    private void OnNavigated(Type pageType, object? parameter)
    {
        var page = (ContentPage)_services.GetRequiredService(pageType);

        if (parameter is int id && page.BindingContext is ViewModels.DetailKunjunganViewModel detailVm)
            detailVm.KunjunganId = id;

        // Content dipindah ke Frame, sehingga binding context harus dipasang
        // eksplisit di content-nya. Kalau tidak, BindingContext warisan dari
        // halaman asal ikut hilang (WorkspacePage sendiri tidak punya VM) dan
        // semua binding di dalamnya gagal (label kosong, IsVisible diabaikan).
        if (page.Content is not null)
            page.Content.BindingContext = page.BindingContext;

        // Frame hanya menerima View, jadi konten halaman yang dipasang.
        // Muat data dipanggil manual via MuatAsync di bawah.
        ContentFrame.Content = page.Content;
        MuatAsync(page.BindingContext);
    }

    private static void MuatAsync(object? bindingContext)
    {
        switch (bindingContext)
        {
            case ViewModels.HrDashboardViewModel dvm:
                _ = dvm.MuatAsync();
                break;
            case ViewModels.HrPerizinanViewModel pvm:
                _ = pvm.MuatAsync();
                break;
            case ViewModels.MonitoringViewModel mvm:
                _ = mvm.MuatAsync();
                break;
            case ViewModels.LaporanHrViewModel lvm:
                _ = lvm.MuatAsync();
                break;
            case ViewModels.DetailKunjunganViewModel dkvm:
                _ = dkvm.MuatAsync();
                break;
            case ViewModels.MasterDataViewModel vmaster:
                _ = vmaster.MuatAsync();
                break;
            case ViewModels.PayrollViewModel payvm:
                _ = payvm.MuatAsync();
                break;
        }
    }

    // ---- Menu sidebar ----

    private void BukaDashboard()
    {
        LabelBreadcrumb.Text = "Ringkasan / Dashboard HR";
        SorotMenu(ItemDashboard);
        OnNavigated(typeof(HrDashboardPage), null);
    }

    private void MenuDashboard_Tapped(object? sender, TappedEventArgs e) => BukaDashboard();

    private void BukaMonitoring()
    {
        LabelBreadcrumb.Text = "Operasional / Pemantauan absensi";
        SorotMenu(ItemAbsensi);
        OnNavigated(typeof(MonitoringPage), null);
    }

    private void MenuAbsensi_Tapped(object? sender, TappedEventArgs e) => BukaMonitoring();

    private void MenuLaporan_Tapped(object? sender, TappedEventArgs e)
    {
        LabelBreadcrumb.Text = "Operasional / Pemantauan laporan";
        SorotMenu(ItemLaporan);
        OnNavigated(typeof(LaporanHrPage), null);
    }

    private void MenuApproval_Tapped(object? sender, TappedEventArgs e)
    {
        LabelBreadcrumb.Text = "Kepegawaian / Perizinan HR & sales";
        SorotMenu(ItemApproval);
        OnNavigated(typeof(HrPerizinanPage), null);
    }

    private void MenuMaster_Tapped(object? sender, TappedEventArgs e)
    {
        LabelBreadcrumb.Text = "Kepegawaian / Data master";
        SorotMenu(ItemMaster);
        OnNavigated(typeof(MasterDataPage), null);
    }

    private void MenuPayroll_Tapped(object? sender, TappedEventArgs e)
    {
        LabelBreadcrumb.Text = "Kepegawaian / Perhitungan gaji";
        SorotMenu(ItemPayroll);
        OnNavigated(typeof(PayrollPage), null);
    }

    private async void Logout_Tapped(object? sender, TappedEventArgs e)
    {
        _session.Logout();
        _nav.Navigated -= OnNavigated;
        await Shell.Current.GoToAsync("//login");
    }

    /// <summary>Menandai item menu yang aktif dengan satu pill kuning (RoundRectangle 10).</summary>
    private void SorotMenu(FloatingCard aktif)
    {
        var warna = Application.Current!.Resources;

        Color latarAktif = (Color)warna["PrimaryBright"];
        Color glowAktif = (Color)warna["ShadowColor"];
        Color teksAktif = (Color)warna["OnPrimary"];
        Color ikonNormal = (Color)warna["TextSecondary"];
        Color teksNormal = (Color)warna["TextPrimary"];

        foreach (var item in new FloatingCard[] { ItemDashboard, ItemAbsensi, ItemLaporan, ItemApproval, ItemMaster, ItemPayroll })
        {
            bool dipilih = ReferenceEquals(item, aktif);

            item.BackgroundColor = dipilih ? latarAktif : Colors.Transparent;
            item.ShadowColor = glowAktif;
            item.ShadowOpacity = 0;

            if (item.Content is HorizontalStackLayout baris && baris.Children.Count >= 2)
            {
                if (baris.Children[0] is Label ikon)
                    ikon.TextColor = dipilih ? teksAktif : ikonNormal;
                if (baris.Children[1] is Label teks)
                    teks.TextColor = dipilih ? teksAktif : teksNormal;
            }
        }

        SorotRingkas(aktif);
    }

    /// <summary>Menandai tombol aktif pada navigasi ringkas layar HP.</summary>
    private void SorotRingkas(FloatingCard aktif)
    {
        var sumber = Application.Current!.Resources;

        Color latarAktif = (Color)sumber["PrimaryBright"];
        Color latarNormal = (Color)sumber["Surface"];
        Color garisNormal = (Color)sumber["BorderNeutral"];

        var peta = new Dictionary<FloatingCard, Border>
        {
            [ItemDashboard] = NavDashboard,
            [ItemAbsensi] = NavAbsensi,
            [ItemLaporan] = NavLaporan,
            [ItemApproval] = NavApproval,
            [ItemMaster] = NavMaster,
            [ItemPayroll] = NavPayroll
        };

        foreach (var (item, tombol) in peta)
        {
            bool dipilih = ReferenceEquals(item, aktif);
            tombol.BackgroundColor = dipilih ? latarAktif : latarNormal;
            tombol.Stroke = dipilih ? Colors.Transparent : garisNormal;

            if (tombol.Content is HorizontalStackLayout baris)
                foreach (var anak in baris.Children.OfType<Label>())
                    anak.TextColor = dipilih
                        ? (Color)sumber["OnPrimary"]
                        : (Color)sumber["TextPrimary"];
        }
    }
}
