using System.Collections.ObjectModel;
using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.HR.ViewModels;

public partial class MasterDataViewModel : ObservableObject
{
    private readonly SalesRepository _salesRepo;
    private readonly TokoRepository _tokoRepo;

    public MasterDataViewModel(SalesRepository salesRepo, TokoRepository tokoRepo)
    {
        _salesRepo = salesRepo;
        _tokoRepo = tokoRepo;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTabSales))]
    [NotifyPropertyChangedFor(nameof(IsTabToko))]
    private int _tabAktif;

    public bool IsTabSales => TabAktif == 0;
    public bool IsTabToko => TabAktif == 1;

    public ObservableCollection<Sales> DaftarSales { get; } = new();
    public ObservableCollection<Toko> DaftarToko { get; } = new();

    [ObservableProperty]
    private string _jumlahSales = "0";

    [ObservableProperty]
    private string _jumlahToko = "0";

    public async Task MuatAsync()
    {
        DaftarSales.Clear();
        foreach (var s in await _salesRepo.GetAllAsync())
            DaftarSales.Add(s);

        DaftarToko.Clear();
        foreach (var t in await _tokoRepo.GetAllAsync())
            DaftarToko.Add(t);

        JumlahSales = DaftarSales.Count.ToString();
        JumlahToko = DaftarToko.Count.ToString();
    }

    [RelayCommand]
    private void TampilkanSales() => TabAktif = 0;

    [RelayCommand]
    private void TampilkanToko() => TabAktif = 1;

    [RelayCommand]
    private async Task TambahSalesAsync() =>
        await Shell.Current.DisplayAlert("Tambah sales",
            "Form tambah data sales (demo - integrasi backend menyusul).", "OK");

    [RelayCommand]
    private async Task TambahTokoAsync() =>
        await Shell.Current.DisplayAlert("Tambah toko",
            "Form tambah data toko beserta koordinat GPS radius (demo).", "OK");
}
