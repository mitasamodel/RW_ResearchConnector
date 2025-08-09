using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchConnector
{
	public enum ListerKind { Building, Item, Research /*, Research, Items */ }

	[StaticConstructorOnStartup]
	public static class ResearchConnector
	{
		internal const string modName = "ResearchConnector";
		static public readonly string logFile = @Environment.CurrentDirectory + @"\Mods\ResearchConnector.log";
		public static readonly List<(string PackageId, string Name)> Mods;

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
		internal static readonly Dictionary<ListerKind, Func<string, ILister>> ListerFactories = new Dictionary<ListerKind, Func<string, ILister>>
		{
			{ListerKind.Building, modId => new BuildingsLister(modId) },
		};

		static ResearchConnector()
		{
#if DEBUG
			File.WriteAllText(logFile, "[ResearchConnector] Debug start\n");    //create/rewrite file
#endif


			// This investigation is to include tiles/bridges/plates (also stuffed).
			// They are not "Thing"s, so they have to be treated separately.
			var tileDefs = DefDatabase<BuildableDef>.AllDefsListForReading
				.Where(def => def.BuildableByPlayer)
				.ToList();

			Utils.Log("=Buildable");
			foreach (var def in tileDefs)
			{
				if ( def is ThingDef thingDef)
				{
					Utils.Log($"Thing: Label[{thingDef.label}] Def[{thingDef.defName}] Cat[{thingDef.category}] Des[{thingDef.designationCategory}]");
				}
				else
				{
					Utils.Log($"Not-Thing: Label[{def.label}] Def[{def.defName}] Cat[-] Des[{def.designationCategory}]");
				}
				
			}
			

			// Mods list: MUST go back to window drawing. The cache must be created there -> Dictionary / List
			Mods = LoadedModManager.RunningModsListForReading
				.Select(mod => (mod.PackageId, mod.Name))
				.OrderBy(mod => mod.Name)
				.ToList();
			Mods.Insert(0, (null, "=Everything="));   // Dummy mod on top - used for "all mods" option

			//Utils.Log($"Mods:");
			//foreach(var mod in Mods)
			//	Utils.Log($"{mod.Name}: {mod.PackageId}");

		}
	}
}
