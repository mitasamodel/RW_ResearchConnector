using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ResearchConnector
{
	public static class Utils
	{
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

		public static void Log(string str)
		{
#if DEBUG
			File.AppendAllText(ResearchConnector.logFile, str + "\n");
#endif
		}
	}
}
