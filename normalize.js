const fs = require('fs');
const source = process.argv[2] || 'C:/Program Files (x86)/Steam/steamapps/common/Satisfactory/CommunityResources/Docs/de.json';
const groups = JSON.parse(fs.readFileSync(source,'utf16le').replace(/^\uFEFF/,''));
const all = groups.flatMap(g => g.Classes.map(c => ({...c,native:g.NativeClass})));
const classes = Object.fromEntries(all.map(c=>[c.ClassName,c]));
const refs = s => [...(s||'').matchAll(/\.([A-Za-z0-9_]+_C)/g)].map(m=>m[1]);
const items = {};
for(const c of all) if(c.mForm) items[c.ClassName]={Id:c.ClassName,Name:c.mDisplayName||c.ClassName,SinkPoints:Number(c.mResourceSinkPoints)||0,Fluid:c.mForm!=='RF_SOLID',Resource:c.native.includes('FGResourceDescriptor')};
const amounts = s => [...(s||'').matchAll(/ItemClass=.*?\.([A-Za-z0-9_]+_C).*?,Amount=([\d.]+)/g)].map(m=>({Item:m[1],Amount:Number(m[2])/(items[m[1]]?.Fluid?1000:1)}));
const buildings={};
for(const c of all) if(c.ClassName.startsWith('Build_')&&c.mPowerConsumption!==undefined) buildings[c.ClassName]={Id:c.ClassName,Name:c.mDisplayName||c.ClassName,Power:Number(c.mPowerConsumption),Exponent:Number(c.mPowerConsumptionExponent)||1.321929,Speed:Number(c.mManufacturingSpeed)||1,Cost:[]};
const recipes=[];
for(const c of all.filter(c=>c.native.endsWith("FGRecipe'"))) {
 const produced=refs(c.mProducedIn), inputs=amounts(c.mIngredients),outputs=amounts(c.mProduct);
 if(produced.includes('BP_BuildGun_C')) for(const p of outputs){const id=p.Item.replace(/^Desc_/,'Build_');if(buildings[id])buildings[id].Cost=inputs;}
 const machine=produced.find(p=>buildings[p]&&classes[p].mManufacturingSpeed!==undefined);
 if(machine&&outputs.length&&Number(c.mManufactoringDuration)>0) recipes.push({Id:c.ClassName,Name:c.mDisplayName||items[outputs[0].Item]?.Name||c.ClassName,Alternate:/Alternate/.test(c.ClassName),Machine:machine,Seconds:Number(c.mManufactoringDuration),Inputs:inputs,Outputs:outputs,PowerConstant:Number(c.mVariablePowerConsumptionConstant)||0,PowerFactor:Number(c.mVariablePowerConsumptionFactor)||0});
}
fs.writeFileSync(process.argv[3] || 'work/app/game.json',JSON.stringify({Source:source,Build:'24656030',Imported:new Date().toISOString(),Items:items,Buildings:buildings,Recipes:recipes},null,2));
console.log(`${Object.keys(items).length} items, ${recipes.length} machine recipes, ${Object.keys(buildings).length} buildings`);


