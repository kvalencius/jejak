using AbsensiSales.Mobile.ViewModels;

namespace AbsensiSales.Mobile.Views;

public partial class AbsensiPertamaPage : ContentPage
{
    private readonly AbsensiPertamaViewModel _viewModel;

    public AbsensiPertamaPage(AbsensiPertamaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Hentikan timer saat keluar dari halaman ini
        _viewModel.OnDisappearing();
    }
}
