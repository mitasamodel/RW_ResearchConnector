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
	public enum ResearchPrereqSource
	{
		Legacy,    // researchPrerequisite
		List       // researchPrerequisites
	}

	public class ResearchAssignedLister
	{
		private readonly Action<ResearchProjectDef> _onClick;

		public ResearchAssignedLister(Action<ResearchProjectDef> onClick)
		{
			_onClick = onClick;
		}

		public void Draw(Rect inRect, ILister lister)
		{
			List<ResPrereqList> resList;
			var selectedDef = lister.SelectedDef();

			if (selectedDef != null)
			{
				if ((resList = GetResearchPrerequisites(selectedDef)) != null)
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
						_onClick?.Invoke(toRemove);
				}
			}
		}
	}
}
