using System.Globalization;

namespace AbsensiSales.Mobile.Converters;

/// <summary>
/// Mengubah string menjadi boolean: true bila string berisi teks.
/// Dipakai untuk menyembunyikan label pesan/error yang kosong.
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
