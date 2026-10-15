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
    }

    [RelayCommand]
    private async Task SetujuiAsync(PermohonanItem? item)
    {
        if (item is null)
            return;

        var semua = await _permohonanRepo.GetAllAsync();
        var asli = semua.FirstOrDefault(p => p.Id == item.Id);
        if (asli is null)
            return;

        asli.Status = nameof(StatusPermohonan.Disetujui);
        asli.CatatanHr = "Disetujui oleh HR.";
        await _permohonanRepo.SaveAsync(asli);

        await Shell.Current.DisplayAlert("Disetujui",
            $"Permohonan {item.Jenis} dari {item.NamaSales} disetujui.", "OK");
        await MuatAsync();
    }

    [RelayCommand]
    private async Task TolakAsync(PermohonanItem? item)
    {
        if (item is null)
            return;

        var semua = await _permohonanRepo.GetAllAsync();
        var asli = semua.FirstOrDefault(p => p.Id == item.Id);
        if (asli is null)
            return;

        asli.Status = nameof(StatusPermohonan.Ditolak);
        asli.CatatanHr = "Ditolak oleh HR.";
        await _permohonanRepo.SaveAsync(asli);

        await Shell.Current.DisplayAlert("Ditolak",
            $"Permohonan {item.Jenis} dari {item.NamaSales} ditolak.", "OK");
        await MuatAsync();
    }
}
