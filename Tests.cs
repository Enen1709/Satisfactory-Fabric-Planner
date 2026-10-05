namespace SatisfactoryPlanner;
static class Tests {
 public static void Run(GameData data){
  var engine=new Engine(data);void Assert(bool ok,string message){if(!ok)throw new Exception(message);}
  var iron=engine.Calculate(new Settings{Target="Desc_IronPlate_C",Rate=5});Assert(Math.Abs(iron.Raw["Desc_OreIron_C"]-7.5)<1e-6,"Iron ore rate");Assert(iron.Stages.Sum(s=>s.Count)==3,"Iron plate machines");Assert(iron.Costs.GetValueOrDefault("Desc_Cable_C")>=8,"Building cost import");
  var reinforced=engine.Calculate(new Settings());Assert(Math.Abs(reinforced.Raw["Desc_OreIron_C"]-60)<1e-6,"Reinforced plate ore");
  var basePlan=engine.Calculate(new Settings{Target="Desc_IronPlate_C",Rate=100});var oc=engine.Calculate(new Settings{Target="Desc_IronPlate_C",Rate=100,Overclock=true});Assert(oc.Stages.Sum(s=>s.Count)<basePlan.Stages.Sum(s=>s.Count),"Overclock counts");Assert(oc.Power>basePlan.Power,"Overclock power");Assert(oc.Raw["Desc_OreIron_C"]==basePlan.Raw["Desc_OreIron_C"],"Overclock mass balance");Assert(oc.Shards>0,"Power shards");
  var plastic=engine.Calculate(new Settings{Target="Desc_Plastic_C",Rate=20});Assert(Math.Abs(plastic.Surplus["Desc_HeavyOilResidue_C"]-10)<1e-6,"Plastic byproduct");
  var aluminum=engine.Calculate(new Settings{Target="Desc_AluminumIngot_C",Rate=60});Assert(aluminum.Raw["Desc_Water_C"]>0,"Aluminum water");Assert(!aluminum.Surplus.ContainsKey("Desc_Water_C"),"Water reuse");
  var nitrogen=engine.Calculate(new Settings{Target="Desc_NitrogenGas_C",Rate=900,Overclock=true});Assert(nitrogen.Stages.Any(x=>x.Building.Id=="Build_FrackingSmasher_C"&&Math.Abs(x.Clock-250)<1e-6),"Nitrogen compressor overclock");
  var saved=System.Text.Json.JsonSerializer.Deserialize<Settings>(System.Text.Json.JsonSerializer.Serialize(new Settings{Overclock=true,Rate=42,Choices=new(){["Desc_IronPlate_C"]="Recipe_IronPlate_C"}}))!;Assert(saved.Rate==42&&saved.Overclock&&saved.Choices.Count==1,"Plan serialization");
  int ok=0;var unsupported=new List<string>();foreach(var item in data.Recipes.Select(r=>r.Outputs[0].Item).Distinct()){try{var p=engine.Calculate(new Settings{Target=item,Rate=5});Assert(double.IsFinite(p.Power)&&p.Stages.All(s=>s.Count>0&&s.Clock<=100.00001),"Finite plan "+item);
   var balance=new Dictionary<string,double>();void Add(string id,double value)=>balance[id]=balance.GetValueOrDefault(id)+value;
   foreach(var stage in p.Stages.Where(x=>x.Recipe!=null)){var r=stage.Recipe!;double runs=stage.Rate/r.Outputs[0].Amount;foreach(var q in r.Outputs)Add(q.Item,q.Amount*runs);foreach(var q in r.Inputs)Add(q.Item,-q.Amount*runs);}
   foreach(var q in p.Raw)Add(q.Key,q.Value);foreach(var q in p.External)Add(q.Key,q.Value);foreach(var q in p.Surplus)Add(q.Key,-q.Value);Add(item,-5);Assert(balance.Values.All(x=>Math.Abs(x)<1e-5),"Mass balance "+item);ok++;}catch(Exception ex){unsupported.Add(data.Name(item)+": "+ex.Message);}}
  File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"Tests.txt"),$"Core tests passed. {ok} default products calculated.\r\n"+string.Join("\r\n",unsupported));Assert(unsupported.Count==0,"Unsupported defaults: "+string.Join(", ",unsupported));
 }
}
