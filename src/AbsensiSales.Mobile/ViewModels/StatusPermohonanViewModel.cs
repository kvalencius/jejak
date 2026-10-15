using System.Collections.ObjectModel;
using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using AbsensiSales.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

public partial class StatusPermohonanViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly PermohonanRepository _permohonanRepo;

    public StatusPermohonanViewModel(SessionService session, PermohonanRepository permohonanRepo)
    {
        _session = session;
        _permohonanRepo = permohonanRepo;
    }

    public ObservableCollection<PermohonanItemViewModel> DaftarPermohonan { get; } = new();

    [ObservableProperty]
    private string _infoSales = string.Empty;

    [ObservableProperty]
    private bool _kosong;

    public async void OnAppearing()
    {
        var sales = _session.SalesAktif;
        if (sales is null)
            return;

        InfoSales = $"{sales.Nama} - {sales.Nip}";
        var semua = await _permohonanRepo.GetBySalesAsync(sales.Id);

        DaftarPermohonan.Clear();
        foreach (var p in semua)
        {
            DaftarPermohonan.Add(new PermohonanItemViewModel
            {
                Jenis = p.Jenis,
                Periode = p.TanggalMulai == p.TanggalSelesai
                    ? p.TanggalMulai
                    : $"{p.TanggalMulai} -> {p.TanggalSelesai}",
                Alasan = p.Alasan,
                Status = p.Status,
                Diajukan = p.DiajukanPada
            });
        }

        Kosong = DaftarPermohonan.Count == 0;
    }

    [RelayCommand]
    private async Task AjukanBaruAsync() => await Shell.Current.GoToAsync("pengajuan-izin");
}
