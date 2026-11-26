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

        // Dipanggil juga saat kembali dari halaman pendaftaran wajah,
        // sehingga status token diperiksa ulang otomatis.
        _viewModel.OnAppearing();
    }
}
