using System.Reflection;
using Microsoft.Maui.Controls.Shapes;

namespace AbsensiSales.HR.Services;

/// <summary>
/// Penskalaan UI global: seluruh ukuran elemen (font, padding, margin, jarak,
/// lebar/tinggi, sudut membulat, radius bayangan) dikalikan <see cref="Faktor"/>
/// sehingga tampilan menyusut dari 100% menjadi 80%.
/// Nilai dibaca lewat BindableProperty (bukan properti CLR) karena di MAUI
/// banyak ukuran yang diimplementasikan lewat interface.
/// Berjalan otomatis saat halaman muncul dan terus mengikuti elemen baru
/// (konten yang diganti &amp; item bindable layout) supaya tidak ada elemen
/// yang tertinggal. Idempoten: elemen yang sama tidak pernah diskalakan dua kali.
/// </summary>
public static class UiScale
{
    public const double Faktor = 0.8;

    private static readonly BindableProperty SudahProperty =
        BindableProperty.CreateAttached("Sudah", typeof(bool), typeof(UiScale), false);

    private static readonly string[] NamaProperti =
    {
        "FontSize", "Margin", "Padding", "WidthRequest", "HeightRequest",
        "MinimumWidthRequest", "MinimumHeightRequest", "CornerRadius",
        "Spacing", "ColumnSpacing", "RowSpacing", "ShadowRadius", "ShadowOffset"
    };

    private static readonly Dictionary<Type, List<BindableProperty>> Cache = new();

    // StrokeShape bisa dibagikan antar elemen (lewat Style), jadi cukup sekali.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, object> BentukSudut = new();

    public static void Terapkan(Element? induk)
    {
        try
        {
            TerapkanDalam(induk);
        }
        catch (Exception ex)
        {
            Catat(ex);
        }
    }

    private static void TerapkanDalam(Element? induk)
    {
        if (induk is null || (bool)induk.GetValue(SudahProperty))
            return;

        induk.SetValue(SudahProperty, true);

        if (induk is VisualElement ve)
        {
            foreach (var properti in Properti(ve.GetType()))
                SkalaProperti(ve, properti);

            if (ve is Grid grid)
                SkalaGrid(grid);

            if (ve is Border border && border.StrokeShape is RoundRectangle sudut && !SudahSkala(sudut))
                sudut.CornerRadius = new CornerRadius(sudut.CornerRadius.TopLeft * Faktor);
        }

        // Anak layout (item binding) lahir belakangan tetap ikut diskalakan.
        // Kontainer ber-template (ContentView, Frame, ScrollView) memakai
        // event Content saja supaya bagian dalam template tidak ikut diubah.
        if (induk is Layout)
            induk.ChildAdded += (_, e) => Terapkan(e.Element);

        induk.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "Content" && AnakContent(induk) is Element anak)
                Terapkan(anak);
        };

        foreach (var anak in Anak(induk))
            Terapkan(anak);
    }

    private static IEnumerable<Element> Anak(Element induk)
    {
        if (induk is IContentView isi && isi.Content is Element dariInterface)
            yield return dariInterface;

        var properti = induk.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
        if (properti?.GetValue(induk) is Element dariProperti)
            yield return dariProperti;

        var children = induk.GetType().GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);
        if (children is not null && children.PropertyType.IsGenericType
            && typeof(IView).IsAssignableFrom(children.PropertyType.GetGenericArguments()[0])
            && children.GetValue(induk) is System.Collections.IEnumerable daftar)
        {
            foreach (var isi2 in daftar)
                if (isi2 is Element e)
                    yield return e;
        }
    }

    private static Element? AnakContent(Element induk)
    {
        if (induk is IContentView isi)
            return isi.Content as Element;

        var properti = induk.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
        return properti?.GetValue(induk) as Element;
    }

    private static void SkalaProperti(VisualElement ve, BindableProperty properti)
    {
        switch (ve.GetValue(properti))
        {
            case double d when double.IsFinite(d) && d > 0:
                ve.SetValue(properti, d * Faktor);
                break;
            case Thickness t:
                ve.SetValue(properti, new Thickness(
                    t.Left * Faktor, t.Top * Faktor, t.Right * Faktor, t.Bottom * Faktor));
                break;
        }
    }

    private static void SkalaGrid(Grid grid)
    {
        foreach (var kolom in grid.ColumnDefinitions)
            if (kolom.Width.IsAbsolute)
                kolom.Width = new GridLength(kolom.Width.Value * Faktor);

        foreach (var baris in grid.RowDefinitions)
            if (baris.Height.IsAbsolute)
                baris.Height = new GridLength(baris.Height.Value * Faktor);
    }

    /// <summary>True kalau bentuk ini sudah pernah diskalakan (sekali ditandai saat dipanggil pertama).</summary>
    private static bool SudahSkala(object bentuk)
    {
        if (BentukSudut.TryGetValue(bentuk, out _))
            return true;

        BentukSudut.Add(bentuk, true);
        return false;
    }

    private static List<BindableProperty> Properti(Type tipe)
    {
        if (Cache.TryGetValue(tipe, out var tersimpan))
            return tersimpan;

        var hasil = new List<BindableProperty>();
        var sudah = new HashSet<BindableProperty>();

        for (var t = tipe; t is not null && t != typeof(object); t = t.BaseType)
        {
            foreach (var lapangan in t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (lapangan.FieldType != typeof(BindableProperty))
                    continue;

                if (lapangan.GetValue(null) is BindableProperty bp
                    && NamaProperti.Contains(bp.PropertyName, StringComparer.Ordinal)
                    && sudah.Add(bp))
                {
                    hasil.Add(bp);
                }
            }
        }

        Cache[tipe] = hasil;
        return hasil;
    }

    private static void Catat(Exception ex)
    {
        try
        {
            var folder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AbsensiSalesHR");
            Directory.CreateDirectory(folder);
            File.AppendAllText(System.IO.Path.Combine(folder, "uiscale.log"), DateTime.Now + "\n" + ex + "\n\n");
        }
        catch
        {
            // logging tidak boleh bikin crash
        }
    }
}
