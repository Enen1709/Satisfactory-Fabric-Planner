using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace SatisfactoryPlanner;
internal static class Ui {
 public static string Language {get;set;}="de";
 public static CultureInfo Culture=>CultureInfo.GetCultureInfo(Language=="en"?"en-US":"de-DE");
 static readonly Dictionary<string,string> English=Load();
 static Dictionary<string,string> Load(){using var s=typeof(Ui).Assembly.GetManifestResourceStream("SatisfactoryPlanner.UiStrings.json")!;return JsonSerializer.Deserialize<Dictionary<string,string>>(s)!;}
 static readonly (Regex Pattern,string Value)[] ToEnglish=Compile(English.Select(k=>new KeyValuePair<string,string>(k.Key,k.Value)));
 static readonly (Regex Pattern,string Value)[] ToGerman=Compile(English.Select(k=>new KeyValuePair<string,string>(k.Value,k.Key)).GroupBy(k=>k.Key).Select(g=>g.First()));
 static (Regex,string)[] Compile(IEnumerable<KeyValuePair<string,string>> pairs)=>pairs.Where(k=>k.Key!=k.Value).OrderByDescending(k=>k.Key.Length).Select(k=>(new Regex(@"(?<![\p{L}])"+Regex.Escape(k.Key)+@"(?![\p{L}])",RegexOptions.CultureInvariant),k.Value)).ToArray();
 public static string T(string text)=>Translate(text,"de",Language);
 public static string F(string text,params object[] values)=>string.Format(Culture,T(text),values);
 public static string Translate(string text,string from,string to){if(from==to||string.IsNullOrEmpty(text))return text;if(to=="en"&&English.TryGetValue(text,out var exact))return exact;var reverse=to=="de"?English.FirstOrDefault(p=>p.Value==text):default;if(reverse.Key!=null)return reverse.Key;var rules=to=="en"?ToEnglish:ToGerman;foreach(var (pattern,value) in rules)text=pattern.Replace(text,_=>value);return text;}
 public static void Apply(Control root,string from="de",string? to=null){to??=Language;
  if(root is not ComboBox && root is not NumericUpDown && (root is not TextBox box||box.ReadOnly))root.Text=Translate(root.Text,from,to);
  if(root is TextBox text)text.PlaceholderText=Translate(text.PlaceholderText,from,to);
  if(root is DataGridView grid){foreach(DataGridViewColumn c in grid.Columns)c.HeaderText=Translate(c.HeaderText,from,to);foreach(DataGridViewRow row in grid.Rows)foreach(DataGridViewCell c in row.Cells)if(c is not DataGridViewComboBoxCell && c.Value is string v){string translated=Translate(v,from,to);if(v!=translated)c.Value=translated;}}
  if(root is TreeView tree){void Node(TreeNode n){n.Text=Translate(n.Text,from,to);foreach(TreeNode child in n.Nodes)Node(child);}foreach(TreeNode n in tree.Nodes)Node(n);}
  foreach(Control child in root.Controls)Apply(child,from,to);
 }
}
public partial class GameData {
 Dictionary<string,Dictionary<string,string>> localizedNames=[];
 void LoadLanguages(){using var stream=typeof(GameData).Assembly.GetManifestResourceStream("SatisfactoryPlanner.languages.json")!;localizedNames=JsonSerializer.Deserialize<Dictionary<string,Dictionary<string,string>>>(stream)!;}
 public void SetLanguage(string language){var names=localizedNames.GetValueOrDefault(language)??localizedNames["de"];foreach(var item in Items.Values)item.Name=names.GetValueOrDefault(item.Id,item.Name);foreach(var building in Buildings.Values)building.Name=names.GetValueOrDefault(building.Id,building.Name);foreach(var recipe in Recipes)recipe.Name=names.GetValueOrDefault(recipe.Id,recipe.Name);}
 string OriginalName(string id)=>localizedNames.TryGetValue("de",out var names)?names.GetValueOrDefault(id,id):id;
}
