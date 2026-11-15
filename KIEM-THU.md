# Kiểm tra giao diện HENRY homestay

## Kết quả thực chạy

- Chrome headless mở trực tiếp file://, kiểm tra 8 HTML × 4 chiều rộng: 1440, 1366, 768, 390 px. Cả 32 trường hợp không tràn ngang, không hỏng ảnh, không gọi ảnh/CSS/script bên ngoài, không có lỗi JavaScript.
- 18 kiểm tra tương tác đạt: bộ lọc loại phòng, kết quả rỗng và đặt lại; sắp xếp giá; lưu/bỏ lưu và đọc yêu thích trong hồ sơ; cập nhật tên hồ sơ; xác nhận mật khẩu; phản hồi đăng ký/đăng nhập; hiện mật khẩu; quên mật khẩu; lọc ẩm thực; đọc bài; kiểm tra tổng tiền; điều hướng từ phòng sang đơn mẫu; menu điện thoại.
- Taste Skill inspector: cả 6 trang mới có 0 FAIL ở 1440, 768 và 390 px. Login, register và profile không có WARN. Rooms, booking và food mỗi trang có 1 WARN được giải thích bên dưới.
- Lint 6 HTML mới và 2 CSS chung: 0 FAIL. Hai WARN về bóng đổ chỉ thuộc menu nổi và thông báo nổi, phù hợp vai trò overlay.
- Đã xem ảnh toàn trang của cả 6 trang mới ở ba kích thước. Bản chụp và báo cáo máy nằm trong `.taste-skill` ở thư mục cha.

## Những gì đã sửa

- Font Georgia ngoài DESIGN.md đã thay bằng Inter trong toàn bộ trang mới; hai mẫu gốc giữ kiểu chữ theo yêu cầu của người dùng.
- Giới hạn min-width cho cột tài khoản để sửa tràn ngang trên tablet.
- Ảnh phòng ngủ gốc có phần giao diện nằm bên phải: cắt bằng CSS ở trang đăng nhập để chỉ hiển thị không gian phòng.
- Menu điện thoại chỉ giữ một nút hamburger, tài khoản truy cập bằng avatar.
- Thông tin liên hệ, số tài khoản và giấy tờ trong bản sao của trang cũ đã thay bằng giá trị mẫu hoặc bỏ trống. HTML gốc ở thư mục cha không bị sửa.

Không bỏ qua hoặc tắt quy tắc font của design hook. Một chú thích lint chỉ dành cho gradient phủ tối trung tính trên ảnh đăng nhập: đây là scrim để đọc chữ, được chính Taste Skill cho phép.

## Các cảnh báo được giữ có lý do

| Cảnh báo | Quyết định |
| --- | --- |
| Rooms: nhiều chữ dưới 13 px | 11–12 px dành cho tiện nghi, thông số phụ, đường dẫn và ghi chú; tên phòng 16 px, giá 16 px, bộ lọc 13–14 px. Không dùng chữ nhỏ cho hành động chính. |
| Booking: hai cột có chiều cao khác nhau | Cột trái gồm nhiều phần của kỳ nghỉ; cột phải là một hóa đơn độc lập. Không kéo dài hóa đơn để tạo khoảng rỗng. |
| Food: h1 trên 32 px | Đây là trang đọc dạng blog theo yêu cầu; giữ tiêu đề editorial 46 px desktop, 36 px điện thoại. Cảnh báo phát sinh vì chạy cùng cổng kiểm tra app để dùng hệ giao diện chung. |

## Nhận xét hình ảnh cuối

1. Câu hỏi chính của mỗi trang được trả lời trước: phòng nào phù hợp; đăng nhập/đăng ký; món gì có ở bếp; kỳ nghỉ và chi phí ra sao; thông tin tài khoản là gì.
2. Các trang dùng một khung trái/phải nhất quán. Ảnh và danh sách dùng hết chiều rộng, không có vùng trắng vô tình cạnh cột tài khoản.
3. Nền sáng, panel trắng, màu đỏ hồng chỉ nhấn thương hiệu và hành động. Monogram H dạng mái nhà xuất hiện trong header/footer chung.
4. Tổng đơn: 1.190.000 × 2 + 150.000 − 100.000 = 2.430.000 ₫; đã trả đủ; còn lại 0 ₫. Tổng và số tiền được tính từ cùng dữ liệu mẫu.
5. Trạng thái rỗng của kết quả phòng và yêu thích có hành động tiếp theo. Số 0 tiền còn lại hiển thị là 0, không thay bằng dấu gạch.
6. Không có biểu đồ không cần thiết. Chỉ hiển thị các số tiền và thông tin kỳ nghỉ trực tiếp.
7. Trên điện thoại, menu chính thu gọn, bộ lọc mở riêng, các phần hồ sơ xếp dọc; tab tài khoản cuộn trong hàng của nó.

Đánh giá nội bộ bằng mắt: phân cấp 8/10, nhất quán 8/10, responsive 8/10. Giới hạn: ảnh mẫu có sẵn không đồng nhất địa điểm, chưa có ảnh món ăn thật; hai trang được yêu cầu giữ nguyên có phong cách khác nhau, nên toàn bộ website chưa thể đồng nhất tuyệt đối mà không thay thiết kế hai trang đó. Không có đánh giá người dùng thực tế.

Phong cách đã rút: Achromatic ledger, seed 1929619870. Palette, Inter và bo góc theo DESIGN.md; điều hướng theo hai trang sẵn có, không đưa rail/dashboard nhiều chủ nhà vào website này. Ý tưởng: những phòng và bếp của một nhà trở thành danh mục lưu trú và góc chuyện nhà; thao tác sống là bộ lọc phòng kết hợp danh sách yêu thích. Không tìm ảnh ngoài; chỉ dùng tài nguyên từ HTML cung cấp. Các nội dung mẫu cần xác nhận được liệt kê trong README.
