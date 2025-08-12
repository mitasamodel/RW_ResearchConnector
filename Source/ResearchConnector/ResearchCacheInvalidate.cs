using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Verse;

namespace ResearchConnector
{
	public static class ResearchCacheInvalidate
	{
		static readonly FieldInfo F_Unlocked = typeof(ResearchProjectDef)
			.GetField("cachedUnlockedDefs", BindingFlags.Instance | BindingFlags.NonPublic);
		static readonly FieldInfo F_Hyperlinks = typeof(ResearchProjectDef)
			.GetField("cachedHyperlinks", BindingFlags.Instance | BindingFlags.NonPublic);
		static readonly FieldInfo F_Desc = typeof(ResearchProjectDef)
			.GetField("cachedDescription", BindingFlags.Instance | BindingFlags.NonPublic);

		//static readonly string _className = nameof(ResearchCacheInvalidate);

		/// <summary>
		/// Invalidate only the specified ResearchProjectDefs (their UnlockedDefs + related UI caches).
		/// </summary>
		public static void InvalidateProjects(IEnumerable<ResearchProjectDef> projects)
		{
			if (projects == null) return;

			foreach (var rp in projects)
			{
				InvalidateProject(rp);
			}
		}
		public static void InvalidateProject(ResearchProjectDef rp)
		{
//#if DEBUG
//			Utils.LogNL($"[{_className}] Try invalidate [{rp?.defName}]");
//#endif
			if (rp == null) return;
			F_Unlocked?.SetValue(rp, null);
			F_Hyperlinks?.SetValue(rp, null);
			F_Desc?.SetValue(rp, null);
		}

		/// <summary>
		/// Flush the Research tab’s own caches so groupings/headers recompute on next draw.
		/// </summary>
		public static void InvalidateResearchWindow()
		{
			var tabDef = MainButtonDefOf.Research;
			var win = tabDef?.TabWindow; // MainTabWindow_Research if open
			if (win != null && win.GetType().Name == "MainTabWindow_Research")
			{
				var t = win.GetType();
				t.GetField("cachedVisibleResearchProjects", BindingFlags.Instance | BindingFlags.NonPublic)
				 ?.SetValue(win, null);
				t.GetField("cachedUnlockedDefsGroupedByPrerequisites", BindingFlags.Instance | BindingFlags.NonPublic)
				 ?.SetValue(win, null);
				t.GetMethod("UpdateSelectedProject", BindingFlags.Instance | BindingFlags.NonPublic)
				 ?.Invoke(win, new object[] { Find.ResearchManager });
			}

			// Drop cached instance so the tab fully rebuilds when (re)opened.
			typeof(MainButtonDef).GetField("cachedTabWindow", BindingFlags.Instance | BindingFlags.NonPublic)
				?.SetValue(tabDef, null);
		}

		/// <summary>
		/// Convenience: pass the exact projects that were added/removed for an item.
		/// </summary>
		public static void InvalidateAfterItemChange(
			IEnumerable<ResearchProjectDef> addedProjects,
			IEnumerable<ResearchProjectDef> removedProjects,
			bool reopenTab = false)
		{
			// 1) Invalidate only affected projects
			InvalidateProjects(addedProjects);
			InvalidateProjects(removedProjects);

			// 2) Flush the window so groupings like "also unlocked by ..." recompute
			InvalidateResearchWindow();

			// 3) Optional: reopen immediately so user sees the change
			if (reopenTab)
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
		}
	}

}
