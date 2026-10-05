using System.Drawing.Drawing2D;
namespace SatisfactoryPlanner;
internal static class Theme {
 public static bool Dark {get;set;}
 public static Color Ink=>Dark?Color.FromArgb(232,239,248):Color.FromArgb(26,38,57);
 public static Color Muted=>Dark?Color.FromArgb(158,176,199):Color.FromArgb(108,122,143);
 public static readonly Color Orange=Color.FromArgb(239,126,46);
 public static Color Canvas=>Dark?Color.FromArgb(15,23,35):Color.FromArgb(244,247,251);
 public static Color Surface=>Dark?Color.FromArgb(25,36,53):Color.White;
 public static Color Line=>Dark?Color.FromArgb(47,62,82):Color.FromArgb(227,233,241);
 public static Color Selected=>Dark?Color.FromArgb(41,62,91):Color.FromArgb(230,238,250);
 public static Color SelectedInk=>Dark?Color.FromArgb(157,198,255):Color.FromArgb(47,91,163);
 public static Color Warm=>Dark?Color.FromArgb(48,36,29):Color.FromArgb(255,246,235);
 public static Color WarmLine=>Dark?Color.FromArgb(100,69,42):Color.FromArgb(246,222,193);
 public static Color Warning=>Dark?Color.FromArgb(255,188,114):Color.FromArgb(150,87,29);
 public static GraphicsPath Round(Rectangle r,int radius){int d=radius*2;var p=new GraphicsPath();p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
 public static Label Label(string text,float size=10, bool bold=false,Color? color=null)=>new(){Text=text,AutoSize=false,Font=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular),ForeColor=color??Ink,BackColor=Color.Transparent,AutoEllipsis=true};
}
internal class Card:Panel {
 [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)] public Color Surface {get;set;}=Theme.Surface; [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)] public Color Border {get;set;}=Theme.Line;
 public Card(){DoubleBuffered=true;BackColor=Color.Transparent;Padding=new Padding(18);}
 protected override void OnPaintBackground(PaintEventArgs e){e.Graphics.Clear(Parent?.BackColor??Theme.Canvas);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using var p=Theme.Round(new Rectangle(0,0,Width-1,Height-1),14);using var b=new SolidBrush(Surface);e.Graphics.FillPath(b,p);using var pen=new Pen(Border);e.Graphics.DrawPath(pen,p);}
}
internal class ModernButton:Button {
 [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)] public bool Primary {get;set;}[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)] public bool Selected {get;set;}[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)] public bool Sidebar {get;set;}
 public ModernButton(){FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;Height=38;Font=new Font("Segoe UI",10,FontStyle.Bold);SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(Parent?.BackColor??Theme.Canvas);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;var fill=Primary?Theme.Orange:Selected?Theme.Selected:Sidebar?Color.FromArgb(42,55,74):Theme.Surface;var fg=Primary||Sidebar?Color.White:Selected?Theme.SelectedInk:Theme.Ink;using var path=Theme.Round(new Rectangle(0,0,Width-1,Height-1),9);using var brush=new SolidBrush(fill);e.Graphics.FillPath(brush,path);using var pen=new Pen(Primary||Sidebar?fill:Theme.Line);e.Graphics.DrawPath(pen,path);TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,fg,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);if(Focused){using var focus=new Pen(fg){DashStyle=DashStyle.Dot};e.Graphics.DrawRectangle(focus,5,5,Width-11,Height-11);}}
}

