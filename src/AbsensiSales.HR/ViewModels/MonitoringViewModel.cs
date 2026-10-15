using System.Collections.ObjectModel;
using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.HR.ViewModels;

public partial class MonitoringViewModel : ObservableObject
{
    private readonly SalesRepository _salesRepo;
    private readonly KunjunganRepository _kunjunganRepo;
    private readonly Services.HrNavigationService _nav;

    public MonitoringViewModel(
        SalesRepository salesRepo,
        KunjunganRepository kunjunganRepo,
        Services.HrNavigationService nav)
    {
        _salesRepo = salesRepo;
        _kunjunganRepo = kunjunganRepo;
        _nav = nav;
    }

    public ObservableCollection<SalesMonitorItem> DaftarSales { get; } = new();

    [ObservableProperty]
    private string _totalSales = "0";

    [ObservableProperty]
    private string _jumlahHadir = "0";

    [ObservableProperty]
    private string _persenHadir = "0% dari total";

    [ObservableProperty]
    private string _jumlahBelumAbsen = "0";

    [ObservableProperty]
    private string _kunjunganTercatat = "0";

    [ObservableProperty]
    private string _updatePada = string.Empty;

    [ObservableProperty]
    private string _ringkasanTindakLanjut = string.Empty;

    public async Task MuatAsync()
    {
        var tanggal = DateTime.Now.ToString("yyyy-MM-dd");
        var semuaSales = await _salesRepo.GetAllAsync();
        var semuaKunjungan = await _kunjunganRepo.GetAllAsync(tanggal);

        int hadir = 0;
        DaftarSales.Clear();

        foreach (var s in semuaSales)
        {
            var kunjunganSales = semuaKunjungan
                .Where(k => k.SalesId == s.Id && k.Status == nameof(StatusKunjungan.Tersimpan))
                .ToList();

            bool hadirHariIni = kunjunganSales.Count > 0
                || semuaKunjungan.Any(k => k.SalesId == s.Id);

            if (kunjunganSales.Count > 0)
                hadir++;

            var terakhir = kunjunganSales.OrderByDescending(k => k.WaktuCatat).FirstOrDefault();

            DaftarSales.Add(new SalesMonitorItem
            {
                Inisial = s.Inisial,
                Nama = s.Nama,
                Nip = s.Nip,
                Wilayah = s.Wilayah,
                Status = kunjunganSales.Count > 0 ? "Hadir" : "Belum absen",
                Waktu = terakhir?.WaktuCatat ?? "-",
                SalesId = s.Id
            });
        }

        TotalSales = semuaSales.Count.ToString();
        JumlahHadir = hadir.ToString();
        PersenHadir = $"{(semuaSales.Count > 0 ? hadir * 100 / semuaSales.Count : 0)}% dari total";
        JumlahBelumAbsen = (semuaSales.Count - hadir).ToString();
        KunjunganTercatat = semuaKunjungan.Count.ToString();
        UpdatePada = DateTime.Now.ToString("HH.mm") + " WIB";

        int gagal = semuaKunjungan.Count(k => k.Status == nameof(StatusKunjungan.GagalValidasi));
        int draf = semuaKunjungan.Count(k => k.Status == nameof(StatusKunjungan.Draf));
        RingkasanTindakLanjut =
            $"Pantau {semuaSales.Count - hadir} sales belum absen, "
            + $"{semuaKunjungan.Count(k => k.Status == nameof(StatusKunjungan.Tersimpan))} kunjungan tersimpan"
            + (gagal + draf > 0 ? $", {gagal + draf} perlu tindak lanjut." : ".");
    }

    [RelayCommand]
    private void BukaLaporan() =>
        _nav.NavigateTo(typeof(Views.LaporanHrPage));

    [RelayCommand]
    private async Task KirimPengingatAsync() =>
        await Shell.Current.DisplayAlert("Pengingat terkirim",
            "Notifikasi pengingat dikirim ke sales yang belum absen (demo).", "OK");
}
