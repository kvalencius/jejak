using AbsensiSales.Core.Services;
using AbsensiSales.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

public partial class VerifikasiWajahViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly FaceService _face;
    private readonly AbsensiFlowState _flow;

    public VerifikasiWajahViewModel(SessionService session, FaceService face, AbsensiFlowState flow)
    {
        _session = session;
        _face = face;
        _flow = flow;
    }

    [ObservableProperty]
    private string _namaToko = string.Empty;

    [ObservableProperty]
    private string _identitasTerdeteksi = string.Empty;

    [ObservableProperty]
    private string _waktuVerifikasi = string.Empty;

    [ObservableProperty]
    private string _ringkasanSkor = string.Empty;

    [ObservableProperty]
    private bool _hasilCocok;

    [ObservableProperty]
    private bool _hasilDiproses;

    [ObservableProperty]
    private bool _sudahDiperiksa;

    [ObservableProperty]
    private string _pesanHasil = string.Empty;

    public void OnAppearing()
    {
        NamaToko = _flow.TokoTerpilih?.NamaToko ?? "Toko";
        SudahDiperiksa = false;
        HasilDiproses = false;
        HasilCocok = false;
    }

    [RelayCommand]
    private async Task PindaiWajahAsync()
    {
        var sales = _session.SalesAktif;
        if (sales is null)
            return;

        HasilDiproses = true;
        SudahDiperiksa = false;

        // Beri jeda singkat agar UI "kamera" terlihat sedang memproses
        await Task.Delay(1200);

        var hasil = _face.Verifikasi(sales.Nama);

        HasilDiproses = false;
        SudahDiperiksa = true;
        HasilCocok = hasil.Cocok;

        IdentitasTerdeteksi = hasil.Cocok ? $"{sales.Nama} - {sales.Nip}" : "Tidak dikenal";
        WaktuVerifikasi = DateTime.Now.ToString("dd MMM yyyy - HH.mm.ss 'WIB'");
        RingkasanSkor = $"Kecocokan {(hasil.Skor * 100):F0}%"
            + (hasil.Simulasi ? " - mode simulasi" : " - kamera nyata");
        PesanHasil = hasil.Pesan;

        if (hasil.Cocok)
        {
            _flow.SkorWajah = hasil.Skor;
            _flow.WaktuVerifikasiWajah = WaktuVerifikasi;
        }
    }

    [RelayCommand]
    private async Task LanjutFotoAsync()
    {
        if (!HasilCocok)
        {
            await Shell.Current.DisplayAlert("Verifikasi gagal",
                "Wajah belum cocok. Ulangi pemindaian sebelum melanjutkan.", "OK");
            return;
        }

        await Shell.Current.GoToAsync("foto-toko");
    }
}
