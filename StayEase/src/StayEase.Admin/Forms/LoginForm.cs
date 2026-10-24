using StayEase.Admin.Services;
namespace StayEase.Admin.Forms;

public sealed class LoginForm:Form
{
    private readonly TextBox _server=Ui.Input("http://localhost:5188",340);
    private readonly TextBox _email=Ui.Input("admin@stayease.local",340);
    private readonly TextBox _password=Ui.Input("",340);
    private readonly Label _hint=Ui.Label("Đăng nhập bằng tài khoản có quyền Admin.",10,false,Ui.Gray);
    private readonly Button _login=Ui.Button("Đăng nhập quản trị →",true,340);
    public LoginForm()
    {
        Text="StayEase · Đăng nhập quản trị";StartPosition=FormStartPosition.CenterScreen;
        MinimumSize=new Size(520,600);Size=new Size(600,650);BackColor=Ui.Page;
        FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(85,40,85,40),ColumnCount=1,RowCount=13,AutoScroll=true};
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));Controls.Add(root);
        var logo=Ui.Label("❧ STAYEASE",24,true);logo.TextAlign=ContentAlignment.MiddleCenter;
        var sub=Ui.Label("SANCTUARY  /  ADMIN CENTER",10,true,Ui.Rust);
        sub.TextAlign=ContentAlignment.MiddleCenter;
        root.Controls.Add(logo);root.Controls.Add(sub);
        root.Controls.Add(Ui.Label("HỆ THỐNG QUẢN TRỊ HOMESTAY",12,true));
        root.Controls.Add(Ui.Label("Địa chỉ máy chủ API",10,true));root.Controls.Add(_server);
        root.Controls.Add(Ui.Label("Email quản trị",10,true));root.Controls.Add(_email);
        root.Controls.Add(Ui.Label("Mật khẩu",10,true));_password.PasswordChar='●';root.Controls.Add(_password);
        root.Controls.Add(Ui.Label("Demo: admin@stayease.local / Admin@12345",9,false,Ui.Gray));
        root.Controls.Add(_login);root.Controls.Add(_hint);
        _login.Click+=async (_,_)=>await LoginAsync();
        AcceptButton=_login;
    }
    private async Task LoginAsync()
    {
        _login.Enabled=false;_hint.Text="Đang kết nối API...";
        var api=new AdminClient();
        try
        {
            api.Configure(_server.Text);
            await api.LoginAsync(_email.Text,_password.Text);
            Hide();
            using(var main=new MainForm(api))main.ShowDialog(this);
            Close();
        }
        catch(Exception ex){api.Dispose();_hint.ForeColor=Color.Firebrick;_hint.Text=ex.Message;}
        finally{_login.Enabled=true;}
    }
}
