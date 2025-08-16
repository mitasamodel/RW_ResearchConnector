using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public class WindowResizerHeight
	{
		public Vector2 minWindowSize = new Vector2(150f, 150f);

		private bool _draggingH;

		private float _startHeight;

		private float _startMouseY;

		public const float gripThickness = 8f;

		public Rect DoResizeControl(Rect winRect, Rect inRect)
		{
			var ev = Event.current;
			float mouseY = ev.mousePosition.y;

			Rect gripRect = new Rect(0f, inRect.height - gripThickness, inRect.width, gripThickness);
			if (ev.type == EventType.Repaint)
			{
				Widgets.DrawLineHorizontal(gripRect.x, gripRect.y + 2, gripRect.width, Color.gray);
				Widgets.DrawLineHorizontal(gripRect.x, gripRect.yMax - 3, gripRect.width, Color.gray);
			}

			if (ev.type != EventType.Repaint)
			{

				if (ev.type == EventType.MouseDown && gripRect.Contains(ev.mousePosition))
				{
					_draggingH = true;
					_startHeight = winRect.height;
					_startMouseY = mouseY;
					ev.Use();   // Mark the event as used so it doesn't propagate further
				}

				if (_draggingH)
				{
					float dy = mouseY - _startMouseY;
					winRect.height = _startHeight + dy;

					if (winRect.height < minWindowSize.y)
						winRect.height = minWindowSize.y;
					winRect.yMax = Mathf.Min(UI.screenHeight, winRect.yMax);

					if (ev.type == EventType.MouseUp || ev.rawType == EventType.MouseUp || !UnityEngine.Input.GetMouseButton(0))
						_draggingH = false;

					ev.Use();   // Mark the event as used so it doesn't propagate further (no drugging the window itself)
				}
			}

			return new Rect(winRect.x, winRect.y, (int)winRect.width, (int)winRect.height);
		}
	}
}
