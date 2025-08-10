using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using static ResearchConnector.Utils;

namespace ResearchConnector
{
	internal static class Utils_Data
	{
		public static List<ThingDef> GetBuildingsList()
		{
			return DefDatabase<ThingDef>.AllDefsListForReading
				.Where(def => def.BuildableByPlayer)
				.ToList();
		}
		public static void DisplayBuildingTags(ThingDef def)
		{
			Log(" -Tags: ");
			foreach (var tag in def.building?.buildingTags)
			{
				Log($"[{tag}] ");
			}
			LogNL("");
		}
		public static void DisplayComps(ThingDef def)
		{
			Log(" -Comps: ");
			if (def.comps != null)
			{
				foreach (var comp in def.comps)
				{
					if (comp.compClass != null)
						Log($"cl[{comp.compClass.Name}] ");
					else
						Log($"[{comp}] ");
				}
				LogNL("");
			}
		}
	}
}
