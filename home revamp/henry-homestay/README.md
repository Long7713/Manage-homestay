# HENRY homestay — bộ giao diện HTML

Mở `index.html` trực tiếp bằng Chrome hoặc Edge. Không cần cài thư viện, chạy server hay build. Giữ nguyên cả thư mục `assets` bên cạnh các file HTML.

## Các trang

| File | Nội dung |
| --- | --- |
| `index.html` | Trang chủ từ mẫu có sẵn; giữ bố cục và thiết kế |
| `room-detail.html` | Chi tiết phòng từ mẫu có sẵn; giữ bố cục và thiết kế |
| `rooms.html` | Kết quả tìm phòng, bộ lọc, sắp xếp, lưu phòng |
| `login.html` | Đăng nhập, hiện/ẩn mật khẩu, quên mật khẩu mẫu |
| `register.html` | Đăng ký và kiểm tra xác nhận mật khẩu |
| `food.html` | Góc ẩm thực dạng bài viết, lọc chủ đề, đọc bài trong hộp thoại |
| `booking-detail.html` | Chi tiết kỳ nghỉ, chi phí, thanh toán mẫu, in và sao chép mã |
| `profile.html` | Hồ sơ, bảo mật, đơn đặt phòng và phòng đã lưu |

## Cấu trúc để học và chỉnh sửa

- `assets/css/base.css`: token màu, font, bố cục chung, header, footer, nút và form.
- `assets/css/pages.css`: bố cục riêng theo từng trang; có chú thích chia khu vực.
- `assets/js/components.js`: header, footer và bộ icon SVG nét viền dùng chung cho 6 trang mới; không dùng fetch nên mở được qua file://.
- `assets/js/data.js`: danh sách 6 phòng và đơn mẫu, định dạng tiền, kiểm tra tổng chi phí.
- `assets/js/rooms.js`, `forms.js`, `food.js`, `booking.js`, `profile.js`: tương tác theo trang.
- `assets/css/legacy-*.css`, `assets/js/legacy-*.js`: CSS/JS tách ra từ hai HTML mẫu và liên kết điều hướng. Hai mẫu có thiết kế riêng được giữ lại theo yêu cầu.
- `assets/images`, `assets/fonts`: ảnh từ HTML đã cung cấp và font được lưu nội bộ; không cần tải tài nguyên khi mở website.

Các HTML gốc và DESIGN.md ở thư mục cha không bị sửa. Hai bản trong bộ bàn giao chỉ thay thương hiệu, thông tin giả lập, đường dẫn tài nguyên và điều hướng; bỏ kích thước cố định của bản xuất để có thể cuộn trang.

## Phạm vi minh họa

Không backend, cơ sở dữ liệu, xác thực, gửi email hay thanh toán. Đăng nhập/đăng ký hiển thị phản hồi mẫu. Hồ sơ và ảnh đại diện chỉ thay đổi trong phiên hiện tại; không tải lên máy chủ. Phòng yêu thích được lưu trong localStorage trên trình duyệt. Bộ lọc dùng danh sách mẫu; ngày tìm kiếm không kiểm tra phòng trống thật.

Tên khách, email thuộc miền `.example`, giá, ngày lưu trú, trạng thái đơn, món ăn, chính sách và thành phần món ăn đều là dữ liệu mẫu cần chủ homestay xác nhận. Không có số điện thoại, tài khoản ngân hàng hoặc giấy tờ thật trong các trang mới. Luồng VietQR kế thừa được chuyển sang trang chi tiết đơn mẫu, không tạo thanh toán.

Ảnh được tái sử dụng từ chính HTML đã cung cấp (bảng đối chiếu tại `assets/image-map.json`), không tìm thêm ảnh bên ngoài. Trang ẩm thực dùng ảnh gian bếp và ban công có sẵn, nội dung món ăn được trình bày bằng chữ. Cần bổ sung ảnh món thật của HENRY nếu muốn minh họa từng món trước khi sử dụng thực tế.

Hệ trang mới dùng Inter, nền `#fcf9f8`, chữ `#222222`, nhấn `#ba0036` từ DESIGN.md. Kết cấu dựa trên Achromatic ledger, seed 1929619870; màu nút, bo góc và điều hướng theo thương hiệu có sẵn. Không thêm sidebar quản trị nhiều chủ nhà.
