using AbsensiSales.Mobile.ViewModels;

namespace AbsensiSales.Mobile.Views;

public partial class DaftarWajahPage : ContentPage, IQueryAttributable
{
    private readonly DaftarWajahViewModel _viewModel;

    public DaftarWajahPage(DaftarWajahViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("asal", out var asal) && asal is string nilai)
        {
            _viewModel.Asal = nilai;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}
