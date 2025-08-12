using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchConnector
{
	public static class DefExtensions
	{
#if DEBUG
		const string _className = nameof(DefExtensions);
#endif
		public static bool AddResearchPrerequisite(this Def def, ResearchProjectDef resDef)
		{
			if (def == null || resDef == null) return false;

			bool changed = false;

			switch (def)
			{
				case ThingDef d:
					d.researchPrerequisites ??= new List<ResearchProjectDef>();
					changed |= d.researchPrerequisites.AddIfNotExists(resDef);
					break;
				case RecipeDef d:
					d.researchPrerequisites ??= new List<ResearchProjectDef>();
					changed |= d.researchPrerequisites.AddIfNotExists(resDef);
					break;
				default:
#if DEBUG
					Utils.LogNL($"[{_className}] Unsupported def type. Def[{def.defName}] Type[{def.GetType()}]");
#endif
					break;
			}

			if (changed)
			{
				AddResearchHyperlink(def, resDef);
				ResearchCacheInvalidate.InvalidateProject(resDef);
				ActionLogger.Add(def, resDef);
			}

			return changed;
		}

		private static void AddResearchHyperlink(Def def, ResearchProjectDef resDef)
		{
			def.descriptionHyperlinks ??= new List<DefHyperlink>();
			if (!def.descriptionHyperlinks.Any(link => link.def == resDef))
				def.descriptionHyperlinks.Add(resDef);
		}
	}
}
