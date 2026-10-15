using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class PayrollPage : ContentPage
{
    public PayrollPage(PayrollViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
