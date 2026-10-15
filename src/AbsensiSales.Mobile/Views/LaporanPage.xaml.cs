using AbsensiSales.Mobile.ViewModels;

namespace AbsensiSales.Mobile.Views;

public partial class LaporanPage : ContentPage
{
    private readonly LaporanViewModel _viewModel;

    public LaporanPage(LaporanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}
