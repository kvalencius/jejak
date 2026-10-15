using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class MasterDataPage : ContentPage
{
    public MasterDataPage(MasterDataViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
