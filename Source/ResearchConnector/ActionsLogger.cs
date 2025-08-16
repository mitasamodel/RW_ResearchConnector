using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace ResearchConnector
{
	/// <summary>
	/// Net-effect of user actions per (ThingDef, ResearchProjectDef).
	/// +1 => ResearchProjectDef will be ADDED
	/// -1 => ResearchProjectDef will be REMOVED
	///  0 => no net change (pair cancelled)
	/// </summary>
	internal static class ActionLogger
	{
		public enum ActionType
		{
			Add,
			Remove,
		}
		//private static readonly string _className = nameof(ActionLogger);

		// Key is (thingDefName, researchDefName)
		private readonly struct Key : IEquatable<Key>
		{
			public readonly string type;
			public readonly string def;
			public readonly string research;
			public readonly bool legacy;

			public Key(string type, string def, string research, bool legacy)
			{
				this.type = type ?? "";
				this.def = def ?? "";
				this.research = research ?? "";
				this.legacy = legacy;
			}

			public bool Equals(Key other) =>
				type == other.type &&
				def == other.def &&
				research == other.research &&
				legacy == other.legacy;
			public override bool Equals(object obj) => obj is Key k && Equals(k);
			// Magical for me Method to generate hash code. Suggested by chatGPT
			// More reading required. https://ericlippert.com/2011/02/28/guidelines-and-rules-for-gethashcode/
			public override int GetHashCode()
			{
				unchecked
				{
					int h = 17;
					h = h * 31 + (type?.GetHashCode() ?? 0);
					h = h * 31 + (def?.GetHashCode() ?? 0);
					h = h * 31 + (research?.GetHashCode() ?? 0);
					h = h * 31 + legacy.GetHashCode();
					return h;
				}
			}
			public override string ToString() => $"{type}::{def}::{research}::" + (legacy ? "{legacy}" : "");
		}

		// Keep action list in a dictionary.
		private static readonly Dictionary<Key, ActionType> _net = new Dictionary<Key, ActionType>();

		public static void Clear() => _net.Clear();

		public static void Add(Def def, ResearchProjectDef research, bool legacy = false)
		{
			if (def == null || research == null) return;

			// Normal key
			var k1 = new Key(def.GetType().FullName, def.defName, research.defName, legacy);
			// Legacy-reversed key
			//var k2 = new Key(def.GetType().FullName, def.defName, research.defName, !legacy);

			if (_net.TryGetValue(k1, out var cur))
			{
				if (cur == ActionType.Remove)
				{
					_net.Remove(k1);    // cancels out
				}
				else
				{
					_net[k1] = ActionType.Add;      // keep the same
				}
			}
			else
			{
				_net[k1] = ActionType.Add;
			}
		}

		public static void Remove(Def def, ResearchProjectDef research, bool legacy = false)
		{
			//#if DEBUG
			//			Utils.Log($"[{_className}] Remove [{research?.defName}] from [{thing?.defName}]: ");
			//#endif
			if (def == null || research == null) return;
			var k = new Key(def.GetType().FullName, def.defName, research.defName, legacy);

			if (_net.TryGetValue(k, out var cur))
			{
				if (cur == ActionType.Add)
				{
					_net.Remove(k);
				}
				else
				{
					_net[k] = ActionType.Remove;
				}
			}
			else
			{
				_net[k] = ActionType.Remove;
			}
		}
		public struct Entry
		{
			public string Type;
			public string ThingDefName;      // e.g. "Steel_LongSword"
			public string ResearchDefName;   // e.g. "Smithing"
			public ActionType Action;
			public bool Legacy;              // true if this is a legacy action
		}

		/// <summary>True if anything still needs exporting.</summary>
		public static bool HasItems => _net.Count > 0;

		/// <summary>Enumerate net actions to be turned into PatchOperations.</summary>
		public static IEnumerable<Entry> Enumerate()
		{
			foreach (var kv in _net)
				yield return new Entry
				{
					Type = kv.Key.type,
					ThingDefName = kv.Key.def,
					ResearchDefName = kv.Key.research,
					Legacy = kv.Key.legacy,
					Action = kv.Value,
				};
		}

		public static void ListAll()
		{
#if DEBUG
			Logger.LogNL("Export");
			foreach (var kv in _net)
			{
				Logger.LogNL($"{kv.Key}[{kv.Value}]");
			}
#endif
		}
	}
}
