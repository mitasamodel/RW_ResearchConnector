using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

using static ResearchConnector.Utils_Research;

namespace ResearchConnector
{
	public class ResearchAssignedLister
	{
		public ResearchAssignedLister() { }

		public void Draw(Rect inRect, Def selectedDef)
		{
			List<ResPrereqList> resList;

			if (selectedDef != null)
			{
				if ((resList = GetAllResearchPrerequisites(selectedDef)) != null)
				{
					ResearchProjectDef toRemove = null;
					Rect rowRect = inRect;
					rowRect.height = Utils_GUI.rowHeight;
					foreach (var res in resList)
					{
						Widgets.DrawHighlightIfMouseover(rowRect);
						Widgets.Label(rowRect, res.def.label);
						if (Widgets.ButtonInvisible(rowRect))
							toRemove = res.def;
						rowRect.y += Utils_GUI.rowHeight;
					}

					if (toRemove != null)
						selectedDef.RemoveResearchPrerequisite(toRemove);
				}
			}
		}

		private void RemoveResearch(Def def, ResearchProjectDef resDef)
		{
			if (resDef == null) return;
			if (def == null) return;

			def.RemoveResearchPrerequisite(resDef);
		}
	}
}
