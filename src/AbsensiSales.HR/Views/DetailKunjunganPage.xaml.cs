using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class DetailKunjunganPage : ContentPage
{
    public DetailKunjunganPage(DetailKunjunganViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
