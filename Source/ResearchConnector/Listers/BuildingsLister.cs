using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public class BuildingsLister : BaseLister<ThingDef>
	{
		private ThingDef _selected = null;
		private float _selectedRowHeight = 3f * Utils_GUI.rowHeight;

		public BuildingsLister(string modId = null) : base(modId) { }
		private Dictionary<string, ThingDef> DefDict
		{
			get
			{
				if (_defDict == null)
					_defDict = _defList.ToDictionary(def => def.defName);
				return _defDict;
			}
		}
		protected override void DrawRow(Rect rowRect, ThingDef def)
		{
			bool wrap = Utils_GUI.SetWrap(false);

			if (def == _selected)
			{
				rowRect.height = _selectedRowHeight;
				Widgets.DrawBoxSolid(rowRect, ResearchConnector.SelectedColor);
			}

			Widgets.DrawHighlightIfMouseover(rowRect);
			Widgets.Label(new Rect(rowRect.x, rowRect.y, rowRect.width, Utils_GUI.rowHeight), def.label);
			if (def == _selected)
			{



				Rect infoRect = new Rect(rowRect.x + Utils_GUI.rowHeight, rowRect.y + Utils_GUI.rowHeight, Utils_GUI.rowHeight, Utils_GUI.rowHeight);
				if (Widgets.ButtonImage(infoRect, TexButton.Info))
				{
					Find.WindowStack.Add(new Dialog_InfoCard(def));
				}
				Rect defRect = new Rect(infoRect.xMax + 4f, infoRect.y, rowRect.width - infoRect.width - 4f, Utils_GUI.rowHeight);
				Widgets.Label(defRect, def.defName);
				TooltipHandler.TipRegion(defRect, def.defName);


				Widgets.Label(new Rect(rowRect.x, rowRect.y + 2 * Utils_GUI.rowHeight, rowRect.width, Utils_GUI.rowHeight), " -Cat: " + def.category.ToString());
			}
			if (Widgets.ButtonInvisible(rowRect)) _selected = def;

			Utils_GUI.RestoreWrap(wrap);
		}

		protected override IEnumerable<ThingDef> BuildList()
		{
			return DefDatabase<ThingDef>.AllDefsListForReading
				.Where(def => def.category == ThingCategory.Building && def.BuildableByPlayer);
		}

		protected override float GetRowHeight(ThingDef def)
		{
			return def == _selected ? _selectedRowHeight : Utils_GUI.rowHeight;
		}

		public override Def SelectedDef()
		{
			return _selected;
		}
	}
}
