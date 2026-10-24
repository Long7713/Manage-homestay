using System.Drawing.Drawing2D;
namespace StayEase.Admin.Forms;

internal static class Ui
{
    public static readonly Color Green=Color.FromArgb(17,43,29);
    public static readonly Color Green2=Color.FromArgb(32,72,49);
    public static readonly Color Page=Color.FromArgb(248,247,244);
    public static readonly Color Gray=Color.FromArgb(110,119,111);
    public static readonly Color Rust=Color.FromArgb(155,76,43);
    public static readonly Color Border=Color.FromArgb(230,229,224);
    public static Font Font(int size,bool bold=false)=>new("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular);
    public static Button Button(string text,bool dark=true,int w=145)
    {
        return new Button{Text=text,Width=w,Height=38,BackColor=dark?Green:Color.White,
            ForeColor=dark?Color.White:Green,FlatStyle=FlatStyle.Flat,
            Font=Font(10,true),Cursor=Cursors.Hand,Margin=new Padding(5)}.Borderless();
    }
    public static Button Borderless(this Button b)
    {
        b.FlatAppearance.BorderSize=0;
        return b;
    }
    public static Label Label(string text,int size=10,bool bold=false,Color? color=null)
    {
        return new Label{Text=text,Font=Font(size,bold),ForeColor=color??Green,
            AutoSize=true,Margin=new Padding(3,7,3,7)};
    }
    public static TextBox Input(string value="",int width=220)
    {
        return new TextBox{Text=value,Width=width,Font=Font(11),BorderStyle=BorderStyle.FixedSingle,Margin=new Padding(5,8,12,8)};
    }
    public static Panel Card(int minH=110)
    {
        return new Panel{BackColor=Color.White,MinimumSize=new Size(0,minH),Margin=new Padding(0,0,0,15),Padding=new Padding(20)};
    }
    public static DataGridView Grid(object? data=null)
    {
        var grid = new DataGridView {
            Dock = DockStyle.Fill,
            DataSource = data,
            AutoGenerateColumns = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 44,
            RowTemplate = { Height = 42 },
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Border,
            RowHeadersVisible = false,
            ReadOnly = true,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = Font(10),
            ForeColor = Green
        };

        // Cấu hình màu sắc khi chọn dòng/ô thông qua DefaultCellStyle
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 233, 222);
        grid.DefaultCellStyle.SelectionForeColor = Green;

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 241);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Green;
        grid.ColumnHeadersDefaultCellStyle.Font = Font(10, true);
        return grid;
    }
    public static FlowLayoutPanel Horizontal()=>new(){Dock=DockStyle.Top,AutoSize=true,WrapContents=true,
        FlowDirection=FlowDirection.LeftToRight,Padding=new Padding(0,0,0,15)};
    public static Panel Header(string title,string subtitle="")
    {
        var p=new Panel{Dock=DockStyle.Top,Height=90};
        var h=Label(title,21,true);h.Location=new Point(0,3);p.Controls.Add(h);
        var d=Label(subtitle,10,false,Gray);d.Location=new Point(2,46);p.Controls.Add(d);
        return p;
    }
}
