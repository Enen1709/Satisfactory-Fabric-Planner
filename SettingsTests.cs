namespace SatisfactoryPlanner;
public partial class MainForm {
 internal void TestSettingsMenu(){
  Opacity=0;ShowInTaskbar=false;Show();Application.DoEvents();SetLanguage("de");
  var expected=data.Recipes.Select(r=>r.Outputs[0].Item).Distinct().ToDictionary(id=>id,id=>data.Options(id)[0].Id);
  double power=plan!.Power;int count=plan.Stages.Sum(s=>s.Count);string material=plan.Costs.Keys.First();settings.OwnedMaterials.Add(material);string position=settings.MapPositions.Keys.First();float mapX=settings.MapPositions[position].X;
  using(var dialog=CreateSettingsMenu()){dialog.Opacity=0;dialog.Show(this);Application.DoEvents();var language=dialog.Controls.OfType<ComboBox>().Single();var dark=dialog.Controls.OfType<CheckBox>().Single();language.SelectedIndex=1;dark.Checked=true;dialog.Controls.OfType<ModernButton>().Single(b=>b.Primary).PerformClick();Application.DoEvents();}
  if(Ui.Language!="en"||!Theme.Dark||ReadPreferences().Language!="en"||!ReadPreferences().DarkMode)throw new Exception("Settings menu did not apply or save language and dark mode.");
  if(data.Name("Desc_IronPlate_C")!="Iron Plate"||stages.Columns[0].HeaderText!="Product"||!notes.Text.Contains("DATA SOURCE")||notes.Text.Contains("Enrico")||notes.Text.Contains("BERECHNUNG"))throw new Exception("English translation is incomplete.");
  if(plan!.Power!=power||plan.Stages.Sum(s=>s.Count)!=count||!settings.OwnedMaterials.Contains(material)||settings.MapPositions[position].X!=mapX)throw new Exception("Language change affected the plan.");
  foreach(var (id,recipe) in expected)if(data.Options(id)[0].Id!=recipe)throw new Exception("Language changed a default recipe.");
  SetLanguage("de");SetDark(false,false);SavePreferences();
  if(data.Name("Desc_IronPlate_C")!="Eisenplatte"||stages.Columns[0].HeaderText!="Produkt"||ReadPreferences().Language!="de"||ReadPreferences().DarkMode)throw new Exception("Switching back to German/light mode failed.");
  using(var dialog=CreateSettingsMenu()){dialog.Opacity=0;dialog.Show(this);dialog.Controls.OfType<ComboBox>().Single().SelectedIndex=1;dialog.Controls.OfType<ModernButton>().Single(b=>!b.Primary).PerformClick();}if(Ui.Language!="de")throw new Exception("Cancel changed the language.");
  foreach(var language in new[]{"en","de","en","de"}){SetLanguage(language);if(plan!.Power!=power)throw new Exception("Repeated language switching changed power.");}
  Close();
 }
}
