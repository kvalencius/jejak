using AbsensiSales.Core.Repositories;
using AbsensiSales.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly SalesRepository _salesRepo;
    private readonly SessionService _session;

    public LoginViewModel(SalesRepository salesRepo, SessionService session)
    {
        _salesRepo = salesRepo;
        _session = session;
    }

    [ObservableProperty]
    private string _username = "andi.pratama";

    [ObservableProperty]
    private string _password = "demo123";

    [ObservableProperty]
    private string _pesanError = string.Empty;

    [ObservableProperty]
    private bool _sedangMemproses;

    public string HariIni => DateTime.Now.ToString("dddd, dd MMMM yyyy");

    [RelayCommand]
    private async Task MasukAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            PesanError = "Username dan password wajib diisi.";
            return;
        }

        SedangMemproses = true;
        PesanError = string.Empty;

        var sales = await _salesRepo.LoginAsync(Username.Trim(), Password);

        SedangMemproses = false;

        if (sales is null)
        {
            PesanError = "Username atau password salah.";
            return;
        }

        _session.SalesAktif = sales;
        await Shell.Current.GoToAsync("//utama/absensi");
    }

    [RelayCommand]
    private async Task BukaRegisterAsync() =>
        await Shell.Current.GoToAsync("register");
}
