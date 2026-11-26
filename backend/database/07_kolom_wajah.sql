-- ============================================================
-- 07 — kolom pendukung token wajah
--
-- tbl_sales.face_data sudah ada di 01_schema.sql (TEXT, menyimpan
-- token Base64 hasil model SFace, bukan foto). File ini menambah
-- waktu pembaruan token agar HR/audit bisa melihat kapan wajah
-- didaftarkan ulang.
--
-- Jalankan sekali di phpMyAdmin setelah 01_schema.sql.
-- ============================================================

ALTER TABLE tbl_sales
    ADD COLUMN IF NOT EXISTS face_updated_at DATETIME NULL AFTER face_data;
