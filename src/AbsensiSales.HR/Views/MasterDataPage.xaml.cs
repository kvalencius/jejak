using AbsensiSales.HR.ViewModels;

namespace AbsensiSales.HR.Views;

public partial class MasterDataPage : ContentPage
{
    private bool _fokusTerpasang;

    public MasterDataPage(MasterDataViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_fokusTerpasang)
            return;

        _fokusTerpasang = true;

        // Semua Entry dibungkus Border (FormInputBorder). Alirkan fokusnya
        // supaya border berubah amber saat mengetik, sama seperti halaman login.
        foreach (var border in CariBorder(this))
        {
            if (border.Content is Entry entry)
                PasangFokusBorder(border, entry);
        }
    }

    private static IEnumerable<Border> CariBorder(Element induk)
    {
        foreach (var anak in Anak(induk))
        {
            if (anak is Border border)
                yield return border;

            foreach (var turunan in CariBorder(anak))
                yield return turunan;
        }
    }

    private static IEnumerable<Element> Anak(Element induk)
    {
        if (induk is IContentView isi && isi.Content is Element dariInterface)
            yield return dariInterface;

        if (induk is IList<IView> daftar)
            foreach (var anak in daftar)
                if (anak is Element e)
                    yield return e;
    }

    private static void PasangFokusBorder(Border pembungkus, Entry masukan)
    {
        masukan.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Entry.IsFocused))
                VisualStateManager.GoToState(pembungkus, masukan.IsFocused ? "Focused" : "Unfocused");
        };
    }
}
