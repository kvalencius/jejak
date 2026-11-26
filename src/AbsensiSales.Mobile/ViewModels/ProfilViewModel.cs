using AbsensiSales.Core.Api;
using AbsensiSales.Core.Services;
using AbsensiSales.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

public partial class ProfilViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly ApiService _api;

    public ProfilViewModel(SessionService session, ApiService api)
    {
        _session = session;
        _api = api;
    }

    [ObservableProperty]
    private string _namaSales = string.Empty;

    [ObservableProperty]
    private string _inisial = "?";

    [ObservableProperty]
    private string _nip = "-";

    [ObservableProperty]
    private string _wilayah = "-";

    [ObservableProperty]
    private string _email = "-";

    [ObservableProperty]
    private string _peran = "Sales";

    [ObservableProperty]
    private string _statusWajah = "Memeriksa...";

    public async void OnAppearing()
    {
        var sales = _session.SalesAktif;
        if (sales is null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        NamaSales = sales.Nama;
        Inisial = sales.Inisial;
        Nip = ApiMapper.AtauDash(sales.Nip);
        Wilayah = ApiMapper.AtauDash(sales.Wilayah);
        Email = ApiMapper.AtauDash(_session.EmailPengguna);
        Peran = ApiMapper.AtauDash(_session.RolePengguna);

        await MuatStatusWajahAsync();
    }

    private async Task MuatStatusWajahAsync()
    {
        var hasil = await _api.GetStatusWajahAsync();
        if (!hasil.Sukses || hasil.Data is null)
        {
            StatusWajah = "Gagal dimuat";
            return;
        }

        StatusWajah = hasil.Data.Terdaftar ? "Terdaftar" : "Belum terdaftar";
    }

    [RelayCommand]
    private async Task KelolaWajahAsync() => await Shell.Current.GoToAsync("daftar-wajah");

    [RelayCommand]
    private async Task KeluarAsync()
    {
        try
        {
            await _api.LogoutAsync();
        }
        catch
        {
            // Token lokal tetap dibersihkan walau server tidak dapat dihubungi.
            _api.LogoutLokal();
        }

        _session.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}
