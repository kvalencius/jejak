using System.Globalization;

namespace AbsensiSales.Mobile.Converters;

/// <summary>
/// Membalik nilai boolean (dipakai untuk mengatur IsEnabled tombol).
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;
}
