using AbsensiSales.Mobile.ViewModels;

namespace AbsensiSales.Mobile.Views;

public partial class VerifikasiDataPage : ContentPage
{
    private readonly VerifikasiDataViewModel _viewModel;

    public VerifikasiDataPage(VerifikasiDataViewModel viewModel)
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
