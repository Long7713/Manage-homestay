# HỆ THỐNG QUẢN LÝ & ĐẶT PHÒNG HENRY HOMESTAY
**Đồ án môn: Ngôn ngữ lập trình C#**  
*(Kiến trúc phân lớp 3-Layer + Entity Framework Core + ASP.NET Core MVC + Windows Forms)*

---

## 1. Kiến trúc tổng thể của hệ thống (Đạt chuẩn Rubric mục 5, 6, 9)

Toàn bộ mã nguồn được đặt tại thư mục `D:\Csharp\HenryHomestay` (mở bằng file `HenryHomestay.sln` hoặc `HenryHomestay.slnx` trong Visual Studio), được tách thành **5 Project theo kiến trúc phân lớp**:

1. **`HenryHomestay.Core` (Tầng Thực thể & Mô hình dùng chung)**:
   - `Entities/DomainEntities.cs`: Chứa 13 lớp Entity ánh xạ 1–1 với 13 bảng CSDL chuẩn 3NF (`User`, `RoomType`, `Room`, `Amenity`, `RoomAmenity`, `RoomImage`, `SavedRoom`, `Booking`, `BookingRoom`, `RoomNight`, `Payment`, `SystemSetting`, `ContactInquiry`).
   - `Models/AppDtos.cs`: Chứa các lớp DTO / ViewModel (`RoomDisplayDto`, `BookingSummaryDto`, `PaymentSummaryDto`, `DashboardStatsDto`, `OperationResult<T>`).
2. **`HenryHomestay.DAL` (Tầng Truy cập dữ liệu — Data Access Layer)**:
   - `HenryDbContext.cs`: Lớp `DbContext` của **Entity Framework Core**, định nghĩa khóa chính kép (Composite Primary Keys), khóa ngoại (Foreign Keys) và chỉ mục (Indexes).
   - `DatabaseConfigManager.cs`: Quản lý kết nối CSDL dùng chung (`dbconfig.json`). Tự động phát hiện và kết nối **Microsoft SQL Server (`HenryHomestayDB`)**, đồng thời hỗ trợ chế độ CSDL chia sẻ cục bộ (`HenryHomestayShared.db`) để cả Web và Desktop luôn hoạt động đồng bộ trên mọi máy tính.
   - `DbInitializer.cs`: Tự động khởi tạo bảng và nạp dữ liệu mẫu (6 phòng, 4 tiện ích, 9 tài khoản khách/nhân viên/admin, 7 đơn đặt phòng, lịch đêm giữ phòng và 7 giao dịch thanh toán).
3. **`HenryHomestay.BLL` (Tầng Nghiệp vụ — Business Logic Layer)**:
   - `HomestayServices.cs`: Chứa 6 lớp dịch vụ xử lý toàn bộ quy tắc nghiệp vụ dùng chung cho cả Web và Desktop:
     - `AuthService`: Đăng nhập, phân quyền (`ADMIN` / `STAFF` / `CUSTOMER`), đăng ký khách hàng, cập nhật hồ sơ, đổi mật khẩu.
     - `RoomService`: Tìm kiếm & lọc phòng trống theo khoảng ngày nhận/trả (đối chiếu bảng `room_nights`), thêm/sửa/xóa/ẩn-hiện phòng, lưu phòng yêu thích.
     - `BookingService`: Tạo đơn đặt phòng mới, kiểm tra chống trùng lịch theo từng đêm (`room_nights`), kiểm tra sức chứa khách tối đa và thú cưng, đổi lịch/đổi phòng, hủy đơn và tự động xét hoàn cọc theo quy tắc **24 giờ trước nhận phòng**.
     - `PaymentService`: Quản lý giao dịch thanh toán, xác nhận đã nhận tiền chuyển khoản, xác nhận hoàn tiền đơn hủy.
     - `UserService`: Quản lý khách hàng và nhân viên, khóa/mở khóa tài khoản.
     - `SystemService`: Quản lý cấu hình ngân hàng / VietQR / tỷ lệ đặt cọc, nhận lời nhắn liên hệ và thống kê Dashboard.
4. **`HenryHomestay.Web` (Ứng dụng Web Người dùng — ASP.NET Core MVC)**:
   - Tích hợp 100% giao diện HTML/CSS/JS từ `henry-homestay`.
   - `Controllers/HomestayWebController.cs`: Phục vụ các trang giao diện, sinh động `/assets/js/data.js` trực tiếp từ CSDL và cung cấp các API nghiệp vụ (`/api/auth/*`, `/api/bookings/*`, `/api/rooms/*`, `/api/profile/*`, `/api/contact/*`).
   - `wwwroot/assets/js/henry-live.js`: Kết nối trực tiếp các biểu mẫu trên Web với C# Controller & CSDL.
5. **`HenryHomestay.Admin` (Ứng dụng Quản trị Desktop — Windows Forms)**:
   - `LoginForm.cs`: Màn hình đăng nhập có kiểm tra quyền `ADMIN`/`STAFF`, chọn nhanh tài khoản Demo và hộp thoại cấu hình kết nối **Microsoft SQL Server**.
   - `MainAdminForm.cs` & `AdminDialogs.cs`: Giao diện quản trị đầy đủ 7 phân hệ (Tổng quan & Xuất báo cáo CSV, Quản lý phòng + Lịch đặt phòng theo tháng, Quản lý đơn đặt phòng, Quản lý thanh toán, Quản lý khách hàng, Quản lý nhân viên, Cấu hình hệ thống).

---

## 2. Cách chạy nhanh hệ thống để kiểm tra & Demo

Bạn có thể mở `D:\Csharp\HenryHomestay\HenryHomestay.sln` trong Visual Studio, hoặc bấm đúp trực tiếp vào 2 file chạy nhanh đã tạo sẵn tại `D:\Csharp\HenryHomestay`:

1. **Chạy Web Người dùng**: Bấm đúp `1-Chay-Web-NguoiDung.bat` $\rightarrow$ trình duyệt mở tại `http://localhost:5188`.
2. **Chạy Ứng dụng Quản trị Desktop**: Bấm đúp `2-Chay-App-QuanTri-Desktop.bat`.

### Tài khoản đăng nhập có sẵn trong CSDL:
| Vai trò | Email đăng nhập | Mật khẩu | Quyền hạn |
| :--- | :--- | :--- | :--- |
| **Quản lý (ADMIN)** | `admin@henry.example` | `123456` | Toàn quyền trên ứng dụng Desktop (bao gồm Quản lý nhân viên & Cấu hình hệ thống) |
| **Lễ tân (STAFF)** | `bich.le@henry.example` | `123456` | Đăng nhập Desktop: Quản lý phòng, Đơn đặt phòng, Thanh toán, Khách hàng (khóa quyền Nhân viên & Cấu hình) |
| **Kế toán (STAFF)** | `cuong.tran@henry.example` | `123456` | Đăng nhập Desktop: Xác nhận thanh toán, Xuất báo cáo doanh thu |
| **Khách hàng (Web)** | `quochuy@example.com` | `123456` | Đăng nhập trên trang Web người dùng (`login.html` / `profile.html`) |

---

## 3. Kịch bản Demo sự liên kết dữ liệu giữa Web và Desktop (Rubric mục 6 & 8)

1. **Khách đặt phòng trên Web $\rightarrow$ Hiện ngay trên Desktop**:
   - Trên Web (`http://localhost:5188/rooms.html`), chọn một phòng (VD: *Vista View 42*), bấm **Đặt phòng** $\rightarrow$ chọn **Đặt cọc trước 30%** $\rightarrow$ bấm **Bắt đầu thanh toán** $\rightarrow$ bấm **Xác nhận thanh toán**.
   - Hệ thống sinh mã đơn mới (VD: `HN-270008`, trạng thái `Chờ xác nhận`).
   - Sang ứng dụng **Desktop Admin**, bấm **🔄 Làm mới dữ liệu**:
     - Mục **Quản lý thanh toán** hiện thông báo đỏ `(3)` giao dịch chờ xác nhận.
     - Vào tab **Xác nhận thanh toán**, chọn đơn `HN-270008` và bấm **✓ Xác nhận đã nhận tiền**.
   - Quay lại trang Web (`profile.html#bookings` hoặc `booking-detail.html?code=HN-270008`) tải lại trang $\rightarrow$ trạng thái đơn đã tự động chuyển sang **Đã xác nhận**!
2. **Quản trị viên sửa giá phòng / thêm phòng / đổi cấu hình ngân hàng trên Desktop $\rightarrow$ Web cập nhật ngay**:
   - Trên ứng dụng **Desktop Admin**, vào **Quản lý phòng** $\rightarrow$ sửa giá phòng hoặc ẩn/hiện một phòng, hoặc vào **Cấu hình hệ thống** đổi số tài khoản / tỷ lệ đặt cọc.
   - Tải lại trang Web (`rooms.html` hoặc `booking-detail.html`) $\rightarrow$ giá phòng, danh sách phòng và thông tin chuyển khoản QR trên Web thay đổi ngay lập tức!
