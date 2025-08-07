using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public class Buildings
	{
		private Vector2 _scroll = Vector2.zero;
		private string _modId;
		private Dictionary<string, ThingDef> _defDict = null;
		private List<ThingDef> _defList;
		private string _search = "";
		private List<ThingDef> _filteredList;

		public Buildings(string modId = "ludeon.rimworld")
		{
			_modId = modId;
			RebuildCache(_modId);
		}

		private Dictionary<string, ThingDef> DefDict
		{
			get
			{
				if ( _defDict == null )
					_defDict = _defList.ToDictionary(def => def.defName);
				return _defDict;
			}
		}

		// Draw the list and add filtering
		public void Draw(Rect inRect)
		{
			Rect searchFieldRect = new Rect(inRect.x, inRect.y, inRect.width, GUI_Utils.rowHeight);
			var newSearch = Widgets.TextField(searchFieldRect, _search);
			if ( newSearch != _search)
			{
				_search = newSearch;
				UpdateFilter();
			}
			var buildings = _filteredList;
			Rect scrollPositionRect = new Rect(inRect.x, searchFieldRect.yMax, inRect.width, inRect.height - searchFieldRect.height);
			Rect scrollContentRect = new Rect(0f, 0f, scrollPositionRect.width - GUI_Utils.scrollWidth, buildings.Count * GUI_Utils.rowHeight);
			float scrollY = 0f;
			Widgets.BeginScrollView(scrollPositionRect, ref _scroll, scrollContentRect);
			foreach (var def in buildings)
			{
				Rect rowRect = new Rect(0f, scrollY, scrollContentRect.width, GUI_Utils.rowHeight);
				Widgets.Label(rowRect, def.label);
				scrollY += GUI_Utils.rowHeight;
			}
			Widgets.EndScrollView();
		}

		public void RebuildCache(string modId)
		{
#if DEBUG
			int cnt = _defList?.Count ?? 0;
#endif
			_modId = modId;
			_defList = DefDatabase<ThingDef>.AllDefsListForReading
				.Where(def => (def.category == ThingCategory.Building && def.BuildableByPlayer) && (_modId == null || def.modContentPack.PackageId == _modId))
				.ToList();
			_defDict = null;
#if DEBUG
			Utils.Log($"Cache rebuilt. Was: {cnt}. New: {_defList.Count}");
#endif
			UpdateFilter();
		}

		private void UpdateFilter()
		{
#if DEBUG
			int cnt = _filteredList?.Count ?? 0;
#endif
			_filteredList = _defList
				.Where(def => (string.IsNullOrEmpty(_search) || def.label.ContainsIgnoreCase(_search) || def.defName.ContainsIgnoreCase(_search)))
				.ToList();
#if DEBUG
			Utils.Log($"Filter rebuilt. Was: {cnt}. New: {_filteredList.Count}");
#endif
		}
	}
}
