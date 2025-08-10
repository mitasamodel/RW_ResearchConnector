using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public class ResearchToAssignLister : BaseLister<ResearchProjectDef>
	{
		private readonly float rowHeight = Utils_GUI.rowHeight;

		public ResearchToAssignLister()
		{
			RebuildCache("=Everything=");		// Always all data displayed
		}
		protected override IEnumerable<ResearchProjectDef> BuildList()
		{
			return DefDatabase<ResearchProjectDef>.AllDefsListForReading
				.OrderBy(def => def.label);
		}

		protected override void DrawRow(Rect rowRect, ResearchProjectDef def)
		{
			Widgets.Label(rowRect, def.label);
		}

		protected override float GetRowHeight(ResearchProjectDef def)
		{
			return rowHeight;
		}
		public override Def SelectedDef()
		{
			return null;
		}
	}
}
