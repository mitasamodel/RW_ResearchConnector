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
		/// <summary>
		/// Adds a research prerequisite to the def if it doesn't already exist.
		/// </summary>
		/// <param name="def">Supported def (like ThingDef)</param>
		/// <param name="resDef">ResearchProjectDef</param>
		/// <returns></returns>
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
				def.AddDescriptionHyperlink(resDef);
				resDef.AddDescriptionHyperlink(def);
				ResearchCacheInvalidate.InvalidateProject(resDef);
				ActionLogger.Add(def, resDef);
			}

			return changed;
		}

		/// <summary>
		/// Removes a research prerequisite from the def if it exists.
		/// </summary>
		/// <param name="def">Supported def (like ThingDef)</param>
		/// <param name="resDef">ResearchProjectDef</param>
		/// <returns></returns>
		public static bool RemoveResearchPrerequisite(this Def def, ResearchProjectDef resDef)
		{
			if (def == null || resDef == null) return false;
			bool changed = false;

			switch (def)
			{
				case ThingDef d:
					changed |= d.researchPrerequisites?.RemoveAll(res => res == resDef) > 0;
					break;
				case RecipeDef d:
					changed |= d.researchPrerequisites?.RemoveAll(res => res == resDef) > 0;
					break;
				default:
#if DEBUG
					Utils.LogNL($"[{_className}] Unsupported def type. Def[{def.defName}] Type[{def.GetType()}]");
#endif
					break;
			}

			if (changed)
			{
				def.RemoveResearchHyperlink(resDef);
				resDef.RemoveResearchHyperlink(def);
				ResearchCacheInvalidate.InvalidateProject(resDef);
				ActionLogger.Remove(def, resDef);
			}

			return changed;
		}

		private static void AddDescriptionHyperlink(this Def def, Def linkDef)
		{
			if (def == null || linkDef == null) return;
			def.descriptionHyperlinks ??= new List<DefHyperlink>();
			if (!def.descriptionHyperlinks.Any(link => link.def == linkDef))
				def.descriptionHyperlinks.Add(linkDef);

		}
		private static void RemoveResearchHyperlink(this Def def, Def linkDef)
		{
			if (def == null || linkDef == null) return;
			def.descriptionHyperlinks?.RemoveAll(link => link.def == linkDef);
		}
	}
}
