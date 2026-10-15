using AbsensiSales.Mobile.ViewModels;

namespace AbsensiSales.Mobile.Views;

public partial class StatusPermohonanPage : ContentPage
{
    private readonly StatusPermohonanViewModel _viewModel;

    public StatusPermohonanPage(StatusPermohonanViewModel viewModel)
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
