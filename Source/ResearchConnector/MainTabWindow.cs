using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace ResearchConnector
{
	public class MainButtonWorker_ResearchConnector : MainButtonWorker
	{
		public override void Activate()
		{
			if (Find.WindowStack.WindowOfType<Window_ResearchConnector>() != null)
				Find.WindowStack.TryRemove(typeof(Window_ResearchConnector));
			else
				Find.WindowStack.Add(new Window_ResearchConnector());
		}
	}

	public class Window_ResearchConnector : Window
	{
		private readonly List<(string PackageId, string Name)> Mods;

		private readonly List<SelectorRow> dataForSelector_Mods;

		private readonly List<SelectorRow> dataForSelector_Type;

		private readonly ResearchToAssignLister researchesToAssign;

		private readonly ResearchAssignedLister assigned;

		const float windowMargin = 18f;
		[TweakValue("0_MY", 0f, 50f)]
		static float verticalGap = 20f;
		[TweakValue("0_MY", 100f, 500f)]
		static float leftColumnWidth = 300f;
		[TweakValue("0_MY", 100f, 300f)]
		static float middleColumnWidth = 200f;
		[TweakValue("0_MY", 100f, 300f)]
		static float rightColumnWidth = 200f;
		[TweakValue("0_MY", 400f, 1200f)]
		static float width = leftColumnWidth + middleColumnWidth + rightColumnWidth + 2 * verticalGap + 2 * windowMargin;
		[TweakValue("0_MY", 400f, 1000f)]
		static float height = 500f;
		[TweakValue("0_MY", 0f, 20f)]
		static float buttonMargin = 10f;
		[TweakValue("0_MY", 10f, 50f)]
		static float buttonHeight = 10f + 2 * buttonMargin;


		public override Vector2 InitialSize => new Vector2(width, height);

		// Mod selection
		// Static fields, so the window will save the selected items between sessions
		private static string selectedModId;
		private static string selectedModName;
		private static Vector2 scrollPositionModSelect = Vector2.zero;

		// Type selection
		private ListerType _currentType = ListerType.Building;
		private ILister _currentLister = null;
		public ILister CurrentLister
		{
			get
			{
				if (_currentLister == null)
				{
#if DEBUG
					//Utils.LogNL($"[MainTabWindow] New Lister");
					//var sw = Stopwatch.StartNew();
#endif
					if (ResearchConnector.ListerFactories.TryGetValue(_currentType, out var factory))
					{
						_currentLister = factory(selectedModId);
					}
					else
					{
						Verse.Log.Error($"[{ResearchConnector.modName}] Unexpected ListerType value: {_currentType}. Please report it to mod author.");
						_currentType = ListerType.Building;
						_currentLister = ResearchConnector.ListerFactories[ListerType.Building](selectedModId);
					}

#if DEBUG
					//sw.Stop();
					//Utils.LogNL($"[MainTabWindow] Lister creation took {sw.ElapsedMilliseconds} ms");
#endif
				}

				return _currentLister;
			}
		}

		float rowHeight = Utils_GUI.rowHeight;

		public Window_ResearchConnector()
		{
			//Placement and drawing order
			layer = WindowLayer.Dialog;     //on top of all
			draggable = true;
			resizeable = true;

			doCloseX = true;            // show the X button
			doCloseButton = false;      // no bottom "Close" button
			closeOnAccept = false;      // don't close on <Enter>
			closeOnCancel = false;      // don't close on <Esc>
			openMenuOnCancel = true;    // open game menu on <Esc>
			closeOnClickedOutside = false;   // don’t close when clicking outside

			forcePause = false;          // pause if in the window

			preventCameraMotion = false;
			absorbInputAroundWindow = false; // allow interaction with game world
			forceCatchAcceptAndCancelEventEvenIfUnfocused = false;      //don't react to <Enter> or <Esc> if not in focus

			preventDrawTutor = false;       //prevents drawing tutorial
			doWindowBackground = true;      //standard Rimworld window background
			drawShadow = true;
			shadowAlpha = 1f;       //transparency of the shadow
			focusWhenOpened = true;
			onlyOneOfTypeAllowed = true;        //only 1 this window type can be opened
			grayOutIfOtherDialogOpen = false;
			drawInScreenshotMode = true;
			onlyDrawInDevMode = false;

			// Mods list
			Mods = new List<(string PackageId, string Name)>()
			{
				(null, "=Unknown/Undefined="),		// Defs without modContentPack
				("=Everything=", "=Everything=")	// Display all data
			};
			Mods.AddRange(LoadedModManager.RunningModsListForReading
				.Select(mod => (mod.PackageId, mod.Name))
				.OrderBy(mod => mod.Name)
				.ToList());

			selectedModId = "=Everything=";
			selectedModName = Mods_GetNameById(selectedModId);

			// Packages for lister
			dataForSelector_Mods = Mods.Select(m => new SelectorRow(m.Name, m.PackageId, m.PackageId)).ToList();
			dataForSelector_Type = Enum.GetValues(typeof(ListerType))
				.Cast<ListerType>()
				.Select(kind => new SelectorRow(kind.ToString(), null, null))
				.ToList();

			// Available researches to assing
			researchesToAssign = new ResearchToAssignLister(OnAssignResearch);

			// Assigned researches
			assigned = new ResearchAssignedLister();
		}

		public override void DoWindowContents(Rect inRect)
		{
			float curY = 0f;

			// Menu
			curY = DrawMenu(new Rect(0f, curY, inRect.width, rowHeight * 2));

			// Mod selection
			curY += DrawModSelector(new Rect(0f, curY, inRect.width, rowHeight));

			// Type selection
			curY += DrawTypeSelector(new Rect(0f, curY, inRect.width, rowHeight));
			curY += rowHeight;

			//Main area. Split into 3 columns
			Rect mainAreaRect = new Rect(0f, curY, inRect.width, inRect.height - curY);
			//Widgets.DrawBox(mainAreaRect);

			Rect leftColumnRect = new Rect(0f, mainAreaRect.y, leftColumnWidth, mainAreaRect.height);
			Rect middleColumnRect = new Rect(leftColumnRect.xMax + verticalGap, mainAreaRect.y, middleColumnWidth, mainAreaRect.height);
			Rect rightColumnRect = new Rect(middleColumnRect.xMax + verticalGap, mainAreaRect.y, rightColumnWidth, mainAreaRect.height);
			//Widgets.DrawBox(leftColumnRect);
			//Widgets.DrawBox(middleColumnRect);
			//Widgets.DrawBox(rightColumnRect);

			Widgets.DrawLineHorizontal(mainAreaRect.x, mainAreaRect.y, mainAreaRect.width, Color.grey);

			// Left column. List of things in selected Type
			Utils_GUI.LabelCentered(new Rect(leftColumnRect.x, leftColumnRect.y, leftColumnRect.width, rowHeight), GetPlural(_currentType));
			CurrentLister.Draw(new Rect(leftColumnRect.x, leftColumnRect.y + rowHeight, leftColumnRect.width, leftColumnRect.height - rowHeight));

			Utils_GUI.DrawLineVertical(leftColumnRect.xMax + verticalGap / 2, mainAreaRect.y, mainAreaRect.height, Color.grey);

			// Middle column. List of assigned research
			Utils_GUI.LabelCentered(new Rect(middleColumnRect.x, middleColumnRect.y, middleColumnRect.width, rowHeight), "Assigned research");
			assigned.Draw(new Rect(middleColumnRect.x, middleColumnRect.y + rowHeight, middleColumnRect.width, middleColumnRect.height - rowHeight), CurrentLister.SelectedDef());

			Utils_GUI.DrawLineVertical(middleColumnRect.xMax + verticalGap / 2, mainAreaRect.y, mainAreaRect.height, Color.grey);

			// Right column. List of all research
			Utils_GUI.LabelCentered(new Rect(rightColumnRect.x, rightColumnRect.y, rightColumnRect.width, rowHeight), "All research");
			researchesToAssign.Draw(new Rect(rightColumnRect.x, rightColumnRect.y + rowHeight, rightColumnRect.width, rightColumnRect.height - rowHeight));
		}

		private string GetPlural(ListerType type)
		{
			if (ResearchConnector.DictPlural.TryGetValue(type, out string str))
				return str;
			else
				return type.ToString();
		}

		private float DrawMenu(Rect inRect)
		{
			float height = inRect.height;

			// Export to XML button
			string export = "Export";
			Rect exportButtonRect = new Rect(inRect.x, inRect.y, Text.CalcSize(export).x + buttonMargin * 2, buttonHeight);
			Widgets.DrawBox(exportButtonRect);
			Widgets.DrawHighlightIfMouseover(exportButtonRect);
			Utils_GUI.LabelCentered(exportButtonRect, export);
			if (Widgets.ButtonInvisible(exportButtonRect))
			{
				ActionLogger.ListAll();
			}

			return height;
		}

		//private void OnRemoveResearch(ResearchProjectDef resDef)
		//{
		//	if (resDef == null) return;
		//	var def = CurrentLister.SelectedDef();
		//	if (def == null) return;

		//	def.RemoveResearchPrerequisite(resDef);
		//}

		private void OnAssignResearch(ResearchProjectDef resDef)
		{
			if (resDef == null) return;
			var def = CurrentLister.SelectedDef();
			if (def == null) return;

			def.AddResearchPrerequisite(resDef);

		}

		private float DrawTypeSelector(Rect inRect)
		{
			Utils_GUI.LabelWithSelection(inRect, "Type: ", _currentType.ToString(), "Select type",
				() => new Dialog_Selector(dataForSelector_Type, OnTypeSelect, Vector2.zero, null, windowRect)
			);
			return inRect.height;
		}

		private float DrawModSelector(Rect inRect)
		{
			Utils_GUI.LabelWithSelection(inRect, "Mod:", selectedModName, "Select mod",
				() => new Dialog_Selector
					(
						dataForSelector_Mods,
						OnModSelect,            // Get executed if clicked on element in the list
						scrollPositionModSelect,
						newScroll => scrollPositionModSelect = newScroll,       // Save scroll position on Close
						windowRect          // Where to draw - at the same location as this window (windowRect)
					)
			);
			return inRect.height;
		}

		private void OnModSelect(int? idx)
		{
			if (idx is int i)
			{
				var mod = Mods.FirstOrDefault(m => m.Name == dataForSelector_Mods[i].Label);
				if (mod.Name != null)
				{
					selectedModId = mod.PackageId;
					selectedModName = mod.Name;
					CurrentLister.RebuildCache(selectedModId);
				}
			}
			else
			{
				string typeName = idx.GetType()?.Name ?? "null";
				Verse.Log.Error($"[{ResearchConnector.modName}] Unexpected 'idx' value in mod selector: [{idx}]:[{typeName}]. Please report it to mod's author.");
			}
		}

		private void OnTypeSelect(int? idx)
		{
			if (idx is int i)
			{
				_currentType = (ListerType)i;
				_currentLister = null;      // Force rebuild @ next access
			}
			else
			{
				string typeName = idx.GetType()?.Name ?? "null";
				Verse.Log.Error($"[{ResearchConnector.modName}] Unexpected 'idx' value in mod selector: [{idx}]:[{typeName}]. Please report it to mod's author.");
			}
		}

		private string Mods_GetIdByName(string modName) => Mods.FirstOrDefault(m => m.Name == modName).PackageId;
		private string Mods_GetNameById(string modId) => Mods.FirstOrDefault(m => m.PackageId == modId).Name;
	}
}
