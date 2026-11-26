using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class HrLoginPage : ContentPage
{
    public HrLoginPage(HrLoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        PasangFokusBorder(EntryUsername, UsernameInput);
        PasangFokusBorder(EntryPassword, PasswordInput);
    }

    /// <summary>
    /// Mengalirkan fokus masukan ke VisualStateManager wrapper Border:
    /// saat Entry terfokus Border jadi aksen amber, saat lepas kembali
    /// ke border netral.
    /// </summary>
    private static void PasangFokusBorder(Border pembungkus, Entry masukan)
    {
        masukan.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Entry.IsFocused))
                VisualStateManager.GoToState(pembungkus, masukan.IsFocused ? "Focused" : "Unfocused");
        };
    }
}
