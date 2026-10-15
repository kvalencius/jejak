using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class LaporanHrPage : ContentPage
{
    public LaporanHrPage(LaporanHrViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
