namespace SatisfactoryPlanner;
public partial class MainForm {
 internal void PreviewPage(string title){void Find(Control parent){foreach(Control c in parent.Controls){if(c is ModernButton b&&b.Text==Ui.T(title)){b.PerformClick();return;}Find(c);}}Find(this);}
 EventHandler? overviewResizeHandler;
 void UpdateOverview(){
  if(plan==null)return;
  machineValue.Text=plan.Stages.Sum(s=>s.Count).ToString();machineHint.Text=$"{plan.Stages.Select(s=>s.Building.Id).Distinct().Count()} verschiedene Maschinentypen";
  powerValue.Text=N(plan.Power)+" MW";powerHint.Text="Spitze: "+N(plan.Peak)+" MW";
  rawValue.Text=plan.Raw.Count.ToString();rawHint.Text="Rohstoffarten dauerhaft zuführen";
  surplusValue.Text=plan.Surplus.Count==0?"Keine":plan.Surplus.Count.ToString();surplusHint.Text=plan.Surplus.Count==0?"Alles wird in der Kette verwendet":"Materialarten abführen";
  overview.SuspendLayout();foreach(Control c in overview.Controls.Cast<Control>().ToArray())c.Dispose();overview.Controls.Clear();
  var flow=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=Padding.Empty};overview.Controls.Add(flow);
  var groups=plan.Stages.GroupBy(s=>s.Building.Id).OrderBy(g=>g.All(s=>s.Recipe!=null)).ThenBy(g=>g.First().Building.Name).ToList();
  var machineCard=new Card{Height=94+groups.Count*44,Margin=new Padding(0,0,0,14)};flow.Controls.Add(machineCard);
  var heading=Theme.Label("Das musst du bauen",17,true);heading.Location=new Point(20,15);heading.Size=new Size(550,30);machineCard.Controls.Add(heading);
  var help=Theme.Label("Baue zuerst die Förderanlagen und verbinde danach die Produktionsmaschinen.",10,false,Theme.Muted);help.Location=new Point(20,49);help.Size=new Size(780,27);machineCard.Controls.Add(help);
  int y=83;foreach(var group in groups){var items=group.ToList();int count=items.Sum(s=>s.Count);var number=Theme.Label(count+" ×",15,true,Theme.Orange);number.Location=new Point(23,y);number.Size=new Size(75,34);machineCard.Controls.Add(number);var name=Theme.Label(items[0].Building.Name,11,true);name.Location=new Point(100,y+4);name.Size=new Size(230,30);machineCard.Controls.Add(name);var desc=Theme.Label(string.Join(" · ",items.Select(s=>data.Name(s.Item)).Distinct()),10,false,Theme.Muted);desc.Location=new Point(335,y+5);desc.Size=new Size(450,28);desc.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;machineCard.Controls.Add(desc);y+=44;}
  var materials=plan.Raw.Concat(plan.External).OrderBy(x=>data.Name(x.Key)).ToList();
  var rawCard=new Card{Height=94+Math.Min(materials.Count,7)*35,Margin=new Padding(0,0,0,14)};flow.Controls.Add(rawCard);var rawTitle=Theme.Label("Das muss jede Minute ankommen",17,true);rawTitle.Location=new Point(20,15);rawTitle.Size=new Size(650,30);rawCard.Controls.Add(rawTitle);var rawHelp=Theme.Label("Diese Mengen sind der Gesamtbedarf deiner Fabrik – nicht je Maschine.",10,false,Theme.Muted);rawHelp.Location=new Point(20,49);rawHelp.Size=new Size(780,27);rawCard.Controls.Add(rawHelp);y=83;foreach(var (id,amount) in materials.Take(7)){var l=Theme.Label(data.Name(id),11,true);l.Location=new Point(23,y);l.Size=new Size(400,27);rawCard.Controls.Add(l);var v=Theme.Label(N(amount)+" "+data.Unit(id)+(plan.External.ContainsKey(id)?" · extern":""),11,false,Theme.Ink);v.Location=new Point(440,y);v.Size=new Size(330,27);v.TextAlign=ContentAlignment.TopRight;v.Anchor=AnchorStyles.Top|AnchorStyles.Left;v.Tag="right-value";rawCard.Controls.Add(v);y+=35;}if(materials.Count>7){rawCard.Height+=27;var more=Theme.Label($"Alle {materials.Count} Materialien findest du unter „Rohstoffe“.",9,false,Theme.Muted);more.Location=new Point(23,y);more.Size=new Size(650,24);rawCard.Controls.Add(more);}
  var next=new Card{Height=145,Surface=Theme.Warm,Border=Theme.WarmLine,Margin=new Padding(0,0,0,14)};flow.Controls.Add(next);var nextTitle=Theme.Label("So setzt du deinen Plan um",15,true);nextTitle.Location=new Point(20,14);nextTitle.Size=new Size(700,29);next.Controls.Add(nextTitle);var guide=Theme.Label("1   Sammle die Materialien unter „Baukosten“.\n2   Baue die Maschinen und stelle den Takt aus „Maschinen“ ein.\n3   Verbinde die Zutaten und versorge die Fabrik mit ausreichend Strom.",11);guide.Location=new Point(20,49);guide.Size=new Size(800,79);next.Controls.Add(guide);
  if(plan.Surplus.Count>0||plan.External.Count>0||plan.Stages.Any(s=>s.Recipe?.Alternate==true)){var message=Theme.Label("Achte auf deinen Plan: "+(plan.Surplus.Count>0?"Entsorgungshinweise findest du unter „Nebenprodukte“. ":"")+(plan.External.Count>0?"Externe Eingänge bereitstellen. ":"")+(plan.Stages.Any(s=>s.Recipe?.Alternate==true)?"Verwendete Alternativrezepte im Spiel freischalten.":""),10,false,Theme.Warning);message.Height=55;message.Margin=new Padding(4,0,0,10);flow.Controls.Add(message);}
  void ResizeCards(){int width=Math.Max(400,overview.ClientSize.Width-24);flow.Width=width;foreach(Control c in flow.Controls){c.Width=width;foreach(Control child in c.Controls)if(child.Tag is string tag&&tag=="right-value")child.Left=width-child.Width-23;}}
  if(overviewResizeHandler!=null)overview.Resize-=overviewResizeHandler;overviewResizeHandler=(_,_)=>ResizeCards();overview.Resize+=overviewResizeHandler;ResizeCards();overview.ResumeLayout();
 }
}





