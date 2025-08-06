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
		public override Vector2 InitialSize => new Vector2(600f, 500f);

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

		private string selectedModId = null;
		private string selectedModName = null;
		private Vector2 scrollPositionModSelect = Vector2.zero;

		[TweakValue("0_MY", 10f, 40f)]
		const float rowHeight = 22f;
		[TweakValue("0_MY", 50, 150)]
		const float labelsWidth1 = 80f;

		public override void DoWindowContents(Rect inRect)
		{
			float curY = 0f;

			// Mod selection
			GUI_Utils.LabelWithSelection(inRect, curY, "Mod:", selectedModName, "Select mod",
				new Dialog_ModSelector
					(
						(modId, modName) =>
						{
							selectedModId = modId;
							selectedModName = modName;
						},
						scrollPositionModSelect,
						newScroll =>
						{
							scrollPositionModSelect = newScroll;
						}
					)
				);
			curY += GUI_Utils.rowHeight;

			// Item selection

		}

		
	}
}
