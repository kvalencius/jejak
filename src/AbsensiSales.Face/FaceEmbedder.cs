using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AbsensiSales.Face;

/// <summary>
/// Mengubah gambar wajah menjadi embedding (vektor ciri) memakai model SFace (ONNX).
/// Diport dari prototype MatanaProjectPrototypeCAM_001 (VRyouji).
/// Kelas ini tidak tahu sumber gambar (webcam, kamera MAUI, file),
/// sehingga bisa dipakai ulang dari aplikasi.
/// </summary>
public sealed class FaceEmbedder : IDisposable
{
    private const int InputSize = 112;
    private readonly InferenceSession _session;
    private readonly string _inputName;

    public FaceEmbedder(string modelPath)
        : this(File.ReadAllBytes(modelPath))
    {
    }

    public FaceEmbedder(byte[] modelBytes)
    {
        ArgumentNullException.ThrowIfNull(modelBytes);
        _session = new InferenceSession(modelBytes);
        _inputName = _session.InputMetadata.Keys.First();
    }

    /// <summary>
    /// Buat embedding dari gambar lintas platform (tanpa OpenCV) dengan crop
    /// persegi di sekitar wajah agar tidak gepeng saat di-resize.
    /// </summary>
    public float[] GetEmbedding(RgbImage image, FaceBox box)
    {
        ArgumentNullException.ThrowIfNull(image);

        float centerX = box.X + box.W / 2f;
        float centerY = box.Y + box.H / 2f;
        float side = MathF.Max(box.W, box.H) * 1.3f;

        var rgb = FaceImageLoader.CropResizeRgb(
            image,
            centerX - side / 2f,
            centerY - side / 2f,
            side,
            side,
            InputSize,
            InputSize);

        var tensor = new DenseTensor<float>(new[] { 1, 3, InputSize, InputSize });
        var span = tensor.Buffer.Span;
        for (int y = 0; y < InputSize; y++)
        {
            for (int x = 0; x < InputSize; x++)
            {
                for (int channel = 0; channel < 3; channel++)
                {
                    span[channel * InputSize * InputSize + y * InputSize + x] =
                        rgb[(y * InputSize + x) * 3 + channel];
                }
            }
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, tensor)
        };

        using var results = _session.Run(inputs);
        return results.First().AsEnumerable<float>().ToArray();
    }

    public static double CosineSimilarity(float[] a, float[] b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        // Template dengan dimensi berbeda tidak boleh dibandingkan.
        if (a.Length == 0 || a.Length != b.Length)
            return 0;

        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        return dot / (Math.Sqrt(na) * Math.Sqrt(nb) + 1e-9);
    }

    public void Dispose() => _session.Dispose();
}
