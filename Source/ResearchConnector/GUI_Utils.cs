using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public static class GUI_Utils
	{
		public const float rowHeight = 22f;
		public const float labelWidth = 80f;
		public const float scrollWidth = 16f;

		public static void LabelWithSelection(
			Rect inRect,    // Parent Rect
			float curY,     // Current row (coordinate)
			string labelText,
			string selectionString,
			string selectionDefString,
			Window window       // Dialog for selection
		)
		{
			Rect labelRect = new Rect(0f, curY, labelWidth, rowHeight);
			Widgets.Label(labelRect, labelText);
			Rect selectRect = new Rect(labelRect.xMax, curY, inRect.width - labelRect.xMax, rowHeight);
			Widgets.DrawHighlightIfMouseover(selectRect);
			Widgets.Label(selectRect, "▼ " + (selectionString ?? selectionDefString));
			Widgets.DrawBox(selectRect);
			if (Widgets.ButtonInvisible(selectRect))
			{
				Find.WindowStack.Add(window);
			}
		}
	}
}
