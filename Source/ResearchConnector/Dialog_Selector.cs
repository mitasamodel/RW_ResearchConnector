using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public class SelectorRow
	{
		public string Label { get; }
		public string Tooltip { get; }
		public string ExtraSearchField { get; }
		public SelectorRow(string label, string tooltips, string extraSearchField)
		{
			Label = label;
			Tooltip = tooltips;
			ExtraSearchField = extraSearchField;
		}
	}

	public class Dialog_Selector : Window
	{
		private readonly float rowH = Utils_GUI.rowHeight;
		private readonly float scrollW = Utils_GUI.scrollWidth;

		public override Vector2 InitialSize => new Vector2(500f, 600f);
		
		private Rect? _anchorScreenRect;    // Screen-space rect of the control that opened this dialog (optional)

		private readonly List<SelectorRow> _inputList;
		private List<int> _filteredIndexes;
		private Vector2 _scroll = Vector2.zero;

		// "Action" or "delegate" is basically a pointer to a function (in C).
		private Action<int?> _onSelect;     // Executes method, stored in _onSelect, with 1 argument - nullable int ("null" is allowed too)
		private Action<Vector2> _onCloseScroll;

		private string _search = "";

		public Dialog_Selector(List<SelectorRow> inputList)
			: this(inputList, null, Vector2.zero) { }
		public Dialog_Selector(List<SelectorRow> inputList, Action<int?> onSelect)
			: this(inputList, onSelect, Vector2.zero) { }
		public Dialog_Selector(List<SelectorRow> inputList, Action<int?> onSelect, Vector2 scroll, Action<Vector2> onCloseScroll = null, Rect? anchorScreenRect = null)
		{
			forcePause = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
			doCloseX = true;

			_inputList = inputList;
			_filteredIndexes = null;
			_onSelect = onSelect;
			_scroll = scroll;
			_onCloseScroll = onCloseScroll;
			_anchorScreenRect = anchorScreenRect;

			UpdateFilter();
		}

		public override void DoWindowContents(Rect inRect)
		{
			// Search field
			Rect searchRect = new Rect(0f, 0f, inRect.width, rowH);
			string newSearch = Widgets.TextField(searchRect, _search);
			if (newSearch != _search)
			{
				_search = newSearch;
				UpdateFilter();
			}

			//Scrollable area
			var indexesList = _filteredIndexes;
			float contentHeight = indexesList.Count * rowH;
			Rect positionRect = new Rect(0f, searchRect.height, inRect.width, inRect.height - searchRect.height);   // Where scroll area located.
			Rect contentRect = new Rect(0f, 0f, positionRect.width - scrollW, contentHeight);     //The content inside the scroll area. Coordinates are separate.
			Widgets.BeginScrollView(positionRect, ref _scroll, contentRect, true);
			float curY = 0f;

			foreach (var idx in indexesList)
			{
				var item = _inputList[idx];
				Rect rowRect = new Rect(0, curY, contentRect.width, Utils_GUI.rowHeight);
				Widgets.DrawHighlightIfMouseover(rowRect);
				Widgets.Label(rowRect, item.Label);
				if (Widgets.ButtonInvisible(rowRect))
				{
					_onSelect?.Invoke(idx);     // Invoke - call a Method (which is stored in _onSelect)
					Close();
				}

				curY += Utils_GUI.rowHeight;
			}
			Widgets.EndScrollView();
		}

		private void UpdateFilter()
		{
#if DEBUG
			int cnt = _filteredIndexes?.Count ?? 0;
#endif
			_filteredIndexes = Enumerable.Range(0, _inputList.Count)
				.Where(i =>
					string.IsNullOrEmpty(_search) ||
					_inputList[i].Label.ContainsIgnoreCase(_search) ||
					_inputList[i].ExtraSearchField.ContainsIgnoreCase(_search)
				)
				.ToList();
#if DEBUG
			Utils.LogNL($"[Dialog_Selector] Filter rebuilt. Was: {cnt}. New: {_filteredIndexes.Count}");
#endif
		}

		public override void PreClose()
		{
			base.PreClose();
			_onCloseScroll?.Invoke(_scroll);
		}

		// Position the dialog relative to the anchor if provided.
		protected override void SetInitialSizeAndPosition()
		{
			var size = InitialSize;
			if (_anchorScreenRect is Rect r)
			{
				float x = r.x + 18f;
				float y = r.y + 18f;

				// (nice to have) clamp to screen
				x = Mathf.Clamp(x, 0f, UI.screenWidth - size.x);
				y = Mathf.Clamp(y, 0f, UI.screenHeight - size.y);

				windowRect = new Rect(x, y, size.x, size.y);
				return;
			}
			else
			{
				// Fallback to default centering
				base.SetInitialSizeAndPosition();
			}
		}
	}
}
