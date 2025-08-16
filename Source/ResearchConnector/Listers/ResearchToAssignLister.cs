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
		private Action<ResearchProjectDef> _onClick;
		private static ResearchProjectDef _selected = null;
		private const float _selectedRowHeight = 2 * Utils_GUI.rowHeight;

		public ResearchToAssignLister(Action<ResearchProjectDef> onClick)
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
			var wrap = Utils_GUI.SetWrap(false);

			if (_selected != null && def == _selected)
			{
				rowRect.height = _selectedRowHeight;  // If selected, increase height
				Widgets.DrawBoxSolid(rowRect, ResearchConnector.SelectedColor);
			}

			Rect iconRect = new Rect(rowRect.x, rowRect.y, rowHeight, rowHeight);
			if (Widgets.ButtonImage(iconRect, TexButton.Add, Color.white, ResearchConnector.SelectedButtonGreen))
				EnqueuePost(() => _onClick?.Invoke(def));   // Add action to the list. Will be executed after foreach loop finished

			Rect labelRect = new Rect(iconRect.xMax + 4f, iconRect.y, rowRect.width - iconRect.width - 4f, rowHeight);
			Widgets.DrawHighlightIfMouseover(labelRect);
			Widgets.Label(labelRect, def.label);
			if (Utils_GUI.ButtonInvisibleDoubleClick(labelRect))
				EnqueuePost(() => _onClick?.Invoke(def));
			if (Widgets.ButtonInvisible(labelRect))
			{
				if (_selected != def)
					_selected = def;
				else
					_selected = null;
			}

			if (def == _selected)
			{
				Rect infoRect = new Rect(iconRect.x + Utils_GUI.rowHeight, iconRect.y + Utils_GUI.rowHeight, Utils_GUI.rowHeight, Utils_GUI.rowHeight);
				if (Widgets.ButtonImage(infoRect, TexButton.Info))
				{
					Find.WindowStack.Add(new Dialog_InfoCard(def));
				}
				Rect defRect = new Rect(infoRect.xMax + 4f, infoRect.y, rowRect.width - infoRect.width - 4f, Utils_GUI.rowHeight);
				Widgets.Label(defRect, def.defName);
			}

			Utils_GUI.RestoreWrap(wrap);
		}

		protected override float GetRowHeight(ResearchProjectDef def)
		{
			if (def == _selected)
				return _selectedRowHeight;  // If selected, return larger height
			return rowHeight;
		}
		public override Def SelectedDef()
		{
			return null;
		}
	}
}
