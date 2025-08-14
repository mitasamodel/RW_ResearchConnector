using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ResearchConnector
{
	public static class Utils
	{
		/// <summary>
		/// Returns string consisted of spaces
		/// </summary>
		/// <param name="strLength">Original string length</param>
		/// <param name="maxLength">Max length to add up spaces</param>
		/// <returns></returns>
		public static string TabsAfter(int strLength, int maxLength)
		{
			// How many characters remain to the max length
			int diff = maxLength - (strLength + 1);		// +1 due to "]"
			diff = diff < 1 ? 1 : diff;

			return new string(' ', diff);
		}

		/// <summary>
		/// Contains regardless of cAsE
		/// </summary>
		/// <param name="source"></param>
		/// <param name="toCheck"></param>
		/// <returns></returns>
		public static bool ContainsIgnoreCase(this string source, string toCheck)
		{
			return source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		static string Truncate(string text, int maxLength)
		{
			if (string.IsNullOrEmpty(text)) return text;
			return text.Length <= maxLength ? text : text.Substring(0, maxLength);
		}

		public static bool AddIfNotExists<T>(this List<T> list, T item)
		{
			if (list.Contains(item)) return false;
			list.Add(item);
			return true;
		}
	}
}
