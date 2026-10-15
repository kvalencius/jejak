using Microsoft.Maui.Graphics;

namespace AbsensiSales.HR.Controls;

/// <summary>
/// Bayangan lembut untuk elemen melayang (floating).
/// Dibuat dengan menumpuk beberapa lapis rounded rectangle berwarna solid
/// dengan alpha yang makin tipis ke arah luar, sehingga terlihat seperti
/// box-shadow blur tanpa memakai gradient.
/// </summary>
public class SoftShadowView : GraphicsView, IDrawable
{
    private const int JumlahLapis = 14;

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(SoftShadowView), 16d, propertyChanged: OnGambarUlang);

    public static readonly BindableProperty ShadowColorProperty =
        BindableProperty.Create(nameof(ShadowColor), typeof(Color), typeof(SoftShadowView), Color.FromArgb("#FFF7DB"), propertyChanged: OnGambarUlang);

    public static readonly BindableProperty ShadowOpacityProperty =
        BindableProperty.Create(nameof(ShadowOpacity), typeof(double), typeof(SoftShadowView), 0.5d, propertyChanged: OnGambarUlang);

    public static readonly BindableProperty ShadowRadiusProperty =
        BindableProperty.Create(nameof(ShadowRadius), typeof(double), typeof(SoftShadowView), 12d, propertyChanged: OnGambarUlang);

    public static readonly BindableProperty ShadowOffsetProperty =
        BindableProperty.Create(nameof(ShadowOffset), typeof(double), typeof(SoftShadowView), 5d, propertyChanged: OnGambarUlang);

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
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

    public SoftShadowView()
    {
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        Drawable = this;
    }

    private static void OnGambarUlang(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SoftShadowView view)
            view.Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        double radius = ShadowRadius;
        double geser = ShadowOffset;
        double opasitas = ShadowOpacity;

        if (radius <= 0 || geser < 0 || opasitas <= 0)
            return;

        // Ruang cadangan di dalam view agar bayangan tidak terpotong.
        float pad = (float)radius;
        var kartu = new RectF(
            dirtyRect.X + pad,
            dirtyRect.Y + pad,
            dirtyRect.Width - pad * 2,
            dirtyRect.Height - pad * 2 - (float)(geser * 2));

        if (kartu.Width <= 0 || kartu.Height <= 0)
            return;

        float sudut = (float)CornerRadius;

        // Lapis terluar dulu (paling tipis), ditumpuk sampai lapis terdalam.
        for (int i = JumlahLapis; i >= 1; i--)
        {
            float t = (i - 1) / (float)(JumlahLapis - 1);
            float melebar = pad * t;
            float alpha = (float)(opasitas * Math.Pow(1 - t, 2.2));

            if (alpha <= 0.002f)
                continue;

            canvas.FillColor = ShadowColor.WithAlpha(alpha);
            canvas.FillRoundedRectangle(
                kartu.X - melebar,
                kartu.Y - melebar + (float)geser,
                kartu.Width + melebar * 2,
                kartu.Height + melebar * 2,
                sudut);
        }
    }
}
