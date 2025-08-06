using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchConnector
{
	public static class Utils
	{
		/// <summary>
		/// Contains with StringComparer Property
		/// </summary>
		/// <param name="source"></param>
		/// <param name="toCheck"></param>
		/// <param name="comp"></param>
		/// <returns></returns>
		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			return source?.IndexOf(toCheck, comp) >= 0;
		}

		public static void Log(string str)
		{
#if DEBUG
			Verse.Log.Message("[ResearchConnector] " + str);
#endif
		}
	}
}
