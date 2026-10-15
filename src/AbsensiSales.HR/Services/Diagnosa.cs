using System.Reflection;
using System.Text;

namespace AbsensiSales.HR.Services;

/// <summary>Log diagnostik sementara ke %LOCALAPPDATA%\AbsensiSalesHR\diag.log</summary>
public static class Diagnosa
{
    public static void Catat(string judul, Page? hal)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== {judul} {DateTime.Now:HH:mm:ss} ===");

            if (Application.Current?.Windows.FirstOrDefault() is Window jendela)
                sb.AppendLine($"Window: {jendela.Width}x{jendela.Height}");

            if (hal is null)
                return;

            sb.AppendLine($"Page {hal.GetType().Name}: bounds={hal.Bounds}");
            Tulis(sb, "  ", hal, 0);

            var folder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AbsensiSalesHR");
            Directory.CreateDirectory(folder);
            File.AppendAllText(System.IO.Path.Combine(folder, "diag.log"), sb.ToString());
        }
        catch
        {
            // jangan pernah bikin crash
        }
    }

    private static void Tulis(StringBuilder sb, string indent, Element e, int kedalaman)
    {
        if (kedalaman > 3)
            return;

        if (e is VisualElement ve)
            sb.AppendLine($"{indent}{ve.GetType().Name} bounds={ve.Bounds} {Nilai(ve)}");

        if (e is Grid grid)
            sb.AppendLine($"{indent}  kolom=[{string.Join(",", grid.ColumnDefinitions.Select(k => k.Width.ToString()))}]");

        foreach (var anak in Anak(e))
            Tulis(sb, indent + "  ", anak, kedalaman + 1);
    }

    private static IEnumerable<Element> Anak(Element induk)
    {
        if (induk is IContentView isi && isi.Content is Element dariInterface)
            yield return dariInterface;

        var children = induk.GetType().GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);
        if (children?.GetValue(induk) is System.Collections.IEnumerable daftar)
            foreach (var isi2 in daftar)
                if (isi2 is Element e)
                    yield return e;
    }

    private static string Nilai(VisualElement ve)
    {
        var bagian = new List<string>();
        foreach (var nama in new[] { "FontSize", "Margin", "Padding", "WidthRequest", "HeightRequest", "Spacing" })
        {
            var bp = Cari(ve.GetType(), nama);
            if (bp is not null)
                bagian.Add($"{nama}={ve.GetValue(bp)}");
        }
        return string.Join(" ", bagian);
    }

    private static BindableProperty? Cari(Type tipe, string nama)
    {
        for (var t = tipe; t is not null && t != typeof(object); t = t.BaseType)
        {
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (f.FieldType == typeof(BindableProperty) && f.GetValue(null) is BindableProperty bp
                    && bp.PropertyName == nama)
                    return bp;
            }
        }
        return null;
    }
}
