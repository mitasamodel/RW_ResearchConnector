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

		/// <summary>
		/// Return list of all research prerequisites.
		/// Markes as legacy if the ResearchProjectDef came from a single research prerequisite (researchPrerequisite).
		/// </summary>
		/// <param name="def"></param>
		/// <returns></returns>
		public static List<ResPrereqList> GetAllResearchPrerequisites(Def def)
		{
			if (def == null) return null;
			List<ResPrereqList> resList = new List<ResPrereqList>();

			// Legacy field: researchPrerequisite
			if (def.GetLegacyResearchPrerequisite() is ResearchProjectDef legacyRes)
				resList.Add(new ResPrereqList(legacyRes, true));

			// List: researchPrerequisites
			var list = def.GetOrInitResearchPrerequisitesList();
			if (list != null)
			{
				foreach (var res in list)
					resList.Add(new ResPrereqList(res));
			}

			if (resList.Count > 0)
				return resList;
			else
				return null;
		}
	}
}
