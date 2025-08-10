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
		private Action<Def> _onClick;

		public ResearchToAssignLister(Action<Def> onClick)
		{
			_onClick = onClick;     // Method to be called on click
			RebuildCache("=Everything=");       // Always all data displayed
		}
		protected override IEnumerable<ResearchProjectDef> BuildList()
		{
			return DefDatabase<ResearchProjectDef>.AllDefsListForReading
				.OrderBy(def => def.label);
		}

		protected override void DrawRow(Rect rowRect, ResearchProjectDef def)
		{
			Widgets.DrawHighlightIfMouseover(rowRect);
			Widgets.Label(rowRect, def.label);
			if (Widgets.ButtonInvisible(rowRect))
				EnqueuePost(() => _onClick?.Invoke(def));	// Add action to the list. Will be executed after foreach loop finished
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
