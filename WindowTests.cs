namespace SatisfactoryPlanner;
public partial class MainForm {
 internal void TestWindowRestore(){
  Opacity=0;ShowInTaskbar=false;Show();Application.DoEvents();
  Control? Find(Control root){if(root.Name=="result-navigation")return root;foreach(Control c in root.Controls){var result=Find(c);if(result!=null)return result;}return null;}
  var navigation=Find(this)??throw new Exception("Navigation not found.");
  var buttons=navigation.Controls.OfType<ModernButton>().ToArray();var expected=buttons.Select(b=>b.Width).ToArray();
  for(int cycle=0;cycle<3;cycle++){
   WindowState=FormWindowState.Minimized;Application.DoEvents();WindowState=FormWindowState.Normal;Application.DoEvents();
   if(!buttons.Select(b=>b.Width).SequenceEqual(expected))throw new Exception("Button widths changed after minimizing and restoring.");
   Width=MinimumSize.Width;Application.DoEvents();Width=1440;Application.DoEvents();
   if(!buttons.Select(b=>b.Width).SequenceEqual(expected))throw new Exception("Button widths were not restored after resizing.");
  }
  SetDark(true,false);WindowState=FormWindowState.Minimized;Application.DoEvents();WindowState=FormWindowState.Normal;Application.DoEvents();
  if(!buttons.Select(b=>b.Width).SequenceEqual(expected)||buttons.Any(b=>b.Width<60))throw new Exception("Navigation failed in dark mode.");
  Close();
 }
}
