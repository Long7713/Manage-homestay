using System.Drawing;
using System.Windows.Forms;
using HenryHomestay.BLL;
using HenryHomestay.Core.Entities;
using HenryHomestay.Core.Models;
using HenryHomestay.DAL;

namespace HenryHomestay.Admin;

public static class AdminDialogs
{
    private static readonly Color ColGround = Color.FromArgb(252, 249, 248);     // #fcf9f8
    private static readonly Color ColSurface = Color.White;                      // #ffffff
    private static readonly Color ColSoft = Color.FromArgb(246, 243, 242);       // #f6f3f2
    private static readonly Color ColSoftWarm = Color.FromArgb(250, 247, 244);   // #faf7f4
    private static readonly Color ColInk = Color.FromArgb(34, 34, 34);           // #222222
    private static readonly Color ColMuted = Color.FromArgb(101, 101, 101);      // #656565
    private static readonly Color ColLine = Color.FromArgb(233, 229, 227);       // #e9e5e3
    private static readonly Color ColBorderInput = Color.FromArgb(214, 208, 206);// #d6d0ce
    private static readonly Color ColAccent = Color.FromArgb(186, 0, 54);        // #ba0036
    private static readonly Color ColGreen = Color.FromArgb(0, 106, 69);         // #006a45
    private static readonly Color ColAmber = Color.FromArgb(180, 83, 9);         // #b45309
    private static readonly Color ColAmberSoft = Color.FromArgb(254, 243, 199);  // #fef3c7
    private static readonly Color ColRedSoft = Color.FromArgb(252, 232, 236);    // #fce8ec
    private static readonly Color ColRedBorder = Color.FromArgb(243, 193, 204);  // #f3c1cc

    public static string ResolveImagePath(string imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName)) return "";
        var trimmed = imageName.Trim();
        if (File.Exists(trimmed)) return trimmed;
        var fileName = Path.GetFileName(trimmed);
        if (string.IsNullOrWhiteSpace(fileName)) return "";

        var pWeb = Path.Combine(DatabaseConfigManager.GetSolutionRoot(), "HenryHomestay.Web", "wwwroot", "assets", "images", fileName);
        if (File.Exists(pWeb)) return pWeb;
        var pUser = Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "user", "assets", "images", fileName);
        if (File.Exists(pUser)) return pUser;
        var pLegacy = Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "henry-homestay", "assets", "images", fileName);
        return File.Exists(pLegacy) ? pLegacy : "";
    }

    /// <summary>
    /// Copies a local image file into the Web wwwroot assets folder and (if present) the user assets folder,
    /// returning either a web-relative path ("assets/images/...") or just the destination filename.
    /// </summary>
    public static string CopyImageToAssets(string sourceFilePath, string filePrefix, bool returnWebRelativePath = false)
    {
        var ext = Path.GetExtension(sourceFilePath);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".png";
        var destFileName = $"{filePrefix}-{DateTime.Now:yyyyMMddHHmmss}{ext.ToLowerInvariant()}";

        var webAssetsDir = Path.Combine(DatabaseConfigManager.GetSolutionRoot(), "HenryHomestay.Web", "wwwroot", "assets", "images");
        Directory.CreateDirectory(webAssetsDir);
        File.Copy(sourceFilePath, Path.Combine(webAssetsDir, destFileName), overwrite: true);

        var userAssetsDir = Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "user", "assets", "images");
        if (Directory.Exists(userAssetsDir))
        {
            File.Copy(sourceFilePath, Path.Combine(userAssetsDir, destFileName), overwrite: true);
        }

        return returnWebRelativePath ? $"assets/images/{destFileName}" : destFileName;
    }

    public static Image? LoadImageSafe(string imageNameOrPath)
    {
        var path = ResolveImagePath(imageNameOrPath);
        if (string.IsNullOrEmpty(path)) return null;
        if (string.Equals(Path.GetExtension(path), ".svg", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var img = Image.FromStream(fs);
            return new Bitmap(img);
        }
        catch
        {
            return null;
        }
    }

    public static Bitmap CreateQrPlaceholderBitmap(int size = 160, string caption = "VietQR · SVG")
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);

        using var darkBrush = new SolidBrush(ColInk);
        using var softBrush = new SolidBrush(ColSoft);
        using var accentBrush = new SolidBrush(ColAccent);
        using var borderPen = new Pen(ColLine, 1);

        g.DrawRectangle(borderPen, 0, 0, size - 1, size - 1);

        int pad = 18;
        int finder = 34;
        void DrawFinder(int x, int y)
        {
            g.FillRectangle(darkBrush, x, y, finder, finder);
            g.FillRectangle(Brushes.White, x + 5, y + 5, finder - 10, finder - 10);
            g.FillRectangle(accentBrush, x + 10, y + 10, finder - 20, finder - 20);
        }

        DrawFinder(pad, pad);
        DrawFinder(size - pad - finder, pad);
        DrawFinder(pad, size - pad - finder - 14);

        // Decorative QR data blocks
        int[] xs = [64, 76, 88, 100, 64, 88, 112, 76, 100, 112];
        int[] ys = [26, 38, 26, 50, 64, 76, 64, 88, 88, 100];
        for (int i = 0; i < xs.Length; i++)
        {
            g.FillRectangle(i % 3 == 0 ? softBrush : darkBrush, xs[i], ys[i], 8, 8);
        }

        using var font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        using var mutedBrush = new SolidBrush(ColMuted);
        var sf = new StringFormat { Alignment = StringAlignment.Center };
        g.DrawString(caption, font, mutedBrush, new RectangleF(0, size - 24, size, 20), sf);
        return bmp;
    }

    private static void StyleDialogGrid(DataGridView dgv)
    {
        dgv.BackgroundColor = ColSurface;
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = ColSoft;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = ColMuted;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.8F, FontStyle.Bold);
        dgv.ColumnHeadersHeight = 36;
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        dgv.DefaultCellStyle.ForeColor = ColInk;
        dgv.DefaultCellStyle.SelectionBackColor = ColSoftWarm;
        dgv.DefaultCellStyle.SelectionForeColor = ColInk;
        dgv.RowTemplate.Height = 34;
        dgv.GridColor = ColLine;
    }

    private static Button MakeNeutralButton(string text, Point loc, Size size)
    {
        var b = new Button
        {
            Text = text,
            Location = loc,
            Size = size,
            FlatStyle = FlatStyle.Flat,
            BackColor = ColSoft,
            ForeColor = ColInk,
            Font = new Font("Segoe UI", 9.2F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = ColBorderInput;
        b.FlatAppearance.BorderSize = 1;
        return b;
    }

    private static Button MakePrimaryButton(string text, Point loc, Size size)
    {
        var b = new Button
        {
            Text = text,
            Location = loc,
            Size = size,
            FlatStyle = FlatStyle.Flat,
            BackColor = ColAccent,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    public static void ShowRoomDetailDialog(
        Form owner,
        RoomDisplayDto room,
        BookingService bookingService,
        Action onEditRequested)
    {
        using var dlg = new Form
        {
            Text = $"Chi tiết {room.Name} (Mã: {room.RoomCode})",
            Size = new Size(680, 650),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ColGround,
            Font = new Font("Segoe UI", 9.5F)
        };

        var pic = new PictureBox
        {
            Location = new Point(24, 18),
            Size = new Size(615, 200),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = ColSoft
        };
        pic.Image = LoadImageSafe(room.PrimaryImage);
        dlg.Controls.Add(pic);

        var infoText =
            $"• Tên phòng: {room.Name}   |   Mã phòng: {room.RoomCode}   |   Loại phòng: {room.TypeName}\n" +
            $"• Diện tích: {room.AreaM2} m²   |   Giường: {room.BedInfo}   |   Hướng nhìn: {room.ViewDescription}\n" +
            $"• Tiện ích: {(room.AmenityNames.Any() ? string.Join(", ", room.AmenityNames) : "—")}\n" +
            $"• Số khách tối đa: {room.MaxGuests} người   |   Cho phép thú cưng: {(room.AllowPets ? "Có" : "Không")}\n" +
            $"• Giá mỗi đêm: {room.PricePerNight:N0} ₫   |   Hiển thị Web: {(room.Status == "ACTIVE" ? "Đang hiển thị" : "Đã ẩn")}\n" +
            $"• Tình trạng hiện tại: {room.OccupancyStatus}\n" +
            $"• Mô tả: {room.Description}";

        var lblInfo = new Label
        {
            Text = infoText,
            Location = new Point(24, 230),
            Size = new Size(615, 145),
            ForeColor = ColInk
        };
        dlg.Controls.Add(lblInfo);

        var lblUp = new Label
        {
            Text = "Lịch đặt sắp tới của phòng:",
            Location = new Point(24, 380),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = ColInk
        };
        dlg.Controls.Add(lblUp);

        var dgv = new DataGridView
        {
            Location = new Point(24, 406),
            Size = new Size(615, 140),
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        StyleDialogGrid(dgv);
        dgv.Columns.Add("Code", "Mã đơn");
        dgv.Columns.Add("Guest", "Khách hàng");
        dgv.Columns.Add("CheckIn", "Nhận phòng");
        dgv.Columns.Add("CheckOut", "Trả phòng");
        dgv.Columns.Add("Status", "Trạng thái");

        var upcoming = bookingService.GetBookings(roomId: room.Id)
            .Where(b => b.Status != "CANCELLED" && b.CheckOutDate >= DateTime.Today)
            .OrderBy(b => b.CheckInDate)
            .ToList();

        foreach (var b in upcoming)
        {
            dgv.Rows.Add(b.BookingCode, b.CustomerName, b.CheckInDate.ToString("dd/MM/yyyy"), b.CheckOutDate.ToString("dd/MM/yyyy"), b.StatusDisplay);
        }
        dlg.Controls.Add(dgv);

        var btnEdit = MakeNeutralButton("Sửa thông tin phòng", new Point(375, 560), new Size(155, 36));
        btnEdit.Click += (_, _) => { dlg.Close(); onEditRequested(); };

        var btnClose = MakeNeutralButton("Đóng", new Point(540, 560), new Size(98, 36));
        btnClose.BackColor = ColSurface;
        btnClose.Click += (_, _) => dlg.Close();

        dlg.Controls.AddRange(new Control[] { btnEdit, btnClose });
        dlg.ShowDialog(owner);
    }

    public static bool ShowRoomFormDialog(
        Form owner,
        RoomService roomService,
        RoomDisplayDto? existingRoom)
    {
        bool isNew = existingRoom == null;
        bool saved = false;
        var roomTypes = roomService.GetRoomTypes();

        using var dlg = new Form
        {
            Text = isNew ? "Thêm phòng mới" : $"Sửa thông tin {existingRoom!.Name}",
            Size = new Size(620, 620),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ColGround,
            Font = new Font("Segoe UI", 9.5F)
        };

        Label Lbl(string text, int x, int y) => new() { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ColInk };

        dlg.Controls.Add(Lbl("Mã phòng (VD: 41, 82)", 24, 18));
        var txtCode = new TextBox { Location = new Point(24, 40), Width = 140, Text = existingRoom?.RoomCode ?? $"R{DateTime.Now:mmss}" };

        dlg.Controls.Add(Lbl("Tên phòng *", 180, 18));
        var txtName = new TextBox { Location = new Point(180, 40), Width = 220, Text = existingRoom?.Name ?? "" };

        dlg.Controls.Add(Lbl("Loại phòng *", 415, 18));
        var cboType = new ComboBox { Location = new Point(415, 40), Width = 165, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var rt in roomTypes) cboType.Items.Add(rt.Name);
        cboType.SelectedIndex = existingRoom != null ? Math.Max(0, roomTypes.FindIndex(t => t.Id == existingRoom.RoomTypeId)) : 0;

        dlg.Controls.Add(Lbl("Diện tích (m²)", 24, 82));
        var numArea = new NumericUpDown { Location = new Point(24, 104), Width = 140, Minimum = 10, Maximum = 500, Value = existingRoom?.AreaM2 ?? 35 };

        dlg.Controls.Add(Lbl("Thông tin giường", 180, 82));
        var txtBed = new TextBox { Location = new Point(180, 104), Width = 190, Text = existingRoom?.BedInfo ?? "1 giường Queen" };

        dlg.Controls.Add(Lbl("Hướng nhìn (View)", 385, 82));
        var txtView = new TextBox { Location = new Point(385, 104), Width = 195, Text = existingRoom?.ViewDescription ?? "Ban công đón nắng" };

        dlg.Controls.Add(Lbl("Mô tả chi tiết", 24, 146));
        var txtDesc = new TextBox { Location = new Point(24, 168), Width = 556, Height = 64, Multiline = true, Text = existingRoom?.Description ?? "" };

        dlg.Controls.Add(Lbl("Tiện ích phòng", 24, 246));
        var chkWifi = new CheckBox { Text = "Wi-Fi", Location = new Point(24, 270), AutoSize = true, Checked = existingRoom == null || existingRoom.AmenityCodes.Contains("wifi") };
        var chkAircon = new CheckBox { Text = "Điều hòa", Location = new Point(130, 270), AutoSize = true, Checked = existingRoom == null || existingRoom.AmenityCodes.Contains("aircon") };
        var chkBalcony = new CheckBox { Text = "Ban công riêng", Location = new Point(250, 270), AutoSize = true, Checked = existingRoom != null && existingRoom.AmenityCodes.Contains("balcony") };
        var chkKitchen = new CheckBox { Text = "Bếp nhỏ", Location = new Point(400, 270), AutoSize = true, Checked = existingRoom != null && existingRoom.AmenityCodes.Contains("kitchen") };

        dlg.Controls.Add(Lbl("Số người tối đa", 24, 312));
        var numGuests = new NumericUpDown { Location = new Point(24, 334), Width = 160, Minimum = 1, Maximum = 20, Value = existingRoom?.MaxGuests ?? 2 };

        dlg.Controls.Add(Lbl("Cho phép thú cưng", 210, 312));
        var cboPets = new ComboBox { Location = new Point(210, 334), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
        cboPets.Items.AddRange(new object[] { "Không", "Có" });
        cboPets.SelectedIndex = existingRoom != null && existingRoom.AllowPets ? 1 : 0;

        dlg.Controls.Add(Lbl("Giá / đêm (₫)", 395, 312));
        var numPrice = new NumericUpDown { Location = new Point(395, 334), Width = 185, Minimum = 0, Maximum = 100000000, Increment = 50000, ThousandsSeparator = true, Value = existingRoom?.PricePerNight ?? 1190000 };

        dlg.Controls.Add(Lbl("Ảnh đại diện (trong assets/images)", 24, 380));
        var cboImage = new ComboBox { Location = new Point(24, 402), Width = 240 };
        for (int i = 1; i <= 18; i++) cboImage.Items.Add($"photo-{i:D2}.jpg");
        cboImage.Text = existingRoom?.PrimaryImage ?? "photo-02.jpg";

        var btnBrowseImg = MakeNeutralButton("Chọn...", new Point(272, 401), new Size(82, 28));
        btnBrowseImg.Click += (_, _) =>
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh đại diện phòng",
                Filter = "Hình ảnh (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp"
            };
            if (ofd.ShowDialog(dlg) != DialogResult.OK) return;

            var info = new FileInfo(ofd.FileName);
            if (info.Length > 2 * 1024 * 1024)
            {
                MessageBox.Show("Ảnh quá lớn (tối đa 2 MB).", "Kiểm tra tệp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            cboImage.Text = ofd.FileName;
        };

        dlg.Controls.Add(Lbl("Hiển thị trên Website", 380, 380));
        var cboVisible = new ComboBox { Location = new Point(380, 402), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        cboVisible.Items.AddRange(new object[] { "Đang hiển thị", "Ẩn" });
        cboVisible.SelectedIndex = existingRoom != null && existingRoom.Status == "HIDDEN" ? 1 : 0;

        var btnSave = MakePrimaryButton(isNew ? "Thêm phòng" : "Lưu thay đổi", new Point(320, 515), new Size(145, 40));
        btnSave.Click += (_, _) =>
        {
            string imageValue = cboImage.Text.Trim();
            if (!string.IsNullOrWhiteSpace(imageValue) && Path.IsPathRooted(imageValue) && File.Exists(imageValue))
            {
                try
                {
                    imageValue = CopyImageToAssets(imageValue, "room", returnWebRelativePath: false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể sao chép ảnh phòng: {ex.Message}", "Lỗi tệp ảnh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else if (imageValue.Contains('/') || imageValue.Contains('\\'))
            {
                imageValue = Path.GetFileName(imageValue);
            }

            var ams = new List<string>();
            if (chkWifi.Checked) ams.Add("wifi");
            if (chkAircon.Checked) ams.Add("aircon");
            if (chkBalcony.Checked) ams.Add("balcony");
            if (chkKitchen.Checked) ams.Add("kitchen");

            int typeId = roomTypes[Math.Max(0, cboType.SelectedIndex)].Id;
            var res = roomService.SaveRoom(
                existingRoom?.Id,
                txtCode.Text,
                txtName.Text,
                typeId,
                (int)numArea.Value,
                txtBed.Text,
                txtView.Text,
                txtDesc.Text,
                (byte)numGuests.Value,
                cboPets.SelectedIndex == 1,
                numPrice.Value,
                cboVisible.SelectedIndex == 1 ? "HIDDEN" : "ACTIVE",
                imageValue,
                ams);

            if (!res.Success)
            {
                MessageBox.Show(res.Message, "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            saved = true;
            MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dlg.Close();
        };

        var btnCancel = MakeNeutralButton("Đóng", new Point(475, 515), new Size(105, 40));
        btnCancel.BackColor = ColSurface;
        btnCancel.Click += (_, _) => dlg.Close();

        dlg.Controls.AddRange(new Control[]
        {
            txtCode, txtName, cboType, numArea, txtBed, txtView, txtDesc,
            chkWifi, chkAircon, chkBalcony, chkKitchen,
            numGuests, cboPets, numPrice, cboImage, btnBrowseImg, cboVisible,
            btnSave, btnCancel
        });

        dlg.ShowDialog(owner);
        return saved;
    }

    public static void ShowOrderDetailDialog(
        Form owner,
        BookingSummaryDto o,
        BookingService bookingService,
        PaymentService paymentService,
        RoomService roomService,
        long staffUserId,
        Action onRefresh,
        Action onGoToPaymentConfirmTab)
    {
        using var dlg = new Form
        {
            Text = $"Chi tiết đơn đặt phòng {o.BookingCode}",
            Size = new Size(660, 560),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ColGround,
            Font = new Font("Segoe UI", 9.5F)
        };

        var lblStatus = new Label
        {
            Text = $"Trạng thái: {o.StatusDisplay.ToUpperInvariant()}",
            Location = new Point(24, 18),
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = o.Status == "CANCELLED" ? ColAccent : ColGreen
        };
        dlg.Controls.Add(lblStatus);

        int topY = 50;
        if (o.Status == "CANCELLED")
        {
            string refundNote = o.PaidAmount <= 0
                ? "Đơn đã hủy, chưa có khoản thanh toán nào cần hoàn."
                : o.RefundStatus == "REFUNDED"
                    ? $"Đã hoàn {o.PaidAmount:N0} ₫ cho khách (đã xác nhận trên hệ thống)."
                    : o.RefundStatus == "WAITING_REFUND"
                        ? $"Hủy trước 24h nhận phòng → hoàn tiền cọc. Cần hoàn {o.PaidAmount:N0} ₫ cho khách."
                        : $"Hủy sau 24h nhận phòng → không hoàn tiền cọc ({o.PaidAmount:N0} ₫).";

            var lblRefundBanner = new Label
            {
                Text = refundNote,
                Location = new Point(24, topY),
                Size = new Size(595, 36),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = o.RefundStatus == "WAITING_REFUND" ? ColAmberSoft : ColSoft,
                ForeColor = o.RefundStatus == "WAITING_REFUND" ? ColAmber : ColInk,
                Font = new Font("Segoe UI", 9.2F, FontStyle.Bold)
            };
            dlg.Controls.Add(lblRefundBanner);
            topY += 46;
        }

        string details =
            "1. THÔNG TIN KHÁCH HÀNG\n" +
            $"   • Họ và tên: {o.CustomerName}          • Số điện thoại: {o.CustomerPhone}\n" +
            $"   • Email: {o.CustomerEmail}          • Số CCCD: {(string.IsNullOrEmpty(o.CustomerCccd) ? "—" : o.CustomerCccd)}\n\n" +
            "2. THÔNG TIN LƯU TRÚ\n" +
            $"   • Phòng đã đặt: {o.RoomName} (Mã phòng: {o.RoomCode})\n" +
            $"   • Ngày nhận phòng: {o.CheckInDate:dd/MM/yyyy} (từ 14:00)   →   Ngày trả phòng: {o.CheckOutDate:dd/MM/yyyy} (trước 12:00)\n" +
            $"   • Số đêm lưu trú: {o.Nights} đêm\n" +
            $"   • Số lượng khách: {o.Adults} người lớn, {o.Children} trẻ em, {o.Infants} em bé, {o.Pets} thú cưng\n\n" +
            "3. CHI TIẾT THANH TOÁN\n" +
            $"   • Hình thức thanh toán: {(o.PaymentOption == "DEPOSIT" ? $"Đặt cọc trước {o.DepositPercent}%" : "Thanh toán toàn bộ 100%")}\n" +
            $"   • Tổng tiền đơn: {o.TotalAmount:N0} ₫\n" +
            $"   • Đã thanh toán: {o.PaidAmount:N0} ₫          • Còn lại: {o.RemainingAmount:N0} ₫\n\n" +
            $"Mã đơn {o.BookingCode} · tạo lúc {o.CreatedAt:dd/MM/yyyy HH:mm}";

        var lblBody = new Label
        {
            Text = details,
            Location = new Point(24, topY),
            Size = new Size(595, 340),
            ForeColor = ColInk
        };
        dlg.Controls.Add(lblBody);

        var flowBtns = new FlowLayoutPanel
        {
            Location = new Point(24, 450),
            Size = new Size(600, 48),
            FlowDirection = FlowDirection.RightToLeft
        };
        dlg.Controls.Add(flowBtns);

        Button MakeBtn(string text, Color bg, Color fg, EventHandler onClick, Color? borderColor = null)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(95, 36),
                Padding = new Padding(10, 4, 10, 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            if (borderColor.HasValue)
            {
                b.FlatAppearance.BorderColor = borderColor.Value;
                b.FlatAppearance.BorderSize = 1;
            }
            else
            {
                b.FlatAppearance.BorderSize = 0;
            }
            b.Click += onClick;
            return b;
        }

        flowBtns.Controls.Add(MakeBtn("Đóng", ColSurface, ColInk, (_, _) => dlg.Close(), ColLine));

        if (o.Status != "CANCELLED" && o.Status != "COMPLETED")
        {
            flowBtns.Controls.Add(MakeBtn("Hủy đơn", ColRedSoft, ColAccent, (_, _) =>
            {
                if (MessageBox.Show($"Xác nhận khách đã liên hệ hủy đơn {o.BookingCode}?", "Xác nhận hủy đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                var res = bookingService.CancelBooking(o.Id, staffUserId, "Nhân viên hủy đơn trên ứng dụng Quản trị");
                MessageBox.Show(res.Message, "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dlg.Close();
                onRefresh();
            }, ColRedBorder));

            flowBtns.Controls.Add(MakeBtn("Sửa đơn", ColSoft, ColInk, (_, _) =>
            {
                dlg.Close();
                ShowOrderEditDialog(owner, o, bookingService, roomService, onRefresh);
            }, ColBorderInput));
        }

        if (o.Status == "PENDING_PAYMENT")
        {
            flowBtns.Controls.Add(MakeBtn("Đến xác nhận thanh toán", ColAccent, Color.White, (_, _) =>
            {
                dlg.Close();
                onGoToPaymentConfirmTab();
            }));
        }
        else if (o.Status == "CONFIRMED")
        {
            flowBtns.Controls.Add(MakeBtn("Khách nhận phòng", ColGreen, Color.White, (_, _) =>
            {
                var res = bookingService.AdvanceBookingStatus(o.Id, "CHECKED_IN");
                MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dlg.Close();
                onRefresh();
            }));
        }
        else if (o.Status == "CHECKED_IN")
        {
            flowBtns.Controls.Add(MakeBtn("Hoàn thành", ColGreen, Color.White, (_, _) =>
            {
                var res = bookingService.AdvanceBookingStatus(o.Id, "COMPLETED");
                MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dlg.Close();
                onRefresh();
            }));
        }
        else if (o.Status == "CANCELLED" && o.RefundStatus == "WAITING_REFUND")
        {
            flowBtns.Controls.Add(MakeBtn("Xác nhận đã hoàn tiền", ColGreen, Color.White, (_, _) =>
            {
                var res = paymentService.ConfirmRefund(o.Id, staffUserId);
                MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dlg.Close();
                onRefresh();
            }));
        }

        dlg.ShowDialog(owner);
    }

    public static void ShowOrderEditDialog(
        Form owner,
        BookingSummaryDto o,
        BookingService bookingService,
        RoomService roomService,
        Action onRefresh)
    {
        var rooms = roomService.GetRooms(includeHidden: true)
            .Where(r => r.Status == "ACTIVE" || r.Id == o.RoomId)
            .ToList();

        using var dlg = new Form
        {
            Text = $"Sửa đơn đặt phòng {o.BookingCode}",
            Size = new Size(520, 360),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ColGround,
            Font = new Font("Segoe UI", 9.5F)
        };

        var lblIn = new Label { Text = "Ngày nhận phòng:", Location = new Point(24, 22), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ColInk };
        var dtpIn = new DateTimePicker { Location = new Point(24, 45), Width = 215, Format = DateTimePickerFormat.Short, Value = o.CheckInDate };

        var lblOut = new Label { Text = "Ngày trả phòng:", Location = new Point(260, 22), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ColInk };
        var dtpOut = new DateTimePicker { Location = new Point(260, 45), Width = 215, Format = DateTimePickerFormat.Short, Value = o.CheckOutDate };

        var lblRoom = new Label { Text = "Phòng lưu trú:", Location = new Point(24, 92), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ColInk };
        var cboRoom = new ComboBox { Location = new Point(24, 115), Width = 450, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var r in rooms)
            cboRoom.Items.Add($"{r.Name} · {r.PricePerNight:N0} ₫/đêm · tối đa {r.MaxGuests} người");
        cboRoom.SelectedIndex = Math.Max(0, rooms.FindIndex(r => r.Id == o.RoomId));

        var lblNote = new Label
        {
            Text = "Hệ thống sẽ tự động kiểm tra trùng lịch (room_nights): nếu ngày hoặc phòng mới đã có đơn khác thì không thể đổi.",
            Location = new Point(24, 156),
            Size = new Size(450, 40),
            ForeColor = ColMuted
        };

        var lblErr = new Label
        {
            Text = "",
            Location = new Point(24, 198),
            Size = new Size(450, 48),
            ForeColor = ColAccent,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        var btnSave = MakePrimaryButton("Lưu thay đổi", new Point(245, 255), new Size(125, 38));
        btnSave.Click += (_, _) =>
        {
            int targetRoomId = rooms[Math.Max(0, cboRoom.SelectedIndex)].Id;
            var res = bookingService.UpdateBookingScheduleOrRoom(o.Id, dtpIn.Value.Date, dtpOut.Value.Date, targetRoomId);
            if (!res.Success)
            {
                lblErr.Text = res.Message;
                return;
            }
            MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dlg.Close();
            onRefresh();
        };

        var btnClose = MakeNeutralButton("Đóng", new Point(380, 255), new Size(94, 38));
        btnClose.BackColor = ColSurface;
        btnClose.Click += (_, _) => dlg.Close();

        dlg.Controls.AddRange(new Control[] { lblIn, dtpIn, lblOut, dtpOut, lblRoom, cboRoom, lblNote, lblErr, btnSave, btnClose });
        dlg.ShowDialog(owner);
    }

    public static void ShowCreateBookingDialog(
        Form owner,
        BookingService bookingService,
        RoomService roomService,
        Action onRefresh)
    {
        var rooms = roomService.GetRooms(includeHidden: false);
        if (!rooms.Any())
        {
            MessageBox.Show("Chưa có phòng nào đang mở bán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dlg = new Form
        {
            Text = "Tạo đơn đặt phòng mới (Khách đặt tại quầy / qua điện thoại)",
            Size = new Size(560, 500),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ColGround,
            Font = new Font("Segoe UI", 9.5F)
        };

        Label Lbl(string t, int x, int y) => new() { Text = t, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ColInk };

        dlg.Controls.Add(Lbl("Họ và tên khách *", 24, 18));
        var txtName = new TextBox { Location = new Point(24, 40), Width = 240 };

        dlg.Controls.Add(Lbl("Số điện thoại *", 280, 18));
        var txtPhone = new TextBox { Location = new Point(280, 40), Width = 235 };

        dlg.Controls.Add(Lbl("Email *", 24, 78));
        var txtEmail = new TextBox { Location = new Point(24, 100), Width = 240, Text = "khach@henry.example" };

        dlg.Controls.Add(Lbl("Số CCCD", 280, 78));
        var txtCccd = new TextBox { Location = new Point(280, 100), Width = 235 };

        dlg.Controls.Add(Lbl("Chọn phòng *", 24, 140));
        var cboRoom = new ComboBox { Location = new Point(24, 162), Width = 491, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var r in rooms)
            cboRoom.Items.Add($"{r.Name} ({r.RoomCode}) · {r.PricePerNight:N0} ₫/đêm · tối đa {r.MaxGuests} khách");
        cboRoom.SelectedIndex = 0;

        dlg.Controls.Add(Lbl("Ngày nhận phòng", 24, 204));
        var dtpIn = new DateTimePicker { Location = new Point(24, 226), Width = 240, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(1) };

        dlg.Controls.Add(Lbl("Ngày trả phòng", 280, 204));
        var dtpOut = new DateTimePicker { Location = new Point(280, 226), Width = 235, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(3) };

        dlg.Controls.Add(Lbl("Người lớn", 24, 268));
        var numAdults = new NumericUpDown { Location = new Point(24, 290), Width = 110, Minimum = 1, Maximum = 15, Value = 2 };

        dlg.Controls.Add(Lbl("Trẻ em", 150, 268));
        var numChildren = new NumericUpDown { Location = new Point(150, 290), Width = 110, Minimum = 0, Maximum = 10, Value = 0 };

        dlg.Controls.Add(Lbl("Thú cưng", 280, 268));
        var numPets = new NumericUpDown { Location = new Point(280, 290), Width = 100, Minimum = 0, Maximum = 5, Value = 0 };

        dlg.Controls.Add(Lbl("Hình thức TT", 395, 268));
        var cboOpt = new ComboBox { Location = new Point(395, 290), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        cboOpt.Items.AddRange(new object[] { "Đặt cọc 30%", "Toàn bộ 100%" });
        cboOpt.SelectedIndex = 0;

        var lblErr = new Label { Text = "", Location = new Point(24, 332), Size = new Size(491, 42), ForeColor = ColAccent, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

        var btnCreate = MakePrimaryButton("Tạo đơn đặt phòng", new Point(260, 390), new Size(150, 40));
        btnCreate.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                lblErr.Text = "Vui lòng nhập họ tên và số điện thoại khách hàng.";
                return;
            }
            var rm = rooms[Math.Max(0, cboRoom.SelectedIndex)];
            var res = bookingService.CreateBooking(
                null,
                rm.Id,
                dtpIn.Value.Date,
                dtpOut.Value.Date,
                (byte)numAdults.Value,
                (byte)numChildren.Value,
                0,
                (byte)numPets.Value,
                txtName.Text,
                txtPhone.Text,
                txtEmail.Text,
                txtCccd.Text,
                null,
                cboOpt.SelectedIndex == 0 ? "DEPOSIT" : "FULL");

            if (!res.Success)
            {
                lblErr.Text = res.Message;
                return;
            }
            MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dlg.Close();
            onRefresh();
        };

        var btnClose = MakeNeutralButton("Đóng", new Point(420, 390), new Size(95, 40));
        btnClose.BackColor = ColSurface;
        btnClose.Click += (_, _) => dlg.Close();

        dlg.Controls.AddRange(new Control[]
        {
            txtName, txtPhone, txtEmail, txtCccd, cboRoom, dtpIn, dtpOut,
            numAdults, numChildren, numPets, cboOpt, lblErr, btnCreate, btnClose
        });
        dlg.ShowDialog(owner);
    }

    public static void ShowCustomerHistoryDialog(Form owner, User customer, BookingService bookingService)
    {
        using var dlg = new Form
        {
            Text = $"Lịch sử đặt phòng — {customer.FullName} ({customer.Phone})",
            Size = new Size(720, 420),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ColGround,
            Font = new Font("Segoe UI", 9.5F)
        };

        var dgv = new DataGridView
        {
            Location = new Point(20, 20),
            Size = new Size(665, 295),
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        StyleDialogGrid(dgv);
        dgv.Columns.Add("Code", "Mã đơn");
        dgv.Columns.Add("Room", "Phòng");
        dgv.Columns.Add("In", "Nhận phòng");
        dgv.Columns.Add("Out", "Trả phòng");
        dgv.Columns.Add("Total", "Tổng tiền");
        dgv.Columns.Add("Status", "Trạng thái");

        var list = bookingService.GetBookings(userId: customer.Id);
        foreach (var o in list)
        {
            dgv.Rows.Add(o.BookingCode, o.RoomName, o.CheckInDate.ToString("dd/MM/yyyy"), o.CheckOutDate.ToString("dd/MM/yyyy"), $"{o.TotalAmount:N0} ₫", o.StatusDisplay);
        }
        dlg.Controls.Add(dgv);

        var btnClose = MakeNeutralButton("Đóng", new Point(585, 328), new Size(100, 36));
        btnClose.BackColor = ColSurface;
        btnClose.Click += (_, _) => dlg.Close();
        dlg.Controls.Add(btnClose);

        dlg.ShowDialog(owner);
    }

    public static void ShowStaffFormDialog(Form owner, UserService userService, User? existingStaff, Action onRefresh)
    {
        bool isNew = existingStaff == null;
        string[] roles = ["Quản lý", "Lễ tân", "Kế toán", "Buồng phòng", "Bảo vệ"];

        using var dlg = new Form
        {
            Text = isNew ? "Thêm nhân viên mới" : $"Sửa thông tin {existingStaff!.FullName}",
            Size = new Size(500, 380),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ColGround,
            Font = new Font("Segoe UI", 9.5F)
        };

        Label Lbl(string t, int x, int y) => new() { Text = t, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ColInk };

        dlg.Controls.Add(Lbl("Họ và tên *", 24, 18));
        var txtName = new TextBox { Location = new Point(24, 40), Width = 220, Text = existingStaff?.FullName ?? "" };

        dlg.Controls.Add(Lbl("Chức vụ *", 260, 18));
        var cboRole = new ComboBox { Location = new Point(260, 40), Width = 195, DropDownStyle = ComboBoxStyle.DropDownList };
        cboRole.Items.AddRange(roles);
        cboRole.SelectedItem = existingStaff?.StaffPosition ?? "Lễ tân";
        if (cboRole.SelectedIndex < 0) cboRole.SelectedIndex = 1;

        dlg.Controls.Add(Lbl("Số điện thoại *", 24, 82));
        var txtPhone = new TextBox { Location = new Point(24, 104), Width = 220, Text = existingStaff?.Phone ?? "" };

        dlg.Controls.Add(Lbl("Email *", 260, 82));
        var txtEmail = new TextBox { Location = new Point(260, 104), Width = 195, Text = existingStaff?.Email ?? "" };

        dlg.Controls.Add(Lbl("Ngày vào làm", 24, 146));
        var dtpHire = new DateTimePicker { Location = new Point(24, 168), Width = 220, Format = DateTimePickerFormat.Short, Value = existingStaff?.HireDate ?? DateTime.Today };

        dlg.Controls.Add(Lbl(isNew ? "Mật khẩu khởi tạo" : "Đổi mật khẩu (bỏ trống nếu giữ nguyên)", 260, 146));
        var txtPwd = new TextBox { Location = new Point(260, 168), Width = 195, Text = isNew ? "123456" : "" };

        var btnSave = MakePrimaryButton(isNew ? "Thêm nhân viên" : "Lưu thay đổi", new Point(215, 265), new Size(135, 38));
        btnSave.Click += (_, _) =>
        {
            var res = userService.SaveStaff(
                existingStaff?.Id,
                txtName.Text,
                cboRole.Text,
                txtPhone.Text,
                txtEmail.Text,
                dtpHire.Value,
                txtPwd.Text);

            if (!res.Success)
            {
                MessageBox.Show(res.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dlg.Close();
            onRefresh();
        };

        var btnClose = MakeNeutralButton("Đóng", new Point(360, 265), new Size(95, 38));
        btnClose.BackColor = ColSurface;
        btnClose.Click += (_, _) => dlg.Close();

        dlg.Controls.AddRange(new Control[] { txtName, cboRole, txtPhone, txtEmail, dtpHire, txtPwd, btnSave, btnClose });
        dlg.ShowDialog(owner);
    }
}
