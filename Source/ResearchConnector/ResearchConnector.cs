using ResearchConnector.Listers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using static ResearchConnector.Utils;
using static ResearchConnector.Utils_Data;

namespace ResearchConnector
{
	public enum ListerType { Building, Item, Research, Recipe /*, Research, Items */ }

	[StaticConstructorOnStartup]
	public static class ResearchConnector
	{
		internal const string modName = "ResearchConnector";
		static public readonly string logFile = @Environment.CurrentDirectory + @"\Mods\ResearchConnector.log";

		// Dictionary. Key is an enum-item for selection
		// Value is a pointer to a function, which takes 1 value as input (type: string) and returns 1 value (type: ILister)
		// Example without Lambda expression:
		//
		// ILister MakeBuildingsLister(string modId)
		// {
		//  	return new BuildingsLister(modId);
		// }
		//
		// Here we would need separated method name ("MakeBuildingsLister"). But for Dictionary we don't need it, we store it directly inside.
		// "internal" - all classes in that assembly can access it.
		internal static readonly Dictionary<ListerType, Func<string, ILister>> ListerFactories = new Dictionary<ListerType, Func<string, ILister>>
		{
			{ListerType.Building, modId => new BuildingsLister(modId) },
			{ListerType.Research, modId => new ResearchLister(modId) },
			{ListerType.Item, modId => new ItemsLister(modId) },
			{ListerType.Recipe, modId => new RecipesLister(modId) },
		};

		internal static readonly Dictionary<ListerType, string> DictPlural = new Dictionary<ListerType, string>
		{
			{ListerType.Building, "Buildings" },
			{ListerType.Research, "Research" },
			{ListerType.Item, "Items" },
			{ListerType.Recipe, "Recipes" },
		};



		static ResearchConnector()
		{
#if DEBUG
			File.WriteAllText(logFile, "[ResearchConnector] Debug start\n");    //create/rewrite file
#endif

#if DEBUG
#pragma warning disable CS0168 // Variable is declared but never used
			int maxL1, maxL2, maxL3, maxL4;
#pragma warning restore CS0168 // Variable is declared but never used

			// Some defs for future:
			// PlantDef
			// RecipeDef

			// Mods
			var mods = LoadedModManager.RunningModsListForReading
				.Select(mod => (mod.PackageId, mod.Name))
				.OrderBy(mod => mod.Name)
				.ToList();
			maxL1 = Math.Min(30, mods.Max(m => m.PackageId.Length));
			foreach (var mod in mods)
			{
				LogNL($"[Mod] ID[{mod.PackageId}]{TabsAfter(mod.PackageId.Length, maxL1)}Name[{mod.Name}]");
			}
			LogNL("");

			// Buildings
			List<ThingDef> buildings = GetBuildingsList();
			//maxL1 = Math.Min(20, buildings.Max(def => def.defName.Length));
			//maxL2 = Math.Min(25, buildings.Max(def => def.label.Length));
			//foreach (var def in buildings)
			//{
			//	Utils.LogNL($"[Building] Def[{def.defName}]{TabsAfter(def.defName.Length, maxL1)}" +
			//		$"Label[{def.label}]{TabsAfter(def.label.Length, maxL2)}Mod[{def.modContentPack?.PackageId}]");

			//	// Tags
			//	//DisplayBuildingTags(def);

			//	// Comps
			//	//DisplayComps(def);
			//}

			// Find item without modContentPack
			// Well, there are some items without this info. No idea why. For example - VVE_GarageCabinet
			var list = DefDatabase<ThingDef>.AllDefsListForReading
				.Where(def => def.category == ThingCategory.Building && def.BuildableByPlayer).ToList();
			foreach (var def in list)
			{
				var pack = def.modContentPack;
				if (pack == null)
				{
					//Utils.LogNL($"No-mod item: {def.defName}");
				}
				else
				{
					var pid = pack.PackageId;
					if (pid != pid.ToLowerInvariant())
					{
						Utils.LogNL($"Case-mismatch mod ID: {pid} (item: {def.defName})");
					}
				}
			}

			// This investigation is to include tiles/bridges/plates (also stuffed).
			// They are not "Thing"s, so they have to be treated separately.
			var tileDefs = DefDatabase<BuildableDef>.AllDefsListForReading
				.Where(def => def.BuildableByPlayer)
				.ToList();
			//Utils.Log("=Buildable");
			//foreach (var def in tileDefs)
			//{
			//	if ( def is ThingDef thingDef)
			//	{
			//		Utils.Log($"Thing: Label[{thingDef.label}] Def[{thingDef.defName}] Cat[{thingDef.category}] Des[{thingDef.designationCategory}]");
			//	}
			//	else
			//	{
			//		Utils.Log($"Not-Thing: Label[{def.label}] Def[{def.defName}] Cat[-] Des[{def.designationCategory}]");
			//	}

			//}

			// Researches
			var researchList = DefDatabase<ResearchProjectDef>.AllDefsListForReading
				.ToList();
			//Utils.Log("=Research");
			//foreach(var def in researchList)
			//{
			//	Utils.Log($"Def[{def.defName}] Label[{def.label}]");
			//}

			// Weapons
			var weapons = DefDatabase<ThingDef>.AllDefsListForReading
				.Where(def => def.IsWeapon)
				.ToList();
			//foreach (var def in weapons)
			//{
			//	Utils.LogNL($"Weapon[{def.label}] Def[{def.defName}]");
			//	Utils.Log($" -weaponTags: ");
			//	foreach (var item in def.weaponTags)
			//	{
			//		Utils.Log($"{item} ");
			//	}
			//	Utils.LogNL($"");
			//	if (def.thingCategories != null)
			//	{
			//		Utils.Log($" -thingCategories: ");
			//		foreach (var item in def.thingCategories)
			//		{
			//			Utils.Log($"{item} ");
			//		}
			//		Utils.LogNL($"");
			//	}
			//	if (def.weaponClasses != null)
			//	{
			//		Utils.Log($" -weaponClasses: ");
			//		foreach (var item in def.weaponClasses)
			//		{
			//			Utils.Log($"{item} ");
			//		}
			//		Utils.LogNL($"");
			//	}
			//}
#endif
		}





	}
}
