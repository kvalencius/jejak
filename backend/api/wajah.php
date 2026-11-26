<?php
/**
 * wajah.php — pendaftaran & pengelolaan token wajah milik sales sendiri.
 *
 * Aplikasi hanya mengirim token wajah (Base64 dari 128 float32 hasil model
 * SFace), bukan foto. Server tidak pernah menerima atau menyimpan foto wajah,
 * dan tidak melakukan pencocokan: pencocokan terjadi di perangkat.
 *
 * GET  -> status token wajah milik akun yang login
 *   {
 *     "success": true,
 *     "data": {
 *       "terdaftar": true,
 *       "face_data": "<684 karakter Base64>",
 *       "diperbarui": "2026-11-25 10:00:00"
 *     }
 *   }
 *
 * POST { "action": "simpan", "face_data": "<Base64>" }
 * POST { "action": "hapus" }
 */

declare(strict_types=1);

require_once __DIR__ . '/auth.php';

$pengguna = wajibLogin($pdo);
wajibRole($pengguna, 'Sales');

if ($pengguna['id_sales'] === null) {
    gagal('Profil sales tidak ditemukan untuk akun ini.', 'SALES_PROFILE_NOT_FOUND', 400);
}
$idSales = (int)$pengguna['id_sales'];

$kolomSales = kolomTabel($pdo, 'tbl_sales');
$adaFaceData = in_array('face_data', $kolomSales, true);
$adaDiperbarui = in_array('face_updated_at', $kolomSales, true);
$selectDiperbarui = $adaDiperbarui ? 'face_updated_at' : 'NULL AS face_updated_at';

if (!$adaFaceData) {
    gagal(
        'Kolom tbl_sales.face_data belum ada. Import backend/database/07_kolom_wajah.sql.',
        'SCHEMA_WAJAH_BELUM_SIAP',
        500
    );
}

$method = strtoupper($_SERVER['REQUEST_METHOD'] ?? 'GET');

/* ============================================================
 * GET — status token wajah sendiri
 * ============================================================ */
if ($method === 'GET') {
    $stmt = $pdo->prepare(
        "SELECT face_data, {$selectDiperbarui} FROM tbl_sales WHERE id_sales = ? LIMIT 1"
    );
    $stmt->execute([$idSales]);
    $row = $stmt->fetch();

    if (!$row) {
        gagal('Profil sales tidak ditemukan.', 'SALES_NOT_FOUND', 404);
    }

    $faceData = $row['face_data'] !== null ? trim((string)$row['face_data']) : '';

    sukses([
        'terdaftar'  => $faceData !== '',
        'face_data'  => $faceData !== '' ? $faceData : null,
        'diperbarui' => $row['face_updated_at'] ?? null,
    ], $faceData !== '' ? 'Token wajah sudah terdaftar.' : 'Token wajah belum terdaftar.');
}

/* ============================================================
 * POST — simpan / hapus token wajah
 * ============================================================ */
wajibMethod('POST');

$data = inputJson();
$action = strtolower(trim((string)(ambil($data, 'action') ?? '')));

if (!in_array($action, ['simpan', 'hapus'], true)) {
    gagal("Aksi tidak valid. Gunakan 'simpan' atau 'hapus'.", 'INVALID_ACTION', 400);
}

if ($action === 'hapus') {
    try {
        $sql = 'UPDATE tbl_sales SET face_data = NULL';
        if ($adaDiperbarui) {
            $sql .= ', face_updated_at = NULL';
        }
        $sql .= ' WHERE id_sales = ?';
        $pdo->prepare($sql)->execute([$idSales]);
    } catch (PDOException $e) {
        error_log('[wajah:hapus] ' . $e->getMessage());
        gagal('Token wajah gagal dihapus.', 'WAJAH_HAPUS_GAGAL', 500);
    }

    sukses(['terdaftar' => false], 'Token wajah dihapus.');
}

$faceData = trim((string)(ambil($data, 'face_data') ?? ''));

if ($faceData === '') {
    gagal('face_data wajib diisi.', 'FACE_DATA_KOSONG', 400);
}

// Token = 128 float32 little-endian -> 512 byte -> 684 karakter Base64.
const PANJANG_TOKEN_BYTE = 512;
const PANJANG_TOKEN_TEKS = 684;

if (strlen($faceData) !== PANJANG_TOKEN_TEKS) {
    gagal(
        'Panjang token wajah tidak sesuai (harus ' . PANJANG_TOKEN_TEKS . ' karakter).',
        'FACE_DATA_PANJANG_SALAH',
        400
    );
}

if (!preg_match('/^[A-Za-z0-9+\/]+={0,2}$/', $faceData)) {
    gagal('Token wajah bukan Base64 yang valid.', 'FACE_DATA_BUKAN_BASE64', 400);
}

$biner = base64_decode($faceData, true);
if ($biner === false || strlen($biner) !== PANJANG_TOKEN_BYTE) {
    gagal(
        'Isi token wajah harus ' . PANJANG_TOKEN_BYTE . ' byte (128 nilai float32).',
        'FACE_DATA_ISI_SALAH',
        400
    );
}

try {
    $sql = 'UPDATE tbl_sales SET face_data = ?';
    $nilai = [$faceData];

    if ($adaDiperbarui) {
        $sql .= ', face_updated_at = NOW()';
    }
    $sql .= ' WHERE id_sales = ?';
    $nilai[] = $idSales;

    $pdo->prepare($sql)->execute($nilai);
} catch (PDOException $e) {
    error_log('[wajah:simpan] ' . $e->getMessage());
    gagal('Token wajah gagal disimpan.', 'WAJAH_SIMPAN_GAGAL', 500);
}

$stmt = $pdo->prepare(
    "SELECT {$selectDiperbarui} FROM tbl_sales WHERE id_sales = ? LIMIT 1"
);
$stmt->execute([$idSales]);
$baru = $stmt->fetch();

sukses([
    'terdaftar'  => true,
    'diperbarui' => $baru['face_updated_at'] ?? null,
], 'Token wajah berhasil disimpan.');
