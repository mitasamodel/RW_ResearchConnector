using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchConnector
{
	[StaticConstructorOnStartup]
	public static class ResearchConnector
	{
		public static readonly List<(string PackageId, string Name)> Mods;

		static ResearchConnector()
		{
			Mods = LoadedModManager.RunningModsListForReading
				.Select(mod => (mod.PackageId, mod.Name))
				.OrderBy(mod => mod.Name)
				.ToList();
			Mods.Insert(0, (null, "=All mods="));   // Dummy mod on top - used for "all mods" option
			Utils.Log($"Mods qty: {Mods.Count}");
		}
	}
}
