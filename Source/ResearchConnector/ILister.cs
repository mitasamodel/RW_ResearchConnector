using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ResearchConnector
{
	public interface ILister
	{
		void Draw(Rect inRect);
		void RebuildCache(string modId);
		Def SelectedDef();
	}
}
