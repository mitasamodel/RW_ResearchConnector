using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ResearchConnector
{
	public static class Utils_GUI
	{
		// RW constants
		public const float buttonHeight = 30f;
		public const float rowHeight = 22f;
		public const float scrollWidth = 16f;
		public const float windowMargin = 18f;

		// My constants
		public const float labelWidth = 80f;

		public static bool SetWrap(bool set)
		{
			var wrap = Text.WordWrap;
			Text.WordWrap = set;
			return wrap;
		}

		public static void RestoreWrap(bool wrap)
		{
			Text.WordWrap = wrap;
		}

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

		public static void DrawTextureWithHighlight(Rect rect, Texture2D texture, Color color)
		{
			Widgets.DrawTextureFitted(rect, texture, 1f);
			if (Mouse.IsOver(rect))
			{
				GUI.color = new Color(color.r, color.g, color.b, 0.5f);
				Widgets.DrawTextureFitted(rect, texture, 1f);
				GUI.color = Color.white;
			}
		}

		public static bool ButtonInvisibleDoubleClick(Rect rect, bool doMouseoverSound = true, int button = 0)
		{
			if (doMouseoverSound)
			{
				MouseoverSounds.DoRegion(rect);
			}
			Event ev = Event.current;
			if (ev.type == EventType.MouseDown && ev.button == button && rect.Contains(ev.mousePosition))
			{
				if (ev.clickCount == 2)
				{
					ev.Use(); // consume the event
					return true;
				}
			}
			return false;
		}
	}
}
