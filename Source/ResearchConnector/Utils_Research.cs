using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchConnector
{
	public static class Utils_Research
	{
		/// <summary>
		/// isLegacy set to true if the def has a single research prerequisite (researchPrerequisite).
		/// </summary>
		public readonly struct ResPrereqList
		{
			public readonly ResearchProjectDef def;
			public readonly bool isLegacy;
			public ResPrereqList(ResearchProjectDef def, bool isLegacy = false)
			{
				this.def = def;
				this.isLegacy = isLegacy;
			}
		}

		public static List<ResPrereqList> GetResearchPrerequisites(Def def)
		{
			if (def == null) return null;
			List<ResPrereqList> resList = null;

			switch (def)
			{
				case ThingDef thingDef:
					if (thingDef.researchPrerequisites != null)
					{
						resList ??= new List<ResPrereqList>();
						foreach (var rec in thingDef.researchPrerequisites)
							resList.Add(new ResPrereqList(rec));
					}
					break;
				case RecipeDef recipeDef:
					// Check if recipeDef has a single research prerequisite.
					if (recipeDef.researchPrerequisite != null)
					{
						resList ??= new List<ResPrereqList>();
						resList.Add(new ResPrereqList(recipeDef.researchPrerequisite, true));
					}

					// Add a list of research prerequisites if exists.
					if (recipeDef.researchPrerequisites != null)
					{
						resList ??= new List<ResPrereqList>();
						foreach (var rec in recipeDef.researchPrerequisites)
							resList.Add(new ResPrereqList(rec));
					}
					break;
				default:
#if DEBUG
					Utils.LogNL($"[ResearchAssignedLister] GetResearchPrerequisites: Unsupported def type: {def.GetType().Name}");
#endif
					Verse.Log.Warning($"[{ResearchConnector.modName}] GetResearchPrerequisites: Yet unsupported def type: {def.GetType().Name}");
					break;
			}

			return resList;
		}
	}
}
