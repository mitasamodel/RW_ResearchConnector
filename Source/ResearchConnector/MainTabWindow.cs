using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
		// Sometimes if the window is closed via "X" button on top right, it still renders the data
		// even if PostClose() has been already executed. It leads to null-reference exceptions.
		private bool isClosed = false;

		[TweakValue("0_MY", 0f, 50f)]
		static float verticalGap = 20f;
		[TweakValue("0_MY", 200f, 500f)]
		static float leftColumnWidth = 300f;
		[TweakValue("0_MY", 200f, 500f)]
		static float middleColumnWidth = 300f;
		[TweakValue("0_MY", 200f, 500f)]
		static float rightColumnWidth = 300f;
		[TweakValue("0_MY", 400f, 1200f)]
		static float width = leftColumnWidth + middleColumnWidth + rightColumnWidth + 2 * verticalGap;
		[TweakValue("0_MY", 400f, 1000f)]
		static float height = 500f;

		public override Vector2 InitialSize => new Vector2(width, height);

		// Mod selection
		// Static fields, so the window will save the selected items between sessions
		private static string selectedModId = "ludeon.rimworld";
		private static string selectedModName = "Core";
		private static Vector2 scrollPositionModSelect = Vector2.zero;

		// Category selection
		// For now only buildings
		private static ThingCategory category = ThingCategory.Building;

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
		}

		

		//string search = "";
		Vector2 scrollPos = Vector2.zero;

		BuildingsLister buildingsArea = new BuildingsLister(selectedModId);

		public override void DoWindowContents(Rect inRect)
		{
			if (isClosed) return;	// If PostClose() Method has been executed already, this window must be closed.

			float curY = 0f;

			// Mod selection
			GUI_Utils.LabelWithSelection(inRect, curY, "Mod:", selectedModName, "Select mod",
				new Dialog_ModSelector
					(
						(modId, modName) =>
						{
							selectedModId = modId;
							selectedModName = modName;
							buildingsArea.RebuildCache(selectedModId);
						},
						scrollPositionModSelect,
						newScroll =>
						{
							scrollPositionModSelect = newScroll;
						}
					)
				);
			curY += GUI_Utils.rowHeight;

			// Item selection - TMP, TODO
			var tmpLabelRect1 = new Rect(0f, curY, GUI_Utils.labelWidth, GUI_Utils.rowHeight);
			Widgets.Label(tmpLabelRect1, "Type: ");
			var tmpSelectionRect1 = new Rect(tmpLabelRect1.width, curY, inRect.width - tmpLabelRect1.width, GUI_Utils.rowHeight);
			Widgets.DrawHighlightIfMouseover(tmpSelectionRect1);
			Widgets.Label(tmpSelectionRect1, "x Buildings");
			Widgets.DrawBox(tmpSelectionRect1);
			curY += GUI_Utils.rowHeight;

			//Main area. Split into 3 columns
			Rect mainAreaRect = new Rect(0f, curY, inRect.width, inRect.height - curY);
			Widgets.DrawBox(mainAreaRect);

			Rect leftColumnRect = new Rect(0f, mainAreaRect.y, leftColumnWidth, mainAreaRect.height);
			Rect middleColumnRect = new Rect(leftColumnRect.xMax + verticalGap, mainAreaRect.y, middleColumnWidth, mainAreaRect.height);
			Rect rightDolumnRect = new Rect(middleColumnRect.xMax + verticalGap, mainAreaRect.y, rightColumnWidth, mainAreaRect.height);

			Widgets.DrawBox(leftColumnRect);
			Widgets.DrawBox(middleColumnRect);
			Widgets.DrawBox(rightDolumnRect);


			// Left column. List of buildings
			buildingsArea.Draw(leftColumnRect);


			
			
			
			Widgets.Label(middleColumnRect, middleColumnRect.xMax.ToString());
			Widgets.Label(rightDolumnRect, rightDolumnRect.xMax.ToString());
		}
	}
}
