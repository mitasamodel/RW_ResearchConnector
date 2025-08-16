using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

using static ResearchConnector.Utils_Research;

namespace ResearchConnector
{
	public class ResearchAssignedLister
	{


		private ResearchProjectDef _selected = null;
		private const float _selectedRowHeight = 2 * Utils_GUI.rowHeight;

		public ResearchAssignedLister() { }

		public void Draw(Rect inRect, Def selectedDef)
		{
			List<ResPrereqList> resList;

			if (selectedDef != null)
			{
				if ((resList = GetAllResearchPrerequisites(selectedDef)) != null)
				{
					var wrap = Utils_GUI.SetWrap(false);

					ResearchProjectDef toRemove = null;
					Rect rowRect = inRect;
					foreach (var res in resList)
					{
						if (res.def == _selected)
						{
							rowRect.height = _selectedRowHeight;
							if (res.isLegacy) rowRect.height += Utils_GUI.rowHeight;
							Widgets.DrawBoxSolid(rowRect, ResearchConnector.SelectedColor);
						}
						else
							rowRect.height = Utils_GUI.rowHeight;

						Rect iconRect = new Rect(rowRect.x, rowRect.y, Utils_GUI.rowHeight, Utils_GUI.rowHeight);
						if (Widgets.ButtonImage(iconRect, ResearchConnector.IconRemove, Color.white, ResearchConnector.SelectedButtonRed))
							toRemove = res.def;

						Rect labelRect = new Rect(iconRect.xMax + 4f, iconRect.y, rowRect.width - iconRect.width - 4f, Utils_GUI.rowHeight);
						Widgets.DrawHighlightIfMouseover(labelRect);
						if (res.isLegacy)
							Widgets.Label(labelRect, "[L] " + res.def.label);    // Legacy marker
						else
							Widgets.Label(labelRect, res.def.label);

						if (Utils_GUI.ButtonInvisibleDoubleClick(labelRect))
							toRemove = res.def;
						if (Widgets.ButtonInvisible(labelRect))
						{
							if (_selected != res.def)
								_selected = res.def;
							else
								_selected = null;
						}

						if (res.def == _selected)
						{
							Rect infoRect = new Rect(iconRect.x, iconRect.y + Utils_GUI.rowHeight, rowRect.width, Utils_GUI.rowHeight);
							DrawInfoRow(infoRect, res.def);

							if (res.isLegacy)
								DrawLegacyRow(new Rect(rowRect.x, infoRect.y + Utils_GUI.rowHeight, rowRect.width, Utils_GUI.rowHeight));
						}

						rowRect.y += rowRect.height;
					}

					if (toRemove != null)
						selectedDef.RemoveResearchPrerequisite(toRemove);

					Utils_GUI.SetWrap(wrap);
				}
			}
		}

		private void DrawInfoRow(Rect inRect, ResearchProjectDef def)
		{
			Rect infoRect = new Rect(inRect.x + Utils_GUI.rowHeight, inRect.y, Utils_GUI.rowHeight, Utils_GUI.rowHeight);
			if (Widgets.ButtonImage(infoRect, TexButton.Info))
			{
				Find.WindowStack.Add(new Dialog_InfoCard(def));
			}
			Rect defRect = new Rect(infoRect.xMax + 4f, infoRect.y, inRect.width - infoRect.width - 4f, Utils_GUI.rowHeight);
			Widgets.Label(defRect, def.defName);
		}

		private void DrawLegacyRow(Rect inRect)
		{
			Rect legacyIcon = new Rect(inRect.x + Utils_GUI.rowHeight, inRect.y, Utils_GUI.rowHeight, Utils_GUI.rowHeight);
			Utils_GUI.LabelCentered(legacyIcon, "[L]");
			Rect legacyLabel = new Rect(legacyIcon.xMax + 4f, legacyIcon.y, inRect.width - legacyIcon.width - 4f, Utils_GUI.rowHeight);
			Widgets.Label(legacyLabel, "Legacy");
			Rect legacyTooltipRect = new Rect(legacyIcon.x, legacyIcon.y, inRect.width, Utils_GUI.rowHeight);
			TooltipHandler.TipRegion(legacyTooltipRect, ResearchConnector.LegacyResearchTag);
		}
	}
}
