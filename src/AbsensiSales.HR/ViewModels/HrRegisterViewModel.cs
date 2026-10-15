using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.HR.ViewModels;

public partial class HrRegisterViewModel : ObservableObject
{
    [ObservableProperty]
    private string _nama = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [RelayCommand]
    private async Task DaftarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nama) || string.IsNullOrWhiteSpace(Username)
            || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Gagal", "Semua kolom wajib diisi.", "OK");
            return;
        }

        await Shell.Current.DisplayAlert("Berhasil",
            "Akun HR terdaftar (demo). Silakan masuk dengan admin / admin123.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task KembaliAsync() => await Shell.Current.GoToAsync("..");
}
