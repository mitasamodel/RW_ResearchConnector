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
		//private static readonly string _className = nameof(ActionLogger);

		// Key is (thingDefName, researchDefName)
		private readonly struct Key : IEquatable<Key>
		{
			public readonly string type;
			public readonly string def;
			public readonly string research;

			public Key(string type, string def, string research)
			{
				this.type = type ?? "";
				this.def = def ?? "";
				this.research = research ?? "";
			}

			public bool Equals(Key other) => type == other.type && def == other.def && research == other.research;
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
					return h;
				}
			}
			public override string ToString() => $"{type}::{def}::{research}";
		}

		// Keep values only in {-1, +1}. When a transition hits 0, we remove the entry.
		private static readonly Dictionary<Key, sbyte> _net = new Dictionary<Key, sbyte>();

		public static void Clear() => _net.Clear();

		public static void Add(Def def, ResearchProjectDef research)
		{
			if (def == null || research == null) return;
			var k = new Key(def.GetType().FullName, def.defName, research.defName);

			if (_net.TryGetValue(k, out var cur))
			{
				if (cur == -1)
				{
					_net.Remove(k); // remove cancels pending remove -> zero
				}
				else
				{
					_net[k] = 1;              // remains +1
				}
			}
			else
			{
				_net[k] = 1;
			}

		}

		public static void Remove(Def def, ResearchProjectDef research)
		{
//#if DEBUG
//			Utils.Log($"[{_className}] Remove [{research?.defName}] from [{thing?.defName}]: ");
//#endif
			if (def == null || research == null) return;
			var k = new Key(def.GetType().FullName, def.defName, research.defName);

			if (_net.TryGetValue(k, out var cur))
			{
				if (cur == 1)
				{
					_net.Remove(k);  // remove cancels pending add -> zero
//#if DEBUG
//					Utils.LogNL("[]");
//#endif
				}
				else
				{
					_net[k] = -1;             // remains -1
//#if DEBUG
//					Utils.LogNL(_net[k].ToString());
//#endif
				}
			}
			else
			{
				_net[k] = -1;
//#if DEBUG
//				Utils.LogNL(_net[k].ToString());
//#endif
			}
		}
		public struct Entry
		{
			public string Type;
			public string ThingDefName;      // e.g. "Steel_LongSword"
			public string ResearchDefName;   // e.g. "Smithing"
			public sbyte Delta;             // +1 add, -1 remove
		}

		/// <summary>True if anything still needs exporting.</summary>
		public static bool HasItems => _net.Count > 0;

		/// <summary>Enumerate net actions to be turned into PatchOperations.</summary>
		public static IEnumerable<Entry> Enumerate()
		{
			foreach (var kv in _net)
				yield return new Entry { Type = kv.Key.type, ThingDefName = kv.Key.def, ResearchDefName = kv.Key.research, Delta = kv.Value };
		}

		public static void ListAll()
		{
#if DEBUG
			foreach (var kv in _net)
			{
				Utils.LogNL($"{kv.Key}[{kv.Value}]");
			}
#endif
		}
	}
}
