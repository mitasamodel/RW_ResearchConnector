using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public class ItemsLister : BaseLister<ThingDef>
	{
		private ThingDef _selected;
		public ItemsLister(string modId = null) : base(modId)
		{
		}

		protected override IEnumerable<ThingDef> BuildList()
		{
			var list = DefDatabase<ThingDef>.AllDefsListForReading
				.Where(def => def.category == ThingCategory.Item);
			return list;
		}

		protected override void DrawRow(Rect rowRect, ThingDef def)
		{
			Widgets.DrawHighlightIfMouseover(rowRect);
			Widgets.Label(rowRect, def.label);
			if (Widgets.ButtonInvisible(rowRect)) _selected = def;
		}

		protected override float GetRowHeight(ThingDef def)
		{
			return Utils_GUI.rowHeight;
		}
		public override Def SelectedDef()
		{
			return _selected;
		}
	}
}
