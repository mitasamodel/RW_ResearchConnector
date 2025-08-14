using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchConnector
{
	public static class Logger
	{
		private static readonly Dictionary<string, bool> _oneTimeLogs = new Dictionary<string, bool>();

		public static void LogOnce(string str)
		{
#if DEBUG
			if (_oneTimeLogs.ContainsKey(str)) return;
			_oneTimeLogs[str] = true;
			LogNL(str);
#endif
		}
		public static void Log(string str)
		{
#if DEBUG
			File.AppendAllText(ResearchConnector.logFile, str);
#endif
		}
		public static void LogNL(string str)
		{
#if DEBUG
			Log(str + "\n");
#endif
		}
	}
}
