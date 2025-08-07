using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchConnector
{
	[StaticConstructorOnStartup]
	public static class ResearchConnector
	{
		static public readonly string logFile = @Environment.CurrentDirectory + @"\Mods\ResearchConnector.log";

		public static readonly List<(string PackageId, string Name)> Mods;
		public static readonly List<ThingDef> Buildings;

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
