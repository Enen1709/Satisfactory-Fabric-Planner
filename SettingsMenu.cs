using System.Text.Json;
namespace SatisfactoryPlanner;
public partial class MainForm {
 readonly ToolTip tips=new(){AutoPopDelay=12000,InitialDelay=300};
 internal void SetLanguage(string language){
  language=language=="en"?"en":"de";string previous=Ui.Language;Ui.Language=language;System.Threading.Thread.CurrentThread.CurrentCulture=Ui.Culture;System.Threading.Thread.CurrentThread.CurrentUICulture=Ui.Culture;
  bool old=initializing;initializing=true;data.SetLanguage(language);Ui.Apply(this,previous,language);
  int index=purity.SelectedIndex;var purityTexts=new[]{"Unrein · halber Ertrag","Normal · einfacher Ertrag","Rein · doppelter Ertrag"};purity.Items.Clear();purity.Items.AddRange(purityTexts.Select(Ui.T).Cast<object>().ToArray());purity.SelectedIndex=Math.Max(0,index);
  search.Text="";Filter();target.SelectedValue=settings.Target;rate.Text=rate.Value.ToString("N3",Ui.Culture);
  void TranslateTips(Control parent){foreach(Control control in parent.Controls){var text=tips.GetToolTip(control)??"";if(text.Length>0)tips.SetToolTip(control,Ui.Translate(text,previous,language));TranslateTips(control);}}TranslateTips(this);
  initializing=old;if(!old)Calculate();Invalidate(true);
 }
 void SavePreferences(){try{Directory.CreateDirectory(Path.GetDirectoryName(AppearanceFile)!);File.WriteAllText(AppearanceFile,JsonSerializer.Serialize(new Appearance(Theme.Dark,Ui.Language),new JsonSerializerOptions{WriteIndented=true}));}catch(Exception ex)when(ex is IOException or UnauthorizedAccessException){MessageBox.Show(Ui.T("Einstellungen konnten nicht gespeichert werden:")+"\n"+ex.Message,Ui.T("Einstellungen speichern"),MessageBoxButtons.OK,MessageBoxIcon.Information);}}
 Form CreateSettingsMenu(){
  var dialog=new Form{Text=Ui.T("Einstellungen"),ClientSize=new Size(540,420),FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,StartPosition=FormStartPosition.CenterParent,BackColor=Theme.Canvas,ForeColor=Theme.Ink,Font=new Font("Segoe UI",10),ShowInTaskbar=false};
  void Label(string text,int x,int y,int width,int height,float size=10,bool bold=false,Color? color=null){var l=Theme.Label(Ui.T(text),size,bold,color);l.Location=new Point(x,y);l.Size=new Size(width,height);dialog.Controls.Add(l);}
  Label("Einstellungen",26,19,475,36,22,true);Label("Passe die App an dich an.",27,62,475,25,10,false,Theme.Muted);
  Label("Sprache",27,105,465,25,11,true);
  var languages=new ComboBox{Name="language",DropDownStyle=ComboBoxStyle.DropDownList,Location=new Point(27,137),Size=new Size(482,32),Font=new Font("Segoe UI",11),BackColor=Theme.Surface,ForeColor=Theme.Ink};languages.DrawMode=DrawMode.OwnerDrawFixed;languages.DrawItem+=(_,e)=>{using var background=new SolidBrush((e.State&DrawItemState.Selected)!=0?Theme.Selected:Theme.Surface);e.Graphics.FillRectangle(background,e.Bounds);if(e.Index>=0)TextRenderer.DrawText(e.Graphics,languages.Items[e.Index]?.ToString()??"",languages.Font,new Rectangle(e.Bounds.X+5,e.Bounds.Y,e.Bounds.Width-8,e.Bounds.Height),Theme.Ink,TextFormatFlags.VerticalCenter);};languages.Items.AddRange(["Deutsch","English"]);languages.SelectedIndex=Ui.Language=="en"?1:0;dialog.Controls.Add(languages);
  Label("Sprache für Oberfläche, Produkte und Rezepte.",27,176,475,30,10,false,Theme.Muted);
  Label("Darstellung",27,224,465,25,11,true);
  var dark=new CheckBox{Name="darkMode",Text=Ui.T("Dunkelmodus"),Checked=Theme.Dark,Location=new Point(27,258),AutoSize=true,ForeColor=Theme.Ink};dialog.Controls.Add(dark);
  Label("Dunkle Farben für Karten, Tabellen und Eingabefelder.",27,290,475,25,10,false,Theme.Muted);
  Label("Deine Auswahl wird gespeichert und beim nächsten Start geladen.",27,328,482,28,9,false,Theme.Muted);
  var cancel=new ModernButton{Text=Ui.T("Abbrechen"),Location=new Point(279,370),Size=new Size(109,35),DialogResult=DialogResult.Cancel};dialog.Controls.Add(cancel);
  var save=new ModernButton{Text=Ui.T("Speichern"),Primary=true,Location=new Point(400,370),Size=new Size(109,35)};dialog.Controls.Add(save);dialog.AcceptButton=save;dialog.CancelButton=cancel;
  save.Click+=(_,_)=>{SetLanguage(languages.SelectedIndex==1?"en":"de");SetDark(dark.Checked,false);SavePreferences();dialog.DialogResult=DialogResult.OK;dialog.Close();};return dialog;
 }
 void OpenSettings(){using var dialog=CreateSettingsMenu();dialog.ShowDialog(this);}
 internal void PreviewSettings(){using var dialog=CreateSettingsMenu();dialog.Show(this);Application.DoEvents();using var bitmap=new Bitmap(dialog.Width,dialog.Height);dialog.DrawToBitmap(bitmap,new Rectangle(0,0,dialog.Width,dialog.Height));bitmap.Save(Path.Combine(AppContext.BaseDirectory,"Einstellungen.png"));dialog.Close();}
}

