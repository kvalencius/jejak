using System.Net.Mail;
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
            await Shell.Current.DisplayAlertAsync("Gagal", "Nama, username, dan password wajib diisi.", "OK");
            return;
        }

        if (!string.IsNullOrWhiteSpace(Email) && !MailAddress.TryCreate(Email.Trim(), out _))
        {
            await Shell.Current.DisplayAlertAsync("Gagal", "Format email tidak valid.", "OK");
            return;
        }

        if (Password.Trim().Length < 6)
        {
            await Shell.Current.DisplayAlertAsync("Gagal", "Password minimal 6 karakter.", "OK");
            return;
        }

        await Shell.Current.DisplayAlertAsync("Berhasil",
            "Akun HR terdaftar (demo). Silakan masuk dengan admin / admin123.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task KembaliAsync() => await Shell.Current.GoToAsync("..");
}
