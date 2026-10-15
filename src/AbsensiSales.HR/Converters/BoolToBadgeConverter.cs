using System.Globalization;

namespace AbsensiSales.HR.Converters;

/// <summary>
/// Mengubah nilai boolean menjadi label badge (Setuju/Proses).
/// </summary>
public class BoolToBadgeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? "Disetujui" : "Menunggu";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
