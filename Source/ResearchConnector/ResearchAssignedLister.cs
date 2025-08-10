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
		//private static Def _selectedDef;

		public ResearchAssignedLister()
		{

		}

		public void Draw(Rect inRect, ILister lister)
		{
			var simpleDef = lister.SelectedDef();
			if (simpleDef is ThingDef def && def.researchPrerequisites != null )
			{
				Rect rowRect = inRect;
				foreach(var res in  def.researchPrerequisites)
				{
					Widgets.Label(rowRect, res.label);
					rowRect.y += Utils_GUI.rowHeight;
				}
			}

		}
	}
}
