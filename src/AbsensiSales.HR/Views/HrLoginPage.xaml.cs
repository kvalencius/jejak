using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class HrLoginPage : ContentPage
{
    public HrLoginPage(HrLoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
