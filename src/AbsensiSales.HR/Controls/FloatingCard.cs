using Microsoft.Maui.Controls.Shapes;

namespace AbsensiSales.HR.Controls;

/// <summary>
/// Kartu melayang (floating) berupa Border dengan sudut membulat
/// dan bayangan lembut di belakangnya.
/// Dipakai lewat ControlTemplate (lihat FloatingCardTemplate di Styles.xaml)
/// agar konten di dalamnya bisa ditulis seperti Border biasa.
/// </summary>
public class FloatingCard : ContentView
{
    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(FloatingCard), 16d);

    public static readonly BindableProperty StrokeProperty =
        BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(FloatingCard), null);

    public static readonly BindableProperty StrokeThicknessProperty =
        BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(FloatingCard), 0d);

    public static readonly BindableProperty ShadowColorProperty =
        BindableProperty.Create(nameof(ShadowColor), typeof(Color), typeof(FloatingCard), Color.FromArgb("#FFF7DB"));

    public static readonly BindableProperty ShadowOpacityProperty =
        BindableProperty.Create(nameof(ShadowOpacity), typeof(double), typeof(FloatingCard), 0.5d);

    public static readonly BindableProperty ShadowRadiusProperty =
        BindableProperty.Create(nameof(ShadowRadius), typeof(double), typeof(FloatingCard), 12d);

    public static readonly BindableProperty ShadowOffsetProperty =
        BindableProperty.Create(nameof(ShadowOffset), typeof(double), typeof(FloatingCard), 5d);

    private SoftShadowView? _shadow;
    private Border? _card;

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Brush? Stroke
    {
        get => (Brush?)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public Color ShadowColor
    {
        get => (Color)GetValue(ShadowColorProperty);
        set => SetValue(ShadowColorProperty, value);
    }

    public double ShadowOpacity
    {
        get => (double)GetValue(ShadowOpacityProperty);
        set => SetValue(ShadowOpacityProperty, value);
    }

    public double ShadowRadius
    {
        get => (double)GetValue(ShadowRadiusProperty);
        set => SetValue(ShadowRadiusProperty, value);
    }

    public double ShadowOffset
    {
        get => (double)GetValue(ShadowOffsetProperty);
        set => SetValue(ShadowOffsetProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _shadow = GetTemplateChild("PART_Shadow") as SoftShadowView;
        _card = GetTemplateChild("PART_Card") as Border;

        Sinkron();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName is not null)
            Sinkron();
    }

    /// <summary>Menyamakan nilai kartu dan bayangan dengan properti kontrol.</summary>
    private void Sinkron()
    {
        // Ruang bayangan selalu disediakan selama ShadowRadius > 0 agar
        // layout tidak "melompat" saat bayangan dinyalakan/dimatikan.
        double pad = ShadowRadius > 0 ? ShadowRadius : 0;
        double geser = ShadowRadius > 0 ? ShadowOffset : 0;

        if (_shadow is not null)
        {
            _shadow.IsVisible = pad > 0 && ShadowOpacity > 0;
            _shadow.CornerRadius = CornerRadius;
            _shadow.ShadowColor = ShadowColor;
            _shadow.ShadowOpacity = ShadowOpacity;
            _shadow.ShadowRadius = ShadowRadius;
            _shadow.ShadowOffset = ShadowOffset;
        }

        if (_card is null)
            return;

        _card.Margin = new Thickness(pad, pad, pad, pad + geser * 2);
        _card.BackgroundColor = BackgroundColor;
        _card.Padding = Padding;
        _card.Stroke = Stroke;
        _card.StrokeThickness = StrokeThickness;
        _card.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(CornerRadius) };
    }
}
