using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class ApprovalPage : ContentPage
{
    public ApprovalPage(ApprovalViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
