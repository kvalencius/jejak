using AbsensiSales.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.HR.ViewModels;

public partial class HrLoginViewModel : ObservableObject
{
    private readonly SessionService _session;

    public HrLoginViewModel(SessionService session) => _session = session;

    [ObservableProperty]
    private string _username = "admin";

    [ObservableProperty]
    private string _password = "admin123";

    [ObservableProperty]
    private string _pesanError = string.Empty;

    [RelayCommand]
    private async Task MasukAsync()
    {
        // Mockup: akun HR disederhanakan (single admin).
        // Backend nanti diganti autentikasi sesungguhnya.
        if (Username.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase)
            && Password == "admin123")
        {
            _session.HrLogin = true;
            await Shell.Current.GoToAsync("workspace");
            return;
        }

        PesanError = "Username atau password HR salah. Gunakan admin / admin123 untuk demo.";
    }

    [RelayCommand]
    private async Task BukaRegisterAsync() => await Shell.Current.GoToAsync("register-hr");
}
