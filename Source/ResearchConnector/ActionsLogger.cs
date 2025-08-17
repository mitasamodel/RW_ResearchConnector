using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchConnector
{
	public class ActionsLogger
	{
		//================== Suggested model

		// Key for Defs: "Verse.ThingDef::Bed"
		public readonly struct DefKey : IEquatable<DefKey>
		{
			public readonly string DefType; // e.g., "Verse.ThingDef", "Verse.RecipeDef"
			public readonly string DefName; // e.g., "Bed"

			public DefKey(Def def)
				: this(def.GetType().FullName, def.defName) { }
			public DefKey(string defType, string defName)
			{
				DefType = defType ?? "";
				DefName = defName ?? "";
			}

			public bool Equals(DefKey other) =>
				string.Equals(DefType, other.DefType, StringComparison.Ordinal) &&
				string.Equals(DefName, other.DefName, StringComparison.Ordinal);

			public override bool Equals(object obj) => obj is DefKey dk && Equals(dk);

			public override int GetHashCode()
				=> (DefType, DefName).GetHashCode(); // .NET Framework 4.8

			public override string ToString() => $"{DefType}::{DefName}";
		}

		// Key for Research: "ResearchDefName" or "ResearchDefName#legacy"
		public readonly struct ResearchKey : IEquatable<ResearchKey>
		{
			public readonly string ResearchDefName;
			public readonly bool Legacy;

			public ResearchKey(ResearchProjectDef researchDef, bool legacy = false)
				: this(researchDef?.defName, legacy) { }
			public ResearchKey(string researchDefName, bool legacy = false)
			{
				ResearchDefName = researchDefName ?? "";
				Legacy = legacy;
			}

			public bool Equals(ResearchKey other) =>
				Legacy == other.Legacy &&
				string.Equals(ResearchDefName, other.ResearchDefName, StringComparison.Ordinal);

			public override bool Equals(object obj) => obj is ResearchKey rk && Equals(rk);

			public override int GetHashCode()
				=> (ResearchDefName, Legacy).GetHashCode();

			public override string ToString() => Legacy ? $"{ResearchDefName}#legacy" : ResearchDefName;
		}

		// Action
		public enum ResearchAction
		{
			None,
			Add,
			Remove,
		}

		//================= The store

		// Top-level: one bucket per (DefType::DefName)
		private static readonly Dictionary<DefKey, Dictionary<ResearchKey, ResearchAction>> _actionsLog
			= new Dictionary<DefKey, Dictionary<ResearchKey, ResearchAction>>();

		// Always create inner maps with Ordinal
		private static Dictionary<ResearchKey, ResearchAction> NewInner()
			=> new Dictionary<ResearchKey, ResearchAction>();

		// Get or create Dict
		private static Dictionary<ResearchKey, ResearchAction> GetOrCreate(DefKey key)
		{
			if (!_actionsLog.TryGetValue(key, out var dict))
			{
				dict = NewInner();      // Create new inner Dictionary
				_actionsLog[key] = dict;
			}
			return dict;
		}

		private static ResearchKey RK(ResearchProjectDef researchDef, bool legacy = false)
			=> new ResearchKey(researchDef, legacy);
		private static ResearchKey RK(string researchDefName, bool legacy = false)
			=> new ResearchKey(researchDefName, legacy);


		//================= Logging API

		public static void Add(Def itemDef, ResearchProjectDef resDef, bool legacy = false)
		{
			// Inner dict - dict per itemDef
			var perItemDict = GetOrCreate(new DefKey(itemDef)); // Get or create inner Dictionary

			// ResearchKey for the action
			var rkey = RK(resDef, legacy);

			// Reduce: pass "old" value if exists, or "default" (first from struct -> "None") if not. Compare with new velue.
			var action = Reduce(perItemDict.TryGetValue(rkey, out var old) ? old : default, ResearchAction.Add);

			// If action is None, remove the entry from the dictionary (cancelled out)
			if (action == ResearchAction.None)
				perItemDict.Remove(rkey);
			else
				perItemDict[rkey] = action;
		}

		public static void Remove(Def itemDef, ResearchProjectDef resDef, bool legacy = false)
		{
			var perItemDict = GetOrCreate(new DefKey(itemDef));
			var rkey = RK(resDef, legacy);
			var action = Reduce(perItemDict.TryGetValue(rkey, out var old) ? old : default, ResearchAction.Remove);
			if (action != ResearchAction.None)
				perItemDict[rkey] = action;
			else
				perItemDict.Remove(rkey);
		}

		private static ResearchAction Reduce(ResearchAction prev, ResearchAction next)
		{
			if (prev == ResearchAction.Add && next == ResearchAction.Remove)
				return ResearchAction.None;

			if (prev == ResearchAction.Remove && next == ResearchAction.Add)
				return ResearchAction.None;

			if (prev == next)
				return prev;

			return next;
		}
		public static IEnumerable<(DefKey Item, Dictionary<ResearchKey, ResearchAction> Actions)> EnumerateDefs()
		{
			foreach (var item in _actionsLog)
				yield return (item.Key, item.Value);
		}
		public static IEnumerable<(DefKey Item, ResearchKey Research, ResearchAction Action)> EnumerateAll()
		{
			foreach (var item in _actionsLog)
				foreach (var kv in item.Value)
					yield return (item.Key, kv.Key, kv.Value);
		}
		public static IEnumerable<(ResearchKey Research, ResearchAction Action)> GetFor(Def def)
		{
			var key = new DefKey(def);
			if (_actionsLog.TryGetValue(key, out var perItem))
				foreach (var kv in perItem)
					yield return (kv.Key, kv.Value);
		}
		public static void Clear() => _actionsLog.Clear();

		public static void ListAll()
		{
			foreach ( var action in _actionsLog)
			{
				var itemKey = action.Key;
				foreach (var kv in action.Value)
				{
					var researchKey = kv.Key;
					var actionType = kv.Value;
					Logger.LogNL($"Item[{itemKey}] Research[{researchKey}] Action[{actionType}]");
				}
			}
		}
		public static bool HasItems => _actionsLog.Count > 0;
	}
}
