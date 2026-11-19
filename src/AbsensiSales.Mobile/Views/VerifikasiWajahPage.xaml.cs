using AbsensiSales.Mobile.ViewModels;

namespace AbsensiSales.Mobile.Views;

public partial class VerifikasiWajahPage : ContentPage
{
    private readonly VerifikasiWajahViewModel _viewModel;

    public VerifikasiWajahPage(VerifikasiWajahViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();

        // Segarkan status izin saat pengguna kembali dari pengaturan Android.
        if (Window is not null)
        {
            Window.Resumed -= OnWindowResumed;
            Window.Resumed += OnWindowResumed;
        }
    }

    protected override void OnDisappearing()
    {
        if (Window is not null)
            Window.Resumed -= OnWindowResumed;

        base.OnDisappearing();
    }

    private async void OnWindowResumed(object? sender, EventArgs e)
    {
        await _viewModel.PeriksaIzinSetelahPengaturanAsync();
    }
}
