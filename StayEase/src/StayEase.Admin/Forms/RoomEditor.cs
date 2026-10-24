using StayEase.Admin.Models;
namespace StayEase.Admin.Forms;

public sealed class RoomEditor:Form
{
    private readonly RoomDto? _room;
    private readonly Dictionary<string,TextBox> _fields=[];
    private readonly NumericUpDown _area=Num(32,1,10000);
    private readonly NumericUpDown _adults=Num(2,1,20);
    private readonly NumericUpDown _children=Num(0,0,20);
    private readonly NumericUpDown _price=Num(1450000,10000,50000000,100000);
    private readonly NumericUpDown _units=Num(2,0,20);
    private readonly CheckBox _active=new(){Text="Đang hoạt động",Checked=true,AutoSize=true};

    private static NumericUpDown Num(decimal initial,decimal min,decimal max,decimal step=1)
        =>new(){Value=initial,Minimum=min,Maximum=max,Increment=step,Width=240,Font=Ui.Font(11),ThousandsSeparator=true};
    public RoomEditor(RoomDto? room=null)
    {
        _room=room;
        Text=room is null?"Thêm hạng phòng":"Cập nhật hạng phòng";
        StartPosition=FormStartPosition.CenterParent;MinimumSize=new Size(680,750);Size=new Size(730,780);
        BackColor=Ui.Page;
        var page=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,AutoScroll=true,
            Padding=new Padding(30),CellBorderStyle=TableLayoutPanelCellBorderStyle.None};
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,34));
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,66));
        Controls.Add(page);
        AddText(page,"Tên hạng phòng","Name",room?.Name??"");
        AddText(page,"Slug (không dấu)","Slug",room?.Slug??"");
        AddText(page,"Mô tả","Description",room?.Description??"",true);
        AddText(page,"Đường dẫn ảnh","ImagePath",room?.ImagePath??"/images/pine-nest.webp");
        AddText(page,"Tiện nghi (ngăn bằng dấu phẩy)","Features",room?.Features??"Wi-Fi,Nước nóng",true);
        AddRow(page,"Diện tích (m²)",_area,room?.AreaSqm);
        AddRow(page,"Người lớn tối đa",_adults,room?.Adults);
        AddRow(page,"Trẻ em tối đa",_children,room?.Children);
        AddRow(page,"Giá theo đêm (VND)",_price,room?.PricePerNight);
        AddRow(page,"Số phòng tạo ban đầu",_units,room is null?2:0);
        if(room is not null)_units.Enabled=false;
        AddRow(page,"Trạng thái",_active,null);_active.Checked=room?.IsActive??true;
        var save=Ui.Button("Lưu thông tin",true,170);
        save.Click+=(_,_)=>{
            if(string.IsNullOrWhiteSpace(_fields["Name"].Text)||string.IsNullOrWhiteSpace(_fields["Slug"].Text))
            {MessageBox.Show(this,"Tên và slug không được để trống.");return;}
            DialogResult=DialogResult.OK;Close();
        };
        AddRow(page,"",save,null);
    }
    private void AddText(TableLayoutPanel page,string label,string field,string value,bool multi=false)
    {
        var tb=Ui.Input(value,365);
        if(multi){tb.Multiline=true;tb.Height=73;tb.ScrollBars=ScrollBars.Vertical;}
        _fields[field]=tb;AddRow(page,label,tb,null);
    }
    private static void AddRow(TableLayoutPanel page,string label,Control c,decimal? value)
    {
        page.RowCount++;
        page.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var lbl=Ui.Label(label,10,true,Ui.Green);
        lbl.MaximumSize=new Size(200,0);lbl.Padding=new Padding(0,8,0,0);
        if(c is NumericUpDown num && value.HasValue)num.Value=Math.Clamp(value.Value,num.Minimum,num.Maximum);
        c.Margin=new Padding(5,6,0,15);
        page.Controls.Add(lbl,0,page.RowCount-1);page.Controls.Add(c,1,page.RowCount-1);
    }
    public RoomDto GetData()=>new()
    {
        Name=_fields["Name"].Text,Slug=_fields["Slug"].Text,
        Description=_fields["Description"].Text,ImagePath=_fields["ImagePath"].Text,
        Features=_fields["Features"].Text,AreaSqm=(int)_area.Value,Adults=(int)_adults.Value,
        Children=(int)_children.Value,PricePerNight=_price.Value,InitialUnits=(int)_units.Value,IsActive=_active.Checked
    };
}

public sealed class CountDialog:Form
{
    private readonly NumericUpDown _n=new(){Minimum=1,Maximum=20,Value=1,Dock=DockStyle.Top,Height=40};
    public int Count=>(int)_n.Value;
    public CountDialog(string room)
    {
        Text="Bổ sung phòng";StartPosition=FormStartPosition.CenterParent;Size=new Size(410,215);
        FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;BackColor=Ui.Page;
        var p=new FlowLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(20),FlowDirection=FlowDirection.TopDown};
        p.Controls.Add(Ui.Label($"Số phòng vật lý thêm cho {room}",11,true));p.Controls.Add(_n);
        var ok=Ui.Button("Xác nhận",true,130);ok.Click+=(_,_)=>{DialogResult=DialogResult.OK;Close();};p.Controls.Add(ok);Controls.Add(p);
    }
}
