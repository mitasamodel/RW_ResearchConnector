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
			Widgets.Label(rowRect, def.label);
		}

		protected override float GetRowHeight(ThingDef def)
		{
			return GUI_Utils.rowHeight;
		}
	}
}
