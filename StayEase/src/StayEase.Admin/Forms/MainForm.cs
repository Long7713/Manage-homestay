using System.ComponentModel;
using System.Globalization;
using System.Text;
using StayEase.Admin.Models;
using StayEase.Admin.Services;

namespace StayEase.Admin.Forms;

public sealed class MainForm:Form
{
    private readonly AdminClient _api;
    private readonly FlowLayoutPanel _content=new()
    {
        Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,
        Padding=new Padding(25,20,25,30),BackColor=Ui.Page
    };
    private readonly Label _section=Ui.Label("Tổng quan hệ thống",15,true);
    private readonly List<Button> _navButtons=[];
    private string _active="Tổng quan";
    private List<RoomDto> _rooms=[];
    private List<BookingDto> _bookings=[];
    private List<AdminUserDto> _adminUsers=[];
    private List<ReportDto> _report=[];

    public MainForm(AdminClient api)
    {
        _api=api;
        Text="StayEase Sanctuary · Quản trị Homestay";WindowState=FormWindowState.Maximized;
        MinimumSize=new Size(1060,720);BackColor=Ui.Page;
        var sidebar=new Panel{Dock=DockStyle.Left,Width=235,BackColor=Ui.Green};
        var main=new Panel{Dock=DockStyle.Fill,BackColor=Ui.Page};
        Controls.Add(main);Controls.Add(sidebar);
        var brand=Ui.Label("❧ StayEase",23,true,Color.White);brand.Location=new Point(25,28);sidebar.Controls.Add(brand);
        var caption=Ui.Label("SANCTUARY · ADMIN",9,true,Color.FromArgb(171,199,178));caption.Location=new Point(28,76);sidebar.Controls.Add(caption);
        var menu=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,
            Padding=new Padding(12,15,12,0),AutoScroll=true,BackColor=Ui.Green};
        var footer=Ui.Label("C# Windows Forms · ASP.NET Core API",8,false,Color.FromArgb(175,197,178));
        footer.Dock=DockStyle.Bottom;footer.Padding=new Padding(14,0,0,15);footer.AutoSize=false;footer.Height=44;
        sidebar.Controls.Add(footer);sidebar.Controls.Add(menu);
        brand.BringToFront();caption.BringToFront();
        menu.Margin=new Padding(0,115,0,0);menu.Padding=new Padding(10,115,10,0);
        AddNav(menu,"▦  Tổng quan",LoadDashboard);
        AddNav(menu,"♧  Hạng phòng",LoadRooms);
        AddNav(menu,"▤  Đơn đặt phòng",LoadBookings);
        AddNav(menu,"♙  Khách hàng",LoadCustomers);
        AddNav(menu,"◈  Thanh toán",LoadPayments);
        AddNav(menu,"↩  Yêu cầu hoàn cọc",LoadRefunds);
        AddNav(menu,"⚙  Cài đặt tỷ lệ cọc",LoadDepositSettings);
        AddNav(menu,"◴  Báo cáo",LoadReports);
        AddNav(menu,"⚙  Tài khoản & phân quyền",LoadUsers);
        var top=new Panel{Dock=DockStyle.Top,Height=78,BackColor=Color.White,Padding=new Padding(25,15,25,12)};
        _section.Location=new Point(25,20);top.Controls.Add(_section);
        var user=Ui.Label($"● {_api.Name}",11,true,Ui.Green);user.Anchor=AnchorStyles.Right|AnchorStyles.Top;
        user.Location=new Point(680,22);top.Controls.Add(user);
        var logout=Ui.Button("Đăng xuất",false,110);logout.Anchor=AnchorStyles.Right|AnchorStyles.Top;
        logout.Location=new Point(850,15);top.Controls.Add(logout);
        top.Resize+=(_,_)=>{logout.Left=top.Width-logout.Width-25;user.Left=Math.Max(350,logout.Left-user.PreferredWidth-25);};
        logout.Click+=(_,_)=>{_api.Dispose();Application.Restart();Close();};
        main.Controls.Add(_content);main.Controls.Add(top);
        _content.SizeChanged+=(_,_)=>FitContent();
        Shown+=async (_,_)=>await SafeLoad(LoadDashboard);
    }

    private void AddNav(FlowLayoutPanel menu,string label,Func<Task> loader)
    {
        var b=new Button{Text=label,TextAlign=ContentAlignment.MiddleLeft,Width=205,Height=48,BackColor=Ui.Green,
            ForeColor=Color.FromArgb(224,236,228),FlatStyle=FlatStyle.Flat,Font=Ui.Font(10,true),
            Cursor=Cursors.Hand,Margin=new Padding(0,4,0,4),Padding=new Padding(16,0,0,0)};
        b.FlatAppearance.BorderSize=0;b.FlatAppearance.MouseOverBackColor=Ui.Green2;
        b.Click+=async (_,_)=>{_active=label;foreach(var item in _navButtons)item.BackColor=Ui.Green;b.BackColor=Ui.Green2;await SafeLoad(loader);};
        menu.Controls.Add(b);_navButtons.Add(b);
    }
    private async Task SafeLoad(Func<Task> action)
    {
        try{UseWaitCursor=true;await action();}
        catch(Exception ex){MessageBox.Show(this,ex.Message,"StayEase · Lỗi kết nối",MessageBoxButtons.OK,MessageBoxIcon.Error);}
        finally{UseWaitCursor=false;}
    }
    private void Page(string name,string subtitle)
    {
        _section.Text=name;_content.SuspendLayout();_content.Controls.Clear();
        _content.Controls.Add(Ui.Header(name,subtitle));
        _content.ResumeLayout();FitContent();
    }
    private void FitContent()
    {
        foreach(Control child in _content.Controls)
            child.Width=Math.Max(550,_content.ClientSize.Width-75);
    }
    private void Add(Control c){_content.Controls.Add(c);FitContent();}
    private static Panel GridCard(DataGridView grid,int height=460)
    {
        var p=Ui.Card(height);p.Height=height;p.Controls.Add(grid);return p;
    }
    private static Panel Metric(string label,string value,string note)
    {
        var c=Ui.Card(122);c.Width=242;c.Height=125;c.Margin=new Padding(0,0,14,14);
        var tag=Ui.Label(label,10,false,Ui.Gray);tag.Location=new Point(16,15);c.Controls.Add(tag);
        var number=Ui.Label(value,22,true);number.Location=new Point(16,40);c.Controls.Add(number);
        var hint=Ui.Label(note,9,false,Ui.Rust);hint.Location=new Point(17,92);c.Controls.Add(hint);
        return c;
    }
    private static FlowLayoutPanel Actions(params Control[] controls)
    {
        var row=Ui.Horizontal();foreach(var c in controls)row.Controls.Add(c);return row;
    }
    private static void HideColumns(DataGridView grid,params string[] names)
    {
        foreach(var name in names)if(grid.Columns.Contains(name))grid.Columns[name]!.Visible=false;
    }
    private static T? Selected<T>(DataGridView grid) where T:class => grid.CurrentRow?.DataBoundItem as T;
    private void SearchBox<T>(DataGridView grid,IReadOnlyList<T> source,Func<T,string> selector,string placeholder)
    {
        var text=Ui.Input("",350);text.PlaceholderText=placeholder;
        text.TextChanged+=(_,_)=>grid.DataSource=source.Where(x=>selector(x).Contains(text.Text,StringComparison.CurrentCultureIgnoreCase)).ToList();
        Add(Actions(Ui.Label("Tìm kiếm:",10,true),text));
    }
    private static string Vnd(decimal number)=>number.ToString("N0",CultureInfo.GetCultureInfo("vi-VN"))+" ₫";

    private async Task LoadDashboard()
    {
        var dto=await _api.GetAsync<DashboardDto>("api/admin/dashboard");
        Page("Tổng quan quản lý","Hoạt động homestay và các chỉ số vận hành gần đây");
        var cards=new FlowLayoutPanel{FlowDirection=FlowDirection.LeftToRight,WrapContents=true,AutoSize=true,
            Margin=new Padding(0,0,0,15)};
        cards.Controls.Add(Metric("Tổng phòng",dto.TotalRooms.ToString(),"Đang hoạt động"));
        cards.Controls.Add(Metric("Hạng phòng",dto.RoomTypes.ToString(),"Danh mục đang kinh doanh"));
        cards.Controls.Add(Metric("Đặt phòng",dto.TotalBookings.ToString(),"Toàn bộ đơn"));
        cards.Controls.Add(Metric("Khách hàng",dto.Customers.ToString(),"Tài khoản hệ thống"));
        cards.Controls.Add(Metric("Chờ thanh toán",dto.Pending.ToString(),"Cần theo dõi"));
        cards.Controls.Add(Metric("Đã xác nhận",dto.Confirmed.ToString(),"Có lịch lưu trú"));
        cards.Controls.Add(Metric("Doanh thu cọc 30 ngày",Vnd(dto.Revenue30Days),"Demo / đã ghi nhận"));
        Add(cards);Add(Ui.Label("ĐẶT PHÒNG GẦN NHẤT",12,true));
        var rows=dto.Recent.Select(b=>new {MãĐơn=b.Reference,Khách=b.GuestName,TổngTiền=Vnd(b.TotalPrice),
            TrạngThái=b.Status,NhậnPhòng=b.CheckIn.ToString("dd/MM/yyyy")}).ToList();
        Add(GridCard(Ui.Grid(rows),390));
    }

    private async Task LoadRooms()
    {
        _rooms=await _api.GetAsync<List<RoomDto>>("api/admin/roomtypes");
        Page("Quản lý hạng phòng","Thêm, chỉnh sửa, ngừng kinh doanh và bổ sung phòng vật lý");
        var add=Ui.Button("+ Thêm hạng phòng",true,180);
        add.Click+=async (_,_)=>{using var dialog=new RoomEditor();if(dialog.ShowDialog(this)==DialogResult.OK)
        {await SafeLoad(async()=>{await _api.PostAsync("api/admin/roomtypes",dialog.GetData());await LoadRooms();});}};
        var edit=Ui.Button("✎ Sửa hạng phòng",false,165);
        var plus=Ui.Button("+ Thêm phòng vật lý",false,185);
        var archive=Ui.Button("Ẩn hạng phòng",false,155);
        Add(Actions(add,edit,plus,archive));
        var grid=Ui.Grid(_rooms);HideColumns(grid,"Description","Features","ImagePath","Slug","InitialUnits");
        if(grid.Columns.Contains("Name"))grid.Columns["Name"]!.HeaderText="Hạng phòng";
        if(grid.Columns.Contains("PricePerNight"))grid.Columns["PricePerNight"]!.HeaderText="Giá / đêm (VND)";
        if(grid.Columns.Contains("UnitCount"))grid.Columns["UnitCount"]!.HeaderText="Số phòng";
        SearchBox(grid,_rooms,r=>$"{r.Name} {r.Slug} {r.PricePerNight}","Tên phòng hoặc slug...");
        Add(GridCard(grid,500));
        edit.Click+=async (_,_)=>{var room=Selected<RoomDto>(grid);if(room is null)return;
            using var dialog=new RoomEditor(room);if(dialog.ShowDialog(this)==DialogResult.OK)
            await SafeLoad(async()=>{await _api.PutAsync($"api/admin/roomtypes/{room.Id}",dialog.GetData());await LoadRooms();});};
        plus.Click+=async (_,_)=>{var room=Selected<RoomDto>(grid);if(room is null)return;
            using var ask=new CountDialog(room.Name);if(ask.ShowDialog(this)==DialogResult.OK)
            await SafeLoad(async()=>{await _api.PostAsync($"api/admin/roomtypes/{room.Id}/units?count={ask.Count}",new {});await LoadRooms();});};
        archive.Click+=async (_,_)=>{var room=Selected<RoomDto>(grid);if(room is null)return;
            if(MessageBox.Show(this,$"Ẩn hạng phòng {room.Name} khỏi website?","Xác nhận",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            await SafeLoad(async()=>{await _api.DeleteAsync($"api/admin/roomtypes/{room.Id}");await LoadRooms();});};
    }

    private async Task LoadBookings()
    {
        _bookings=await _api.GetAsync<List<BookingDto>>("api/admin/bookings");
        Page("Quản lý đặt phòng","Theo dõi đơn, trạng thái nhận phòng, trả phòng và hủy");
        var reload=Ui.Button("↻ Làm mới",false,120);
        var checkIn=Ui.Button("✓ Nhận phòng",true,140);
        var checkOut=Ui.Button("↪ Trả phòng",false,140);
        var cancel=Ui.Button("✕ Hủy đặt phòng",false,160);
        Add(Actions(reload,checkIn,checkOut,cancel));
        var grid=Ui.Grid(_bookings);HideColumns(grid,"Id","GuestEmail","GuestPhone","CreatedAtUtc","Note");
        if(grid.Columns.Contains("Reference"))grid.Columns["Reference"]!.HeaderText="Mã đặt phòng";
        if(grid.Columns.Contains("GuestName"))grid.Columns["GuestName"]!.HeaderText="Khách đặt";
        if(grid.Columns.Contains("Room"))grid.Columns["Room"]!.HeaderText="Hạng phòng";
        if(grid.Columns.Contains("Status"))grid.Columns["Status"]!.HeaderText="Trạng thái";
        SearchBox(grid,_bookings,b=>$"{b.Reference} {b.GuestName} {b.Room} {b.Status}","Mã đơn, tên khách, hạng phòng...");
        Add(GridCard(grid,560));
        reload.Click+=async(_,_)=>await SafeLoad(LoadBookings);
        checkIn.Click+=async(_,_)=>await UpdateBooking(grid,"CheckedIn","đánh dấu đã nhận phòng");
        checkOut.Click+=async(_,_)=>await UpdateBooking(grid,"CheckedOut","đánh dấu đã trả phòng");
        cancel.Click+=async(_,_)=>await UpdateBooking(grid,"Cancelled","hủy đơn");
    }
    private async Task UpdateBooking(DataGridView grid,string newStatus,string description)
    {
        var b=Selected<BookingDto>(grid);if(b is null)return;
        if(MessageBox.Show(this,$"Xác nhận {description} cho {b.Reference}?","Xác nhận cập nhật",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
        await SafeLoad(async()=>{await _api.PatchAsync($"api/admin/bookings/{b.Id}/status",new {status=newStatus});await LoadBookings();});
    }

    private async Task LoadCustomers()
    {
        var customers=await _api.GetAsync<List<CustomerDto>>("api/admin/customers");
        Page("Khách hàng lưu trú","Danh sách khách đã đăng ký và tổng số đặt phòng");
        Add(Ui.Label($"Tổng cộng {customers.Count} tài khoản khách hàng",11,false,Ui.Gray));
        var grid=Ui.Grid(customers);HideColumns(grid,"Id");
        if(grid.Columns.Contains("FullName"))grid.Columns["FullName"]!.HeaderText="Họ tên";
        if(grid.Columns.Contains("Email"))grid.Columns["Email"]!.HeaderText="Email";
        if(grid.Columns.Contains("BookingCount"))grid.Columns["BookingCount"]!.HeaderText="Số lượt đặt";
        SearchBox(grid,customers,c=>$"{c.FullName} {c.Email} {c.PhoneNumber}","Tên khách hoặc email...");
        Add(GridCard(grid,565));
    }

    private async Task LoadPayments()
    {
        var payments=await _api.GetAsync<List<PaymentDto>>("api/admin/payments");
        Page("Thanh toán VietQR & đặt cọc","Chỉ bấm xác nhận sau khi đã kiểm tra tiền về tài khoản ngân hàng thực tế.");
        Add(Ui.Label($"Đã nhận tiền (Admin xác nhận): {Vnd(payments.Where(p=>p.Status=="Succeeded").Sum(p=>p.Amount))}",15,true,Ui.Green));
        var confirm=Ui.Button("✓ Xác nhận đã nhận tiền",true,260);
        Add(Actions(confirm));
        var grid=Ui.Grid(payments);HideColumns(grid,"Id");
        if(grid.Columns.Contains("Amount"))grid.Columns["Amount"]!.HeaderText="Số tiền cọc / thanh toán";
        SearchBox(grid,payments,p=>$"{p.Reference} {p.GuestName} {p.Status}","Mã đơn, khách, trạng thái...");
        Add(GridCard(grid,550));
        confirm.Click+=async(_,_)=>{
            var p=Selected<PaymentDto>(grid);if(p is null)return;
            if(p.Status is not ("Pending" or "AwaitingVerification"))
            {MessageBox.Show(this,"Chỉ xác nhận được giao dịch đang chờ.");return;}
            if(MessageBox.Show(this,$"Bạn đã kiểm tra ngân hàng và xác nhận THỰC NHẬN {Vnd(p.Amount)} cho đơn {p.Reference}?",
                "Xác nhận tiền thực nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;
            await SafeLoad(async()=>{await _api.PostAsync($"api/admin/payments/{p.Id}/confirm",new{});await LoadPayments();});
        };
    }

    private async Task LoadRefunds()
    {
        var refunds=await _api.GetAsync<List<RefundDto>>("api/admin/refunds");
        Page("Yêu cầu hoàn cọc","Đối soát số tài khoản hoàn tiền; cập nhật trạng thái chỉ sau khi chuyển tiền thực tế.");
        var paid=Ui.Button("✓ Đã chuyển hoàn cọc",true,220);
        var not=Ui.Button("✕ Không hoàn cọc",false,180);
        Add(Actions(paid,not));
        var grid=Ui.Grid(refunds);HideColumns(grid,"Id","ContactEmail","ContactPhone","RequestedAtUtc","ProcessedAtUtc");
        SearchBox(grid,refunds,r=>$"{r.Reference} {r.ContactName} {r.BankAccountNumber}","Mã đơn, tên khách, số tài khoản...");
        Add(GridCard(grid,530));
        async Task Decide(string decision)
        {
            var r=Selected<RefundDto>(grid);if(r is null)return;
            if(r.Status!="Pending"){MessageBox.Show(this,"Yêu cầu không ở trạng thái chờ.");return;}
            if(MessageBox.Show(this,$"Đơn {r.Reference} · {Vnd(r.Amount)} · {r.BankName} - {r.BankAccountNumber}.\n"+
                (decision=="Refunded"?"Bạn đã chuyển hoàn tiền THỰC TẾ?":"Xác nhận không hoàn cọc?"),
                "Xác nhận xử lý hoàn cọc",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;
            await SafeLoad(async()=>{await _api.PostAsync($"api/admin/refunds/{r.Id}/decision",new{decision});await LoadRefunds();});
        }
        paid.Click+=async(_,_)=>await Decide("Refunded");
        not.Click+=async(_,_)=>await Decide("NotRefunded");
    }

    private async Task LoadDepositSettings()
    {
        var current=await _api.GetAsync<DepositSettingsDto>("api/admin/settings");
        Page("Cài đặt tỷ lệ đặt cọc","Tỷ lệ mới chỉ có tác dụng với các đơn được tạo sau khi lưu.");
        var box=Ui.Card(205);
        var prompt=Ui.Label("Tỷ lệ cọc (%)",11,true);prompt.Location=new Point(20,22);box.Controls.Add(prompt);
        var value=new NumericUpDown{Minimum=1,Maximum=100,Value=Math.Clamp(current.DepositPercent,1,100),
            Width=120,Height=36,Font=Ui.Font(14,true),Location=new Point(20,57)};
        box.Controls.Add(value);
        var info=Ui.Label("Đang áp dụng: "+current.DepositPercent+"%. Đơn cũ giữ nguyên tỷ lệ tại thời điểm đặt.",10,false,Ui.Gray);
        info.Location=new Point(20,100);box.Controls.Add(info);
        var button=Ui.Button("Lưu tỷ lệ cọc",true,170);button.Location=new Point(180,56);box.Controls.Add(button);
        button.Click+=async(_,_)=>await SafeLoad(async()=>{await _api.PutAsync("api/admin/settings/deposit",new {depositPercent=(int)value.Value});await LoadDepositSettings();});
        Add(box);
    }

    private async Task LoadReports()
    {
        _report=await _api.GetAsync<List<ReportDto>>("api/admin/reports");
        Page("Báo cáo vận hành","Thống kê số đơn theo ngày nhận phòng trong 30 ngày gần nhất");
        var export=Ui.Button("↓ Xuất báo cáo CSV",true,175);
        var refresh=Ui.Button("↻ Làm mới",false,120);
        Add(Actions(export,refresh));
        var reportGrid=Ui.Grid(_report);Add(GridCard(reportGrid,480));
        refresh.Click+=async(_,_)=>await SafeLoad(LoadReports);
        export.Click+=(_,_)=>ExportCsv();
        Add(Ui.Label("Lưu ý: Doanh thu cọc thực tế được tính từ bảng Payments; cột GrossAmount là giá trị đơn đặt phòng của ngày nhận phòng.",10,false,Ui.Gray));
    }
    private void ExportCsv()
    {
        using var dlg=new SaveFileDialog{Filter="CSV UTF-8 (*.csv)|*.csv",FileName="StayEase-Report.csv"};
        if(dlg.ShowDialog(this)!=DialogResult.OK)return;
        var lines=new List<string>{"Date,Bookings,Confirmed,GrossAmount"};
        lines.AddRange(_report.Select(r=>$"{r.Date:yyyy-MM-dd},{r.Bookings},{r.Confirmed},{r.GrossAmount.ToString(CultureInfo.InvariantCulture)}"));
        File.WriteAllLines(dlg.FileName,lines,new UTF8Encoding(true));
        MessageBox.Show(this,"Đã xuất báo cáo CSV.","StayEase");
    }

    private async Task LoadUsers()
    {
        _adminUsers=await _api.GetAsync<List<AdminUserDto>>("api/admin/users");
        Page("Tài khoản & phân quyền","Phân vai trò Customer / Admin. Đảm bảo chỉ người được cấp quyền mới quản trị.");
        var promote=Ui.Button("↑ Cấp quyền Admin",true,180);
        var demote=Ui.Button("↓ Chuyển thành Customer",false,220);
        Add(Actions(promote,demote));
        var grid=Ui.Grid(_adminUsers);HideColumns(grid,"Id");
        SearchBox(grid,_adminUsers,u=>$"{u.FullName} {u.Email}","Tên hoặc email...");
        Add(GridCard(grid,560));
        promote.Click+=async(_,_)=>await SetRole(grid,"Admin");
        demote.Click+=async(_,_)=>await SetRole(grid,"Customer");
    }
    private async Task SetRole(DataGridView grid,string role)
    {
        var u=Selected<AdminUserDto>(grid);if(u is null)return;
        if(MessageBox.Show(this,$"Đổi vai trò {u.Email} thành {role}?","Xác nhận phân quyền",
            MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;
        await SafeLoad(async()=>{await _api.PatchAsync($"api/admin/users/{u.Id}/role",new {role});await LoadUsers();});
    }
}
