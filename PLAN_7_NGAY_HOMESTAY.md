# Kế hoạch 7 ngày — Hệ thống quản lý homestay

**Ngày lập:** 24/10/2026  
**Thời gian:** 24–30/10/2026, múi giờ Việt Nam  
**Nhân lực:** 3 người, dự kiến 6 giờ/người/ngày  
**Ưu tiên:** Hoàn thiện báo cáo trong ngày 27/10, trước mốc nộp 28/10.  
**Trạng thái:** Kế hoạch đề xuất để leader duyệt triển khai. Việc tạo file không đồng nghĩa đã triển khai các công việc trong kế hoạch.

## 1. Mục tiêu và nguyên tắc

1. Hoàn thiện báo cáo phân tích–thiết kế homestay, dùng bố cục báo cáo sân bóng cũ làm tham khảo; không kế thừa mặc định nghiệp vụ sân bóng.
2. Có PRD làm mốc kiểm soát phạm vi, danh mục Use Case (UC), quy tắc nghiệp vụ và tiêu chí nghiệm thu.
3. Triển khai code trong 7 ngày, ưu tiên hệ thống quản lý; FE khách và admin dùng giao diện đơn giản, nhất quán.
4. Hướng tới khoảng 50% dự án, nhưng không cam kết tỷ lệ trước khi chốt toàn bộ backlog. Không dùng số màn hình hoặc số dòng code để tuyên bố đạt 50%.
5. Chỉ triển khai nghiệp vụ và thay đổi đã được leader duyệt. Điểm chưa chốt được ghi rõ, không tự suy diễn.
6. Báo cáo phải phân biệt phần đã thiết kế, đã triển khai và đã kiểm thử. Không dùng dữ liệu minh họa làm bằng chứng kiểm thử thành công.

### Quỹ thời gian

- Tổng tối đa 7 ngày: 3 × 6 × 7 = **126 giờ công**.
- Từ 24–27/10: tối đa **72 giờ công**, gồm cả báo cáo, họp, rà soát và code.
- Từ 28–30/10: tối đa **54 giờ công**, tập trung code và kiểm thử sau mốc báo cáo.
- Quỹ thời gian là dự kiến khi cả nhóm làm đủ các ngày; phần còn lại của ngày 24/10 có thể khiến thời gian thực tế ít hơn.
- Khi thiếu thời gian trước 28/10, giảm công việc code; không cắt bước rà soát báo cáo.

## 2. Phạm vi và quyết định đã thống nhất

### 2.1. Sản phẩm và công nghệ

- Hệ thống quản lý nhiều cơ sở homestay, hỗ trợ thuê theo giờ, thuê theo ngày và có gói/combo.
- Có trang dành cho khách và khu vực quản lý nội bộ.
- Khách không cần tài khoản; hồ sơ nhận diện bằng SĐT, chuẩn hóa về dạng `+84…`. Dạng `0…` tương ứng phải được quy về cùng số khi tra cứu.
- Nhân viên tiếp nhận yêu cầu hỗ trợ của khách và tạo mã đặt phòng.
- Công nghệ đã chốt: **C# + ASP.NET Core MVC + MySQL**.
- Phiên bản .NET, thư viện kết nối và môi trường chạy cần được kiểm tra tương thích trước khi khóa cấu hình.
- Bánh/nước bán thêm, kiểm tra phòng sau sử dụng và báo cáo hư hỏng thuộc nghiệp vụ cần thiết kế. Chưa mặc định mở rộng sang nhập–xuất kho hoặc tự tính bồi thường.

### 2.2. Đặt phòng, cọc và lịch sử

- Đơn chờ xác nhận có thời hạn cọc 15 phút.
- Nhân viên ghi nhận cọc dựa trên thông tin ngân hàng đã nhận tiền; ảnh chuyển khoản của khách không thay thế việc đối chiếu.
- Hết hạn không cọc: giải phóng giữ chỗ, lưu đơn hết hạn để tra cứu; không tự xóa bản ghi.
- Mã đơn phải duy nhất, giữ nguyên và không tái sử dụng để lấp khoảng trống do đơn hết hạn.
- Thao tác “Ẩn khỏi vận hành” không làm mất lịch sử, giao dịch hoặc căn cứ vi phạm.
- Tiền đến đúng hạn nhưng nhân viên ghi nhận muộn phải được đối chiếu, không quy lỗi không cọc cho khách. Nhân viên xin lỗi và thống nhất phương án hoàn 100% khoản tiền đã nhận, đổi phòng hoặc tạo đơn thay thế phù hợp.
- Tiền đến muộn khoảng 1–5 phút: thông báo chuyển trễ, trao đổi tạo đơn mới và kiểm tra lại phòng trống. Không tự hồi sinh đơn hết hạn hoặc ghi nhận thu cùng một khoản tiền hai lần.

### 2.3. Chặn và mở chặn khách

- Mỗi đơn hết hạn do không cọc hợp lệ là một lần vi phạm; tác vụ kiểm tra chạy nhiều lần không tăng thêm số lần.
- Đủ 3 lần vi phạm liên tiếp thì chặn tạo đơn mới cho SĐT đó.
- Trước khi bị chặn, một lần cọc hợp lệ đưa bộ đếm liên tiếp về 0.
- Hủy đơn không tính vào bộ đếm không cọc.
- Không tự mở chặn. Nhân viên phải có xác nhận của quản lý mới thực hiện mở chặn.
- Lưu người thực hiện, quản lý xác nhận, thời điểm và lý do mở chặn.
- Mở chặn được phê duyệt đưa bộ đếm liên tiếp về 0, nhưng giữ nguyên toàn bộ lịch sử cũ.
- Ẩn đơn không mở chặn và không làm giảm bộ đếm.

### 2.4. Hủy và hoàn cọc

Gọi `t` là thời gian từ lúc ghi nhận yêu cầu hủy đến giờ bắt đầu sử dụng phòng. Mức hoàn tính trên **tiền cọc giá phòng**:

| Thời gian còn lại | Mức hoàn cọc |
|---|---:|
| `t ≥ 72 giờ` | 60% |
| `48 giờ ≤ t < 72 giờ` | 40% |
| `0 ≤ t < 48 giờ` | 0% |

Trường hợp yêu cầu hủy sau giờ bắt đầu sử dụng chưa được chốt; không tự áp dụng thêm chính sách.

### 2.5. Sử dụng và dọn phòng

- Năm nhãn vận hành đã thống nhất: **Trống → Chờ xác nhận → Xác nhận → Đang sử dụng → Sau sử dụng → Trống**.
- Thiết kế phải phân biệt vòng đời đơn đặt với tình trạng thực tế của phòng. Một trường trạng thái trên phòng không được thay thế lịch đặt theo khoảng thời gian.
- Chừa tối thiểu 20 phút sau lượt sử dụng trước để kiểm tra/dọn phòng và thông báo giờ có thể nhận cho khách tiếp theo.
- Nhân viên kiểm tra, báo cáo hư hỏng nếu có, dọn và xác nhận sẵn sàng; đủ 20 phút không tự chuyển phòng sang Trống.
- Không có khách tiếp theo vẫn thực hiện quy trình Sau sử dụng → kiểm tra/dọn → xác nhận Trống.
- Đặt, gia hạn và đổi phòng phải kiểm tra chồng lấn theo thời gian, tính cả khoảng dọn phòng.

## 3. Đầu ra báo cáo và các quyết định còn thiếu

### Hồ sơ cần hoàn thiện trước 28/10

| Đầu ra | Nội dung và điều kiện đạt |
|---|---|
| Báo cáo Word | Tổng quan, yêu cầu, UC, thiết kế, database, giao diện, kế hoạch kiểm thử và mức triển khai thực tế |
| PRD | Phạm vi được duyệt, giới hạn, quy tắc nghiệp vụ, tiêu chí nghiệm thu và danh sách chưa chốt |
| Bộ UC | Actor, mục tiêu, tiền/hậu điều kiện, luồng chính, thay thế, ngoại lệ và quy tắc liên quan |
| Sơ đồ | Use case tổng quát; hoạt động, trạng thái, lớp và tuần tự cho các nghiệp vụ chính |
| Thiết kế dữ liệu | ERD, từ điển dữ liệu, khóa/ràng buộc và quan hệ với UC; không giả định đã có script chạy được |
| Kế hoạch kiểm thử | Ca kiểm thử có đầu vào và kết quả mong đợi; tách riêng kết quả thực chạy nếu có |

### Những điểm leader cần chốt

1. Thuê ngày tính mỗi 24 giờ hay theo mốc nhận/trả cố định; quy tắc tính thêm giờ, làm tròn và giá thuê.
2. Combo là gói thời lượng hay gồm cả sản phẩm; giá và thành phần áp dụng.
3. Mức cọc, cách tính cọc; tiền đến đúng thời điểm hết hạn 15 phút; tiền đến muộn ngoài khoảng 1–5 phút.
4. FE khách gửi yêu cầu trực tiếp vào hệ thống hay liên hệ bên ngoài; chưa mặc định xây chức năng chat trực tiếp.
5. Phân quyền theo cơ sở, phạm vi chặn SĐT toàn hệ thống và cách thể hiện phê duyệt của quản lý trong ứng dụng.
6. Quy tắc đơn đã xác nhận nhưng khách không đến; phân biệt với không cọc.
7. Quy trình xử lý phòng hư hỏng và việc thu bồi thường, nếu cần đưa vào phạm vi.
8. Vị trí, phiên bản và cấu trúc MySQL hiện có để đối chiếu trước khi thay đổi schema.

Các nội dung chưa chốt được nêu minh bạch trong tài liệu, không viết thành quy tắc đã được duyệt. Nếu ảnh hưởng trực tiếp phần code, tạm hoãn chức năng phụ thuộc thay vì tự đặt chính sách.

## 4. Lịch triển khai 24–30/10

| Ngày | Báo cáo/PRD — ưu tiên trước | Code sau khi phần tương ứng được duyệt | Đầu ra kiểm tra cuối ngày |
|---|---|---|---|
| **1 — 24/10** | Chốt mục lục, actor, danh mục UC và sổ quyết định; viết tổng quan, phạm vi và quy tắc đã thống nhất | Kiểm tra môi trường; khởi tạo MVC và kết nối MySQL nếu đủ điều kiện | Leader duyệt danh mục UC; xác định điểm còn thiếu và người xử lý |
| **2 — 25/10** | Viết đặc tả UC chính, PRD; thiết kế ERD và sơ đồ luồng đặt–cọc–hết hạn | Dựng bố cục admin/khách; dữ liệu cơ sở/phòng và đăng nhập nội bộ theo thiết kế được duyệt | Có bản thảo UC, ERD và bản khung ứng dụng chạy được nếu môi trường sẵn sàng |
| **3 — 26/10** | Hoàn thiện sơ đồ, từ điển dữ liệu, thiết kế màn hình và ca kiểm thử; ghép bản báo cáo đầy đủ | Bắt đầu quản lý cơ sở/phòng, hồ sơ khách và tra cứu lịch; không mở rộng tính năng | Có bản báo cáo đầy đủ để leader đọc, danh sách lỗi nội dung và thiết kế |
| **4 — 27/10** | Leader rà soát; sửa nội dung, sơ đồ, mục lục, hình/bảng và định dạng; đóng bản nộp | Chỉ xử lý lỗi cần thiết cho bằng chứng minh họa; không ép code đủ chức năng | Bản báo cáo và PRD sẵn sàng nộp; ghi rõ phần đã/chưa triển khai |
| **5 — 28/10** | Hỗ trợ nộp và giải thích thiết kế; giữ nguyên bản đã nộp | Tập trung admin tạo đơn, mã đơn, giữ chỗ 15 phút và hiển thị lịch sử khách | Demo tạo đơn và kiểm tra trùng lịch trên MySQL |
| **6 — 29/10** | Ghi nhận tiến độ code riêng, không làm sai nội dung bản nộp | Ghi nhận cọc, hết hạn, vi phạm/chặn nếu đủ thời gian; nối FE khách theo phương án đã duyệt | Demo luồng đặt–cọc/hết hạn, FE khách dùng dữ liệu thật ở phần hoàn thành |
| **7 — 30/10** | Tổng hợp mức hoàn thành và tồn đọng | Kiểm thử tích hợp, sửa lỗi; hoàn thiện cách chạy, dữ liệu demo và bàn giao | Demo chung, kết quả kiểm thử và đánh giá mục tiêu khoảng 50% theo backlog đã duyệt |

Không cố triển khai tất cả nghiệp vụ tài chính, combo, sản phẩm, hư hỏng và vận hành phòng trong tuần đầu. Các phần này vẫn được phân tích trong báo cáo; thứ tự code tiếp theo do leader duyệt.

## 5. Phân công đề xuất

Chưa gán tên cá nhân; leader phân công theo năng lực thực tế.

| Người | Trách nhiệm tài liệu | Trách nhiệm code |
|---|---|---|
| **Người 1 — leader/phân tích** | Chốt phạm vi, tổng hợp báo cáo và PRD, duyệt UC | Phối hợp quy tắc nghiệp vụ, rà soát tích hợp và nghiệm thu |
| **Người 2 — dữ liệu/backend** | ERD, từ điển dữ liệu, lớp/tuần tự cho nghiệp vụ chính | MySQL, xử lý C#, giao dịch, đặt phòng và cọc |
| **Người 3 — FE/kiểm thử** | Thiết kế giao diện, ca kiểm thử, hình minh họa thực tế | Razor/Bootstrap cho admin và khách, tích hợp và kiểm thử |

- Mỗi thành viên viết tài liệu cho phần mình phụ trách; không dồn toàn bộ báo cáo cho leader.
- Dành khoảng 30 phút cuối ngày, nằm trong 6 giờ làm việc, để ghép việc và kiểm tra.
- Mỗi thay đổi cần ghi: lý do, phần ảnh hưởng, thời gian dự kiến và quyết định của leader.

## 6. Đánh giá mốc code và nghiệm thu

### Cách đo mục tiêu khoảng 50%

Sau khi chốt danh mục UC, nhóm lập backlog và ước lượng khối lượng từng chức năng; leader duyệt làm mốc. Tỷ lệ hoàn thành bằng tổng khối lượng chức năng đã đạt tiêu chí nghiệm thu chia tổng khối lượng backlog đã duyệt.

- Chỉ tính hoàn thành khi phần giao diện, xử lý, lưu dữ liệu và kiểm thử cần thiết cho chức năng đều đạt.
- Màn hình dữ liệu giả hoặc API chưa nối vào luồng sử dụng ghi là đang làm, không coi là chức năng hoàn tất.
- Nếu đến 30/10 chưa đạt 50%, báo cáo tỷ lệ thực tế, phần còn thiếu và nguyên nhân; không thay đổi mẫu số để đạt chỉ tiêu.

### Kiểm tra tài liệu trước khi nộp

- [ ] Đề tài và thuật ngữ homestay nhất quán; không còn nghiệp vụ sân bóng chưa được duyệt.
- [ ] PRD, UC, sơ đồ, ERD và thiết kế màn hình khớp nhau.
- [ ] Có mã liên kết yêu cầu → UC → thiết kế → ca kiểm thử.
- [ ] Tách trạng thái phòng, trạng thái đơn và lịch đặt theo thời gian.
- [ ] Các chính sách chưa chốt được chỉ rõ, không viết như đã triển khai.
- [ ] Mục lục, số hình/bảng, định dạng và chính tả được kiểm tra.
- [ ] Bản Word được mở/render để kiểm tra bố cục trước khi giao.

### Các kịch bản kiểm thử thiết kế; thực chạy theo phần code đã có

- [ ] Hai yêu cầu đồng thời không giữ cùng phòng trong khoảng thời gian chồng lấn.
- [ ] Đơn giữ chỗ có xét hạn 15 phút; tác vụ lặp không nhân đôi vi phạm.
- [ ] Đơn hết hạn vẫn tra cứu được; mã không đổi hoặc bị tái sử dụng.
- [ ] Nhận cọc và xử lý hết hạn đồng thời không tạo trạng thái mâu thuẫn.
- [ ] Tiền đến đúng hạn nhưng nhân viên ghi nhận muộn được đối chiếu, không tự quy lỗi cho khách.
- [ ] Đủ 3 vi phạm liên tiếp bị chặn; mở chặn có phê duyệt reset bộ đếm, giữ lịch sử.
- [ ] Ẩn đơn không thay đổi lịch sử giao dịch hoặc số lần vi phạm.
- [ ] Hoàn cọc đúng tại và hai phía của mốc 48 giờ, 72 giờ; hủy không tăng vi phạm không cọc.
- [ ] Lịch có tối thiểu 20 phút dọn; phòng chưa được xác nhận sẵn sàng không tự thành Trống.
- [ ] FE không hiển thị lịch sử khách/giao dịch nội bộ cho người không có quyền.

## 7. Giới hạn và bàn giao

- Hiện trạng repo khi lập kế hoạch: mới có README và commit khởi tạo; chưa có code C# để tái sử dụng được xác minh.
- Chưa xác minh được SDK .NET trong môi trường terminal và schema MySQL hiện có.
- Không mặc định tích hợp ngân hàng tự động, OTP, thanh toán online, Zalo/OTA, quản lý kho hoặc triển khai Vercel.
- Phương án demo đề xuất trong tuần đầu là chạy tại máy phát triển; triển khai online là quyết định riêng.
- Không sửa báo cáo gốc; tạo bản homestay riêng khi triển khai tài liệu được duyệt.
- Bàn giao cuối đợt gồm: bản báo cáo đã nộp, PRD, mã nguồn phần hoàn thành, script/migration nếu đã triển khai, hướng dẫn chạy, dữ liệu demo không chứa thông tin khách thật, kết quả kiểm thử và backlog còn lại.

**Việc tạo file kế hoạch này không thực hiện migration, sửa dữ liệu, cài công cụ, tạo chức năng hoặc thay đổi Git ngoài việc thêm chính file kế hoạch.**
