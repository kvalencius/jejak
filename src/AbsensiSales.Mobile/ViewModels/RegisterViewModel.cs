using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly SalesRepository _salesRepo;

    public RegisterViewModel(SalesRepository salesRepo) => _salesRepo = salesRepo;

    [ObservableProperty]
    private string _nama = string.Empty;

    [ObservableProperty]
    private string _nip = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _pesanError = string.Empty;

    [RelayCommand]
    private async Task DaftarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nama) || string.IsNullOrWhiteSpace(Nip)
            || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            PesanError = "Semua kolom wajib diisi.";
            return;
        }

        if (await _salesRepo.UsernameExistsAsync(Username.Trim()))
        {
            PesanError = "Username sudah digunakan.";
            return;
        }

        await _salesRepo.RegisterAsync(new Sales
        {
            Nama = Nama.Trim(),
            Nip = Nip.Trim(),
            Username = Username.Trim(),
            Password = Password,
            Wilayah = "Surabaya Pusat"
        });

        await Shell.Current.DisplayAlert("Berhasil",
            "Akun sales terdaftar. Silakan masuk.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task KembaliAsync() => await Shell.Current.GoToAsync("..");
}
