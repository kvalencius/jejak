using Shiny.Controls.Keyframe;
using Shiny.Maui.Controls.Keyframe;

namespace AbsensiSales.Mobile.Controls;

/// <summary>
/// Navbar bawah aplikasi. Setiap halaman utama cukup memanggil elemen ini
/// dan menentukan tab aktifnya, misal <c>ActiveTab="absensi"</c>.
///
/// Penanda tab aktif adalah pil kuning tua yang menjulang sedikit ke atas
/// seperti tab pembatas buku. Saat tab diganti, pil meluncur ke kiri/kanan
/// mengikuti tab yang dipilih dan ikon tab tersebut memantul sekali.
/// Animasi memakai Shiny.Maui.Controls.Keyframe.
/// </summary>
public partial class BottomNavBar : ContentView
{
    public const string TabAbsensi = "absensi";
    public const string TabLaporan = "laporan";
    public const string TabProfil = "profil";

    private const double LebarPenandaMaks = 74;
    private const double LebarPenandaMin = 44;
    private const int JumlahTab = 3;

    private static readonly TimeSpan DurasiLuncur = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan DurasiPantul = TimeSpan.FromMilliseconds(440);

    private static readonly string[] RutePerTab =
    {
        "//utama/absensi",
        "//utama/laporan",
        "//utama/profil"
    };

    /// <summary>
    /// Tab yang sedang benar-benar tampil. Diperbarui dari Appearing halaman,
    /// jadi tetap akurat walau halaman dibuka lewat tombol back.
    /// </summary>
    private static int _tabAktif;

    /// <summary>
    /// Tab asal luncuran pil untuk navigasi terakhir. Hanya ditulis oleh pompa
    /// navigasi, bukan oleh Appearing, supaya Appearing telat dari halaman
    /// perantara tidak bisa membuat pil berhenti di tempat acak.
    /// </summary>
    private static int _asalLuncur = -1;

    // Shell tidak suka dua GoToAsync berjalan bersamaan (halaman bisa salah
    // atau kosong). Tap cepat dirapikan lewat satu antrean: hanya satu navigasi
    // berjalan, dan tap terbaru selalu menang.
    private static readonly object KunciPindah = new();
    private static bool _pompaJalan;
    private static int _tujuanTertunda = -1;

    private Player? _pemutarLuncur;
    private Player? _pemutarPantul;
    private int _generasiAnimasi;
    private bool _geometriSiap;
    private bool _sudahAnimasiUntukTampilanIni;
    private ContentPage? _halaman;

    public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
        nameof(ActiveTab),
        typeof(string),
        typeof(BottomNavBar),
        TabAbsensi);

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public BottomNavBar()
    {
        InitializeComponent();
        Items.SizeChanged += OnItemsSizeChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_halaman is not null)
        {
            return;
        }

        // Ikuti Appearing halaman induk agar animasi tetap jalan ketika halaman
        // yang sudah pernah dibuka ditampilkan kembali oleh Shell.
        _halaman = CariHalamanInduk(this);
        if (_halaman is not null)
        {
            _halaman.Appearing += OnHalamanAppearing;
        }
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_halaman is not null)
        {
            _halaman.Appearing -= OnHalamanAppearing;
            _halaman = null;
        }
    }

    private void OnHalamanAppearing(object? sender, EventArgs e)
    {
        // Navbar tiap halaman hanya menampilkan tab miliknya sendiri, jadi
        // Appearing yang telat dari halaman lain tidak bisa menggeser pil ke
        // tab yang salah.
        var index = ActiveIndex;

        // Halaman perantara saat pompa berpindah tidak boleh menimpa tab tujuan.
        if (!_pompaJalan)
        {
            _tabAktif = index;
        }

        _sudahAnimasiUntukTampilanIni = false;
        TampilkanTabAktif(index);
    }

    private void OnItemsSizeChanged(object? sender, EventArgs e)
    {
        if (Items.Width <= 0)
        {
            return;
        }

        AturLebarPenanda();

        if (!_geometriSiap)
        {
            _geometriSiap = true;
            TampilkanTabAktif(ActiveIndex);
            return;
        }

        // Lebar kolom berubah (mis. rotasi layar): posisikan ulang tanpa animasi.
        HentikanAnimasi();
        Indicator.TranslationX = XUntukIndex(ActiveIndex);
    }

    /// <summary>
    /// Menempatkan pil di tab aktif. Jika tab asal luncur berbeda, pil mulai
    /// dari posisi tab asal lalu meluncur ke tab aktif dan ikonnya memantul.
    /// </summary>
    private void TampilkanTabAktif(int index)
    {
        if (!_geometriSiap || _sudahAnimasiUntukTampilanIni)
        {
            return;
        }

        _sudahAnimasiUntukTampilanIni = true;

        var asal = _asalLuncur;
        _asalLuncur = -1;

        if (asal < 0 || asal == index || asal >= JumlahTab)
        {
            HentikanAnimasi();
            Indicator.TranslationX = XUntukIndex(index);
            return;
        }

        Indicator.TranslationX = XUntukIndex(asal);
        _ = LuncurDanPantulAsync(index, XUntukIndex(index));
    }

    private static int IndexDari(string? tab) => tab?.ToLowerInvariant() switch
    {
        TabLaporan => 1,
        TabProfil => 2,
        _ => 0
    };

    private int ActiveIndex => IndexDari(ActiveTab);

    private View KontenPada(int index) => index switch
    {
        1 => LaporanContent,
        2 => ProfilContent,
        _ => AbsensiContent
    };

    private View[] KontenSemua() => new View[] { AbsensiContent, LaporanContent, ProfilContent };

    /// <summary>Lebar pil mengikuti lebar kolom, dibatasi agar tidak terlalu gemuk.</summary>
    private static double LebarPenanda(double lebarKolom) =>
        Math.Max(LebarPenandaMin, Math.Min(LebarPenandaMaks, lebarKolom - 20));

    private double XUntukIndex(int index)
    {
        var lebarKolom = Items.Width / JumlahTab;
        return (index * lebarKolom) + ((lebarKolom - LebarPenanda(lebarKolom)) / 2);
    }

    private void AturLebarPenanda()
    {
        var lebarKolom = Items.Width / JumlahTab;
        Indicator.WidthRequest = LebarPenanda(lebarKolom);
    }

    private void HentikanAnimasi()
    {
        _generasiAnimasi++;

        _pemutarLuncur?.Dispose();
        _pemutarPantul?.Dispose();
        _pemutarLuncur = null;
        _pemutarPantul = null;

        foreach (var konten in KontenSemua())
        {
            konten.Scale = 1;
        }
    }

    private async Task LuncurDanPantulAsync(int index, double targetX)
    {
        var generasi = ++_generasiAnimasi;
        var luncur = LuncurkanPenandaAsync(targetX, generasi);
        var pantul = PantulkanKontenAsync(KontenPada(index), generasi);

        await Task.WhenAll(luncur, pantul);
    }

    private async Task LuncurkanPenandaAsync(double targetX, int generasi)
    {
        _pemutarLuncur?.Dispose();

        // Setiap penulisan posisi diperiksa terhadap generasi aktif. Player yang
        // sudah digantikan bisa saja masih menulis satu frame terakhir; tanpa
        // penjaga ini pil berhenti di posisi acak (navbar "mabuk").
        var timeline = TimelineBuilder
            .Create(DurasiLuncur)
            .Animate(
                Indicator,
                (penanda, nilai) =>
                {
                    if (generasi == _generasiAnimasi)
                    {
                        penanda.TranslationX = nilai;
                    }
                },
                track => track.FromCurrent(Easings.CubicOut).To(targetX))
            .Build();

        var pemutar = new Player(timeline, MauiClock.For(Indicator));
        _pemutarLuncur = pemutar;

        try
        {
            await pemutar.PlayAsync();
        }
        catch (Exception) when (generasi != _generasiAnimasi)
        {
            // Animasi ini digantikan animasi yang lebih baru; aman diabaikan.
        }
        finally
        {
            if (generasi == _generasiAnimasi)
            {
                Indicator.TranslationX = targetX;
            }
        }
    }

    private async Task PantulkanKontenAsync(View konten, int generasi)
    {
        _pemutarPantul?.Dispose();

        foreach (var lain in KontenSemua())
        {
            if (!ReferenceEquals(lain, konten))
            {
                lain.Scale = 1;
            }
        }

        var timeline = TimelineBuilder
            .Create(DurasiPantul)
            .Animate(
                konten,
                (target, nilai) =>
                {
                    if (generasi == _generasiAnimasi)
                    {
                        target.Scale = nilai;
                    }
                },
                track => track
                    .From(1)
                    .Key(0.5, 1.16, Easings.Spring(0.35, 14))
                    .To(1))
            .Build();

        var pemutar = new Player(timeline, MauiClock.For(konten));
        _pemutarPantul = pemutar;

        try
        {
            await pemutar.PlayAsync();
        }
        catch (Exception) when (generasi != _generasiAnimasi)
        {
            // Animasi ini digantikan animasi yang lebih baru; aman diabaikan.
        }
        finally
        {
            if (generasi == _generasiAnimasi)
            {
                konten.Scale = 1;
            }
        }
    }

    private static ContentPage? CariHalamanInduk(Element elemen)
    {
        var induk = elemen.Parent;
        while (induk is not null)
        {
            if (induk is ContentPage halaman)
            {
                return halaman;
            }

            induk = induk.Parent;
        }

        return null;
    }

    private static async Task PindahKeAsync(int index)
    {
        if (index < 0 || index >= JumlahTab)
        {
            return;
        }

        lock (KunciPindah)
        {
            // Tap ke tab yang sedang dituju atau sedang tampil memang tidak perlu
            // apa-apa. Selain itu semua tap diterima langsung — tanpa jeda —
            // supaya perpindahan cepat terasa responsif.
            if (index == _tujuanTertunda || (index == _tabAktif && _tujuanTertunda < 0))
            {
                return;
            }

            // Selalu catat tujuan terbaru. Kalau pompa sedang berjalan, tap ini
            // akan dieksekusi setelahnya dan mengalahkan tap sebelumnya.
            _tujuanTertunda = index;

            if (_pompaJalan)
            {
                return;
            }

            _pompaJalan = true;
        }

        try
        {
            while (true)
            {
                int tujuan;
                lock (KunciPindah)
                {
                    tujuan = _tujuanTertunda;
                    _tujuanTertunda = -1;

                    // Kunci dilepas hanya saat tidak ada tujuan baru, supaya tap
                    // yang datang di sela ini memulai pompa baru, bukan hilang.
                    if (tujuan < 0)
                    {
                        _pompaJalan = false;
                        return;
                    }
                }

                if (tujuan == _tabAktif)
                {
                    continue;
                }

                var shell = Shell.Current;
                if (shell is null)
                {
                    continue;
                }

                // Catat tab asal supaya halaman tujuan meluncur dari tab yang
                // benar. Ditulis di sini (bukan dari Appearing) agar tidak bisa
                // ditimpa peristiwa Appearing yang datang telat.
                _asalLuncur = _tabAktif >= 0 && _tabAktif < JumlahTab ? _tabAktif : -1;
                _tabAktif = tujuan;

                try
                {
                    await shell.GoToAsync(RutePerTab[tujuan], false);
                }
                catch (Exception)
                {
                    // Navigasi gagal (mis. route tidak dikenal). Biarkan tab
                    // aktif kembali ke halaman yang benar-benar tampil agar tap
                    // berikutnya tidak dianggap "sudah di tab itu".
                    _tabAktif = -1;
                    _asalLuncur = -1;
                }
            }
        }
        finally
        {
            lock (KunciPindah)
            {
                _pompaJalan = false;
            }
        }
    }

    private async void OnAbsensiTapped(object? sender, TappedEventArgs e) =>
        await PindahKeAsync(0);

    private async void OnLaporanTapped(object? sender, TappedEventArgs e) =>
        await PindahKeAsync(1);

    private async void OnProfilTapped(object? sender, TappedEventArgs e) =>
        await PindahKeAsync(2);
}
