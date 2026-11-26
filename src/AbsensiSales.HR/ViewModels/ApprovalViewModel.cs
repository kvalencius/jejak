using System.Collections.ObjectModel;
using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.HR.ViewModels;

public partial class ApprovalViewModel : ObservableObject
{
    private readonly PermohonanRepository _permohonanRepo;
    private readonly SalesRepository _salesRepo;

    public ApprovalViewModel(PermohonanRepository permohonanRepo, SalesRepository salesRepo)
    {
        _permohonanRepo = permohonanRepo;
        _salesRepo = salesRepo;
    }

    public ObservableCollection<PermohonanItem> DaftarPermohonan { get; } = new();

    [ObservableProperty]
    private string _jumlahPending = "0";

    [ObservableProperty]
    private string _jumlahSelesai = "0";

    [ObservableProperty]
    private bool _tidakAdaPermohonan = true;

    public async Task MuatAsync()
    {
        var semua = await _permohonanRepo.GetAllAsync();
        DaftarPermohonan.Clear();

        foreach (var p in semua)
        {
            var sales = await _salesRepo.GetByIdAsync(p.SalesId);
            DaftarPermohonan.Add(new PermohonanItem
            {
                Id = p.Id,
                NamaSales = sales?.Nama ?? "-",
                Nip = sales?.Nip ?? "-",
                Jenis = p.Jenis,
                Periode = p.TanggalMulai == p.TanggalSelesai
                    ? p.TanggalMulai
                    : $"{p.TanggalMulai} -> {p.TanggalSelesai}",
                Alasan = p.Alasan,
                Lampiran = string.IsNullOrWhiteSpace(p.Lampiran) ? "Tanpa lampiran" : p.Lampiran,
                Diajukan = p.DiajukanPada,
                Status = p.Status,
                CatatanHr = p.CatatanHr
            });
        }

        JumlahPending = semua.Count(p => p.Status == nameof(StatusPermohonan.Pending)).ToString();
        JumlahSelesai = (semua.Count - int.Parse(JumlahPending)).ToString();
        TidakAdaPermohonan = DaftarPermohonan.Count == 0;
    }

    [RelayCommand]
    private async Task SetujuiAsync(PermohonanItem? item)
    {
        if (item is null || !item.Pending)
            return;

        var asli = await CariAsync(item.Id);
        if (asli is null)
            return;

        asli.Status = nameof(StatusPermohonan.Disetujui);
        asli.CatatanHr = "Disetujui oleh HR.";
        await _permohonanRepo.SaveAsync(asli);

        await Shell.Current.DisplayAlertAsync(
            "Permohonan disetujui",
            $"Permohonan {item.Jenis} dari {item.NamaSales} disetujui.",
            "OK");
        await MuatAsync();
    }

    [RelayCommand]
    private async Task TolakAsync(PermohonanItem? item)
    {
        if (item is null || !item.Pending)
            return;

        var alasan = await Shell.Current.DisplayPromptAsync(
            "Tolak permohonan",
            $"Masukkan alasan penolakan untuk {item.NamaSales}:",
            accept: "Tolak",
            cancel: "Batal",
            placeholder: "Contoh: jadwal kunjungan padat dan belum ada pengganti.",
            maxLength: 250,
            keyboard: Keyboard.Text);

        if (alasan is null)
            return;

        if (string.IsNullOrWhiteSpace(alasan))
        {
            await Shell.Current.DisplayAlertAsync(
                "Alasan wajib diisi",
                "Alasan penolakan tidak boleh kosong.",
                "OK");
            return;
        }

        var asli = await CariAsync(item.Id);
        if (asli is null)
            return;

        asli.Status = nameof(StatusPermohonan.Ditolak);
        asli.CatatanHr = alasan.Trim();
        await _permohonanRepo.SaveAsync(asli);

        await Shell.Current.DisplayAlertAsync(
            "Permohonan ditolak",
            $"Permohonan {item.Jenis} dari {item.NamaSales} ditolak.",
            "OK");
        await MuatAsync();
    }

    private async Task<Permohonan?> CariAsync(int id)
    {
        if (id <= 0)
            return null;

        var semua = await _permohonanRepo.GetAllAsync();
        return semua.FirstOrDefault(p => p.Id == id);
    }
}
