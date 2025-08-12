using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public static class Utils_GUI
	{
		public const float rowHeight = 22f;
		public const float labelWidth = 80f;
		public const float scrollWidth = 16f;

		public static void LabelWithSelection(
			Rect inRect,    // Parent Rect
			string labelText,
			string selectionString,
			string selectionDefString,
			// Func is another pointer to a function, but now it not only takes arguments, but also returns a value. In that case - "Window" type
			// Func<arg1, arg2, arg3, return> - return is always the last
			Func<Window> makeWindow       // Dialog for selection
		)
		{
			Rect labelRect = new Rect(0f, inRect.y, labelWidth, rowHeight);
			Widgets.Label(labelRect, labelText);
			Rect selectRect = new Rect(labelRect.xMax, inRect.y, inRect.width - labelRect.xMax, rowHeight);
			Widgets.DrawHighlightIfMouseover(selectRect);
			Widgets.Label(selectRect, "▼ " + (selectionString ?? selectionDefString));
			//Widgets.DrawBox(selectRect);
			if (Widgets.ButtonInvisible(selectRect))
			{
				Find.WindowStack.Add(makeWindow());
			}
		}

		public static void LabelCentered(Rect inRect, string label)
		{
			var oldAnchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(inRect, label);
			Text.Anchor = oldAnchor;
		}

		public static void DrawLineVertical(float x, float y, float length, Color color)
		{
			Widgets.DrawBoxSolid(new Rect(x, y, 1f, length), color);
		}
	}
}
