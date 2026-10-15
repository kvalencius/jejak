namespace AbsensiSales.HR.Services;

/// <summary>
/// Navigasi sederhana di dalam workspace desktop HR.
/// Halaman konten diganti di dalam Frame milik WorkspacePage
/// sehingga sidebar tetap tampil (pola desktop, bukan routing Shell).
/// </summary>
public class HrNavigationService
{
    public event Action<Type, object?>? Navigated;

    public void NavigateTo(Type pageType, object? parameter = null) =>
        Navigated?.Invoke(pageType, parameter);
}
