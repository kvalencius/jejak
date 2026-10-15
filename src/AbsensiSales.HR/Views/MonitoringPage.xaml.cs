using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class MonitoringPage : ContentPage
{
    public MonitoringPage(MonitoringViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
