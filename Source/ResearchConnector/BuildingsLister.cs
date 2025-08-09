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
		private float _selectedRowHeight = 3f * GUI_Utils.rowHeight;

		public BuildingsLister(string modId = "ludeon.rimworld") : base(modId) { }

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
			if (def == _selected)
			{
				rowRect.height = _selectedRowHeight;
				Widgets.DrawBoxSolid(rowRect, new Color32(144, 97, 29, 128));
			}

			Widgets.DrawHighlightIfMouseover(rowRect);
			Widgets.Label(new Rect(rowRect.x, rowRect.y, rowRect.width, GUI_Utils.rowHeight), def.label);
			if (def == _selected)
			{
				Widgets.Label(new Rect(rowRect.x, rowRect.y + GUI_Utils.rowHeight, rowRect.width, GUI_Utils.rowHeight), " -Def: " + def.defName);
				Widgets.Label(new Rect(rowRect.x, rowRect.y + 2 * GUI_Utils.rowHeight, rowRect.width, GUI_Utils.rowHeight), " -Cat: " + def.category.ToString());
			}
			if (Widgets.ButtonInvisible(rowRect)) _selected = def;
		}

		protected override IEnumerable<ThingDef> BuildList()
		{
			return DefDatabase<ThingDef>.AllDefsListForReading
				.Where(def => def.category == ThingCategory.Building && def.BuildableByPlayer);
		}

		protected override float GetRowHeight(ThingDef def)
		{
			return def == _selected ? _selectedRowHeight : GUI_Utils.rowHeight;
		}
	}
}
