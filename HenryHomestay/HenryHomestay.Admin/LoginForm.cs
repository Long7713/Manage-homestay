using System.Drawing;
using System.Windows.Forms;
using HenryHomestay.BLL;
using HenryHomestay.Core.Entities;
using HenryHomestay.DAL;

namespace HenryHomestay.Admin;

public class LoginForm : Form
{
    private readonly AuthService _authService = new();
    private TextBox _txtAccount = null!;
    private TextBox _txtPassword = null!;
    private Label _lblDbStatus = null!;
    private Label _lblError = null!;
    private ComboBox _cboQuickDemo = null!;

    public User? LoggedInUser { get; private set; }

    public LoginForm()
    {
        Text = "Đăng nhập Quản trị | HENRY HOMESTAY";
        Size = new Size(880, 540);
        MinimumSize = new Size(820, 500);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(252, 249, 248);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);

        BuildUi();
    }

    private void BuildUi()
    {
        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57F));
        Controls.Add(split);

        // Left Brand Banner Panel (Warm Hospitality Surface)
        var leftPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(246, 243, 242), // #f6f3f2
            Padding = new Padding(32)
        };
        leftPanel.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(233, 229, 227), 1);
            e.Graphics.DrawLine(pen, leftPanel.Width - 1, 0, leftPanel.Width - 1, leftPanel.Height);
        };
        split.Controls.Add(leftPanel, 0, 0);

        var lblBadge = new Label
        {
            Text = "HỆ THỐNG QUẢN TRỊ DESKTOP",
            ForeColor = Color.FromArgb(186, 0, 54), // #ba0036
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(32, 48)
        };
        leftPanel.Controls.Add(lblBadge);

        var lblBrand = new Label
        {
            Text = "HENRY\nHOMESTAY",
            ForeColor = Color.FromArgb(27, 28, 28), // #1b1c1c
            Font = new Font("Georgia", 24F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, 78)
        };
        leftPanel.Controls.Add(lblBrand);

        var lblDesc = new Label
        {
            Text = "Ứng dụng quản lý vận hành Homestay dành cho Quản lý, Lễ tân và Kế toán.\n\n• Đồng bộ CSDL thời gian thực với Web\n• Quản lý phòng & Lịch đặt phòng trực quan\n• Kiểm tra chống trùng lịch tự động\n• Xác nhận thanh toán & hoàn cọc 24h",
            ForeColor = Color.FromArgb(101, 101, 101), // #656565
            Font = new Font("Segoe UI", 10F),
            MaximumSize = new Size(300, 0),
            AutoSize = true,
            Location = new Point(32, 195)
        };
        leftPanel.Controls.Add(lblDesc);

        _lblDbStatus = new Label
        {
            Text = "● CSDL: " + DatabaseConfigManager.GetActiveDatabaseSummary(),
            ForeColor = Color.FromArgb(0, 106, 69), // #006a45
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            MaximumSize = new Size(310, 0),
            AutoSize = true,
            Location = new Point(32, 405)
        };
        leftPanel.Controls.Add(_lblDbStatus);

        var btnDbConfig = new Button
        {
            Text = "⚙ Cấu hình kết nối SQL Server",
            Location = new Point(32, 442),
            Size = new Size(235, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(34, 34, 34), // #222222
            Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnDbConfig.FlatAppearance.BorderColor = Color.FromArgb(233, 229, 227); // #e9e5e3
        btnDbConfig.FlatAppearance.BorderSize = 1;
        btnDbConfig.Click += (_, _) => OpenDbConfigDialog();
        leftPanel.Controls.Add(btnDbConfig);

        // Right Login Form Panel
        var rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(252, 249, 248), // #fcf9f8
            Padding = new Padding(44, 36, 44, 36)
        };
        split.Controls.Add(rightPanel, 1, 0);

        var lblTitle = new Label
        {
            Text = "Đăng nhập hệ thống",
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(34, 34, 34),
            AutoSize = true,
            Location = new Point(44, 36)
        };
        rightPanel.Controls.Add(lblTitle);

        var lblSub = new Label
        {
            Text = "Nhập tài khoản quản trị hoặc nhân viên để tiếp tục.",
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(101, 101, 101), // #656565
            AutoSize = true,
            Location = new Point(46, 76)
        };
        rightPanel.Controls.Add(lblSub);

        var lblQuick = new Label
        {
            Text = "Chọn nhanh tài khoản Demo (vấn đáp):",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(34, 34, 34),
            AutoSize = true,
            Location = new Point(46, 118)
        };
        rightPanel.Controls.Add(lblQuick);

        _cboQuickDemo = new ComboBox
        {
            Location = new Point(46, 142),
            Width = 390,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9.5F)
        };
        _cboQuickDemo.Items.AddRange(new object[]
        {
            "Quản lý (Full quyền ADMIN) — admin@henry.example / 123456",
            "Lễ tân (Quyền STAFF) — bich.le@henry.example / 123456",
            "Kế toán (Quyền STAFF) — cuong.tran@henry.example / 123456"
        });
        _cboQuickDemo.SelectedIndexChanged += (_, _) =>
        {
            if (_cboQuickDemo.SelectedIndex == 0) { _txtAccount.Text = "admin@henry.example"; _txtPassword.Text = "123456"; }
            else if (_cboQuickDemo.SelectedIndex == 1) { _txtAccount.Text = "bich.le@henry.example"; _txtPassword.Text = "123456"; }
            else if (_cboQuickDemo.SelectedIndex == 2) { _txtAccount.Text = "cuong.tran@henry.example"; _txtPassword.Text = "123456"; }
        };
        rightPanel.Controls.Add(_cboQuickDemo);

        var lblAcc = new Label
        {
            Text = "Email hoặc Số điện thoại",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(34, 34, 34),
            AutoSize = true,
            Location = new Point(46, 185)
        };
        rightPanel.Controls.Add(lblAcc);

        _txtAccount = new TextBox
        {
            Location = new Point(46, 210),
            Width = 390,
            Font = new Font("Segoe UI", 11F),
            Text = "admin@henry.example"
        };
        rightPanel.Controls.Add(_txtAccount);

        var lblPwd = new Label
        {
            Text = "Mật khẩu",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(34, 34, 34),
            AutoSize = true,
            Location = new Point(46, 255)
        };
        rightPanel.Controls.Add(lblPwd);

        _txtPassword = new TextBox
        {
            Location = new Point(46, 280),
            Width = 340,
            Font = new Font("Segoe UI", 11F),
            UseSystemPasswordChar = true,
            Text = "123456"
        };
        rightPanel.Controls.Add(_txtPassword);

        var btnEye = new Button
        {
            Text = "Hiện",
            Location = new Point(390, 279),
            Size = new Size(46, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(246, 243, 242), // #f6f3f2
            ForeColor = Color.FromArgb(34, 34, 34),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnEye.FlatAppearance.BorderColor = Color.FromArgb(233, 229, 227); // #e9e5e3
        btnEye.Click += (_, _) =>
        {
            _txtPassword.UseSystemPasswordChar = !_txtPassword.UseSystemPasswordChar;
            btnEye.Text = _txtPassword.UseSystemPasswordChar ? "Hiện" : "Ẩn";
        };
        rightPanel.Controls.Add(btnEye);

        _lblError = new Label
        {
            Text = "",
            ForeColor = Color.FromArgb(186, 0, 54),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            MaximumSize = new Size(390, 0),
            AutoSize = true,
            Location = new Point(46, 322)
        };
        rightPanel.Controls.Add(_lblError);

        var btnLogin = new Button
        {
            Text = "ĐĂNG NHẬP HỆ THỐNG",
            Location = new Point(46, 365),
            Size = new Size(390, 44),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(186, 0, 54),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += (_, _) => PerformLogin();
        rightPanel.Controls.Add(btnLogin);

        AcceptButton = btnLogin;
        _cboQuickDemo.SelectedIndex = 0;
    }

    private void PerformLogin()
    {
        _lblError.Text = "";
        var res = _authService.Login(_txtAccount.Text, _txtPassword.Text, requireAdminOrStaff: true);
        if (!res.Success || res.Data == null)
        {
            _lblError.Text = res.Message;
            return;
        }

        LoggedInUser = res.Data;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OpenDbConfigDialog()
    {
        var cfg = DatabaseConfigManager.LoadConfig();
        using var dlg = new Form
        {
            Text = "Cấu hình kết nối Cơ sở dữ liệu (SQL Server / Shared DB)",
            Size = new Size(580, 320),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(252, 249, 248), // #fcf9f8
            Font = new Font("Segoe UI", 9.5F)
        };

        var lblMode = new Label { Text = "Chế độ kết nối:", Location = new Point(24, 22), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(34, 34, 34) };
        var cboMode = new ComboBox { Location = new Point(24, 46), Width = 510, DropDownStyle = ComboBoxStyle.DropDownList };
        cboMode.Items.AddRange(new object[]
        {
            "Auto — Tự động kết nối SQL Server nếu bật, dự phòng Shared DB",
            "SqlServer — Bắt buộc dùng chuỗi kết nối Microsoft SQL Server bên dưới",
            "Sqlite — Dùng file CSDL dùng chung (HenryHomestayShared.db)"
        });
        cboMode.SelectedIndex = cfg.Provider == "SqlServer" ? 1 : cfg.Provider == "Sqlite" ? 2 : 0;

        var lblConn = new Label { Text = "Chuỗi kết nối Microsoft SQL Server:", Location = new Point(24, 88), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(34, 34, 34) };
        var txtConn = new TextBox { Location = new Point(24, 112), Width = 510, Text = cfg.SqlServerConnectionString };

        var lblStatus = new Label { Text = "Đang hoạt động: " + DatabaseConfigManager.GetActiveDatabaseSummary(), Location = new Point(24, 150), MaximumSize = new Size(510, 0), AutoSize = true, ForeColor = Color.FromArgb(0, 106, 69), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

        var btnTest = new Button { Text = "Kiểm tra SQL Server", Location = new Point(24, 215), Size = new Size(160, 36), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(246, 243, 242), ForeColor = Color.FromArgb(34, 34, 34), Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
        btnTest.FlatAppearance.BorderColor = Color.FromArgb(233, 229, 227);
        btnTest.Click += (_, _) =>
        {
            if (DatabaseConfigManager.TrySqlServerConnection(txtConn.Text, out var err))
                MessageBox.Show("Kết nối đến Microsoft SQL Server thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Không thể kết nối SQL Server:\n" + err, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };

        var btnSave = new Button { Text = "Lưu & Khởi tạo CSDL", Location = new Point(354, 215), Size = new Size(180, 36), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(186, 0, 54), ForeColor = Color.White };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) =>
        {
            cfg.Provider = cboMode.SelectedIndex == 1 ? "SqlServer" : cboMode.SelectedIndex == 2 ? "Sqlite" : "Auto";
            cfg.SqlServerConnectionString = txtConn.Text.Trim();
            DatabaseConfigManager.SaveConfig(cfg);
            try
            {
                DbInitializer.Initialize();
                _lblDbStatus.Text = "● CSDL: " + DatabaseConfigManager.GetActiveDatabaseSummary();
                MessageBox.Show("Đã lưu cấu hình và đồng bộ lược đồ CSDL!\n" + DatabaseConfigManager.GetActiveDatabaseSummary(), "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dlg.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo CSDL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        dlg.Controls.AddRange(new Control[] { lblMode, cboMode, lblConn, txtConn, lblStatus, btnTest, btnSave });
        dlg.ShowDialog(this);
    }
}
