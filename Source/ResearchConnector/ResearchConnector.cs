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
			// Mods list: MUST go back to window drawing. The cache must be created there - Dictionary
			Mods = LoadedModManager.RunningModsListForReading
				.Select(mod => (mod.PackageId, mod.Name))
				.OrderBy(mod => mod.Name)
				.ToList();
			Mods.Insert(0, (null, "=Everything="));   // Dummy mod on top - used for "all mods" option

			Utils.Log($"Mods:");
			foreach(var mod in Mods)
				Utils.Log($"{mod.Name}: {mod.PackageId}");

		}
	}
}
