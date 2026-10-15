using AbsensiSales.Mobile.ViewModels;

namespace AbsensiSales.Mobile.Views;

public partial class PengajuanIzinPage : ContentPage
{
    public PengajuanIzinPage(PengajuanIzinViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
