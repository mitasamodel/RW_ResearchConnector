using LudeonTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public class Dialog_ModSelector : Window
	{
		public override Vector2 InitialSize => new Vector2(500f, 600f);

		[TweakValue("0_MY", 10f, 40f)]
		static float rowHeight = 22f;
		const float scrollWidth = 16f;

		//"Action" or "delegate" is basically a pointer to a function (in C).
		private readonly Action<string, string> _onSelect;		// Execute Method passed from Caller, provide selected modId and modName
		private readonly Action<Vector2> _onCloseSave;			// Same, but save scroll position outside (to be re-used after re-oppening)
		private Vector2 _scrollPosition = Vector2.zero;         // Default

		public Dialog_ModSelector(Action<string, string> onSelect)		// Overload with only 1 param - no scrolling control
			: this(onSelect, Vector2.zero, null) { }
		public Dialog_ModSelector(Action<string, string> onSelect, Vector2 initialScroll, Action<Vector2> scrollBack)
		{
			forcePause = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
			doCloseX = true;

			_onSelect = onSelect;
			_scrollPosition = initialScroll;
			_onCloseSave = scrollBack;
		}

		public override void DoWindowContents(Rect inRect)
		{
			//var mods = LoadedModManager.RunningModsListForReading.OrderBy(mod => mod.Name).ToList();
			var mods = LoadedModManager.RunningModsListForReading
				.Select(m => (m.PackageId, m.Name))
				.OrderBy(m => m.Name)
				.ToList();

			mods.Insert(0, ("allModsFakeId", "=All mods="));

			float contentHeight = mods.Count * rowHeight;

			Rect contentRect = new Rect(0f, 0f, inRect.width - scrollWidth, contentHeight);
			Rect visibleRect = new Rect(0f, 0f, inRect.width, inRect.height);

			Widgets.BeginScrollView(visibleRect, ref _scrollPosition, contentRect, true);

			float curY = 0f;
			foreach (var mod in mods)
			{
				Rect rowRect = new Rect(0, curY, contentRect.width, rowHeight);
				Widgets.DrawHighlightIfMouseover(rowRect);
				Widgets.Label(rowRect, mod.Name);
				if (Widgets.ButtonInvisible(rowRect))
				{
					_onSelect?.Invoke(mod.PackageId, mod.Name);
					Close();
				}

				curY += rowHeight;
			}

			Widgets.EndScrollView();
		}

		public override void PreClose()
		{
			base.PreClose();
			_onCloseSave?.Invoke(_scrollPosition);
		}
	}
}
