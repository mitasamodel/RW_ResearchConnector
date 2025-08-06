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

		// "Action" or "delegate" is basically a pointer to a function (in C).
		private readonly Action<string, string> _onSelect;		// Execute Method passed from Caller, provide selected modId and modName
		private readonly Action<Vector2> _onCloseSave;			// Same, but save scroll position outside (to be re-used after re-oppening)
		private Vector2 _scrollPosition = Vector2.zero;         // Default

		private string searchString = "";
		private List<(string PackageId, string Name)> filteredList;

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

			filteredList = ResearchConnector.Mods;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect windowRect = new Rect(0f, 0f, inRect.width, inRect.height);

			// Search field for a mod
			Rect searchRect = new Rect(0f, 0f, windowRect.width, GUI_Utils.rowHeight);
			string newSearchString = Widgets.TextField(searchRect, searchString);
			if ( searchString != newSearchString)
			{
				searchString = newSearchString;
				UpdateFilteredList();
			}

			// Mods and filtering
			var mods = filteredList;

			//Scrollable area
			float contentHeight = mods.Count * GUI_Utils.rowHeight;
			Rect positionRect = new Rect(0f, searchRect.height, windowRect.width, windowRect.height - searchRect.height);	// Where scroll area located
			Rect contentRect = new Rect(0f, 0f, positionRect.width - GUI_Utils.scrollWidth, contentHeight);		//The content inside scroll area. Coordinates are separate
			Widgets.BeginScrollView(positionRect, ref _scrollPosition, contentRect, true);
			float curY = 0f;
			foreach (var mod in mods)
			{
				Rect rowRect = new Rect(0, curY, contentRect.width, GUI_Utils.rowHeight);
				Widgets.DrawHighlightIfMouseover(rowRect);
				Widgets.Label(rowRect, mod.Name);
				if (Widgets.ButtonInvisible(rowRect))
				{
					_onSelect?.Invoke(mod.PackageId, mod.Name);		// Invoke - call a Method (which is stored in _onSelect)
					Close();
				}

				curY += GUI_Utils.rowHeight;
			}
			Widgets.EndScrollView();
		}

		public override void PreClose()
		{
			base.PreClose();
			_onCloseSave?.Invoke(_scrollPosition);
		}

		private void UpdateFilteredList()
		{
			if (string.IsNullOrEmpty(searchString))
				filteredList = ResearchConnector.Mods;
			else
			{
				filteredList = ResearchConnector.Mods
				.Where(mod =>
					string.IsNullOrEmpty(searchString) ||
					mod.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
					mod.PackageId.Contains(searchString, StringComparison.OrdinalIgnoreCase))
				.ToList();
			}
		}
	}
}
