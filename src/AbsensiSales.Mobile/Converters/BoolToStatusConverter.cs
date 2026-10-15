using System.Globalization;

namespace AbsensiSales.Mobile.Converters;

/// <summary>
/// Mengubah nilai boolean hasil verifikasi menjadi teks status.
/// true -> "Wajah cocok", false -> "Tidak cocok".
/// </summary>
public class BoolToStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? "Wajah cocok" : "Tidak cocok";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
