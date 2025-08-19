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
			if (_selectedDef == def)
			{
				Rect infoRect = new Rect(rowRect.x + Utils_GUI.rowHeight, rowRect.y + Utils_GUI.rowHeight, Utils_GUI.rowHeight, Utils_GUI.rowHeight);
				if (Widgets.ButtonImage(infoRect, TexButton.Info))
				{
					Find.WindowStack.Add(new Dialog_InfoCard(def));
				}
				Rect defRect = new Rect(infoRect.xMax + 4f, infoRect.y, rowRect.width - infoRect.width - 4f, Utils_GUI.rowHeight);
				Widgets.Label(defRect, def.defName);
			}

			if (Widgets.ButtonInvisible(rowRect)) _selectedDef = def;
		}

		protected override float GetRowHeight(RecipeDef def)
		{
			if (def == _selectedDef)
			{
				return Utils_GUI.rowHeight * 2; // Double height for selected row
			}
			return Utils_GUI.rowHeight;
		}
	}
}
