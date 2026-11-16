<<<<<<< HEAD
# Hệ thống quản lý homestay

> Cập nhật ngày **24/10/2026**. Kế hoạch **24–30/10/2026**; ưu tiên hoàn thiện báo cáo trong **ngày 27/10**, sẵn sàng nộp trước **28/10**.

## 1. Mục tiêu và tài liệu

Nhóm xây dựng hệ thống quản lý nhiều cơ sở homestay bằng **C# + ASP.NET Core MVC (.NET 10 LTS) + MySQL**, sử dụng **Docker** để thống nhất môi trường. Giai đoạn demo dùng **một chi nhánh**; dữ liệu chính thức các chi nhánh bổ sung sau. Trang khách cung cấp thông tin, giá, địa chỉ và liên hệ Facebook/Zalo/điện thoại. Nhân viên trao đổi với khách bên ngoài rồi tạo đơn nội bộ.

- **Báo cáo:** đủ ba chương theo yêu cầu giảng viên; UC, sơ đồ và dữ liệu nhất quán.
- **[PRD hệ thống homestay](PRD_HE_THONG_HOMESTAY.docx) — phiên bản 1.4:** phạm vi, bảng giá, quy tắc BR01–BR36, UC01–UC19, dữ liệu, kiến trúc, NFR01–NFR12, ca kiểm thử TC01–TC36, ma trận truy vết, quyết định D01–D22 và câu hỏi còn mở; mở bằng WPS Office hoặc phần mềm đọc `.docx`.
- **Ứng dụng:** ưu tiên quản lý nội bộ, FE khách và admin đơn giản; không bắt buộc hoàn thiện trước ngày nộp báo cáo.
- **Kiểm thử:** chỉ ghi nhận kết quả thực chạy; màn hình mẫu hoặc chức năng chưa nối dữ liệu không được tính là hoàn tất.

**Code khoảng 50% là mục tiêu, không phải cam kết khi chưa duyệt backlog.** Mọi thay đổi phạm vi và nghiệp vụ phải được leader chấp thuận.

README này là kế hoạch hiện hành. `PLAN_7_NGAY_HOMESTAY.md` là bản trước đó; khi khác nhau, dùng quyết định cập nhật tại đây và trong PRD.

## 2. Nguồn lực và phân công đề xuất

**3 người × 6 giờ/ngày:** tối đa **126 giờ công** trong bảy ngày. Từ 24–27/10 có tối đa 72 giờ công, gồm cả tài liệu, họp, rà soát và code; từ 28–30/10 có tối đa 54 giờ công. Quỹ thực tế phụ thuộc thời điểm bắt đầu và lịch cá nhân.

| Người | Trách nhiệm chính | Hỗ trợ |
|---|---|---|
| Leader | Chốt nghiệp vụ, PRD, UC; tổng hợp và duyệt báo cáo | Tích hợp, nghiệm thu và trình bày |
| Thành viên 2 | Thiết kế dữ liệu, khảo sát công nghệ, backend và Docker | Sơ đồ nghiệp vụ, kiểm thử dữ liệu |
| Thành viên 3 | Thiết kế giao diện, FE khách/admin, user story và kiểm thử | Trình bày báo cáo, hình minh họa và demo |

Leader gán tên theo năng lực. Mỗi người viết tài liệu phần mình phụ trách, không dồn toàn bộ báo cáo cho leader vào ngày cuối.

## 3. Lịch từng ngày

| Ngày | Công việc ưu tiên | Đầu ra cuối ngày |
|---|---|---|
| **24/10 — Chốt phạm vi** | Tổng hợp nghiệp vụ, chốt danh mục UC, lập PRD, chia việc và bố cục báo cáo | PRD bản đầu, danh mục UC, điểm cần quyết định và người phụ trách |
| **25/10 — Đặc tả và thiết kế** | Viết Chương 1–2; đặc tả UC chính; workflow, lược đồ ý niệm và luận lý | Hai chương đầu, đặc tả luồng chính và thiết kế dữ liệu |
| **26/10 — Hoàn thiện bản thảo** | Chương 3, user story, sơ đồ, lược đồ vật lý, thiết kế màn hình; ghép báo cáo | Bản đầy đủ để leader đọc và đánh dấu chỉnh sửa |
| **27/10 — Đóng bản nộp** | Đối chiếu nghiệp vụ với UC/dữ liệu; sửa nội dung, mục lục, bảng/hình; kiểm tra trên WPS | Báo cáo và PRD sẵn sàng nộp trước 28/10 |
| **28/10 — Nền tảng quản lý** | Docker, MVC, MySQL; đăng nhập nội bộ; cơ sở, phòng và giá | Môi trường nhóm chạy được, có các màn hình quản lý nền tảng |
| **29/10 — Luồng đặt phòng** | Tạo mã đơn, giữ chỗ 15 phút, cọc và lịch sử; FE khách hiển thị giá/liên hệ | Luồng đã chọn nối được giao diện, xử lý và dữ liệu |
| **30/10 — Tích hợp và đánh giá** | Kiểm thử, sửa lỗi, demo, kiểm tra trên máy thành viên và tổng hợp việc còn lại | Kết quả kiểm thử, hướng dẫn chạy và tiến độ thực tế |

Ngày 24–26/10 có thể chuẩn bị môi trường và khung giao diện sau khi hoàn thành phần tài liệu trong ngày. Khi thiếu thời gian, giảm code song song để giữ hạn báo cáo. Chỉ triển khai phần thiết kế đã được leader duyệt.

## 4. Nội dung báo cáo bắt buộc

### Chương 1 — Tổng quan hệ thống

- Bối cảnh quản lý nhiều cơ sở, vấn đề, mục tiêu, người dùng, phạm vi và giới hạn.
- Phân biệt trang khách với hệ thống quản lý nội bộ.
- Kết quả dự kiến; không nhận đã khảo sát thực địa nếu nhóm chưa thực hiện.

### Chương 2 — Khảo sát công nghệ liên quan

- C#, ASP.NET Core MVC, Razor/Bootstrap, MySQL và Docker.
- Vai trò, lý do lựa chọn, hạn chế và cách các thành phần phối hợp; nguồn chính thức.
- Bộ phiên bản tương thích dự kiến. Docker chuẩn hóa môi trường nhưng vẫn cần hệ điều hành, CPU và Docker Engine/Desktop phù hợp; không bảo đảm chạy trên mọi máy hoặc mọi phiên bản.

### Chương 3 — Phân tích hệ thống

- **Nghiệp vụ:** business context; workflow đặt phòng, cọc, hết hạn, hủy, nhận/trả phòng và dọn phòng.
- **Chức năng:** actor, sơ đồ use case, đặc tả UC, user story và tiêu chí nghiệm thu.
- **Dữ liệu ý niệm:** thực thể và quan hệ nghiệp vụ.
- **Dữ liệu luận lý:** bảng, thuộc tính, khóa chính và khóa ngoại.
- **Dữ liệu vật lý:** kiểu dữ liệu MySQL, ràng buộc và chỉ mục dự kiến.
- Phân biệt trạng thái đơn với tình trạng phòng; một trường trạng thái không thay thế lịch đặt theo khoảng thời gian.

## 5. Nghiệp vụ đã thống nhất

### Người dùng và phạm vi

- Khách không cần tài khoản; hồ sơ nội bộ theo SĐT chuẩn `+84…`.
- Trang khách chỉ xem giá, phòng, cơ sở, địa chỉ và liên hệ bên ngoài. Nhân viên tạo đơn sau trao đổi; không có chat tích hợp, biểu mẫu giữ chỗ công khai hoặc thanh toán tự động.
- **Homestay tự check-in**, không có lễ tân: nhân viên gửi mã hộp khóa (lấy thẻ vào phòng) qua Facebook/Zalo/điện thoại.
- Hai vai trò nội bộ: **Nhân viên** làm việc tại các cơ sở được phân công; **Quản lý** xem và thao tác mọi cơ sở, quản lý danh mục, tài khoản, cấu hình và duyệt mở chặn.
- **Hư hỏng và bồi thường ngoài phạm vi** vì là loại vi phạm khác với không cọc. Hệ thống chỉ có cờ **Tạm ngưng khai thác** để không bán phòng đang có sự cố.
- Không có chức năng đổi phòng riêng; đổi phòng thực hiện bằng đơn thay thế liên kết đơn cũ. Không mặc định thêm quản lý kho, OTP hoặc OTA.

### Giá phòng và sản phẩm

- Hai hạng **Standard/Signature**, giá theo ba ảnh mẫu leader cung cấp.
- Combo 3 giờ, 5 giờ, SALE; có qua đêm và ngày. Gói ngày **12h–10h hôm sau**, không phải 24 giờ.
- Nhóm giá T2–T6 hoặc T7–CN theo **ngày bắt đầu gói thuê**. Ngoại lệ: khung qua đêm **24h–12h** tính theo đêm khách đặt (24h tối thứ Sáu → giá T2–T6; 24h tối Chủ nhật → giá T7–CN).
- **Lễ/Tết không đổi giá**, không phụ thu.
- **Không thuê lẻ ngoài gói:** mỗi đơn chọn một gói chuẩn, có thể cộng giờ thêm **80.000 đồng/giờ**. Giờ thêm là **số nguyên, tối thiểu 1 giờ, không có 30 phút** (ví dụ 4 giờ Standard = gói 3 giờ 319.000 + 1 giờ 80.000 = 399.000).
- **Thuê nhiều ngày** là một đơn gồm n gói ngày (12h ngày đầu đến 10h ngày thứ n+1, ở liên tục); giá bằng tổng giá từng ngày theo nhóm ngày của ngày đó (Standard thứ Sáu + thứ Bảy = 669.000 + 719.000 = 1.388.000). Chương trình ưu đãi xem xét sau.
- Giờ thêm đăng ký khi tạo đơn tính vào tiền phòng và cọc. Gia hạn lúc đang ở phải **báo trước giờ kết thúc ít nhất 1 giờ**, phụ thuộc lịch trống và phải thanh toán; sau mốc này không nhận gia hạn.
- **Trả phòng trễ:** dưới 5 phút không tính; từ 5 phút trở lên **100.000 đồng mỗi 10 phút**, phần lẻ làm tròn lên (5 hoặc 10 phút: 100.000; 11 phút: 200.000; 35 phút: 400.000). Nhân viên ghi giờ trả thực tế kèm nguồn.
- **Không nhận phòng sớm:** mã hộp khóa chỉ gửi từ giờ bắt đầu, khi đơn đã xác nhận và phòng sẵn sàng; thời điểm gửi mã là check-in. Hệ thống **không lưu mã hộp khóa**, chỉ ghi người gửi và thời điểm gửi.
- Bánh/nước tính riêng theo giá và số lượng, không nằm trong cọc tiền phòng.

### Cọc và lịch sử

- Cọc **tối thiểu 50%, tối đa 100% tiền phòng** (tiền phòng = gói + giờ thêm đăng ký); hạn chuyển **15 phút** kể từ khi tạo đơn.
- Cọc hợp lệ khi giờ ngân hàng nhận **≤ giờ tạo đơn + 15:00** (đúng mốc vẫn hợp lệ) và đạt tối thiểu 50%.
- Cọc dưới 50%: chưa xác nhận, khách bù trong hạn. Phần vượt quá tiền phòng là tiền trả trước, trừ khi trả phòng và hoàn đủ khi hủy hoặc không đến. Khoản tiền chưa rõ đơn nằm trong danh sách chờ đối chiếu.
- Một SĐT có **tối đa 2 đơn chờ cọc** cùng lúc trên toàn hệ thống.
- **Một đơn một phòng.** Chỉ cảnh báo khi cùng SĐT có đơn chồng giờ ở phòng khác; nhân viên xác nhận lại với khách mới lưu. Không cảnh báo khung giờ sát nhau.
- Nhân viên đối chiếu ngân hàng; lưu riêng giờ ngân hàng nhận tiền và giờ nhân viên ghi nhận.
- Hết hạn giải phóng giữ chỗ nhưng lưu lịch sử; không tự xóa vật lý.
- Mã đơn duy nhất, không đánh lại hoặc tái sử dụng để lấp mã hết hạn.
- **Đơn đang hoạt động / Lịch sử đơn:** đơn đang chạy (chờ cọc còn hạn, đã xác nhận, đang sử dụng) hiện ở danh sách hoạt động; đơn kết thúc (hết hạn, hủy, trả phòng, không đến) tự chuyển sang lịch sử. Không ẩn thủ công, không xóa; giao dịch, vi phạm và dấu vết vẫn tra cứu đầy đủ.
- Tiền đúng hạn nhưng ghi nhận muộn: đối chiếu, xin lỗi, thống nhất hoàn tiền/đổi phòng/đơn thay thế; không quy lỗi không cọc cho khách.
- Tiền đến **sau hạn, bất kể bao nhiêu phút**: đơn giữ Hết hạn; còn phòng thì tạo đơn mới và phân bổ khoản tiền làm cọc, hết phòng thì hoàn 100%; không tự hồi sinh đơn hoặc thu trùng.
- Mọi thời điểm lưu UTC, quy đổi rõ sang `Asia/Ho_Chi_Minh`; không dùng giờ của máy/container (container Docker mặc định chạy UTC).

### Chặn và mở chặn

- Một đơn hết hạn do không cọc hợp lệ là một vi phạm; tác vụ kiểm tra lặp không tăng thêm. Đơn chờ cọc quá hạn tự động không còn giữ phòng, kể cả khi tác vụ nền chưa chạy.
- **Chặn dùng chung toàn hệ thống:** vi phạm ở cơ sở nào cũng cộng chung bộ đếm; khi bị chặn, mọi cơ sở đều thấy cảnh báo kèm cơ sở và mã đơn gây vi phạm, và không tạo được đơn.
- Đủ **3 lần liên tiếp** thì chặn. Trước khi bị chặn, một lần cọc hợp lệ reset bộ đếm về 0.
- Hủy và khách đã cọc không đến **không tính vi phạm không cọc**.
- Mở chặn **phê duyệt trong ứng dụng:** nhân viên tạo yêu cầu kèm lý do, quản lý đăng nhập để duyệt/từ chối; quản lý có thể trực tiếp mở. Lưu người đề nghị, người duyệt, thời điểm, lý do.
- Mở chặn được duyệt reset bộ đếm về 0 nhưng giữ lịch sử cũ; không tự mở chặn.

### Hủy và hoàn cọc

Tính từ lúc khách gửi yêu cầu hủy đến giờ bắt đầu sử dụng; tỷ lệ áp dụng trên **toàn bộ tiền cọc đã nhận** (kể cả khi cọc trên 50%), không theo tổng tiền phòng:

| Thời gian còn lại | Hoàn trên tiền cọc giá phòng |
|---|---:|
| Từ 72 giờ trở lên | 60% |
| Từ 48 giờ đến dưới 72 giờ | 40% |
| Từ 0 đến dưới 48 giờ | 0% |

Hủy **bắt buộc có bằng chứng**: ảnh chụp cuộc trò chuyện Facebook/Zalo hoặc ghi âm khi khách gọi điện; nhân viên tải tệp lên từ máy, tệp không sửa/xóa được, tối đa 20 MB mỗi tệp. Tệp lưu **30 ngày**; quản lý **kiểm tra đơn hủy hằng tuần** và đánh dấu đã kiểm tra. Hệ thống chỉ tự xóa nội dung tệp khi đủ 30 ngày và đơn đã được kiểm tra; mã băm và dấu vết vẫn giữ. Lưu riêng giờ khách gửi yêu cầu (theo bằng chứng) và giờ nhân viên ghi; mức hoàn xét theo giờ khách gửi. Chưa tự áp dụng bảng này cho hủy sau giờ bắt đầu hoặc khi đang sử dụng.

### Khách đã cọc nhưng không đến

- **Không phải vi phạm**, vì vi phạm chỉ xét có cọc hay không.
- Lịch phòng vẫn giữ cho khách đến hết khung đã đặt; khách đến muộn trong khung vẫn nhận phòng, giờ kết thúc không đổi.
- Sau giờ kết thúc mà chưa nhận phòng, nhân viên đánh dấu **Khách không đến**. Đơn giữ trạng thái **Đã xác nhận** và chuyển sang lịch sử.
- Doanh thu bằng tiền cọc đã nhận, không hoàn. Cọc dưới 100% thì hệ thống ghi chú tỷ lệ, ví dụ *đã cọc 159.500 / tiền phòng 319.000 (50%)*; phần còn lại không thành công nợ.

### Nhận trả và dọn phòng

- Nhãn vận hành: **Trống → Chờ xác nhận → Xác nhận → Đang sử dụng → Sau sử dụng → Trống**. Đây là nhãn hiển thị, suy ra từ hai trường độc lập: trạng thái đơn (Chờ cọc, Đã xác nhận, Đang sử dụng, Đã trả phòng, Hết hạn, Đã hủy) và tình trạng phòng (Sẵn sàng, Đang sử dụng, Chờ dọn, Tạm ngưng khai thác).
- Chừa tối thiểu **20 phút** dọn giữa hai lượt, thông báo giờ nhận cho khách sau.
- Nhân viên xác nhận dọn xong; đủ 20 phút không tự chuyển Trống.
- Chưa có khách kế tiếp vẫn thực hiện quy trình dọn và xác nhận sẵn sàng.

## 6. Tiêu chí kỹ thuật phần mềm

Bốn tiêu chí là yêu cầu bắt buộc (NFR08–NFR11 trong PRD):

| Tiêu chí | Cách đáp ứng |
|---|---|
| **Tái sử dụng** | Công thức giá, cọc, hoàn và kiểm tra lịch nằm ở một lớp dịch vụ dùng chung; trang khách và nội bộ dùng chung dữ liệu giá; layout/partial view dùng chung. |
| **Mở rộng** | Cơ sở, hạng, gói, nhóm ngày và tham số chính sách (15 phút, 50%, 20 phút, 3 lần, mốc 72/48 giờ) là dữ liệu cấu hình; thêm gói mới không sửa mã. |
| **Bảo trì** | Phân lớp `Homestay.Web` / `Homestay.Core` / `Homestay.Infrastructure` / `Homestay.Tests`; EF Core migration có phiên bản; kiểm thử tự động cho các BR chính. |
| **Hợp tác** | Nhánh `main` được bảo vệ, nhánh tính năng theo mã UC, pull request có người review; Docker Compose và dữ liệu seed giống nhau cho cả nhóm. |

Docker khóa một bộ phiên bản để cả nhóm chạy giống nhau, nhưng không làm mọi phiên bản .NET/MySQL tương thích với nhau. Máy chạy vẫn cần Docker Engine/Desktop và tài nguyên phù hợp.

## 7. Quyết định đã chốt và câu hỏi còn mở

**Đã chốt ngày 24/10/2026:** D01 gói ngày 12h–10h và combo theo ảnh; D02 trang khách chỉ thông tin và liên hệ; D03 phân quyền theo cơ sở, chặn toàn hệ thống, duyệt mở chặn trong ứng dụng; D04 ranh giới cọc; D05 đã cọc không đến không là vi phạm; D06 hư hỏng ngoài phạm vi; D07 khung 24h theo đêm đặt, Lễ/Tết không đổi giá; D08 Docker và bốn tiêu chí kỹ thuật; D09 đơn đang hoạt động/lịch sử đơn; D10 cọc 50–100% và ghi chú tỷ lệ khi không đến; D11 gói chuẩn + giờ thêm nguyên; D12 tối đa 2 đơn chờ cọc/SĐT; D13 .NET 10 LTS, demo một chi nhánh; D14 bằng chứng hủy bằng ảnh/ghi âm; D15 phí trả trễ và gia hạn báo trước 1 giờ; D16 tự check-in, không nhận sớm; D17 hoàn trên toàn bộ tiền cọc; D18 một đơn một phòng, chỉ cảnh báo đơn chồng giờ, tệp 20 MB; D19 bộ phiên bản; D20 thuê nhiều ngày theo tổng giá từng ngày; D21 lưu tệp 30 ngày, kiểm tra đơn hủy hằng tuần; D22 không lưu mã hộp khóa.

**Còn mở:**

1. **Q04** — Hủy sau giờ bắt đầu (đề xuất 0%, nhất quán với mức dưới 48 giờ).

Các câu hỏi Q01–Q03 và Q05–Q10 đã đóng.

**Bộ phiên bản khóa trong Docker** (tra cứu ngày 24/10/2026):

| Thành phần | Phiên bản | Ghi chú |
|---|---|---|
| .NET SDK (build) | `mcr.microsoft.com/dotnet/sdk:10.0.402` | .NET 10 LTS, hỗ trợ đến 11/2028 |
| ASP.NET Core runtime | `mcr.microsoft.com/dotnet/aspnet:10.0.13` | Image chạy |
| EF Core | `Microsoft.EntityFrameworkCore` 10.0.x | Cùng dòng .NET 10 |
| Thư viện MySQL | `MySql.EntityFrameworkCore` 10.0.12 (Oracle) | **Không dùng Pomelo**: chưa có bản cho EF Core 10 (mới nhất 9.0.0) |
| MySQL | `mysql:8.4.13` | MySQL 8.4 LTS |

Không dùng tag trôi như `latest`. Nâng bản vá có chủ đích: cập nhật tag, chạy lại kiểm thử rồi mới gộp vào `main`.

Nhóm tiếp tục phần đã chốt; chức năng phụ thuộc phải chờ quyết định, không tự đặt chính sách.

## 8. Điều phối và nghiệm thu

Cuối ngày dành **30 phút trong quỹ sáu giờ** để xác định phần hoàn thành có thể kiểm tra, vướng mắc cần leader xử lý và đầu ra cụ thể cho ngày mai.

### Checklist bản nộp

- [ ] Đủ ba chương và dữ liệu ba mức.
- [ ] PRD v1.4, UC, giá, workflow, sơ đồ và dữ liệu nhất quán; không còn nội dung báo cáo hư hỏng từ plan cũ.
- [ ] Điểm chưa chốt được ghi rõ; không tự thêm chức năng.
- [ ] Phân biệt thiết kế, phần đã code và phần đã kiểm thử.
- [ ] Mục lục, hình/bảng, thuật ngữ và chính tả được rà soát.
- [ ] Mở trên WPS không vỡ bảng, mất chữ hoặc sai mục lục.
- [ ] Leader duyệt bản cuối.

### Đánh giá mốc triển khai

Leader duyệt backlog và khối lượng ước lượng trước khi đo tiến độ. Tỷ lệ hoàn thành = khối lượng chức năng đã nghiệm thu / tổng khối lượng đã duyệt; không đổi mẫu số để đạt 50%.

Ca ưu tiên: đặt trùng; tối đa 2 đơn chờ cọc; giờ thêm nguyên, từ chối 30 phút; phí trả trễ 4/5/11/35 phút; không gửi mã trước giờ bắt đầu; cảnh báo đơn cùng SĐT chồng giờ ở phòng khác; thuê 2 ngày thứ Sáu + thứ Bảy; xóa tệp bằng chứng chỉ khi đủ 30 ngày và đã kiểm tra; hết hạn 15 phút, đúng mốc 15:00 và tác vụ lặp; hủy thiếu tệp bằng chứng; tiền đúng hạn nhưng nhập muộn; chặn ở cơ sở A hiển thị ở cơ sở B; mở chặn chỉ quản lý duyệt; hoàn cọc tại 48/72 giờ; khách đã cọc không đến và ghi chú tỷ lệ cọc; đơn kết thúc chuyển sang lịch sử; khung 24h tối thứ Sáu; container giờ UTC; dọn 20 phút; không ghi thu tiền trùng. Chỉ ghi **đã đạt** sau khi thực chạy và có bằng chứng.

> Đây là tài liệu kế hoạch và yêu cầu, không xác nhận ứng dụng, Docker hoặc database đã triển khai.
=======
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
>>>>>>> van-viet
