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
		/// <param name="legacy">If true, uses the legacy field (researchPrerequisite) instead of the list (researchPrerequisites)</param>
		/// <returns></returns>
		public static bool AddResearchPrerequisite(this Def def, ResearchProjectDef resDef, bool legacy = false)
		{
			if (def == null || resDef == null) return false;

			bool changed = false;

			// Single legacy researchPrerequisite field
			if (legacy)
			{
				if (def.GetLegacyResearchPrerequisite() == null)
				{
					def.SetLegacyResearchPrerequisite(resDef);
					changed = true;
				}
			}
			// List: researchPrerequisites
			else
			{
				var list = def.GetOrInitResearchPrerequisitesList();
				changed = list?.AddIfNotExists(resDef) ?? false;
			}

			if (changed)
			{
				def.AddDescriptionHyperlink(resDef);
				resDef.AddDescriptionHyperlink(def);
				ResearchCacheInvalidate.InvalidateProject(resDef);
				ActionLogger.Add(def, resDef, legacy);
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
			bool legacy = false;

			// Single legacy researchPrerequisite field
			if (def.GetLegacyResearchPrerequisite() == resDef)
			{
				def.SetLegacyResearchPrerequisite(null);
				legacy = true;
				changed = true;
			}
			// List: researchPrerequisites
			else
			{
				var list = def.GetOrInitResearchPrerequisitesList();
				changed = list?.RemoveAll(res => res == resDef) > 0;
			}

			if (changed)
			{
				def.RemoveDescriptionHyperlink(resDef);
				resDef.RemoveDescriptionHyperlink(def);
				ResearchCacheInvalidate.InvalidateProject(resDef);
				ActionLogger.Remove(def, resDef, legacy);
			}

			return changed;
		}

		/// <summary>
		/// List could be null, so we initialize it if needed.
		/// </summary>
		/// <param name="def"></param>
		/// <returns></returns>
		public static List<ResearchProjectDef> GetOrInitResearchPrerequisitesList(this Def def)
		{
			if (def == null) return null;
			switch (def)
			{
				case ThingDef d:
					d.researchPrerequisites ??= new List<ResearchProjectDef>();
					return d.researchPrerequisites;
				case RecipeDef d:
					d.researchPrerequisites ??= new List<ResearchProjectDef>();
					return d.researchPrerequisites;
				default:
					{
#if DEBUG
						Logger.LogOnce($"[{_className}] No fields for Def[{def.defName}] Type[{def.GetType()}]");
#endif
						return null;
					}
			}
		}

		/// <summary>
		/// Set legacy research prerequisite field researchPrerequisite.
		/// </summary>
		/// <param name="def"></param>
		/// <param name="resDef">null allowed (means remove this researchPrerequisite)</param>
		public static void SetLegacyResearchPrerequisite(this Def def, ResearchProjectDef resDef)
		{
			if (def == null) return;
			switch (def)
			{
				case RecipeDef d:
					d.researchPrerequisite = resDef;
					break;
				default:
#if DEBUG
					Logger.LogOnce($"[{_className}] No legacy field for Def[{def.defName}] Type[{def.GetType()}]");
#endif
					break;
			}
		}

		/// <summary>
		/// Get legacy research prerequisite field researchPrerequisite.
		/// </summary>
		/// <param name="def"></param>
		/// <returns></returns>
		public static ResearchProjectDef GetLegacyResearchPrerequisite(this Def def)
		{
			if (def == null) return null;
			return def switch
			{
				RecipeDef d => d.researchPrerequisite,
				_ => null,
			};
		}

		public static bool CanHaveResearchPrerequisites(this Def def)
		{
			if (def == null) return false;
			return def switch
			{
				ThingDef _ => true,
				RecipeDef _ => true,
				_ => false,
			};
		}

		public static void AddDescriptionHyperlink(this Def def, Def linkDef)
		{
			if (def == null || linkDef == null) return;
			def.descriptionHyperlinks ??= new List<DefHyperlink>();
			if (!def.descriptionHyperlinks.Any(link => link.def == linkDef))
				def.descriptionHyperlinks.Add(linkDef);

		}
		public static void RemoveDescriptionHyperlink(this Def def, Def linkDef)
		{
			if (def == null || linkDef == null) return;
			def.descriptionHyperlinks?.RemoveAll(link => link.def == linkDef);
		}

		public static bool HasResearchPrerequisite(this Def def, ResearchProjectDef resDef)
		{
			if (def == null || resDef == null) return false;

			var legacy = def.GetLegacyResearchPrerequisite();
			if (legacy != null && legacy == resDef)
				return true;

			var list = def.GetOrInitResearchPrerequisitesList();
			if (list != null)
				return list.Contains(resDef);
			return false;
		}
	}
}