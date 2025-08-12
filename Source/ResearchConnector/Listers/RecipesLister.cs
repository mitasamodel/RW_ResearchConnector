using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector.Listers
{
	public class RecipesLister : BaseLister<RecipeDef>
	{
		private static RecipeDef _selectedDef;

		public RecipesLister(string modId = null) : base(modId) { }

		public override Def SelectedDef()
		{
			return _selectedDef;
		}

		protected override IEnumerable<RecipeDef> BuildList()
		{
			return DefDatabase<RecipeDef>.AllDefsListForReading;
		}

		protected override void DrawRow(Rect rowRect, RecipeDef def)
		{
			if (def == _selectedDef)
			{
				Widgets.DrawBoxSolid(rowRect, new Color32(144, 97, 29, 128));
			}
			Widgets.DrawHighlightIfMouseover(rowRect);
			Widgets.Label(rowRect, def.label);
			if (Widgets.ButtonInvisible(rowRect)) _selectedDef = def;
		}

		protected override float GetRowHeight(RecipeDef def)
		{
			return Utils_GUI.rowHeight;
		}
	}
}
