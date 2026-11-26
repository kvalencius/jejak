using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class HrRegisterPage : ContentPage
{
    public HrRegisterPage(HrRegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        PasangFokusBorder(NamaInput);
        PasangFokusBorder(EmailInput);
        PasangFokusBorder(UsernameInput);
        PasangFokusBorder(PasswordInput);
    }

    /// <summary>
    /// Mengalirkan fokus masukan ke VisualStateManager wrapper Border:
    /// saat Entry terfokus Border jadi aksen amber, saat lepas kembali
    /// ke border netral.
    /// </summary>
    private static void PasangFokusBorder(Entry masukan)
    {
        if (masukan.Parent is not Border pembungkus)
            return;

        masukan.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Entry.IsFocused))
                VisualStateManager.GoToState(pembungkus, masukan.IsFocused ? "Focused" : "Unfocused");
        };
    }
}
