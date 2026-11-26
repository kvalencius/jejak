namespace AbsensiSales.Face;

/// <summary>
/// Engine verifikasi wajah: UltraFace untuk deteksi + SFace untuk embedding.
///
/// Alur yang dipakai aplikasi:
///   foto -> BuatTemplate()      -> token Base64 (disimpan di tbl_sales.face_data)
///   foto -> Verifikasi(template)-> cosine similarity + keputusan threshold
///
/// Tidak ada mode simulasi. Kalau model belum tersedia, semua method
/// mengembalikan kegagalan yang jelas supaya tidak pernah ada absensi
/// yang lolos verifikasi tanpa benar-benar mencocokkan wajah.
/// </summary>
public class FaceVerificationService : IDisposable
{
    private readonly FaceOptions _options;

    private readonly string? _baseFolder;
    private readonly byte[]? _detectorModel;
    private readonly byte[]? _sfaceModel;

    private FaceDetector? _detector;
    private FaceEmbedder? _embedder;
    private bool _sudahInisialisasi;
    private string? _pesanInisialisasi;

    /// <summary>
    /// Muat model dari folder dasar. Model dibaca dari
    /// <c>{baseFolder}/models/version-RFB-320.onnx</c> dan
    /// <c>{baseFolder}/models/face_recognition_sface_2021dec.onnx</c>.
    /// </summary>
    public FaceVerificationService(string baseFolder, FaceOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseFolder);

        _options = options ?? new FaceOptions();
        _baseFolder = baseFolder;
    }

    /// <summary>
    /// Muat model dari byte. Dipakai platform yang membaca model dari paket
    /// aplikasi (Android/iOS) alih-alih folder executable.
    /// </summary>
    public FaceVerificationService(byte[] detectorModel, byte[] sfaceModel, FaceOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(detectorModel);
        ArgumentNullException.ThrowIfNull(sfaceModel);

        _options = options ?? new FaceOptions();
        _detectorModel = detectorModel;
        _sfaceModel = sfaceModel;
    }

    /// <summary>True jika engine siap dipakai.</summary>
    public bool EngineSiap
    {
        get
        {
            Inisialisasi();
            return _detector is not null && _embedder is not null;
        }
    }

    /// <summary>Alasan engine tidak siap, null jika siap.</summary>
    public string? PesanInisialisasi
    {
        get
        {
            Inisialisasi();
            return _pesanInisialisasi;
        }
    }

    /// <summary>Batas cosine similarity agar wajah dianggap cocok.</summary>
    public double MatchThreshold => _options.MatchThreshold;

    /// <summary>
    /// Inisialisasi sekali di awal. Aman dipanggil berulang dari banyak thread;
    /// kegagalan disimpan supaya tidak mencoba memuat ulang terus-menerus.
    /// </summary>
    public void Inisialisasi()
    {
        if (_sudahInisialisasi)
            return;

        _sudahInisialisasi = true;

        try
        {
            var detector = _detectorModel ?? BacaModel("version-RFB-320.onnx");
            var sface = _sfaceModel ?? BacaModel("face_recognition_sface_2021dec.onnx");

            if (detector is null || sface is null)
            {
                _pesanInisialisasi =
                    "Model wajah belum tersedia. Letakkan version-RFB-320.onnx dan " +
                    "face_recognition_sface_2021dec.onnx di folder models (lihat models/README.md).";
                return;
            }

            _detector = new FaceDetector(detector, _options);
            _embedder = new FaceEmbedder(sface);
        }
        catch (Exception ex)
        {
            _detector?.Dispose();
            _detector = null;
            _embedder?.Dispose();
            _embedder = null;
            _pesanInisialisasi = "Model wajah gagal dimuat: " + ex.Message;
        }
    }

    private byte[]? BacaModel(string namaFile)
    {
        if (string.IsNullOrWhiteSpace(_baseFolder))
            return null;

        var path = Path.Combine(_baseFolder, "models", namaFile);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    /* ============================================================
     * PENDAFTARAN: foto -> token Base64
     * ============================================================ */

    /// <summary>
    /// Buat token wajah dari byte foto (JPEG/PNG). Foto tidak disimpan.
    /// Token inilah yang dikirim ke backend untuk disimpan di face_data.
    /// </summary>
    public FaceTemplateResult BuatTemplate(byte[] foto, int maxSide = 1000)
    {
        ArgumentNullException.ThrowIfNull(foto);
        Inisialisasi();

        if (_detector is null || _embedder is null)
            return new FaceTemplateResult(null, 0, _pesanInisialisasi ?? "Engine wajah belum siap.");

        try
        {
            var image = FaceImageLoader.Load(foto, maxSide);
            var faces = _detector.Detect(image);

            if (faces.Count == 0)
            {
                return new FaceTemplateResult(
                    null,
                    0,
                    "Wajah tidak terdeteksi. Coba foto lebih jelas dan menghadap ke kamera.");
            }

            if (faces.Count > _options.MaxFacesAllowed)
            {
                return new FaceTemplateResult(
                    null,
                    faces.Count,
                    $"Terdeteksi {faces.Count} wajah. Pastikan hanya satu wajah dalam foto.");
            }

            var face = faces.OrderByDescending(item => item.W * item.H).First();
            var validasi = ValidasiKualitasWajah(image, face);
            if (validasi is not null)
                return new FaceTemplateResult(null, faces.Count, validasi);

            var embedding = _embedder.GetEmbedding(image, face);
            if (embedding.Length == 0)
            {
                return new FaceTemplateResult(
                    null,
                    faces.Count,
                    "Ciri wajah gagal dihitung. Coba ambil foto ulang.");
            }

            return new FaceTemplateResult(
                FaceTemplateCodec.ToBase64(embedding),
                faces.Count,
                "Token wajah berhasil dibuat.");
        }
        catch (Exception ex)
        {
            return new FaceTemplateResult(null, 0, "Gagal memproses gambar: " + ex.Message);
        }
    }

    /* ============================================================
     * VERIFIKASI: foto -> cosine similarity vs token tersimpan
     * ============================================================ */

    /// <summary>
    /// Bandingkan wajah pada foto dengan token Base64 milik sales.
    /// Cocok bila cosine similarity &gt;= <see cref="MatchThreshold"/>.
    /// </summary>
    public FaceVerifyResult Verifikasi(byte[] foto, string templateBase64, int maxSide = 1000)
    {
        ArgumentNullException.ThrowIfNull(foto);
        Inisialisasi();

        if (_detector is null || _embedder is null)
            return new FaceVerifyResult(false, 0, _pesanInisialisasi ?? "Engine wajah belum siap.");

        if (string.IsNullOrWhiteSpace(templateBase64))
        {
            return new FaceVerifyResult(
                false,
                0,
                "Token wajah belum terdaftar. Lakukan pendaftaran wajah terlebih dahulu.");
        }

        float[] template;
        try
        {
            template = FaceTemplateCodec.FromBase64(templateBase64);
        }
        catch (Exception ex)
        {
            return new FaceVerifyResult(false, 0, "Token wajah tidak valid: " + ex.Message);
        }

        try
        {
            var image = FaceImageLoader.Load(foto, maxSide);
            var faces = _detector.Detect(image);

            if (faces.Count == 0)
            {
                return new FaceVerifyResult(
                    false,
                    0,
                    "Wajah tidak terdeteksi. Posisikan wajah di dalam bingkai dan pastikan cahaya cukup.")
                {
                    JumlahWajah = 0
                };
            }

            if (faces.Count > _options.MaxFacesAllowed)
            {
                return new FaceVerifyResult(
                    false,
                    0,
                    $"Terdeteksi {faces.Count} wajah. Pastikan hanya wajah Anda yang masuk ke kamera.")
                {
                    JumlahWajah = faces.Count
                };
            }

            var face = faces.OrderByDescending(item => item.W * item.H).First();
            var validasi = ValidasiKualitasWajah(image, face);
            if (validasi is not null)
            {
                return new FaceVerifyResult(false, 0, validasi)
                {
                    JumlahWajah = faces.Count
                };
            }

            var embedding = _embedder.GetEmbedding(image, face);

            // Dimensi berbeda berarti token dari engine lain; tidak boleh dibandingkan.
            if (embedding.Length != template.Length)
            {
                return new FaceVerifyResult(
                    false,
                    0,
                    $"Token wajah tidak cocok dengan engine (token {template.Length} nilai, " +
                    $"wajah {embedding.Length} nilai). Daftarkan ulang wajah.")
                {
                    JumlahWajah = faces.Count
                };
            }

            double skor = FaceEmbedder.CosineSimilarity(embedding, template);
            bool cocok = skor >= _options.MatchThreshold;

            return new FaceVerifyResult(
                cocok,
                skor,
                cocok
                    ? $"Wajah cocok (kemiripan {skor:P0}, batas {_options.MatchThreshold:P0})."
                    : $"Wajah tidak cocok (kemiripan {skor:P0}, batas {_options.MatchThreshold:P0}).")
            {
                JumlahWajah = faces.Count
            };
        }
        catch (Exception ex)
        {
            return new FaceVerifyResult(false, 0, "Gagal memproses gambar: " + ex.Message);
        }
    }

    private string? ValidasiKualitasWajah(RgbImage image, FaceBox face)
    {
        if (face.W <= 0 || face.H <= 0)
            return "Kotak wajah tidak valid. Coba ulangi foto.";

        if (MathF.Min(face.W, face.H) < _options.MinFaceSize)
        {
            return $"Wajah terlalu kecil (minimal {_options.MinFaceSize} piksel). " +
                   "Dekatkan wajah ke kamera.";
        }

        double brightness = FaceImageLoader.AverageBrightness(image, face);
        if (brightness < _options.MinBrightness)
            return "Wajah terlalu gelap. Cari tempat dengan cahaya yang lebih baik.";

        if (brightness > _options.MaxBrightness)
            return "Wajah terlalu terang. Hindari cahaya langsung yang menyilaukan.";

        return null;
    }

    public void Dispose()
    {
        _detector?.Dispose();
        _embedder?.Dispose();
    }
}
