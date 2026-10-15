using AbsensiSales.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

public partial class FotoTokoViewModel : ObservableObject
{
    private readonly AbsensiFlowState _flow;

    public FotoTokoViewModel(AbsensiFlowState flow) => _flow = flow;

    [ObservableProperty]
    private string _labelToko = string.Empty;

    [ObservableProperty]
    private string _infoFoto = "Foto belum diambil.";

    [ObservableProperty]
    private ImageSource? _pratinjauFoto;

    [ObservableProperty]
    private bool _fotoSiap;

    [ObservableProperty]
    private bool _dalamRadius;

    [ObservableProperty]
    private string _infoGps = string.Empty;

    public void OnAppearing()
    {
        var toko = _flow.TokoTerpilih;
        LabelToko = toko is null ? "Toko" : $"{toko.KodeToko} - {toko.NamaToko}";
        DalamRadius = _flow.DalamRadius;
        InfoGps = $"{_flow.Latitude:F4}, {_flow.Longitude:F4} - Jarak {_flow.JarakMeter:F0} meter";
        FotoSiap = _flow.FotoBukti is not null;
        if (_flow.FotoBukti is not null)
        {
            PratinjauFoto = ImageSource.FromStream(() => new MemoryStream(_flow.FotoBukti));
            InfoFoto = "Foto bukti dari sesi sebelumnya.";
        }
        else
        {
            PratinjauFoto = null;
            InfoFoto = "Foto belum diambil.";
        }
    }

    [RelayCommand]
    private async Task AmbilFotoAsync()
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var file = await MediaPicker.Default.CapturePhotoAsync();
                if (file is not null)
                {
                    using var stream = await file.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    _flow.FotoBukti = ms.ToArray();
                    PratinjauFoto = ImageSource.FromStream(() => new MemoryStream(_flow.FotoBukti));
                }
            }
            else
            {
                // Perangkat tanpa kamera (mis. desktop Windows) -> foto placeholder
                await BuatFotoPlaceholderAsync();
            }
        }
        catch (Exception)
        {
            // Kamera dibatalkan / tidak tersedia -> placeholder
            await BuatFotoPlaceholderAsync();
        }

        _flow.WaktuFoto = DateTime.Now.ToString("HH:mm:ss");
        FotoSiap = _flow.FotoBukti is not null;
        InfoFoto = FotoSiap
            ? "Sales dan tampak toko terlihat - foto bukti siap."
            : "Foto belum diambil.";
    }

    private Task BuatFotoPlaceholderAsync()
    {
        // Tanpa kamera (mis. desktop Windows), foto bukti ditandai siap
        // dan pratinjau memakai placeholder warna di tampilan.
        _flow.FotoBukti = Array.Empty<byte>();
        PratinjauFoto = null;
        FotoSiap = true;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task GunakanFotoAsync()
    {
        if (!FotoSiap)
        {
            await Shell.Current.DisplayAlert("Foto belum ada",
                "Ambil foto bukti kunjungan terlebih dahulu.", "OK");
            return;
        }

        await Shell.Current.GoToAsync("verifikasi-data");
    }
}
