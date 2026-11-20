-- =================================================================================
-- CSDL HỆ THỐNG QUẢN LÝ HENRY HOMESTAY - BẢN v5 (CHUẨN 3NF - MICROSOFT SQL SERVER)
-- Đồ án môn: Ngôn ngữ lập trình C# (ASP.NET Core MVC + Windows Forms + EF Core)
-- =================================================================================

USE master;
GO

IF DB_ID(N'HenryHomestayDB') IS NULL
BEGIN
    CREATE DATABASE HenryHomestayDB;
END
GO

USE HenryHomestayDB;
GO

-- Xóa các bảng theo thứ tự khóa ngoại nếu cần tạo lại từ đầu
IF OBJECT_ID(N'dbo.contact_inquiries', N'U') IS NOT NULL DROP TABLE dbo.contact_inquiries;
IF OBJECT_ID(N'dbo.system_settings', N'U')   IS NOT NULL DROP TABLE dbo.system_settings;
IF OBJECT_ID(N'dbo.payments', N'U')          IS NOT NULL DROP TABLE dbo.payments;
IF OBJECT_ID(N'dbo.room_nights', N'U')       IS NOT NULL DROP TABLE dbo.room_nights;
IF OBJECT_ID(N'dbo.booking_rooms', N'U')     IS NOT NULL DROP TABLE dbo.booking_rooms;
IF OBJECT_ID(N'dbo.bookings', N'U')          IS NOT NULL DROP TABLE dbo.bookings;
IF OBJECT_ID(N'dbo.saved_rooms', N'U')       IS NOT NULL DROP TABLE dbo.saved_rooms;
IF OBJECT_ID(N'dbo.room_images', N'U')       IS NOT NULL DROP TABLE dbo.room_images;
IF OBJECT_ID(N'dbo.room_amenities', N'U')    IS NOT NULL DROP TABLE dbo.room_amenities;
IF OBJECT_ID(N'dbo.amenities', N'U')         IS NOT NULL DROP TABLE dbo.amenities;
IF OBJECT_ID(N'dbo.rooms', N'U')             IS NOT NULL DROP TABLE dbo.rooms;
IF OBJECT_ID(N'dbo.room_types', N'U')        IS NOT NULL DROP TABLE dbo.room_types;
IF OBJECT_ID(N'dbo.users', N'U')             IS NOT NULL DROP TABLE dbo.users;
GO

-- =================================================================================
-- 1. NGƯỜI DÙNG (users): Khách hàng (CUSTOMER), Nhân viên (STAFF), Quản trị (ADMIN)
-- =================================================================================
CREATE TABLE dbo.users (
    id             BIGINT        IDENTITY(1,1) NOT NULL,
    full_name      NVARCHAR(100) NOT NULL,
    email          VARCHAR(150)  NOT NULL,
    phone          VARCHAR(20)   NOT NULL,
    cccd           VARCHAR(20)   NULL,
    date_of_birth  DATE          NULL,                  -- Bổ sung từ trang Hồ sơ (profile.html)
    avatar_url     NVARCHAR(500) NULL,                  -- Bổ sung ảnh đại diện từ trang Hồ sơ
    password_hash  VARCHAR(255)  NOT NULL,
    role           VARCHAR(20)   NOT NULL CONSTRAINT df_users_role DEFAULT ('CUSTOMER'),
    staff_position NVARCHAR(50)  NULL,                  -- Bổ sung chức vụ nhân viên (Quản lý, Lễ tân, Kế toán...)
    hire_date      DATE          NULL,                  -- Bổ sung ngày vào làm của nhân viên (admin.js)
    is_locked      BIT           NOT NULL CONSTRAINT df_users_locked DEFAULT (0),
    created_at     DATETIME2(0)  NOT NULL CONSTRAINT df_users_created DEFAULT (SYSDATETIME()),
    updated_at     DATETIME2(0)  NOT NULL CONSTRAINT df_users_updated DEFAULT (SYSDATETIME()),

    CONSTRAINT pk_users       PRIMARY KEY (id),
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT uq_users_phone UNIQUE (phone),
    CONSTRAINT ck_users_role  CHECK (role IN ('CUSTOMER', 'STAFF', 'ADMIN'))
);
CREATE INDEX idx_users_role ON dbo.users(role);
GO

-- =================================================================================
-- 2. LOẠI PHÒNG (room_types): Tách bảng danh mục đạt chuẩn 3NF
-- =================================================================================
CREATE TABLE dbo.room_types (
    id          INT           IDENTITY(1,1) NOT NULL,
    code        VARCHAR(30)   NOT NULL,                 -- 'double', 'studio', 'family'
    name        NVARCHAR(80)  NOT NULL,                 -- N'Phòng đôi', N'Studio', N'Phòng gia đình'
    description NVARCHAR(255) NULL,

    CONSTRAINT pk_room_types      PRIMARY KEY (id),
    CONSTRAINT uq_room_types_code UNIQUE (code),
    CONSTRAINT uq_room_types_name UNIQUE (name)
);
GO

-- =================================================================================
-- 3. PHÒNG (rooms): Thông tin chi tiết từng phòng
-- =================================================================================
CREATE TABLE dbo.rooms (
    id               INT           IDENTITY(1,1) NOT NULL,
    room_code        VARCHAR(20)   NOT NULL,            -- Mã phòng hiển thị: '41', '42', '51', '52', '71', '81'
    name             NVARCHAR(100) NOT NULL,
    room_type_id     INT           NOT NULL,
    area_m2          INT           NOT NULL CONSTRAINT df_rooms_area DEFAULT (35), -- Diện tích (m²)
    bed_info         NVARCHAR(100) NULL,                -- VD: N'1 giường Queen', N'2 giường Queen'
    view_description NVARCHAR(150) NULL,                -- VD: N'Ban công đón nắng', N'Cửa sổ nhìn thành phố'
    description      NVARCHAR(MAX) NULL,
    max_guests       TINYINT       NOT NULL,
    allow_pets       BIT           NOT NULL CONSTRAINT df_rooms_pets DEFAULT (0),
    price_per_night  DECIMAL(12,0) NOT NULL,
    status           VARCHAR(20)   NOT NULL CONSTRAINT df_rooms_status DEFAULT ('ACTIVE'),
    created_at       DATETIME2(0)  NOT NULL CONSTRAINT df_rooms_created DEFAULT (SYSDATETIME()),
    updated_at       DATETIME2(0)  NOT NULL CONSTRAINT df_rooms_updated DEFAULT (SYSDATETIME()),

    CONSTRAINT pk_rooms        PRIMARY KEY (id),
    CONSTRAINT uq_rooms_code   UNIQUE (room_code),
    CONSTRAINT uq_rooms_name   UNIQUE (name),
    CONSTRAINT fk_rooms_type   FOREIGN KEY (room_type_id) REFERENCES dbo.room_types(id),
    CONSTRAINT ck_rooms_price  CHECK (price_per_night >= 0),
    CONSTRAINT ck_rooms_guests CHECK (max_guests > 0),
    CONSTRAINT ck_rooms_area   CHECK (area_m2 > 0),
    CONSTRAINT ck_rooms_status CHECK (status IN ('ACTIVE', 'HIDDEN'))
);
CREATE INDEX idx_rooms_search ON dbo.rooms(status, room_type_id, max_guests, allow_pets, price_per_night);
GO

-- =================================================================================
-- 4. TIỆN ÍCH (amenities): Danh mục tiện ích phòng (Chuẩn 3NF)
-- =================================================================================
CREATE TABLE dbo.amenities (
    id   INT          IDENTITY(1,1) NOT NULL,
    code VARCHAR(30)  NOT NULL,                         -- 'wifi', 'aircon', 'balcony', 'kitchen'
    name NVARCHAR(80) NOT NULL,                         -- N'Wi-Fi', N'Điều hòa', N'Ban công riêng', N'Bếp nhỏ'
    icon VARCHAR(50)  NULL,

    CONSTRAINT pk_amenities      PRIMARY KEY (id),
    CONSTRAINT uq_amenities_code UNIQUE (code),
    CONSTRAINT uq_amenities_name UNIQUE (name)
);
GO

-- =================================================================================
-- 5. TIỆN ÍCH CỦA PHÒNG (room_amenities): Quan hệ N-N giữa Phòng và Tiện ích
-- =================================================================================
CREATE TABLE dbo.room_amenities (
    room_id    INT NOT NULL,
    amenity_id INT NOT NULL,

    CONSTRAINT pk_room_amenities PRIMARY KEY (room_id, amenity_id),
    CONSTRAINT fk_ra_room        FOREIGN KEY (room_id)    REFERENCES dbo.rooms(id) ON DELETE CASCADE,
    CONSTRAINT fk_ra_amenity     FOREIGN KEY (amenity_id) REFERENCES dbo.amenities(id) ON DELETE CASCADE
);
GO

-- =================================================================================
-- 6. HÌNH ẢNH PHÒNG (room_images): Tách đa trị từ mảng JSON ảnh (Chuẩn 1NF/3NF)
-- =================================================================================
CREATE TABLE dbo.room_images (
    id            INT           IDENTITY(1,1) NOT NULL,
    room_id       INT           NOT NULL,
    image_url     NVARCHAR(500) NOT NULL,
    is_primary    BIT           NOT NULL CONSTRAINT df_ri_primary DEFAULT (0),
    display_order INT           NOT NULL CONSTRAINT df_ri_order DEFAULT (0),

    CONSTRAINT pk_room_images PRIMARY KEY (id),
    CONSTRAINT fk_ri_room     FOREIGN KEY (room_id) REFERENCES dbo.rooms(id) ON DELETE CASCADE
);
CREATE INDEX idx_ri_room ON dbo.room_images(room_id, is_primary, display_order);
GO

-- =================================================================================
-- 7. PHÒNG ĐÃ LƯU / YÊU THÍCH (saved_rooms): Đồng bộ mục "Phòng đã lưu" trên Web
-- =================================================================================
CREATE TABLE dbo.saved_rooms (
    user_id    BIGINT       NOT NULL,
    room_id    INT          NOT NULL,
    created_at DATETIME2(0) NOT NULL CONSTRAINT df_saved_created DEFAULT (SYSDATETIME()),

    CONSTRAINT pk_saved_rooms PRIMARY KEY (user_id, room_id),
    CONSTRAINT fk_sr_user     FOREIGN KEY (user_id) REFERENCES dbo.users(id) ON DELETE CASCADE,
    CONSTRAINT fk_sr_room     FOREIGN KEY (room_id) REFERENCES dbo.rooms(id) ON DELETE CASCADE
);
GO

-- =================================================================================
-- 8. ĐƠN ĐẶT PHÒNG (bookings): 1 đơn có thể gồm 1 hoặc nhiều phòng (booking_rooms)
-- =================================================================================
CREATE TABLE dbo.bookings (
    id               BIGINT        IDENTITY(1,1) NOT NULL,
    booking_code     VARCHAR(20)   NOT NULL,
    user_id          BIGINT        NOT NULL,

    guest_name       NVARCHAR(100) NOT NULL,
    guest_phone      VARCHAR(20)   NOT NULL,
    guest_email      VARCHAR(150)  NOT NULL,
    guest_cccd       VARCHAR(20)   NULL,
    guest_dob        DATE          NULL,                -- Bổ sung ngày sinh khách lưu trú (booking-detail.html)

    check_in_date    DATE          NOT NULL,
    check_out_date   DATE          NOT NULL,
    nights           AS (DATEDIFF(DAY, check_in_date, check_out_date)) PERSISTED,

    adults           TINYINT       NOT NULL CONSTRAINT df_bk_adults   DEFAULT (1),
    children         TINYINT       NOT NULL CONSTRAINT df_bk_children DEFAULT (0),
    infants          TINYINT       NOT NULL CONSTRAINT df_bk_infants  DEFAULT (0),
    pets             TINYINT       NOT NULL CONSTRAINT df_bk_pets     DEFAULT (0),

    total_amount     DECIMAL(12,0) NOT NULL,
    payment_option   VARCHAR(20)   NOT NULL,            -- 'FULL', 'DEPOSIT'
    deposit_percent  TINYINT       NOT NULL CONSTRAINT df_bk_dep_pct DEFAULT (30),
    deposit_amount   DECIMAL(12,0) NOT NULL CONSTRAINT df_bk_dep_amt DEFAULT (0),
    paid_amount      DECIMAL(12,0) NOT NULL CONSTRAINT df_bk_paid    DEFAULT (0),
    remaining_amount AS (total_amount - paid_amount),

    status           VARCHAR(25)   NOT NULL CONSTRAINT df_bk_status DEFAULT ('PENDING_PAYMENT'),
    expires_at       DATETIME2(0)  NULL,

    cancelled_at     DATETIME2(0)  NULL,
    cancelled_by     BIGINT        NULL,
    cancel_reason    NVARCHAR(255) NULL,
    refund_status    VARCHAR(25)   NOT NULL CONSTRAINT df_bk_refund DEFAULT ('NONE'),

    created_at       DATETIME2(0)  NOT NULL CONSTRAINT df_bk_created DEFAULT (SYSDATETIME()),
    updated_at       DATETIME2(0)  NOT NULL CONSTRAINT df_bk_updated DEFAULT (SYSDATETIME()),

    CONSTRAINT pk_bookings        PRIMARY KEY (id),
    CONSTRAINT uq_bookings_code   UNIQUE (booking_code),
    CONSTRAINT fk_bk_user         FOREIGN KEY (user_id)      REFERENCES dbo.users(id),
    CONSTRAINT fk_bk_cancel       FOREIGN KEY (cancelled_by) REFERENCES dbo.users(id),
    CONSTRAINT ck_bk_dates        CHECK (check_out_date > check_in_date),
    CONSTRAINT ck_bk_paid         CHECK (paid_amount >= 0),
    CONSTRAINT ck_bk_pay_option   CHECK (payment_option IN ('FULL', 'DEPOSIT')),
    CONSTRAINT ck_bk_status       CHECK (status IN ('PENDING_PAYMENT', 'CONFIRMED', 'CHECKED_IN', 'COMPLETED', 'CANCELLED')),
    CONSTRAINT ck_bk_refund       CHECK (refund_status IN ('NONE', 'WAITING_REFUND', 'REFUNDED', 'NO_REFUND'))
);
CREATE INDEX idx_bk_user   ON dbo.bookings(user_id, created_at);
CREATE INDEX idx_bk_filter ON dbo.bookings(status, check_in_date);
CREATE INDEX idx_bk_phone  ON dbo.bookings(guest_phone);
CREATE INDEX idx_bk_expire ON dbo.bookings(status, expires_at);
GO

-- =================================================================================
-- 9. PHÒNG TRONG ĐƠN (booking_rooms): Lưu giá chốt tại thời điểm đặt
-- =================================================================================
CREATE TABLE dbo.booking_rooms (
    booking_id      BIGINT        NOT NULL,
    room_id         INT           NOT NULL,
    price_per_night DECIMAL(12,0) NOT NULL,

    CONSTRAINT pk_booking_rooms PRIMARY KEY (booking_id, room_id),
    CONSTRAINT fk_br_booking    FOREIGN KEY (booking_id) REFERENCES dbo.bookings(id) ON DELETE CASCADE,
    CONSTRAINT fk_br_room       FOREIGN KEY (room_id)    REFERENCES dbo.rooms(id)
);
CREATE INDEX idx_br_room ON dbo.booking_rooms(room_id);
GO

-- =================================================================================
-- 10. LỊCH PHÒNG THEO ĐÊM (room_nights): Chống trùng lịch ở mức CSDL
-- =================================================================================
CREATE TABLE dbo.room_nights (
    room_id    INT    NOT NULL,
    stay_date  DATE   NOT NULL,
    booking_id BIGINT NOT NULL,

    CONSTRAINT pk_room_nights PRIMARY KEY (room_id, stay_date),
    CONSTRAINT fk_rn_room     FOREIGN KEY (room_id)    REFERENCES dbo.rooms(id),
    CONSTRAINT fk_rn_booking  FOREIGN KEY (booking_id) REFERENCES dbo.bookings(id) ON DELETE CASCADE
);
CREATE INDEX idx_rn_booking ON dbo.room_nights(booking_id);
GO

-- =================================================================================
-- 11. GIAO DỊCH THANH TOÁN (payments)
-- =================================================================================
CREATE TABLE dbo.payments (
    id           BIGINT        IDENTITY(1,1) NOT NULL,
    payment_code VARCHAR(20)   NULL,                    -- Bổ sung mã giao dịch hiển thị (VD: 'GD-1', 'GD-2')
    booking_id   BIGINT        NOT NULL,
    amount       DECIMAL(12,0) NOT NULL,
    type         VARCHAR(20)   NOT NULL,                -- 'DEPOSIT', 'FULL', 'BALANCE', 'REFUND'
    method       VARCHAR(25)   NOT NULL,                -- 'ONLINE', 'BANK_TRANSFER', 'CASH'
    status       VARCHAR(20)   NOT NULL CONSTRAINT df_pay_status DEFAULT ('PENDING'),
    gateway_ref  VARCHAR(100)  NULL,
    note         NVARCHAR(255) NULL,
    confirmed_by BIGINT        NULL,
    confirmed_at DATETIME2(0)  NULL,
    created_at   DATETIME2(0)  NOT NULL CONSTRAINT df_pay_created DEFAULT (SYSDATETIME()),

    CONSTRAINT pk_payments      PRIMARY KEY (id),
    CONSTRAINT fk_pay_booking   FOREIGN KEY (booking_id)   REFERENCES dbo.bookings(id),
    CONSTRAINT fk_pay_staff     FOREIGN KEY (confirmed_by) REFERENCES dbo.users(id),
    CONSTRAINT ck_pay_amount    CHECK (amount > 0),
    CONSTRAINT ck_pay_type      CHECK (type IN ('DEPOSIT', 'FULL', 'BALANCE', 'REFUND')),
    CONSTRAINT ck_pay_method    CHECK (method IN ('ONLINE', 'BANK_TRANSFER', 'CASH')),
    CONSTRAINT ck_pay_status    CHECK (status IN ('PENDING', 'CONFIRMED', 'FAILED', 'CANCELLED'))
);
CREATE INDEX idx_pay_booking ON dbo.payments(booking_id);
CREATE INDEX idx_pay_pending ON dbo.payments(status, created_at);
CREATE INDEX idx_pay_time    ON dbo.payments(created_at);
GO

-- =================================================================================
-- 12. CẤU HÌNH HỆ THỐNG (system_settings): Chỉ 1 dòng duy nhất id = 1
-- =================================================================================
CREATE TABLE dbo.system_settings (
    id                  TINYINT       NOT NULL CONSTRAINT df_ss_id DEFAULT (1),
    qr_code_url         NVARCHAR(500) NULL,
    bank_name           NVARCHAR(100) NOT NULL,         -- Bổ sung Tên ngân hàng (VD: N'MB Bank')
    bank_account_number VARCHAR(30)   NOT NULL,
    bank_account_holder NVARCHAR(100) NOT NULL,
    contact_phone       VARCHAR(20)   NOT NULL,
    deposit_percent     TINYINT       NOT NULL CONSTRAINT df_ss_dep  DEFAULT (30),
    change_cancel_hours SMALLINT      NOT NULL CONSTRAINT df_ss_hrs  DEFAULT (24),
    hold_minutes        SMALLINT      NOT NULL CONSTRAINT df_ss_hold DEFAULT (30),
    updated_by          BIGINT        NULL,
    updated_at          DATETIME2(0)  NOT NULL CONSTRAINT df_ss_upd  DEFAULT (SYSDATETIME()),

    CONSTRAINT pk_system_settings PRIMARY KEY (id),
    CONSTRAINT ck_ss_single       CHECK (id = 1),
    CONSTRAINT ck_ss_percent      CHECK (deposit_percent BETWEEN 1 AND 100),
    CONSTRAINT fk_ss_user         FOREIGN KEY (updated_by) REFERENCES dbo.users(id)
);
GO

-- =================================================================================
-- 13. LỜI NHẮN LIÊN HỆ (contact_inquiries): Bổ sung cho trang contact.html
-- =================================================================================
CREATE TABLE dbo.contact_inquiries (
    id                 BIGINT         IDENTITY(1,1) NOT NULL,
    user_id            BIGINT         NULL,
    full_name          NVARCHAR(100)  NOT NULL,
    phone              VARCHAR(20)    NOT NULL,
    email              VARCHAR(150)   NOT NULL,
    service_type       VARCHAR(50)    NULL,             -- 'room', 'whole-house', 'photoshoot', 'long-stay', 'other'
    expected_stay_date DATE           NULL,
    message            NVARCHAR(1000) NULL,
    status             VARCHAR(20)    NOT NULL CONSTRAINT df_ci_status DEFAULT ('NEW'),
    created_at         DATETIME2(0)   NOT NULL CONSTRAINT df_ci_created DEFAULT (SYSDATETIME()),

    CONSTRAINT pk_contact_inquiries PRIMARY KEY (id),
    CONSTRAINT fk_ci_user           FOREIGN KEY (user_id) REFERENCES dbo.users(id),
    CONSTRAINT ck_ci_status         CHECK (status IN ('NEW', 'PROCESSED'))
);
GO

-- =================================================================================
-- DỮ LIỆU MẪU (SEED DATA) ĐỒNG BỘ VỚI HENRY HOMESTAY (data.js & admin.js)
-- =================================================================================

-- 1. Cấu hình hệ thống mặc định
INSERT INTO dbo.system_settings (id, qr_code_url, bank_name, bank_account_number, bank_account_holder, contact_phone, deposit_percent, change_cancel_hours, hold_minutes)
VALUES (1, N'assets/images/payment-qr.svg', N'MB Bank', '0708670042', N'Võ Văn Việt', '0795443500', 30, 24, 30);

-- 2. Người dùng (Mật khẩu mẫu: '123456')
INSERT INTO dbo.users (full_name, email, phone, cccd, date_of_birth, password_hash, role, staff_position, hire_date, is_locked)
VALUES
-- Tài khoản Quản trị & Nhân viên (khớp admin.js)
(N'Nguyễn Văn An',   'admin@henry.example',     '0901234567', '079090000001', '1990-05-15', '123456', 'ADMIN', N'Quản lý',     DATEADD(DAY, -400, CAST(GETDATE() AS DATE)), 0),
(N'Lê Thị Bích',     'bich.le@henry.example',   '0902345678', '079092000002', '1995-08-20', '123456', 'STAFF', N'Lễ tân',      DATEADD(DAY, -210, CAST(GETDATE() AS DATE)), 0),
(N'Trần Quốc Cường', 'cuong.tran@henry.example','0903456789', '079093000003', '1993-11-10', '123456', 'STAFF', N'Kế toán',     DATEADD(DAY, -150, CAST(GETDATE() AS DATE)), 0),
(N'Phạm Thu Hà',     'ha.pham@henry.example',   '0904567890', '079096000004', '1998-02-25', '123456', 'STAFF', N'Buồng phòng', DATEADD(DAY, -60,  CAST(GETDATE() AS DATE)), 1),
-- Tài khoản Khách hàng (khớp admin.js & profile.html)
(N'Nguyễn Quốc Huy', 'quochuy@example.com',     '0912345678', '079201001234', '2001-03-12', '123456', 'CUSTOMER', NULL, NULL, 0),
(N'Trần Thị Mai',    'maitran@example.com',     '0987654321', '079195004567', '1995-07-19', '123456', 'CUSTOMER', NULL, NULL, 0),
(N'Lê Hoàng Nam',    'namle@example.com',       '0903111222', '079198007890', '1998-10-05', '123456', 'CUSTOMER', NULL, NULL, 0),
(N'Phạm Minh Anh',   'minhanh@example.com',     '0938222333', '079200003210', '2000-01-22', '123456', 'CUSTOMER', NULL, NULL, 0),
(N'Đỗ Thùy Linh',    'thuylinh@example.com',    '0977888999', '079197006543', '1997-12-30', '123456', 'CUSTOMER', NULL, NULL, 1);

-- 3. Loại phòng (room_types)
INSERT INTO dbo.room_types (code, name, description)
VALUES
('double', N'Phòng đôi',      N'Phòng đôi tiêu chuẩn với tầm nhìn thoáng đãng, phù hợp cho cặp đôi hoặc nhóm bạn nhỏ.'),
('studio', N'Studio',         N'Không gian khép kín ấm cúng có gian bếp nhỏ tiện nghi.'),
('family', N'Phòng gia đình', N'Không gian rộng rãi với 2 giường lớn dành cho cả gia đình.');

-- 4. Tiện ích (amenities)
INSERT INTO dbo.amenities (code, name, icon)
VALUES
('wifi',    N'Wi-Fi',          'wifi'),
('aircon',  N'Điều hòa',       'aircon'),
('balcony', N'Ban công riêng', 'balcony'),
('kitchen', N'Bếp nhỏ',        'kitchen');

-- 5. Danh sách 6 Phòng (rooms - khớp HenryData.rooms trong data.js & admin.js)
INSERT INTO dbo.rooms (room_code, name, room_type_id, area_m2, bed_info, view_description, description, max_guests, allow_pets, price_per_night, status)
VALUES
('41', N'Vista View 41',   1, 35, N'1 giường Queen', N'Cửa sổ nhìn thành phố',   N'Cửa sổ nhìn thành phố. 1 giường Queen, 35 m².', 4, 0, 1099000, 'ACTIVE'),
('42', N'Vista View 42',   1, 40, N'1 giường Queen', N'Ban công đón nắng',       N'Ban công đón nắng. 1 giường Queen, 40 m².',     4, 1, 1190000, 'ACTIVE'),
('51', N'Vista View 51',   1, 35, N'1 giường Queen', N'Thành phố về đêm',        N'Thành phố về đêm. 1 giường Queen, 35 m².',      4, 0, 1150000, 'ACTIVE'),
('52', N'Vista View 52',   1, 40, N'1 giường Queen', N'Ban công ngắm hoàng hôn', N'Ban công ngắm hoàng hôn. 1 giường Queen, 40 m².',4, 1, 1299000, 'ACTIVE'),
('71', N'Studio 71',       2, 40, N'1 giường King',  N'Góc nhỏ bình yên',        N'Góc nhỏ bình yên. 1 giường King, 40 m².',       2, 1, 1399000, 'ACTIVE'),
('81', N'Family Suite 81', 3, 69, N'2 giường Queen', N'Không gian cho cả nhà',   N'Không gian cho cả nhà. 2 giường Queen, 69 m².', 6, 0, 2099000, 'ACTIVE');

-- 6. Gán Tiện ích cho từng Phòng (room_amenities)
-- Phòng 41 (id=1): wifi, aircon
INSERT INTO dbo.room_amenities (room_id, amenity_id) VALUES (1, 1), (1, 2);
-- Phòng 42 (id=2): wifi, aircon, balcony
INSERT INTO dbo.room_amenities (room_id, amenity_id) VALUES (2, 1), (2, 2), (2, 3);
-- Phòng 51 (id=3): wifi, aircon
INSERT INTO dbo.room_amenities (room_id, amenity_id) VALUES (3, 1), (3, 2);
-- Phòng 52 (id=4): wifi, aircon, balcony
INSERT INTO dbo.room_amenities (room_id, amenity_id) VALUES (4, 1), (4, 2), (4, 3);
-- Phòng 71 (id=5): wifi, aircon, kitchen
INSERT INTO dbo.room_amenities (room_id, amenity_id) VALUES (5, 1), (5, 2), (5, 4);
-- Phòng 81 (id=6): wifi, aircon, balcony, kitchen
INSERT INTO dbo.room_amenities (room_id, amenity_id) VALUES (6, 1), (6, 2), (6, 3), (6, 4);

-- 7. Hình ảnh Phòng (room_images)
INSERT INTO dbo.room_images (room_id, image_url, is_primary, display_order)
VALUES
(1, N'photo-02.jpg', 1, 1),
(2, N'photo-03.jpg', 1, 1),
(3, N'photo-04.jpg', 1, 1),
(4, N'photo-05.jpg', 1, 1),
(5, N'photo-08.jpg', 1, 1),
(6, N'photo-07.jpg', 1, 1);

-- 8. Đơn đặt phòng mẫu (bookings - đồng bộ với admin.js)
DECLARE @today DATE = CAST(GETDATE() AS DATE);

INSERT INTO dbo.bookings (
    booking_code, user_id, guest_name, guest_phone, guest_email, guest_cccd,
    check_in_date, check_out_date, adults, children, infants, pets,
    total_amount, payment_option, deposit_percent, deposit_amount, paid_amount,
    status, refund_status, cancelled_at, created_at
)
VALUES
('HN-270001', 5, N'Nguyễn Quốc Huy', '0912345678', 'quochuy@example.com', '079201001234',
 DATEADD(DAY, 1, @today), DATEADD(DAY, 3, @today), 2, 0, 0, 0,
 2380000, 'DEPOSIT', 30, 714000, 0, 'PENDING_PAYMENT', 'NONE', NULL, SYSDATETIME()),

('HN-270002', 6, N'Trần Thị Mai', '0987654321', 'maitran@example.com', '079195004567',
 DATEADD(DAY, -2, @today), DATEADD(DAY, 1, @today), 2, 1, 0, 0,
 3297000, 'FULL', 30, 0, 3297000, 'CHECKED_IN', 'NONE', NULL, DATEADD(DAY, -6, SYSDATETIME())),

('HN-270003', 7, N'Lê Hoàng Nam', '0903111222', 'namle@example.com', '079198007890',
 DATEADD(DAY, 5, @today), DATEADD(DAY, 7, @today), 2, 0, 1, 1,
 2598000, 'DEPOSIT', 30, 779400, 779400, 'CONFIRMED', 'NONE', NULL, DATEADD(DAY, -3, SYSDATETIME())),

('HN-270004', 8, N'Phạm Minh Anh', '0938222333', 'minhanh@example.com', '079200003210',
 DATEADD(DAY, -8, @today), DATEADD(DAY, -6, @today), 2, 0, 0, 0,
 2798000, 'FULL', 30, 0, 2798000, 'COMPLETED', 'NONE', NULL, DATEADD(DAY, -14, SYSDATETIME())),

('HN-270005', 5, N'Nguyễn Quốc Huy', '0912345678', 'quochuy@example.com', '079201001234',
 DATEADD(DAY, 9, @today), DATEADD(DAY, 12, @today), 4, 2, 0, 0,
 6297000, 'DEPOSIT', 30, 1889100, 1889100, 'CANCELLED', 'WAITING_REFUND', SYSDATETIME(), DATEADD(DAY, -4, SYSDATETIME())),

('HN-270006', 9, N'Đỗ Thùy Linh', '0977888999', 'thuylinh@example.com', '079197006543',
 DATEADD(DAY, 2, @today), DATEADD(DAY, 4, @today), 2, 0, 0, 0,
 2300000, 'FULL', 30, 0, 0, 'PENDING_PAYMENT', 'NONE', NULL, SYSDATETIME()),

('HN-270007', 6, N'Trần Thị Mai', '0987654321', 'maitran@example.com', '079195004567',
 DATEADD(DAY, 12, @today), DATEADD(DAY, 14, @today), 2, 0, 0, 0,
 2380000, 'FULL', 30, 0, 2380000, 'CONFIRMED', 'NONE', NULL, DATEADD(DAY, -1, SYSDATETIME()));

-- 9. Chi tiết phòng trong đơn (booking_rooms)
INSERT INTO dbo.booking_rooms (booking_id, room_id, price_per_night)
VALUES
(1, 2, 1190000), -- HN-270001 -> Phòng 42
(2, 1, 1099000), -- HN-270002 -> Phòng 41
(3, 4, 1299000), -- HN-270003 -> Phòng 52
(4, 5, 1399000), -- HN-270004 -> Phòng 71
(5, 6, 2099000), -- HN-270005 -> Phòng 81
(6, 3, 1150000), -- HN-270006 -> Phòng 51
(7, 2, 1190000); -- HN-270007 -> Phòng 42

-- 10. Lịch giữ phòng theo đêm (room_nights - các đơn chưa hủy)
INSERT INTO dbo.room_nights (room_id, stay_date, booking_id)
VALUES
(2, DATEADD(DAY, 1, @today), 1), (2, DATEADD(DAY, 2, @today), 1),
(1, DATEADD(DAY, -2, @today), 2), (1, DATEADD(DAY, -1, @today), 2), (1, @today, 2),
(4, DATEADD(DAY, 5, @today), 3), (4, DATEADD(DAY, 6, @today), 3),
(5, DATEADD(DAY, -8, @today), 4), (5, DATEADD(DAY, -7, @today), 4),
(3, DATEADD(DAY, 2, @today), 6), (3, DATEADD(DAY, 3, @today), 6),
(2, DATEADD(DAY, 12, @today), 7), (2, DATEADD(DAY, 13, @today), 7);

-- 11. Giao dịch thanh toán mẫu (payments)
INSERT INTO dbo.payments (payment_code, booking_id, amount, type, method, status, confirmed_by, confirmed_at, created_at)
VALUES
('GD-1', 1, 714000,  'DEPOSIT', 'BANK_TRANSFER', 'PENDING',   NULL, NULL, SYSDATETIME()),
('GD-2', 2, 3297000, 'FULL',    'BANK_TRANSFER', 'CONFIRMED', 1, DATEADD(DAY, -6, SYSDATETIME()), DATEADD(DAY, -6, SYSDATETIME())),
('GD-3', 3, 779400,  'DEPOSIT', 'BANK_TRANSFER', 'CONFIRMED', 1, DATEADD(DAY, -3, SYSDATETIME()), DATEADD(DAY, -3, SYSDATETIME())),
('GD-4', 4, 2798000, 'FULL',    'BANK_TRANSFER', 'CONFIRMED', 1, DATEADD(DAY, -14, SYSDATETIME()), DATEADD(DAY, -14, SYSDATETIME())),
('GD-5', 5, 1889100, 'DEPOSIT', 'BANK_TRANSFER', 'CONFIRMED', 1, DATEADD(DAY, -4, SYSDATETIME()), DATEADD(DAY, -4, SYSDATETIME())),
('GD-6', 6, 2300000, 'FULL',    'BANK_TRANSFER', 'PENDING',   NULL, NULL, SYSDATETIME()),
('GD-7', 7, 2380000, 'FULL',    'BANK_TRANSFER', 'CONFIRMED', 1, DATEADD(DAY, -1, SYSDATETIME()), DATEADD(DAY, -1, SYSDATETIME()));
GO

