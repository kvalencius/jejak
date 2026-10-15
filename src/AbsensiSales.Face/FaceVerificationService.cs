using OpenCvSharp;

namespace AbsensiSales.Face;

/// <summary>
/// Layanan verifikasi wajah untuk aplikasi absensi.
/// Jika model ONNX / cascade / foto referensi belum tersedia,
/// layanan otomatis beralih ke mode simulasi agar mockup tetap bisa dijalankan.
/// </summary>
public class FaceVerificationService : IDisposable
{
    private const double MatchThreshold = 0.363;

    private readonly string _modelPath;
    private readonly string _cascadePath;
    private readonly string _referenceDir;
    private CascadeClassifier? _cascade;
    private FaceEmbedder? _embedder;
    private readonly List<(string Nama, float[] Embedding)> _referensi = new();
    private bool _siap;

    public FaceVerificationService(string baseFolder)
    {
        _modelPath = Path.Combine(baseFolder, "models", "face_recognition_sface_2021dec.onnx");
        _cascadePath = Path.Combine(baseFolder, "models", "haarcascade_frontalface_default.xml");
        _referenceDir = Path.Combine(baseFolder, "reference_faces");
    }

    public bool ModeSimulasi => !_siap;

    /// <summary>
    /// Inisialisasi sekali di awal aplikasi. Aman dipanggil berulang.
    /// </summary>
    public void Inisialisasi()
    {
        if (_siap || _embedder is not null)
            return;

        try
        {
            if (!File.Exists(_modelPath) || !File.Exists(_cascadePath))
                return;

            _cascade = new CascadeClassifier(_cascadePath);
            _embedder = new FaceEmbedder(_modelPath);

            if (Directory.Exists(_referenceDir))
            {
                foreach (var file in Directory.GetFiles(_referenceDir)
                    .Where(f => new[] { ".jpg", ".jpeg", ".png" }
                        .Contains(Path.GetExtension(f).ToLower())))
                {
                    using var img = Cv2.ImRead(file);
                    if (img.Empty())
                        continue;

                    var faces = DeteksiWajah(img);
                    if (faces.Length == 0)
                        continue;

                    var utama = faces.OrderByDescending(r => r.Width * r.Height).First();
                    var nama = Path.GetFileNameWithoutExtension(file);
                    _referensi.Add((nama, _embedder.GetEmbedding(img, utama)));
                }
            }

            _siap = _referensi.Count > 0;
        }
        catch
        {
            // Model tidak bisa dimuat -> mode simulasi
            _embedder?.Dispose();
            _embedder = null;
            _siap = false;
        }
    }

    /// <summary>
    /// Verifikasi wajah dari frame kamera webcam terhadap foto referensi sales.
    /// </summary>
    public FaceVerifyResult VerifikasiDariKamera(string namaSales)
    {
        Inisialisasi();

        if (!_siap || _embedder is null || _cascade is null)
            return Simulasi(namaSales);

        try
        {
            using var capture = new VideoCapture(0);
            if (!capture.IsOpened())
                return Simulasi(namaSales);

            using var frame = new Mat();
            capture.Read(frame);
            if (frame.Empty())
                return Simulasi(namaSales);

            var faces = DeteksiWajah(frame);
            if (faces.Length == 0)
                return new FaceVerifyResult(false, 0, "Wajah tidak terdeteksi. Coba posisikan wajah di dalam bingkai.", false);

            var utama = faces.OrderByDescending(r => r.Width * r.Height).First();
            var embedding = _embedder.GetEmbedding(frame, utama);

            string namaTerbaik = "";
            double skorTerbaik = -1;
            foreach (var (nama, refEmbedding) in _referensi)
            {
                double skor = FaceEmbedder.CosineSimilarity(embedding, refEmbedding);
                if (skor > skorTerbaik)
                {
                    skorTerbaik = skor;
                    namaTerbaik = nama;
                }
            }

            bool cocok = skorTerbaik >= MatchThreshold
                && namaTerbaik.Contains(namaSales.Split(' ')[0], StringComparison.OrdinalIgnoreCase);

            return cocok
                ? new FaceVerifyResult(true, skorTerbaik, $"Wajah cocok dengan {namaTerbaik}.", false)
                : new FaceVerifyResult(false, skorTerbaik, "Wajah tidak cocok dengan data referensi.", false);
        }
        catch
        {
            return Simulasi(namaSales);
        }
    }

    private static FaceVerifyResult Simulasi(string namaSales)
    {
        // Skor simulasi stabil di atas threshold agar alur mockup lancar
        var skor = 0.94 + (Math.Abs(namaSales.GetHashCode()) % 5) / 100.0;
        return new FaceVerifyResult(true, skor, $"Simulasi: wajah {namaSales} dianggap cocok (model demo).", true);
    }

    private Rect[] DeteksiWajah(Mat image)
    {
        using var gray = new Mat();
        Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(gray, gray);
        return _cascade!.DetectMultiScale(gray, 1.1, 5, minSize: new Size(80, 80));
    }

    public void Dispose()
    {
        _embedder?.Dispose();
        _cascade?.Dispose();
    }
}
