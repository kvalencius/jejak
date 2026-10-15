using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class HrRegisterPage : ContentPage
{
    public HrRegisterPage(HrRegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
