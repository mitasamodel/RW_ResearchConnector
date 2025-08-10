using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public abstract class BaseLister<TDef> : ILister where TDef : Def
	{
		protected Vector2 _scroll = Vector2.zero;
		protected string _modId;
		protected Dictionary<string, TDef> _defDict = null;
		protected List<TDef> _defList;
		protected string _search = "";
		protected List<TDef> _filteredList;

		public BaseLister(string modId = null)
		{
#if DEBUG
			Utils.LogNL($"[BaseLister] Construct[{this.GetType().Name}] modId[{modId ?? "null"}]");
#endif
			_modId = modId;
			RebuildCache(_modId);
		}

		public void Draw(Rect inRect)
		{
			Rect searchFieldRect = new Rect(inRect.x, inRect.y, inRect.width, Utils_GUI.rowHeight);
			var newSearch = Widgets.TextField(searchFieldRect, _search);
			if (newSearch != _search)
			{
				_search = newSearch;
				UpdateFilter();
			}

			// List of defs.
			var list = _filteredList ?? new List<TDef>();

			// Content height depends on def - if def is selected, the height will be taller.
			float totalHeight = 0;
			foreach (var def in list)
				totalHeight += GetRowHeight(def);

			Rect scrollPositionRect = new Rect(inRect.x, searchFieldRect.yMax, inRect.width, inRect.height - searchFieldRect.height);
			Rect scrollContentRect = new Rect(0f, 0f, scrollPositionRect.width - Utils_GUI.scrollWidth, totalHeight);

			float scrollY = 0f;
			Widgets.BeginScrollView(scrollPositionRect, ref _scroll, scrollContentRect);
			Rect rowRect = new Rect(0f, scrollY, scrollContentRect.width, Utils_GUI.rowHeight);
			foreach (var def in list)
			{
				float height = GetRowHeight(def);
				rowRect.y = scrollY;
				DrawRow(rowRect, def);
				scrollY += height;
			}
			Widgets.EndScrollView();
		}

		public void RebuildCache(string modId)
		{
#if DEBUG
			int cnt = _defList?.Count ?? 0;
#endif
			_modId = modId;
			if (_modId != null)
			{
				_defList = BuildList().Where(def => _modId == "=Everything=" || string.Equals(def.modContentPack?.PackageId, _modId, StringComparison.OrdinalIgnoreCase)).ToList();
			}
			else
			{
				_defList = BuildList().Where(def => def.modContentPack == null).ToList();
			}
			_defDict = null;
#if DEBUG
			Utils.LogNL($"[BaseLister] Cache rebuilt. Was: {cnt}. New: {_defList.Count}");
#endif
			UpdateFilter();
		}

		private void UpdateFilter()
		{
#if DEBUG
			int cnt = _filteredList?.Count ?? 0;
#endif
			_filteredList = _defList
				.Where(def => MatchesSearch(def, _search))
				.ToList();
#if DEBUG
			Utils.LogNL($"[BaseLister] Filter rebuilt. Was: {cnt}. New: {_filteredList.Count}");
#endif
		}

		protected abstract IEnumerable<TDef> BuildList();
		protected virtual bool MatchesSearch(TDef def, string search)
		{
			return string.IsNullOrEmpty(search)
				|| def.label?.ContainsIgnoreCase(search) == true
				|| def.defName?.ContainsIgnoreCase(search) == true;
		}
		protected abstract void DrawRow(Rect rowRect, TDef def);
		protected abstract float GetRowHeight(TDef def);
	}
}
