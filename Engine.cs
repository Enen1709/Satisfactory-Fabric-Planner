using System.Text.Json;

namespace SatisfactoryPlanner;
public class Item {public double SinkPoints {get;set;} public string Id {get;set;}=""; public string Name {get;set;}=""; public bool Fluid {get;set;} public bool Resource {get;set;} }
public class Quantity {public string Item {get;set;}=""; public double Amount {get;set;} }
public class Building {public string Id {get;set;}="";public string Name {get;set;}="";public double Power {get;set;} public double Exponent {get;set;} public double Speed {get;set;} public List<Quantity> Cost {get;set;}=[];}
public class Recipe {public string Id {get;set;}="";public string Name {get;set;}="";public bool Alternate {get;set;}public string Machine {get;set;}="";public double Seconds {get;set;}public List<Quantity> Inputs {get;set;}=[];public List<Quantity> Outputs {get;set;}=[];public double PowerConstant {get;set;}public double PowerFactor {get;set;} }
public partial class GameData {
 public string Source {get;set;}="";public string Build {get;set;}=""; public Dictionary<string,Item> Items {get;set;}=[];public Dictionary<string,Building> Buildings {get;set;}=[];public List<Recipe> Recipes {get;set;}=[];
 public static GameData Load(){using var s=typeof(GameData).Assembly.GetManifestResourceStream("SatisfactoryPlanner.game.json")??typeof(GameData).Assembly.GetManifestResourceStream("SatisfactoryPlaner.game.json")!;var data=JsonSerializer.Deserialize<GameData>(s)!;data.LoadLanguages();return data;}
 public string Name(string id)=>Items.TryGetValue(id,out var i)?i.Name:id;
 public string Unit(string id)=>Items.TryGetValue(id,out var i)&&i.Fluid?"m³/min":Ui.T("Stk./min");
 public List<Recipe> Options(string id)=>Recipes.Where(r=>r.Outputs[0].Item==id).OrderBy(r=>r.Id.Contains("Unpackage")).ThenBy(r=>r.Alternate).ThenBy(r=>r.Id.Contains("Residual")).ThenBy(r=>OriginalName(r.Id),StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("de-DE"),false)).ToList();
}
public class Settings {public HashSet<string> OwnedMaterials {get;set;}=[];public Dictionary<string,MapPosition> MapPositions {get;set;}=[];public string Target {get;set;}="Desc_IronPlateReinforced_C";public double Rate {get;set;}=5;public bool Overclock {get;set;}public double Clock {get;set;}=250;public int Miner {get;set;}=1;public double Purity {get;set;}=1;public int WellNodes {get;set;}=6; public Dictionary<string,string> Choices {get;set;}=[];}
public record Stage(string Item,Recipe? Recipe,Building Building,double Rate,int Count,double Clock,double Power,double Peak,int Shards);
public class Plan {public List<Stage> Stages {get;}=[];public Dictionary<string,double> Raw {get;}=[];public Dictionary<string,double> External {get;}=[];public Dictionary<string,double> Surplus {get;}=[];public Dictionary<string,double> Costs {get;}=[];public List<string> Notes {get;}=[];public double Power=>Stages.Sum(s=>s.Power);public double Peak=>Stages.Sum(s=>s.Peak);public int Shards=>Stages.Sum(s=>s.Shards);}
public class Engine(GameData data) {
 static void Add(Dictionary<string,double> d,string k,double v){d[k]=d.GetValueOrDefault(k)+v;}
 public Plan Calculate(Settings s){
  if(!double.IsFinite(s.Rate)||s.Rate<=0||s.Rate>1e7)throw new InvalidOperationException("Bitte eine Menge zwischen 0 und 10.000.000 eingeben.");
  var plan=new Plan();var selected=new Dictionary<string,Recipe>();var visited=new HashSet<string>();
  void Expand(string id){if(!visited.Add(id)||data.Items[id].Resource)return;var opts=data.Options(id);if(opts.Count==0)return;var r=opts.FirstOrDefault(r=>r.Id==s.Choices.GetValueOrDefault(id))??opts[0];selected[id]=r;foreach(var q in r.Inputs)Expand(q.Item);}
  Expand(s.Target);
  var active=selected.Keys.ToList();int n=active.Count;var a=new double[n,n];var b=new double[n];var objective=new double[n];
  for(int i=0;i<n;i++){b[i]=active[i]==s.Target?-s.Rate:0;objective[i]=-selected[active[i]].Seconds/60;for(int j=0;j<n;j++){var r=selected[active[j]];a[i,j]=r.Inputs.Where(q=>q.Item==active[i]).Sum(q=>q.Amount)-r.Outputs.Where(q=>q.Item==active[i]).Sum(q=>q.Amount);}}
  var runs=n==0?Array.Empty<double>():new LinearProgram(a,b,objective).Solve();
  var balance=new Dictionary<string,double>();
  double cap=(s.Overclock?s.Clock:100)/100;
  void Stage(string item,Recipe? recipe,Building building,double rate,double equivalents,double basePower,double peakPower,bool fixedClock=false){
   if(equivalents<=1e-9)return;int count=(int)Math.Ceiling(equivalents/cap-1e-9);double clock=fixedClock?1:Math.Max(.01,equivalents/count);if(fixedClock)count=(int)Math.Ceiling(equivalents-1e-9);
   int shards=clock>1+1e-8?(int)Math.Ceiling((clock-1)/.5-1e-8)*count:0;
   plan.Stages.Add(new Stage(item,recipe,building,rate,count,clock*100,count*basePower*Math.Pow(clock,building.Exponent),count*peakPower*Math.Pow(clock,building.Exponent),shards));
   foreach(var q in building.Cost)Add(plan.Costs,q.Item,q.Amount*count);
  }
  for(int j=0;j<active.Count;j++){
   var r=selected[active[j]];var run=Math.Max(0,runs[j]);if(run<1e-9)continue;foreach(var q in r.Outputs)Add(balance,q.Item,q.Amount*run);foreach(var q in r.Inputs)Add(balance,q.Item,-q.Amount*run);
   var building=data.Buildings[r.Machine];double bp=building.Power,peak=bp;
   if(r.PowerConstant>0){bp=r.PowerConstant+r.PowerFactor*.5;peak=r.PowerConstant+r.PowerFactor;}
   Stage(active[j],r,building,r.Outputs[0].Amount*run,run*r.Seconds/60/building.Speed,bp,peak);
  }
  Add(balance,s.Target,-s.Rate);
  foreach(var (id,v) in balance){if(v>1e-6)plan.Surplus[id]=v;else if(v < -1e-6){if(selected.ContainsKey(id))throw new InvalidOperationException("Nebenprodukte reichen bei dieser Rezeptkombination nicht aus. Bitte andere Rezepte wählen.");if(data.Items[id].Resource)plan.Raw[id]=-v;else plan.External[id]=-v;}}
  // A raw target has no manufacturing recipe.
  foreach(var (id,rate) in plan.Raw){
   string machine;double throughput;
   if(id=="Desc_Water_C"){machine="Build_WaterPump_C";throughput=120;}
   else if(id=="Desc_LiquidOil_C"){machine="Build_OilPump_C";throughput=120*s.Purity;}
   else if(id=="Desc_NitrogenGas_C"){
    machine="Build_FrackingExtractor_C";throughput=60*s.Purity;
    double equivalentWells=rate/(throughput*s.WellNodes);int wells=(int)Math.Ceiling(equivalentWells/cap-1e-9);double wellClock=Math.Max(.01,equivalentWells/wells);
    int extractors=(int)Math.Ceiling(rate/(throughput*wellClock)-1e-9);
    Stage(id,null,data.Buildings[machine],rate,extractors,0,0,true);
    Stage(id,null,data.Buildings["Build_FrackingSmasher_C"],rate,equivalentWells,150,150);
    plan.Notes.Add($"Stickstoff: {s.WellNodes} gleich reine Unterknoten je Quelle. Quellenextraktoren übernehmen den Takt des Kompressors; ihre eigene Taktanzeige bleibt bei 100 %.");continue;
   }
   else {machine=$"Build_MinerMk{s.Miner}_C";throughput=60*Math.Pow(2,s.Miner-1)*s.Purity;}
   var building=data.Buildings[machine];Stage(id,null,building,rate,rate/throughput,building.Power,building.Power);
  }
  if(plan.Shards>0)Add(plan.Costs,"Desc_CrystalShard_C",plan.Shards);
  if(plan.External.Count>0)plan.Notes.Add("Externe Eingänge haben kein gewähltes Maschinenrezept (z. B. Biomasse, Behälter oder Abfall). Bereitstellung und deren Strom sind nicht enthalten.");
  if(plan.Stages.Any(x=>x.Clock<=1.00001))plan.Notes.Add("Mindesttakt ist 1 %. Bei sehr kleinen Mengen muss die Maschine zeitweise pausieren; der angezeigte Strom ist der Verbrauch im Betrieb.");
  plan.Notes.Add("Baukosten enthalten Produktions- und Fördermaschinen sowie Leistungssplitter. Förderbänder, Rohre, Pumpen, Fundamente, Stromnetz und Kraftwerke hängen vom Layout ab und sind nicht enthalten.");
  var alternatives=plan.Stages.Where(x=>x.Recipe?.Alternate==true).Select(x=>x.Recipe!.Name).Distinct().ToList();if(alternatives.Count>0)plan.Notes.Add("Verwendete Alternativrezepte (im Spiel freischalten): "+string.Join(", ",alternatives));
  plan.Notes.Add("Nebenprodukte werden innerhalb dieser Kette genutzt. Überschüsse müssen abgeführt werden. Keine automatische Abfallverwertung oder Somersloop-Verstärkung.");
  return plan;
 }
}





