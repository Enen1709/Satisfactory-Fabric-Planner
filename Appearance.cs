using System.Text.Json;
namespace SatisfactoryPlanner;
public partial class MainForm {
 readonly CheckBox darkMode=new(){Text="Dunkelmodus",AutoSize=true,ForeColor=Color.White,Margin=new Padding(0,8,0,9),Cursor=Cursors.Hand};
 bool updatingAppearance;
 internal static string? AppearanceFileOverride;
 static string AppearanceFile=>AppearanceFileOverride??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SatisfactoryPlaner","Einstellungen.json");
 internal record Appearance(bool DarkMode,string Language="en");
 static Appearance ReadPreferences(){try{var file=File.Exists(AppearanceFile)?AppearanceFile:Path.Combine(AppContext.BaseDirectory,"Darstellung.json");return JsonSerializer.Deserialize<Appearance>(File.ReadAllText(file))??new(false);}catch(IOException){return new(false);}catch(UnauthorizedAccessException){return new(false);}catch(JsonException){return new(false);}}
 static bool ReadDarkMode()=>ReadPreferences().DarkMode;
 void InitializeAppearance(){foreach(var combo in new[]{target,miner,purity}){combo.DrawMode=DrawMode.OwnerDrawFixed;combo.DrawItem+=(_,e)=>{using var background=new SolidBrush((e.State&DrawItemState.Selected)!=0?Theme.Selected:Theme.Surface);e.Graphics.FillRectangle(background,e.Bounds);if(e.Index>=0){string text=combo==target&&combo.Items[e.Index] is Item item?item.Name:combo.Items[e.Index]?.ToString()??"";TextRenderer.DrawText(e.Graphics,text,combo.Font,new Rectangle(e.Bounds.X+5,e.Bounds.Y,e.Bounds.Width-8,e.Bounds.Height),Theme.Ink,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}e.DrawFocusRectangle();};}var preferences=ReadPreferences();SetLanguage(preferences.Language);SetDark(preferences.DarkMode,false);darkMode.CheckedChanged+=(_,_)=>{if(!updatingAppearance)SetDark(darkMode.Checked,true);};}
 internal void TestAppearance(){darkMode.Checked=true;if(!Theme.Dark||!ReadDarkMode()||stages.BackgroundColor!=Theme.Surface)throw new Exception("Dark-mode activation or persistence failed.");rate.Value=10;if(overview.Controls[0].Controls.OfType<Card>().Any(c=>c.Surface!=Theme.Surface&&c.Surface!=Theme.Warm))throw new Exception("Recalculated cards have incorrect appearance.");darkMode.Checked=false;if(Theme.Dark||ReadDarkMode()||stages.BackgroundColor!=Color.White)throw new Exception("Light-mode activation or persistence failed.");}
 internal void SetDark(bool dark,bool save){
  updatingAppearance=true;darkMode.Checked=dark;updatingAppearance=false;
  bool previous=Theme.Dark;Theme.Dark=dark;
  var light=new[]{Color.FromArgb(26,38,57),Color.FromArgb(108,122,143),Color.FromArgb(244,247,251),Color.White,Color.FromArgb(227,233,241),Color.FromArgb(255,246,235),Color.FromArgb(246,222,193),Color.FromArgb(150,87,29)};
  var night=new[]{Color.FromArgb(232,239,248),Color.FromArgb(158,176,199),Color.FromArgb(15,23,35),Color.FromArgb(25,36,53),Color.FromArgb(47,62,82),Color.FromArgb(48,36,29),Color.FromArgb(100,69,42),Color.FromArgb(255,188,114)};
  Color ConvertColor(Color color){var from=previous?night:light;var to=dark?night:light;for(int i=0;i<from.Length;i++)if(color.ToArgb()==from[i].ToArgb())return to[i];return color;}
  void Apply(Control control,bool sidebar=false){
   sidebar|=control.Tag is string tag&&tag=="sidebar";
   if(!sidebar){if(control.BackColor!=Color.Transparent)control.BackColor=ConvertColor(control.BackColor);control.ForeColor=ConvertColor(control.ForeColor);}
   if(control is Card card){card.Surface=ConvertColor(card.Surface);card.Border=ConvertColor(card.Border);}
   if(control is TextBox or ComboBox or NumericUpDown){control.BackColor=Theme.Surface;control.ForeColor=Theme.Ink;}
   if(control is TreeView tree){tree.BackColor=Theme.Surface;tree.ForeColor=Theme.Ink;tree.LineColor=Theme.Line;}
   foreach(Control child in control.Controls)Apply(child,sidebar);
   if(control is DataGridView grid){grid.ForeColor=Theme.Ink;grid.BackgroundColor=Theme.Surface;grid.RowsDefaultCellStyle.BackColor=Theme.Surface;grid.RowsDefaultCellStyle.ForeColor=Theme.Ink;grid.RowsDefaultCellStyle.SelectionBackColor=Theme.Selected;grid.RowsDefaultCellStyle.SelectionForeColor=Theme.Ink;grid.AlternatingRowsDefaultCellStyle.ForeColor=Theme.Ink;grid.ColumnHeadersDefaultCellStyle.SelectionBackColor=Theme.Selected;grid.ColumnHeadersDefaultCellStyle.SelectionForeColor=Theme.Ink;foreach(DataGridViewColumn column in grid.Columns){column.DefaultCellStyle.ForeColor=Theme.Ink;column.DefaultCellStyle.BackColor=Color.Empty;column.DefaultCellStyle.SelectionBackColor=Theme.Selected;column.DefaultCellStyle.SelectionForeColor=Theme.Ink;}grid.GridColor=Theme.Line;grid.DefaultCellStyle.BackColor=Theme.Surface;grid.DefaultCellStyle.ForeColor=Theme.Ink;grid.DefaultCellStyle.SelectionBackColor=Theme.Selected;grid.DefaultCellStyle.SelectionForeColor=Theme.Ink;grid.AlternatingRowsDefaultCellStyle.BackColor=dark?Color.FromArgb(29,41,59):Color.FromArgb(250,252,255);grid.ColumnHeadersDefaultCellStyle.BackColor=dark?Color.FromArgb(35,49,69):Color.FromArgb(236,241,248);grid.ColumnHeadersDefaultCellStyle.ForeColor=Theme.Muted;}
   control.Invalidate();
  }
  SuspendLayout();Apply(this);ResumeLayout();Invalidate(true);
  if(save)SavePreferences();
 }
}



