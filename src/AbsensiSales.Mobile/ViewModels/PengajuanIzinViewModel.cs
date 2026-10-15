using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using AbsensiSales.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

public partial class PengajuanIzinViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly PermohonanRepository _permohonanRepo;

    public PengajuanIzinViewModel(SessionService session, PermohonanRepository permohonanRepo)
    {
        _session = session;
        _permohonanRepo = permohonanRepo;
    }

    public string[] JenisPermohonan { get; } = { "Cuti", "Izin Sakit" };

    [ObservableProperty]
    private string _jenis = "Cuti";

    [ObservableProperty]
    private DateTime _tanggalMulai = DateTime.Today;

    [ObservableProperty]
    private DateTime _tanggalSelesai = DateTime.Today;

    [ObservableProperty]
    private string _alasan = string.Empty;

    [ObservableProperty]
    private string _lampiran = string.Empty;

    [ObservableProperty]
    private string _pesanError = string.Empty;

    [RelayCommand]
    private void PilihCuti() => Jenis = "Cuti";

    [RelayCommand]
    private void PilihSakit() => Jenis = "Izin Sakit";

    [RelayCommand]
    private async Task KirimAsync()
    {
        var sales = _session.SalesAktif;
        if (sales is null)
            return;

        if (TanggalSelesai < TanggalMulai)
        {
            PesanError = "Tanggal selesai tidak boleh sebelum tanggal mulai.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Alasan))
        {
            PesanError = "Alasan wajib diisi.";
            return;
        }

        await _permohonanRepo.SaveAsync(new Permohonan
        {
            SalesId = sales.Id,
            Jenis = Jenis,
            TanggalMulai = TanggalMulai.ToString("yyyy-MM-dd"),
            TanggalSelesai = TanggalSelesai.ToString("yyyy-MM-dd"),
            Alasan = Alasan.Trim(),
            Lampiran = Lampiran.Trim(),
            DiajukanPada = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            Status = nameof(StatusPermohonan.Pending)
        });

        await Shell.Current.DisplayAlert("Terkirim",
            "Permohonan Anda sudah dikirim dan menunggu persetujuan HR.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task BatalAsync() => await Shell.Current.GoToAsync("..");
}
