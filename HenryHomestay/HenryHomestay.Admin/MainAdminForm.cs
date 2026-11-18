using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HenryHomestay.BLL;
using HenryHomestay.Core.Entities;
using HenryHomestay.Core.Models;
using HenryHomestay.DAL;

namespace HenryHomestay.Admin;

public class MainAdminForm : Form
{
    // Warm Hospitality Color Palette (matching D:\Csharp\admin\admin.css)
    private static readonly Color ColGround = Color.FromArgb(252, 249, 248);      // #fcf9f8
    private static readonly Color ColSurface = Color.White;                       // #ffffff
    private static readonly Color ColSoft = Color.FromArgb(246, 243, 242);        // #f6f3f2
    private static readonly Color ColSoftWarm = Color.FromArgb(250, 247, 244);    // #faf7f4
    private static readonly Color ColInk = Color.FromArgb(34, 34, 34);            // #222222
    private static readonly Color ColBrandDark = Color.FromArgb(27, 28, 28);      // #1b1c1c
    private static readonly Color ColMuted = Color.FromArgb(101, 101, 101);       // #656565
    private static readonly Color ColLine = Color.FromArgb(233, 229, 227);        // #e9e5e3
    private static readonly Color ColBorderInput = Color.FromArgb(214, 208, 206); // #d6d0ce
    private static readonly Color ColAccent = Color.FromArgb(186, 0, 54);         // #ba0036
    private static readonly Color ColAccentSoft = Color.FromArgb(253, 242, 244);  // #fdf2f4
    private static readonly Color ColGreen = Color.FromArgb(0, 106, 69);          // #006a45
    private static readonly Color ColGreenSoft = Color.FromArgb(233, 244, 238);   // #e9f4ee
    private static readonly Color ColBlue = Color.FromArgb(29, 78, 216);          // #1d4ed8
    private static readonly Color ColBlueSoft = Color.FromArgb(239, 246, 255);    // #eff6ff
    private static readonly Color ColAmber = Color.FromArgb(180, 83, 9);          // #b45309
    private static readonly Color ColAmberSoft = Color.FromArgb(254, 243, 199);   // #fef3c7
    private static readonly Color ColRedSoft = Color.FromArgb(252, 232, 236);     // #fce8ec
    private static readonly Color ColRedBorder = Color.FromArgb(243, 193, 204);   // #f3c1cc
    private static readonly Color ColTeal = Color.FromArgb(15, 118, 110);         // #0f766e
    private static readonly Color ColTealSoft = Color.FromArgb(240, 253, 250);    // #f0fdfa

    private readonly User _currentUser;
    private readonly RoomService _roomService = new();
    private readonly BookingService _bookingService = new();
    private readonly PaymentService _paymentService = new();
    private readonly UserService _userService = new();
    private readonly SystemService _systemService = new();

    private Panel _sidebar = null!;
    private Panel _viewContainer = null!;
    private Label _lblPageTitle = null!;
    private string _currentRoute = "rooms";
    private string _roomsSubTab = "list";
    private string _paymentsSubTab = "list";
    private DateTime _calMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    private readonly Dictionary<string, Button> _navButtons = new();

    public bool IsLogoutRequested { get; private set; }

    public MainAdminForm(User currentUser)
    {
        _currentUser = currentUser;
        Text = $"Quản trị | HENRY homestay — {_currentUser.FullName} ({_currentUser.StaffPosition ?? _currentUser.Role})";
        Size = new Size(1360, 840);
        MinimumSize = new Size(1120, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ColGround;
        Font = new Font("Segoe UI", 9.5F);

        BuildLayout();
        NavigateTo("rooms");
    }

    private void BuildLayout()
    {
        // Sidebar (Left, 260px, white with 1px #e9e5e3 right border)
        _sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 260,
            BackColor = ColSurface
        };
        var sidebarRightBorder = new Panel
        {
            Dock = DockStyle.Right,
            Width = 1,
            BackColor = ColLine
        };
        _sidebar.Controls.Add(sidebarRightBorder);
        Controls.Add(_sidebar);

        // Top .side-user card
        var userBoxHost = new Panel
        {
            Dock = DockStyle.Top,
            Height = 104,
            Padding = new Padding(14, 18, 14, 12),
            BackColor = ColSurface
        };
        var userCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColSoft
        };
        userCard.Paint += (_, e) =>
        {
            using var pen = new Pen(ColLine, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, userCard.Width - 1, userCard.Height - 1);
        };

        var avatarLbl = new Label
        {
            Text = "H",
            Size = new Size(42, 42),
            Location = new Point(14, 15),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = ColAccent,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 13.5F, FontStyle.Bold)
        };
        using (var gp = new GraphicsPath())
        {
            gp.AddEllipse(0, 0, 42, 42);
            avatarLbl.Region = new Region(gp);
        }

        var nameLbl = new Label
        {
            Text = "Hệ thống quản lý",
            Location = new Point(66, 15),
            AutoSize = true,
            ForeColor = ColInk,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        var dotLbl = new Label
        {
            Text = "●",
            Location = new Point(66, 38),
            AutoSize = true,
            ForeColor = ColGreen,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
        };
        var roleLbl = new Label
        {
            Text = $"Online · {_currentUser.StaffPosition ?? _currentUser.Role}",
            Location = new Point(80, 38),
            AutoSize = true,
            ForeColor = ColMuted,
            Font = new Font("Segoe UI", 8.5F)
        };
        userCard.Controls.AddRange(new Control[] { avatarLbl, nameLbl, dotLbl, roleLbl });
        userBoxHost.Controls.Add(userCard);
        _sidebar.Controls.Add(userBoxHost);

        // Exact 6 Sidebar Menu Items matching D:\Csharp\admin\admin.js ROUTES
        var routes = new (string Key, string Label)[]
        {
            ("rooms",     "Quản lý phòng"),
            ("orders",    "Quản lý đơn đặt phòng"),
            ("payments",  "Quản lý thanh toán"),
            ("customers", "Quản lý khách hàng"),
            ("staff",     "Quản lý nhân viên"),
            ("settings",  "Cấu hình hệ thống")
        };

        int navY = 114;
        foreach (var (key, label) in routes)
        {
            var btn = new Button
            {
                Text = "  " + label,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(14, navY),
                Size = new Size(231, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColSurface,
                ForeColor = ColInk,
                Font = new Font("Segoe UI", 9.8F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ColSoft;
            string routeKey = key;
            btn.Click += (_, _) => NavigateTo(routeKey);
            _sidebar.Controls.Add(btn);
            _navButtons[key] = btn;
            navY += 48;
        }

        // Bottom .side-foot & .logout-btn
        var sideFoot = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 72,
            Padding = new Padding(14, 14, 14, 14),
            BackColor = ColSurface
        };
        var sideFootBorder = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = ColLine
        };
        var btnLogout = new Button
        {
            Text = "Đăng xuất",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = ColSurface,
            ForeColor = ColInk,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnLogout.FlatAppearance.BorderColor = ColLine;
        btnLogout.FlatAppearance.BorderSize = 1;
        btnLogout.FlatAppearance.MouseOverBackColor = ColRedSoft;
        btnLogout.Click += (_, _) =>
        {
            if (MessageBox.Show("Đăng xuất khỏi hệ thống quản lý?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                IsLogoutRequested = true;
                Close();
            }
        };
        sideFoot.Controls.Add(btnLogout);
        sideFoot.Controls.Add(sideFootBorder);
        _sidebar.Controls.Add(sideFoot);

        // Main Area (Right)
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColGround
        };
        Controls.Add(mainPanel);
        mainPanel.BringToFront();

        // Topbar (72px, white, 1px #e9e5e3 bottom border)
        var topbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            BackColor = ColSurface,
            Padding = new Padding(28, 0, 28, 0)
        };
        var topbarBorder = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = ColLine
        };
        var lblBrand = new Label
        {
            Text = "HENRY HOMESTAY",
            Location = new Point(28, 22),
            AutoSize = true,
            Font = new Font("Georgia", 13.5F, FontStyle.Bold),
            ForeColor = ColBrandDark
        };
        var lblDb = new Label
        {
            Text = DatabaseConfigManager.GetActiveDatabaseSummary(),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(topbar.Width - 520, 27),
            Size = new Size(340, 20),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = ColMuted
        };
        var btnRefresh = new Button
        {
            Text = "Làm mới dữ liệu",
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(topbar.Width - 168, 18),
            Size = new Size(140, 36),
            FlatStyle = FlatStyle.Flat,
            BackColor = ColSurface,
            ForeColor = ColInk,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnRefresh.FlatAppearance.BorderColor = ColLine;
        btnRefresh.FlatAppearance.BorderSize = 1;
        btnRefresh.FlatAppearance.MouseOverBackColor = ColSoft;
        btnRefresh.Click += (_, _) => NavigateTo(_currentRoute);

        topbar.Controls.AddRange(new Control[] { lblBrand, lblDb, btnRefresh, topbarBorder });
        mainPanel.Controls.Add(topbar);

        // Content Header
        var headerBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            Padding = new Padding(28, 20, 28, 8),
            BackColor = ColGround
        };
        _lblPageTitle = new Label
        {
            Text = "Quản lý phòng",
            Location = new Point(28, 20),
            AutoSize = true,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = ColInk
        };
        headerBar.Controls.Add(_lblPageTitle);
        mainPanel.Controls.Add(headerBar);
        headerBar.BringToFront();

        // View Container
        _viewContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 8, 28, 28),
            BackColor = ColGround
        };
        mainPanel.Controls.Add(_viewContainer);
        _viewContainer.BringToFront();
    }

    private void UpdateSidebarHighlight()
    {
        int pendingCount = _paymentService.GetPayments(onlyPending: true).Count;
        foreach (var (key, btn) in _navButtons)
        {
            if (key == "payments")
            {
                btn.Text = pendingCount > 0
                    ? $"  Quản lý thanh toán ({pendingCount})"
                    : "  Quản lý thanh toán";
            }

            bool active = key == _currentRoute;
            btn.BackColor = active ? ColAccentSoft : ColSurface;
            btn.ForeColor = active ? ColAccent : ColInk;
            btn.Font = new Font("Segoe UI", 9.8F, active ? FontStyle.Bold : FontStyle.Regular);
        }
    }

    public void NavigateTo(string route)
    {
        if ((route == "staff" || route == "settings") && _currentUser.Role != "ADMIN")
        {
            MessageBox.Show(
                $"Tài khoản '{_currentUser.FullName}' ({_currentUser.StaffPosition}) thuộc nhóm Nhân viên (STAFF).\nChỉ Quản lý (ADMIN) mới có quyền truy cập mục Quản lý nhân viên và Cấu hình hệ thống.",
                "Phân quyền truy cập",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _currentRoute = route;
        UpdateSidebarHighlight();

        _lblPageTitle.Text = route switch
        {
            "rooms" => "Quản lý phòng",
            "orders" => "Quản lý đơn đặt phòng",
            "payments" => "Quản lý thanh toán",
            "customers" => "Quản lý khách hàng",
            "staff" => "Quản lý nhân viên",
            "settings" => "Cấu hình hệ thống",
            _ => "Quản lý phòng"
        };

        _viewContainer.Controls.Clear();

        switch (route)
        {
            case "rooms": RenderRoomsView(); break;
            case "orders": RenderOrdersView(); break;
            case "payments": RenderPaymentsView(); break;
            case "customers": RenderCustomersView(); break;
            case "staff": RenderStaffView(); break;
            case "settings": RenderSettingsView(); break;
        }
    }

    private static Panel CreateBoxPanel(Padding? padding = null)
    {
        var box = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColSurface,
            Padding = padding ?? new Padding(24)
        };
        box.Paint += (_, e) =>
        {
            using var pen = new Pen(ColLine, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, box.Width - 1, box.Height - 1);
        };
        return box;
    }

    private static void StyleGrid(DataGridView dgv)
    {
        dgv.BackgroundColor = ColSurface;
        dgv.BorderStyle = BorderStyle.None;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = ColSoft;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = ColMuted;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.8F, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        dgv.ColumnHeadersHeight = 42;
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.3F);
        dgv.DefaultCellStyle.BackColor = ColSurface;
        dgv.DefaultCellStyle.ForeColor = ColInk;
        dgv.DefaultCellStyle.SelectionBackColor = ColSoftWarm;
        dgv.DefaultCellStyle.SelectionForeColor = ColInk;
        dgv.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
        dgv.RowTemplate.Height = 54;
        dgv.GridColor = ColLine;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToResizeRows = false;
        dgv.ReadOnly = true;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }

    private static Panel CreateSubTabButton(string text, bool active, int x, int width, Action onClick)
    {
        var container = new Panel
        {
            Location = new Point(x, 0),
            Size = new Size(width, 42),
            BackColor = ColSurface,
            Cursor = Cursors.Hand
        };
        var lbl = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9.8F, active ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = active ? ColAccent : ColMuted,
            Cursor = Cursors.Hand
        };
        var underline = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 2,
            BackColor = active ? ColAccent : Color.Transparent
        };
        lbl.Click += (_, _) => onClick();
        container.Click += (_, _) => onClick();
        container.Controls.Add(lbl);
        container.Controls.Add(underline);
        return container;
    }

    private static Button CreateActionButton(string text, Color bg, Color fg, Point location, int width, Color? borderColor = null)
    {
        var b = new Button
        {
            Text = text,
            Location = location,
            Size = new Size(width, 36),
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
        return b;
    }

    // ==================== 1. ROOMS MANAGEMENT (LIST + CALENDAR) ====================
    private void RenderRoomsView()
    {
        _viewContainer.Controls.Clear();
        var box = CreateBoxPanel();
        _viewContainer.Controls.Add(box);

        var tabBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = ColSurface };
        var tabDivider = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = ColLine };
        var tabList = CreateSubTabButton("Danh sách phòng", _roomsSubTab == "list", 0, 145, () => { _roomsSubTab = "list"; RenderRoomsView(); });
        var tabCal = CreateSubTabButton("Lịch đặt phòng", _roomsSubTab == "cal", 165, 140, () => { _roomsSubTab = "cal"; RenderRoomsView(); });
        tabBar.Controls.AddRange(new Control[] { tabList, tabCal, tabDivider });
        box.Controls.Add(tabBar);

        var bodyPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 16, 0, 0), BackColor = ColSurface };
        box.Controls.Add(bodyPanel);
        bodyPanel.BringToFront();

        if (_roomsSubTab == "list")
            RenderRoomsListTab(bodyPanel);
        else
            RenderRoomsCalendarTab(bodyPanel);
    }

    private void RenderRoomsListTab(Panel parent)
    {
        var rooms = _roomService.GetRooms(includeHidden: true);

        var toolbar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = ColSurface };
        var lblCount = new Label
        {
            Text = $"{rooms.Count} phòng",
            Location = new Point(0, 12),
            AutoSize = true,
            ForeColor = ColMuted,
            Font = new Font("Segoe UI", 9.5F)
        };

        var btnAdd = CreateActionButton("+ Thêm phòng", ColAccent, Color.White, new Point(0, 4), 132);
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Location = new Point(toolbar.Width - 132, 4);
        btnAdd.Click += (_, _) =>
        {
            if (AdminDialogs.ShowRoomFormDialog(this, _roomService, null))
                RenderRoomsView();
        };

        var btnDetail = CreateActionButton("Chi tiết", ColSoft, ColInk, new Point(toolbar.Width - 548, 4), 90, ColBorderInput);
        btnDetail.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        var btnEdit = CreateActionButton("Sửa", ColSoft, ColInk, new Point(toolbar.Width - 450, 4), 80, ColBorderInput);
        btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        var btnToggle = CreateActionButton("Ẩn / Hiện", ColSoft, ColInk, new Point(toolbar.Width - 362, 4), 105, ColBorderInput);
        btnToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        var btnDelete = CreateActionButton("Xóa", ColRedSoft, ColAccent, new Point(toolbar.Width - 249, 4), 80, ColRedBorder);
        btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        toolbar.Controls.AddRange(new Control[] { lblCount, btnDetail, btnEdit, btnToggle, btnDelete, btnAdd });
        parent.Controls.Add(toolbar);

        var dgv = new DataGridView { Dock = DockStyle.Fill };
        StyleGrid(dgv);
        dgv.RowTemplate.Height = 60;

        dgv.Columns.Add("Stt", "STT");
        dgv.Columns["Stt"]!.FillWeight = 32;

        var imgCol = new DataGridViewImageColumn
        {
            Name = "Image",
            HeaderText = "Hình ảnh",
            ImageLayout = DataGridViewImageCellLayout.Zoom,
            FillWeight = 62
        };
        imgCol.DefaultCellStyle.NullValue = null;
        dgv.Columns.Add(imgCol);

        dgv.Columns.Add("Name", "Tên phòng");
        dgv.Columns["Name"]!.FillWeight = 135;
        dgv.Columns.Add("Guests", "Số người tối đa");
        dgv.Columns["Guests"]!.FillWeight = 75;
        dgv.Columns.Add("Pets", "Thú cưng");
        dgv.Columns["Pets"]!.FillWeight = 60;
        dgv.Columns.Add("Price", "Giá / ngày");
        dgv.Columns["Price"]!.FillWeight = 85;
        dgv.Columns.Add("Status", "Trạng thái");
        dgv.Columns["Status"]!.FillWeight = 145;

        var actCol = new DataGridViewButtonColumn
        {
            Name = "Actions",
            HeaderText = "Tác vụ",
            Text = "Chi tiết",
            UseColumnTextForButtonValue = true,
            FlatStyle = FlatStyle.Flat,
            FillWeight = 65
        };
        dgv.Columns.Add(actCol);

        for (int i = 0; i < rooms.Count; i++)
        {
            var r = rooms[i];
            Image? thumb = AdminDialogs.LoadImageSafe(r.PrimaryImage);
            string visText = r.Status == "ACTIVE" ? "Đang hiển thị" : "Đã ẩn";
            int rowIdx = dgv.Rows.Add(
                i + 1,
                (object?)thumb ?? DBNull.Value,
                $"{r.Name}\n{r.TypeName}",
                r.MaxGuests,
                r.AllowPets ? "Có" : "Không",
                $"{r.PricePerNight:N0} ₫",
                $"{visText} · {r.OccupancyStatus}");
            dgv.Rows[rowIdx].Tag = r;

            var statusCell = dgv.Rows[rowIdx].Cells["Status"];
            statusCell.Style.ForeColor = r.Status == "ACTIVE" ? ColGreen : ColMuted;
            statusCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        RoomDisplayDto? SelectedRoom() => dgv.CurrentRow?.Tag as RoomDisplayDto;

        void OpenDetail(RoomDisplayDto r)
        {
            AdminDialogs.ShowRoomDetailDialog(this, r, _bookingService, () =>
            {
                if (AdminDialogs.ShowRoomFormDialog(this, _roomService, r))
                    RenderRoomsView();
            });
        }

        btnDetail.Click += (_, _) =>
        {
            var r = SelectedRoom();
            if (r != null) OpenDetail(r);
        };

        btnEdit.Click += (_, _) =>
        {
            var r = SelectedRoom();
            if (r != null && AdminDialogs.ShowRoomFormDialog(this, _roomService, r))
                RenderRoomsView();
        };

        btnToggle.Click += (_, _) =>
        {
            var r = SelectedRoom();
            if (r == null) return;
            var res = _roomService.ToggleRoomVisibility(r.Id);
            MessageBox.Show(res.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RenderRoomsView();
        };

        btnDelete.Click += (_, _) =>
        {
            var r = SelectedRoom();
            if (r == null) return;
            if (MessageBox.Show($"Bạn có chắc muốn xóa phòng {r.Name}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            var res = _roomService.DeleteRoom(r.Id);
            MessageBox.Show(res.Message, res.Success ? "Thành công" : "Không thể xóa", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Success) RenderRoomsView();
        };

        dgv.CellContentClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "Actions" &&
                dgv.Rows[e.RowIndex].Tag is RoomDisplayDto r)
            {
                OpenDetail(r);
            }
        };
        dgv.CellDoubleClick += (_, _) => btnDetail.PerformClick();

        parent.Controls.Add(dgv);
        dgv.BringToFront();
    }

    private void RenderRoomsCalendarTab(Panel parent)
    {
        var navBar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = ColSurface };
        var btnPrev = CreateActionButton("‹ Tháng trước", ColSurface, ColInk, new Point(0, 4), 112, ColLine);
        var lblMonth = new Label
        {
            Text = $"Tháng {_calMonth.Month} / {_calMonth.Year}",
            Location = new Point(126, 11),
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = ColInk
        };
        var btnNext = CreateActionButton("Tháng sau ›", ColSurface, ColInk, new Point(250, 4), 112, ColLine);
        var lblLegend = new Label
        {
            Text = "Chú thích:  [Vàng] Chờ xác nhận   |   [Xanh dương] Đã xác nhận   |   [Xanh lá] Đã nhận phòng   |   [Xám] Hoàn thành   (Nhấp đúp ô màu để xem đơn)",
            Location = new Point(380, 14),
            AutoSize = true,
            ForeColor = ColMuted,
            Font = new Font("Segoe UI", 8.8F)
        };

        btnPrev.Click += (_, _) => { _calMonth = _calMonth.AddMonths(-1); RenderRoomsView(); };
        btnNext.Click += (_, _) => { _calMonth = _calMonth.AddMonths(1); RenderRoomsView(); };
        navBar.Controls.AddRange(new Control[] { btnPrev, lblMonth, btnNext, lblLegend });
        parent.Controls.Add(navBar);

        var dgv = new DataGridView { Dock = DockStyle.Fill };
        StyleGrid(dgv);
        dgv.RowTemplate.Height = 44;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;

        dgv.Columns.Add("Room", "Phòng");
        dgv.Columns["Room"]!.Width = 170;
        dgv.Columns["Room"]!.Frozen = true;

        int daysInMonth = DateTime.DaysInMonth(_calMonth.Year, _calMonth.Month);
        for (int d = 1; d <= daysInMonth; d++)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = $"D{d}",
                HeaderText = d.ToString(),
                Width = 34
            };
            dgv.Columns.Add(col);
        }

        var rooms = _roomService.GetRooms(includeHidden: true);
        var bookings = _bookingService.GetBookings().Where(b => b.Status != "CANCELLED").ToList();

        foreach (var r in rooms)
        {
            int rowIdx = dgv.Rows.Add();
            var row = dgv.Rows[rowIdx];
            row.Cells[0].Value = r.Name + (r.Status == "HIDDEN" ? " (ẩn)" : "");

            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(_calMonth.Year, _calMonth.Month, d);
                var bk = bookings.FirstOrDefault(b => b.RoomId == r.Id && b.CheckInDate.Date <= date && date < b.CheckOutDate.Date);
                if (bk != null)
                {
                    var cell = row.Cells[d];
                    cell.Value = "●";
                    cell.Tag = bk;
                    cell.ToolTipText = $"{bk.BookingCode} · {bk.CustomerName} · {bk.StatusDisplay}";
                    cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    cell.Style.ForeColor = Color.White;
                    cell.Style.BackColor = bk.Status switch
                    {
                        "PENDING_PAYMENT" => ColAmber,
                        "CONFIRMED" => ColBlue,
                        "CHECKED_IN" => ColGreen,
                        _ => Color.FromArgb(156, 163, 175)
                    };
                }
            }
        }

        dgv.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex <= 0) return;
            if (dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag is BookingSummaryDto bk)
            {
                AdminDialogs.ShowOrderDetailDialog(this, bk, _bookingService, _paymentService, _roomService, _currentUser.Id,
                    () => RenderRoomsView(),
                    () => { _paymentsSubTab = "confirm"; NavigateTo("payments"); });
            }
        };

        parent.Controls.Add(dgv);
        dgv.BringToFront();
    }

    // ==================== 2. ORDERS / BOOKINGS MANAGEMENT ====================
    private void RenderOrdersView()
    {
        var box = CreateBoxPanel();
        _viewContainer.Controls.Add(box);

        var filterPanel = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = ColSurface };
        box.Controls.Add(filterPanel);

        var txtSearch = new TextBox { PlaceholderText = "Tìm theo mã đơn...", Location = new Point(0, 6), Width = 210 };

        var cboStatus = new ComboBox { Location = new Point(220, 6), Width = 175, DropDownStyle = ComboBoxStyle.DropDownList };
        cboStatus.Items.AddRange(new object[]
        {
            "Trạng thái: tất cả",
            "Chờ xác nhận",
            "Đã xác nhận",
            "Đã nhận phòng",
            "Hoàn thành",
            "Đã hủy"
        });
        cboStatus.SelectedIndex = 0;

        var rooms = _roomService.GetRooms(includeHidden: true);
        var cboRoom = new ComboBox { Location = new Point(405, 6), Width = 185, DropDownStyle = ComboBoxStyle.DropDownList };
        cboRoom.Items.Add("Phòng: tất cả");
        foreach (var r in rooms) cboRoom.Items.Add($"{r.Name} ({r.RoomCode})");
        cboRoom.SelectedIndex = 0;

        var chkDate = new CheckBox { Text = "Ngày nhận phòng:", Location = new Point(600, 8), AutoSize = true, ForeColor = ColMuted };
        var dtpDate = new DateTimePicker { Location = new Point(725, 6), Width = 120, Format = DateTimePickerFormat.Short, Enabled = false };
        chkDate.CheckedChanged += (_, _) => dtpDate.Enabled = chkDate.Checked;

        var btnReset = CreateActionButton("Đặt lại", ColSurface, ColInk, new Point(855, 2), 84, ColLine);
        btnReset.Height = 32;

        // Second row of actions + count
        var btnNewOrder = CreateActionButton("+ Tạo đơn", ColAccent, Color.White, new Point(0, 48), 115);
        var btnDetail = CreateActionButton("Chi tiết", ColSoft, ColInk, new Point(125, 48), 95, ColBorderInput);
        var btnEdit = CreateActionButton("Sửa", ColSoft, ColInk, new Point(230, 48), 80, ColBorderInput);
        var btnCancel = CreateActionButton("Hủy đơn", ColRedSoft, ColAccent, new Point(320, 48), 95, ColRedBorder);
        var lblCount = new Label
        {
            Location = new Point(435, 56),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = ColMuted
        };

        filterPanel.Controls.AddRange(new Control[]
        {
            txtSearch, cboStatus, cboRoom, chkDate, dtpDate, btnReset,
            btnNewOrder, btnDetail, btnEdit, btnCancel, lblCount
        });

        var dgv = new DataGridView { Dock = DockStyle.Fill };
        StyleGrid(dgv);
        dgv.Columns.Add("Code", "Mã đơn");
        dgv.Columns["Code"]!.FillWeight = 75;
        dgv.Columns.Add("Guest", "Khách");
        dgv.Columns["Guest"]!.FillWeight = 135;
        dgv.Columns.Add("Room", "Phòng");
        dgv.Columns["Room"]!.FillWeight = 95;
        dgv.Columns.Add("CheckIn", "Nhận phòng");
        dgv.Columns["CheckIn"]!.FillWeight = 75;
        dgv.Columns.Add("CheckOut", "Trả phòng");
        dgv.Columns["CheckOut"]!.FillWeight = 75;
        dgv.Columns.Add("Total", "Tổng tiền");
        dgv.Columns["Total"]!.FillWeight = 85;
        dgv.Columns.Add("Payment", "Thanh toán");
        dgv.Columns["Payment"]!.FillWeight = 130;
        dgv.Columns.Add("Status", "Trạng thái");
        dgv.Columns["Status"]!.FillWeight = 115;

        void LoadGrid()
        {
            string? statusCode = cboStatus.SelectedIndex switch
            {
                1 => "PENDING_PAYMENT",
                2 => "CONFIRMED",
                3 => "CHECKED_IN",
                4 => "COMPLETED",
                5 => "CANCELLED",
                _ => null
            };
            int? roomId = cboRoom.SelectedIndex > 0 ? rooms[cboRoom.SelectedIndex - 1].Id : null;
            DateTime? filterDate = chkDate.Checked ? dtpDate.Value.Date : null;

            var list = _bookingService.GetBookings(txtSearch.Text, statusCode, filterDate, roomId);
            lblCount.Text = $"{list.Count} đơn";

            dgv.Rows.Clear();
            foreach (var o in list)
            {
                string stText = o.StatusDisplay;
                if (o.Status == "CANCELLED" && o.RefundStatus == "WAITING_REFUND")
                    stText += " (Chờ hoàn tiền)";

                int idx = dgv.Rows.Add(
                    o.BookingCode,
                    $"{o.CustomerName} · {o.CustomerPhone}",
                    o.RoomName,
                    o.CheckInDate.ToString("dd/MM/yyyy"),
                    o.CheckOutDate.ToString("dd/MM/yyyy"),
                    $"{o.TotalAmount:N0} ₫",
                    $"Đã trả {o.PaidAmount:N0} ₫ · Còn lại {o.RemainingAmount:N0} ₫",
                    stText);
                dgv.Rows[idx].Tag = o;

                var stCell = dgv.Rows[idx].Cells["Status"];
                stCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                stCell.Style.ForeColor = o.Status switch
                {
                    "PENDING_PAYMENT" => ColAmber,
                    "CONFIRMED" => ColBlue,
                    "CHECKED_IN" => ColGreen,
                    "CANCELLED" => ColAccent,
                    _ => ColMuted
                };
            }
        }

        txtSearch.TextChanged += (_, _) => LoadGrid();
        cboStatus.SelectedIndexChanged += (_, _) => LoadGrid();
        cboRoom.SelectedIndexChanged += (_, _) => LoadGrid();
        chkDate.CheckedChanged += (_, _) => LoadGrid();
        dtpDate.ValueChanged += (_, _) => { if (chkDate.Checked) LoadGrid(); };
        btnReset.Click += (_, _) =>
        {
            txtSearch.Text = "";
            cboStatus.SelectedIndex = 0;
            cboRoom.SelectedIndex = 0;
            chkDate.Checked = false;
            LoadGrid();
        };

        BookingSummaryDto? SelectedOrder() => dgv.CurrentRow?.Tag as BookingSummaryDto;

        btnNewOrder.Click += (_, _) => AdminDialogs.ShowCreateBookingDialog(this, _bookingService, _roomService, () =>
        {
            LoadGrid();
            UpdateSidebarHighlight();
        });

        btnDetail.Click += (_, _) =>
        {
            var o = SelectedOrder();
            if (o == null) return;
            AdminDialogs.ShowOrderDetailDialog(this, o, _bookingService, _paymentService, _roomService, _currentUser.Id,
                () => { LoadGrid(); UpdateSidebarHighlight(); },
                () => { _paymentsSubTab = "confirm"; NavigateTo("payments"); });
        };

        btnEdit.Click += (_, _) =>
        {
            var o = SelectedOrder();
            if (o == null) return;
            if (o.Status == "CANCELLED" || o.Status == "COMPLETED")
            {
                MessageBox.Show("Không thể sửa đơn đã hủy hoặc đã hoàn thành.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            AdminDialogs.ShowOrderEditDialog(this, o, _bookingService, _roomService, LoadGrid);
        };

        btnCancel.Click += (_, _) =>
        {
            var o = SelectedOrder();
            if (o == null) return;
            if (o.Status == "CANCELLED" || o.Status == "COMPLETED")
            {
                MessageBox.Show("Đơn này đã hủy hoặc đã hoàn thành.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"Xác nhận khách đã liên hệ hủy đơn {o.BookingCode}?", "Xác nhận hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            var res = _bookingService.CancelBooking(o.Id, _currentUser.Id);
            MessageBox.Show(res.Message, "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadGrid();
            UpdateSidebarHighlight();
        };

        dgv.CellDoubleClick += (_, _) => btnDetail.PerformClick();

        box.Controls.Add(dgv);
        dgv.BringToFront();
        LoadGrid();
    }

    // ==================== 3. PAYMENTS MANAGEMENT ====================
    private void RenderPaymentsView()
    {
        _viewContainer.Controls.Clear();
        var box = CreateBoxPanel();
        _viewContainer.Controls.Add(box);

        var pendingList = _paymentService.GetPayments(onlyPending: true);

        var tabBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = ColSurface };
        var tabDivider = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = ColLine };
        var tabList = CreateSubTabButton("Danh sách giao dịch", _paymentsSubTab == "list", 0, 165, () => { _paymentsSubTab = "list"; RenderPaymentsView(); });
        string confirmText = pendingList.Count > 0 ? $"Xác nhận thanh toán ({pendingList.Count})" : "Xác nhận thanh toán";
        var tabConfirm = CreateSubTabButton(confirmText, _paymentsSubTab == "confirm", 185, 200, () => { _paymentsSubTab = "confirm"; RenderPaymentsView(); });
        tabBar.Controls.AddRange(new Control[] { tabList, tabConfirm, tabDivider });
        box.Controls.Add(tabBar);

        var bodyPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 16, 0, 0), BackColor = ColSurface };
        box.Controls.Add(bodyPanel);
        bodyPanel.BringToFront();

        if (_paymentsSubTab == "list")
        {
            var dgv = new DataGridView { Dock = DockStyle.Fill };
            StyleGrid(dgv);
            dgv.Columns.Add("Code", "Mã GD");
            dgv.Columns["Code"]!.FillWeight = 60;
            dgv.Columns.Add("Order", "Mã đơn");
            dgv.Columns["Order"]!.FillWeight = 75;
            dgv.Columns.Add("Guest", "Khách hàng");
            dgv.Columns["Guest"]!.FillWeight = 120;
            dgv.Columns.Add("Amount", "Số tiền");
            dgv.Columns["Amount"]!.FillWeight = 90;
            dgv.Columns.Add("Method", "Phương thức thanh toán");
            dgv.Columns["Method"]!.FillWeight = 140;
            dgv.Columns.Add("Time", "Thời gian");
            dgv.Columns["Time"]!.FillWeight = 95;
            dgv.Columns.Add("Status", "Trạng thái");
            dgv.Columns["Status"]!.FillWeight = 95;

            foreach (var t in _paymentService.GetPayments(onlyPending: false))
            {
                int idx = dgv.Rows.Add(
                    t.PaymentCode,
                    t.BookingCode,
                    t.CustomerName,
                    $"{t.Amount:N0} ₫",
                    t.MethodDisplay,
                    t.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    t.StatusDisplay);
                if (t.Amount < 0)
                    dgv.Rows[idx].Cells["Amount"].Style.ForeColor = ColAccent;

                var stCell = dgv.Rows[idx].Cells["Status"];
                stCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                stCell.Style.ForeColor = t.Status switch
                {
                    "CONFIRMED" => ColGreen,
                    "PENDING" => ColAmber,
                    "REFUNDED" => ColTeal,
                    _ => ColMuted
                };
            }
            bodyPanel.Controls.Add(dgv);
            dgv.BringToFront();
        }
        else
        {
            var topNote = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = ColSurface };
            var lblHint = new Label
            {
                Text = "Các đơn đang chờ khách gọi điện xác nhận đã chuyển khoản. Sau khi kiểm tra tiền đã về tài khoản, bấm xác nhận để đơn chuyển sang Đã xác nhận.",
                Location = new Point(0, 14),
                AutoSize = true,
                ForeColor = ColMuted
            };
            var btnApprove = CreateActionButton("Xác nhận đã nhận tiền", ColGreen, Color.White, new Point(topNote.Width - 195, 6), 195);
            btnApprove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            topNote.Controls.AddRange(new Control[] { lblHint, btnApprove });
            bodyPanel.Controls.Add(topNote);

            var dgv = new DataGridView { Dock = DockStyle.Fill };
            StyleGrid(dgv);
            dgv.Columns.Add("Order", "Mã đơn");
            dgv.Columns.Add("Guest", "Khách");
            dgv.Columns.Add("Amount", "Số tiền");
            dgv.Columns.Add("Method", "Phương thức");
            dgv.Columns.Add("Time", "Thời gian");

            var actCol = new DataGridViewButtonColumn
            {
                Name = "Confirm",
                HeaderText = "Tác vụ",
                Text = "Xác nhận đã nhận tiền",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat
            };
            dgv.Columns.Add(actCol);

            foreach (var t in pendingList)
            {
                int idx = dgv.Rows.Add(
                    t.BookingCode,
                    $"{t.CustomerName} ({t.CustomerPhone})",
                    $"{t.Amount:N0} ₫",
                    t.MethodDisplay,
                    t.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                dgv.Rows[idx].Tag = t;
            }

            void ConfirmSelected(PaymentSummaryDto p)
            {
                var res = _paymentService.ConfirmPayment(p.Id, _currentUser.Id);
                MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateSidebarHighlight();
                RenderPaymentsView();
            }

            btnApprove.Click += (_, _) =>
            {
                if (dgv.CurrentRow?.Tag is not PaymentSummaryDto p)
                {
                    MessageBox.Show("Không có giao dịch nào đang được chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                ConfirmSelected(p);
            };

            dgv.CellContentClick += (_, e) =>
            {
                if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "Confirm" &&
                    dgv.Rows[e.RowIndex].Tag is PaymentSummaryDto p)
                {
                    ConfirmSelected(p);
                }
            };

            bodyPanel.Controls.Add(dgv);
            dgv.BringToFront();
        }
    }

    // ==================== 4. CUSTOMERS MANAGEMENT ====================
    private void RenderCustomersView()
    {
        var box = CreateBoxPanel();
        _viewContainer.Controls.Add(box);

        var toolbar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ColSurface };
        var txtSearch = new TextBox { PlaceholderText = "Tìm tên, SĐT, email khách hàng...", Location = new Point(0, 8), Width = 260 };
        var btnHistory = CreateActionButton("Lịch sử đặt phòng", ColAccent, Color.White, new Point(275, 5), 155);
        var btnLock = CreateActionButton("Khóa / Mở khóa", ColRedSoft, ColAccent, new Point(440, 5), 145, ColRedBorder);
        var lblCount = new Label { Location = new Point(605, 13), AutoSize = true, ForeColor = ColMuted, Font = new Font("Segoe UI", 9.5F) };

        toolbar.Controls.AddRange(new Control[] { txtSearch, btnHistory, btnLock, lblCount });
        box.Controls.Add(toolbar);

        var dgv = new DataGridView { Dock = DockStyle.Fill };
        StyleGrid(dgv);
        dgv.Columns.Add("Stt", "STT");
        dgv.Columns["Stt"]!.FillWeight = 38;
        dgv.Columns.Add("Name", "Tên");
        dgv.Columns.Add("Phone", "Số điện thoại");
        dgv.Columns.Add("Email", "Email");
        dgv.Columns.Add("Reg", "Ngày đăng ký");
        dgv.Columns.Add("Orders", "Số đơn đã đặt");
        dgv.Columns.Add("Status", "Trạng thái");

        void LoadCustomers()
        {
            var list = _userService.GetCustomers(txtSearch.Text);
            lblCount.Text = $"{list.Count} tài khoản khách";
            dgv.Rows.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                int idx = dgv.Rows.Add(
                    i + 1,
                    c.FullName,
                    c.Phone,
                    c.Email,
                    c.CreatedAt.ToString("dd/MM/yyyy"),
                    c.Bookings.Count,
                    c.IsLocked ? "Đã khóa" : "Hoạt động");
                dgv.Rows[idx].Tag = c;

                var stCell = dgv.Rows[idx].Cells["Status"];
                stCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                stCell.Style.ForeColor = c.IsLocked ? ColAccent : ColGreen;
            }
        }

        txtSearch.TextChanged += (_, _) => LoadCustomers();
        btnHistory.Click += (_, _) =>
        {
            if (dgv.CurrentRow?.Tag is User c)
                AdminDialogs.ShowCustomerHistoryDialog(this, c, _bookingService);
        };
        btnLock.Click += (_, _) =>
        {
            if (dgv.CurrentRow?.Tag is not User c) return;
            if (MessageBox.Show($"{(c.IsLocked ? "Mở khóa" : "Khóa")} tài khoản {c.FullName}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            var res = _userService.ToggleUserLock(c.Id);
            MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadCustomers();
        };

        dgv.CellDoubleClick += (_, _) => btnHistory.PerformClick();

        box.Controls.Add(dgv);
        dgv.BringToFront();
        LoadCustomers();
    }

    // ==================== 5. STAFF MANAGEMENT ====================
    private void RenderStaffView()
    {
        var box = CreateBoxPanel();
        _viewContainer.Controls.Add(box);

        var toolbar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ColSurface };
        var txtSearch = new TextBox { PlaceholderText = "Tìm họ tên, chức vụ, SĐT...", Location = new Point(0, 8), Width = 230 };
        var lblCount = new Label { Location = new Point(245, 13), AutoSize = true, ForeColor = ColMuted, Font = new Font("Segoe UI", 9.5F) };

        var btnAdd = CreateActionButton("+ Thêm nhân viên", ColAccent, Color.White, new Point(toolbar.Width - 155, 5), 155);
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        var btnEdit = CreateActionButton("Sửa", ColSoft, ColInk, new Point(toolbar.Width - 395, 5), 85, ColBorderInput);
        btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        var btnLock = CreateActionButton("Khóa / Mở khóa", ColRedSoft, ColAccent, new Point(toolbar.Width - 300, 5), 135, ColRedBorder);
        btnLock.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        toolbar.Controls.AddRange(new Control[] { txtSearch, lblCount, btnEdit, btnLock, btnAdd });
        box.Controls.Add(toolbar);

        var dgv = new DataGridView { Dock = DockStyle.Fill };
        StyleGrid(dgv);
        dgv.Columns.Add("Stt", "STT");
        dgv.Columns["Stt"]!.FillWeight = 38;
        dgv.Columns.Add("Name", "Họ và tên");
        dgv.Columns.Add("Role", "Chức vụ");
        dgv.Columns.Add("Phone", "Số điện thoại");
        dgv.Columns.Add("Email", "Email");
        dgv.Columns.Add("Joined", "Ngày vào làm");
        dgv.Columns.Add("Status", "Trạng thái");

        void LoadStaff()
        {
            var list = _userService.GetStaffMembers(txtSearch.Text);
            lblCount.Text = $"{list.Count} nhân viên";
            dgv.Rows.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                var m = list[i];
                int idx = dgv.Rows.Add(
                    i + 1,
                    m.FullName,
                    m.StaffPosition ?? m.Role,
                    m.Phone,
                    m.Email,
                    (m.HireDate ?? m.CreatedAt).ToString("dd/MM/yyyy"),
                    m.IsLocked ? "Đã khóa" : "Đang làm việc");
                dgv.Rows[idx].Tag = m;

                var stCell = dgv.Rows[idx].Cells["Status"];
                stCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                stCell.Style.ForeColor = m.IsLocked ? ColAccent : ColGreen;
            }
        }

        txtSearch.TextChanged += (_, _) => LoadStaff();
        btnAdd.Click += (_, _) => AdminDialogs.ShowStaffFormDialog(this, _userService, null, LoadStaff);
        btnEdit.Click += (_, _) =>
        {
            if (dgv.CurrentRow?.Tag is User m)
                AdminDialogs.ShowStaffFormDialog(this, _userService, m, LoadStaff);
        };
        btnLock.Click += (_, _) =>
        {
            if (dgv.CurrentRow?.Tag is not User m) return;
            if (m.Id == _currentUser.Id)
            {
                MessageBox.Show("Không thể tự khóa tài khoản đang đăng nhập.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"{(m.IsLocked ? "Mở khóa" : "Khóa")} tài khoản nhân viên {m.FullName}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            var res = _userService.ToggleUserLock(m.Id);
            MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStaff();
        };

        box.Controls.Add(dgv);
        dgv.BringToFront();
        LoadStaff();
    }

    // ==================== 6. SYSTEM SETTINGS ====================
    private void RenderSettingsView()
    {
        _viewContainer.Controls.Clear();
        var box = CreateBoxPanel(new Padding(28));
        box.AutoScroll = true;
        _viewContainer.Controls.Add(box);

        var cfg = _systemService.GetSettings();

        Label Lbl(string t, int x, int y, float size = 9.5F, bool bold = true, bool muted = false) => new()
        {
            Text = t,
            Location = new Point(x, y),
            AutoSize = true,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = muted ? ColMuted : ColInk
        };

        box.Controls.Add(Lbl("TÀI KHOẢN NGÂN HÀNG NHẬN TIỀN", 28, 22, 10F, true, true));
        box.Controls.Add(Lbl("Hiển thị trong popup “Bắt đầu thanh toán” của khách.", 28, 46, 9F, false, true));

        // QR Code Preview & File Picker
        box.Controls.Add(Lbl("Mã QR chuyển khoản", 28, 84, 9F, true, true));
        string currentQrPath = cfg.QrCodeUrl ?? "assets/images/payment-qr.svg";

        var picQr = new PictureBox
        {
            Location = new Point(28, 108),
            Size = new Size(160, 160),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = ColSurface,
            BorderStyle = BorderStyle.FixedSingle
        };

        void UpdateQrPreview(string pathOrUrl)
        {
            var img = AdminDialogs.LoadImageSafe(pathOrUrl);
            if (img != null)
            {
                picQr.Image = img;
            }
            else
            {
                string ext = Path.GetExtension(pathOrUrl).ToUpperInvariant();
                string caption = ext == ".SVG" ? "VietQR · SVG" : "Mã QR";
                picQr.Image = AdminDialogs.CreateQrPlaceholderBitmap(160, caption);
            }
        }
        UpdateQrPreview(currentQrPath);

        var lblQrFile = Lbl(currentQrPath, 206, 114, 9F, false, true);
        lblQrFile.MaximumSize = new Size(460, 0);

        var btnChooseQr = CreateActionButton("Chọn ảnh QR...", ColSurface, ColInk, new Point(206, 146), 145, ColLine);
        var lblQrHint = Lbl("PNG, JPG, WebP hoặc SVG, tối đa 1 MB.", 206, 192, 8.8F, false, true);

        btnChooseQr.Click += (_, _) =>
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh mã QR chuyển khoản",
                Filter = "Hình ảnh QR (*.png;*.jpg;*.jpeg;*.webp;*.svg)|*.png;*.jpg;*.jpeg;*.webp;*.svg"
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            var info = new FileInfo(ofd.FileName);
            if (info.Length > 1048576)
            {
                MessageBox.Show("Ảnh quá lớn (tối đa 1 MB).", "Kiểm tra tệp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                currentQrPath = AdminDialogs.CopyImageToAssets(ofd.FileName, "payment-qr", returnWebRelativePath: true);
                lblQrFile.Text = currentQrPath;
                UpdateQrPreview(currentQrPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể sao chép ảnh QR: {ex.Message}", "Lỗi tệp ảnh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };

        // Bank fields
        box.Controls.Add(Lbl("Tên người nhận", 28, 290, 9F, true, true));
        var txtHolder = new TextBox { Location = new Point(28, 314), Width = 310, Font = new Font("Segoe UI", 10.5F), Text = cfg.BankAccountHolder };

        box.Controls.Add(Lbl("Số tài khoản", 365, 290, 9F, true, true));
        var txtAccount = new TextBox { Location = new Point(365, 314), Width = 310, Font = new Font("Segoe UI", 10.5F), Text = cfg.BankAccountNumber };

        box.Controls.Add(Lbl("Ngân hàng", 28, 362, 9F, true, true));
        var txtBank = new TextBox { Location = new Point(28, 386), Width = 310, Font = new Font("Segoe UI", 10.5F), Text = cfg.BankName };

        box.Controls.Add(Lbl("Số điện thoại xác nhận thanh toán", 365, 362, 9F, true, true));
        var txtPhone = new TextBox { Location = new Point(365, 386), Width = 310, Font = new Font("Segoe UI", 10.5F), Text = cfg.ContactPhone };

        // Deposit section
        box.Controls.Add(Lbl("ĐẶT CỌC", 28, 444, 10F, true, true));
        box.Controls.Add(Lbl("Tỷ lệ cọc tối thiểu (%)", 28, 474, 9F, true, true));
        var numDep = new NumericUpDown { Location = new Point(28, 498), Width = 160, Minimum = 1, Maximum = 100, Value = cfg.DepositPercent, Font = new Font("Segoe UI", 10.5F) };
        box.Controls.Add(Lbl("Mặc định 30%. Áp dụng cho lựa chọn “Đặt cọc trước” khi khách thanh toán.", 202, 502, 9F, false, true));

        var btnSave = CreateActionButton("Lưu cấu hình", ColAccent, Color.White, new Point(28, 560), 155);
        btnSave.Height = 40;
        btnSave.Click += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(currentQrPath) && Path.IsPathRooted(currentQrPath) && File.Exists(currentQrPath))
            {
                try
                {
                    currentQrPath = AdminDialogs.CopyImageToAssets(currentQrPath, "payment-qr", returnWebRelativePath: true);
                    lblQrFile.Text = currentQrPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể sao chép ảnh QR: {ex.Message}", "Lỗi tệp ảnh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var res = _systemService.SaveSettings(txtBank.Text, txtAccount.Text, txtHolder.Text, txtPhone.Text, (byte)numDep.Value, currentQrPath, _currentUser.Id);
            MessageBox.Show(res.Message, res.Success ? "Thành công" : "Kiểm tra dữ liệu", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Success)
                RenderSettingsView();
        };

        var btnReset = CreateActionButton("Khôi phục mặc định", ColSoft, ColInk, new Point(195, 560), 165, ColLine);
        btnReset.Height = 40;
        btnReset.Click += (_, _) =>
        {
            if (MessageBox.Show("Khôi phục cấu hình mặc định?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _systemService.ResetDefaultSettings();
            RenderSettingsView();
            MessageBox.Show("Đã khôi phục mặc định.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        box.Controls.AddRange(new Control[]
        {
            picQr, lblQrFile, btnChooseQr, lblQrHint,
            txtHolder, txtAccount, txtBank, txtPhone, numDep, btnSave, btnReset
        });
    }
}
