namespace SatisfactoryPlanner;
internal static class SurplusGuidance {
 static string Number(double n)=>n.ToString("0.###",Ui.Culture);
 internal static bool CanSink(GameData data,string id)=>data.Items.TryGetValue(id,out var item)&&!item.Fluid&&item.SinkPoints>0;
 internal static string Advice(GameData data,string id,double amount){
  if(!data.Items.TryGetValue(id,out var item))return Ui.T("Keine passende Entsorgung bekannt. Für andere Produktionen verwenden oder vorübergehend lagern.");
  var consumers=data.Recipes.Where(r=>r.Inputs.Any(q=>q.Item==id)&&!r.Id.Contains("Unpackage")).OrderBy(r=>r.Alternate).ThenBy(r=>r.Inputs.Count).ThenBy(r=>r.Id,StringComparer.Ordinal).ToList();
  Recipe? Recipe(string recipeId)=>data.Recipes.FirstOrDefault(r=>r.Id==recipeId);
  var packaged=consumers.FirstOrDefault(r=>r.Machine=="Build_Packager_C"&&r.Outputs.Any(q=>CanSink(data,q.Item)));
  string Conversion(Recipe recipe){
   var input=recipe.Inputs.First(q=>q.Item==id);double runs=amount/input.Amount;var building=data.Buildings[recipe.Machine];double needed=runs*recipe.Seconds/60/building.Speed;double count=Math.Max(1,Math.Ceiling(needed-1e-9));double clock=Math.Max(1,needed/count*100);
   var lines=new List<string>{Ui.F("Rezept: {0} ({1}).",recipe.Name,building.Name)};
   if(recipe.Alternate)lines.Add(Ui.T("Dieses Alternativrezept zuerst im Spiel freischalten."));
   lines.Add(Number(amount)+" "+data.Unit(id)+" → "+string.Join(" + ",recipe.Outputs.Select(q=>Number(q.Amount*runs)+" "+data.Unit(q.Item)+" "+data.Name(q.Item)))+".");
   lines.Add(Ui.F("Zusätzlich: {0} × {1} bei ca. {2} % Takt.",Number(count),building.Name,Number(clock)));
   var extra=recipe.Inputs.Where(q=>q.Item!=id).ToList();if(extra.Count>0)lines.Add(Ui.T("Weitere Zutaten pro Minute:")+" "+string.Join(" + ",extra.Select(q=>Number(q.Amount*runs)+" "+data.Unit(q.Item)+" "+data.Name(q.Item)))+".");
   if(needed/count<.01)lines.Add(Ui.T("Bei weniger als 1 % Bedarf zeitweise pausieren."));return string.Join("\n",lines);
  }
  if(id=="Desc_HeavyOilResidue_C"&&Recipe("Recipe_PetroleumCoke_C") is Recipe coke&&CanSink(data,coke.Outputs[0].Item)){
   string text=Ui.T("Nicht direkt schredderbar. Zu Petrolkoks verarbeiten und diesen per Förderband zum AWESOME-Schredder schicken.")+"\n"+Conversion(coke);
   if(Recipe("Recipe_ResidualFuel_C") is Recipe fuel)text+="\n"+Ui.F("Alternative: {0} herstellen und zur Stromerzeugung nutzen.",fuel.Name);return text;
  }
  if(id=="Desc_Water_C"){
   var wet=consumers.FirstOrDefault(r=>r.Outputs[0].Item=="Desc_Cement_C");
   var lines=new List<string>{Ui.T("In eine andere Produktionslinie zurückführen und dort den frischen Wasserzulauf reduzieren. Nicht direkt schredderbar.")};
   if(wet!=null&&CanSink(data,wet.Outputs[0].Item))lines.Add(Ui.T("Dauerhafte Entsorgung: zu Beton verarbeiten und diesen schreddern.")+"\n"+Conversion(wet));
   else if(packaged!=null)lines.Add(Ui.T("Abfüllen und das abgefüllte Produkt schreddern.")+"\n"+Conversion(packaged));
   return string.Join("\n",lines);
  }
  if(id=="Desc_NuclearWaste_C")return Ui.T("Nicht schredderbar. Über die Plutonium-Produktionskette zu Plutonium-Brennstäben verarbeiten; diese können in den AWESOME-Schredder. Bis dahin radioaktiven Abfall lagern.");
  if(id=="Desc_PlutoniumWaste_C"&&Recipe("Recipe_Ficsonium_C") is Recipe ficsonium)return Ui.F("Nicht schredderbar. Mit dem Rezept {0} zu Ficsonium weiterverarbeiten, anschließend Ficsonium-Brennstäbe herstellen und im Atomkraftwerk verwenden. Diese Brennstäbe erzeugen keinen weiteren Abfall. Zusätzliche Zutaten und spätere Forschung nötig; bis dahin lagern.",ficsonium.Name);
  if(CanSink(data,id)){
   string text=Ui.T("Per Förderband zum AWESOME-Schredder schicken. Mit einem Smart-Splitter auf „Überlauf“ nur den Überschuss abzweigen.");
   var usable=consumers.Where(r=>!r.Id.Contains("Package")).Take(2).ToList();if(usable.Count>0)text+="\n"+Ui.T("Alternativ weiterverarbeiten:")+" "+string.Join("; ",usable.Select(r=>r.Name+" ("+data.Buildings[r.Machine].Name+")"))+".";return text;
  }
  if(item.Fluid&&packaged!=null)return Ui.T("Nicht direkt schredderbar. Abfüllen und das abgefüllte Produkt schreddern.")+"\n"+Conversion(packaged)+"\n"+Ui.T("Behälter werden dabei mitgeschreddert: dauerhaft neue Behälter zuführen.");
  var processing=consumers.FirstOrDefault(r=>r.Outputs.Count==1&&CanSink(data,r.Outputs[0].Item))??consumers.FirstOrDefault();
  if(processing!=null){var text=Ui.T("Nicht direkt schredderbar. Für eine weitere Produktion verwenden:")+"\n"+Conversion(processing);if(processing.Outputs.Count==1&&CanSink(data,processing.Outputs[0].Item))text+="\n"+Ui.T("Das entstandene Produkt kann in den AWESOME-Schredder.");else text+="\n"+Ui.T("Die Ausgänge dieser zusätzlichen Produktion ebenfalls verwenden oder abführen.");return text;}
  return Ui.T("Nicht schredderbar. Für andere Produktionen verwenden oder vorübergehend lagern. Lager werden bei dauerhaftem Überschuss irgendwann voll.");
 }
 internal static void Test(GameData data){
  string previous=Ui.Language;Ui.Language="de";data.SetLanguage("de");
  if(CanSink(data,"Desc_Water_C")||CanSink(data,"Desc_HeavyOilResidue_C")||CanSink(data,"Desc_NuclearWaste_C")||!CanSink(data,"Desc_Plastic_C"))throw new Exception("Sink eligibility failed.");
  var heavy=Advice(data,"Desc_HeavyOilResidue_C",10);if(!heavy.Contains("30 Stk./min Petrolkoks")||!heavy.Contains("25 %"))throw new Exception("Heavy oil disposal rate is incorrect.");
  var water=Advice(data,"Desc_Water_C",20);if(!water.Contains("Weitere Zutaten")||!water.Contains("Alternativrezept"))throw new Exception("Water conversion lacks required ingredients or alternate-recipe notice.");
  var acid=Advice(data,"Desc_SulfuricAcid_C",10);if(!acid.Contains("Nicht direkt schredderbar"))throw new Exception("Fluid was recommended for direct sinking.");
  var plutonium=Advice(data,"Desc_PlutoniumWaste_C",5);if(!plutonium.Contains("Ficsonium"))throw new Exception("Plutonium waste guidance is missing.");
  Ui.Language="en";data.SetLanguage("en");if(!Advice(data,"Desc_HeavyOilResidue_C",10).Contains("Petroleum Coke")||!Advice(data,"Desc_Plastic_C",20).Contains("AWESOME Sink"))throw new Exception("Disposal advice is not localized.");
  Ui.Language=previous;data.SetLanguage(previous);
 }
}
