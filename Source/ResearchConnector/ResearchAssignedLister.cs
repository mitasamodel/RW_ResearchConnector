using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public class ResearchAssignedLister
	{
		private readonly Action<ResearchProjectDef> _onClick;

		public ResearchAssignedLister(Action<ResearchProjectDef> onClick)
		{
			_onClick = onClick;
		}

		public void Draw(Rect inRect, ILister lister)
		{
			var simpleDef = lister.SelectedDef();
			if (simpleDef is ThingDef def && def.researchPrerequisites != null )
			{
				ResearchProjectDef toRemove = null;
				Rect rowRect = inRect;
				rowRect.height = Utils_GUI.rowHeight;
				foreach(var res in  def.researchPrerequisites)
				{
					Widgets.DrawHighlightIfMouseover(rowRect);
					Widgets.Label(rowRect, res.label);
					if (Widgets.ButtonInvisible(rowRect))
						toRemove = res;
					rowRect.y += Utils_GUI.rowHeight;
				}

				if ( toRemove != null )
					_onClick?.Invoke(toRemove);
			}

		}
	}
}
