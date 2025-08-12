using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public class ResearchLister : BaseLister<ResearchProjectDef>
	{
		public ResearchLister(string modId = null) : base(modId) { }

		protected override IEnumerable<ResearchProjectDef> BuildList()
		{
			var list = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			return list;
		}

		protected override void DrawRow(Rect rowRect, ResearchProjectDef def)
		{
			Widgets.Label(rowRect, def.label);
		}

		protected override float GetRowHeight(ResearchProjectDef def)
		{
			return Utils_GUI.rowHeight;
		}
		public override Def SelectedDef()
		{
			return null;
		}
	}
}
