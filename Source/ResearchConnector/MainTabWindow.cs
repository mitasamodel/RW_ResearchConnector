using System.Diagnostics;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
		[TweakValue("0_MY", 200f, 500f)]
		static float leftColumnWidth = 300f;
		[TweakValue("0_MY", 200f, 500f)]
		static float middleColumnWidth = 300f;
		[TweakValue("0_MY", 100f, 300f)]
		static float rightColumnWidth = 200f;
		[TweakValue("0_MY", 400f, 1200f)]
		static float width = leftColumnWidth + middleColumnWidth + rightColumnWidth + 2 * verticalGap + 2 * windowMargin;
		[TweakValue("0_MY", 400f, 1000f)]
		static float height = 500f;

		public override Vector2 InitialSize => new Vector2(width, height);

		// Mod selection
		// Static fields, so the window will save the selected items between sessions
		private static string selectedModId = "ludeon.rimworld";
		private static string selectedModName = "Core";
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

			optionalTitle = "This is optional title";
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

			// Packages for lister
			dataForSelector_Mods = Mods.Select(m => new SelectorRow(m.Name, m.PackageId, m.PackageId)).ToList();
			dataForSelector_Type = Enum.GetValues(typeof(ListerType))
				.Cast<ListerType>()
				.Select(kind => new SelectorRow(kind.ToString(), null, null))
				.ToList();

			// Available researches to assing
			researchesToAssign = new ResearchToAssignLister(OnAssignResearch);

			// Assigned researches
			assigned = new ResearchAssignedLister(OnRemoveResearch);
		}

		public override void DoWindowContents(Rect inRect)
		{
			float curY = 0f;

			// Mod selection
			curY += DrawModSelector(new Rect(0f, curY, inRect.width, rowHeight));

			// Type selection
			curY += DrawTypeSelector(new Rect(0f, curY, inRect.width, rowHeight));

			//Main area. Split into 3 columns
			Rect mainAreaRect = new Rect(0f, curY, inRect.width, inRect.height - curY);
			Widgets.DrawBox(mainAreaRect);

			Rect leftColumnRect = new Rect(0f, mainAreaRect.y, leftColumnWidth, mainAreaRect.height);
			Rect middleColumnRect = new Rect(leftColumnRect.xMax + verticalGap, mainAreaRect.y, middleColumnWidth, mainAreaRect.height);
			Rect rightColumnRect = new Rect(middleColumnRect.xMax + verticalGap, mainAreaRect.y, rightColumnWidth, mainAreaRect.height);
			Widgets.DrawBox(leftColumnRect);
			Widgets.DrawBox(middleColumnRect);
			Widgets.DrawBox(rightColumnRect);

			// Left column. List of things in selected Type
			CurrentLister.Draw(leftColumnRect);

			// Middle column. List of assigned researches
			assigned.Draw(middleColumnRect, CurrentLister);

			// Right column. List of researches
			researchesToAssign.Draw(rightColumnRect);
		}

		private void OnRemoveResearch(ResearchProjectDef resDef)
		{
			if ( resDef == null ) return;
			var def = CurrentLister.SelectedDef();
			if (def == null) return;

			// Currently only for Things
			if (def is ThingDef thingDef)
			{
				if (thingDef.researchPrerequisites == null) return;

				thingDef.researchPrerequisites.Remove(resDef);
				RemoveResearchHyperling(thingDef, resDef);
			}
			else
				Utils.LogNL($"[Not-ThingDef] {def.defName}");
		}

		private void RemoveResearchHyperling(ThingDef def, ResearchProjectDef resDef)
		{
			if (def == null ) return;
			if (resDef == null) return;
			if (def.descriptionHyperlinks == null) return;
			def.descriptionHyperlinks.RemoveAll(link => link.def == resDef);
		}

		private void AddResearchHyperlink(ThingDef toDef, ResearchProjectDef resDef)
		{
			if (toDef == null) return;
			if (resDef == null) return;
			if (toDef.descriptionHyperlinks == null)
				toDef.descriptionHyperlinks = new List<DefHyperlink>();
			if (!toDef.descriptionHyperlinks.Any(link => link.def == resDef))
				toDef.descriptionHyperlinks.Add(resDef);
		}

		private void OnAssignResearch(Def resDef)
		{
			if (resDef == null) return;
			if (resDef is ResearchProjectDef research)
			{
				var def = CurrentLister.SelectedDef();
				if (def == null) return;

				// Currently only for Things
				if (def is ThingDef thingDef)
				{
					if (thingDef.researchPrerequisites == null)
						thingDef.researchPrerequisites = new List<ResearchProjectDef>();

					if (!thingDef.researchPrerequisites.Contains(research))
					{
						thingDef.researchPrerequisites.Add(research);
						AddResearchHyperlink(thingDef, research);
					}
				}
				else
					Utils.LogNL($"[Not-ThingDef] {def.defName}");
			}
			else
				Verse.Log.Error($"[{ResearchConnector.modName}] Unexpected research type - not a researchDef: [{resDef.defName}]. Please report it to mod's author.");
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


	}
}
