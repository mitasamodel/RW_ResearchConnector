using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Verse;

namespace ResearchConnector
{
	public class ExportXML
	{
		private static string OutputDir =>
			Path.Combine(GenFilePaths.SaveDataFolderPath, "DevOutput", "ResearchConnector", "Export");

		private const string OutputFileName = "Patch_Research.xml";

		// Chache for DefDatabase methods to avoid reflection overhead.
		private static readonly Dictionary<string, MethodInfo> _defDatabaseMethods = new Dictionary<string, MethodInfo>();

		public static void GenerateAll()
		{
#if DEBUG
			Logger.LogNL("[ExportXML] Generating XML for all actions.");
#endif
			if (!ActionsLogger.HasItems)
			{
				Logger.LogNL("[ExportXML] Nothing to export.");
				return;
			}

			Directory.CreateDirectory(OutputDir);
			var path = Path.Combine(OutputDir, OutputFileName);

			var doc = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XElement("Patch")
			);

			foreach (var e in ActionsLogger.Enumerate())
			{
				// Resolve the method to get the Def by name.
				if (!_defDatabaseMethods.TryGetValue(e.Type, out var method))
				{
					// 1) Resolve def type (e.g., string "Verse.ThingDef" -> typeof(Verse.ThingDef))
					var defType = AccessTools.TypeByName(e.Type);
					if (defType == null)
					{
						Logger.LogNL($"[ExportXML] WARNING: Cannot resolve type '{e.Type}'. Def[{e.DefName}]");
						Verse.Log.Error($"[{ResearchConnector.modName}: ExportXML] WARNING: Cannot resolve type '{e.Type}'. Def[{e.DefName}]");
						continue;
					}
#if DEBUG
					else
						Logger.LogNL($"{e.DefName}[{defType.Name}] Action: {e.Action}[{e.ResearchDefName}]" + (e.Legacy ? "{legacy}" : ""));
#endif
					// 2) Get the method to get the Def by name (e.g., "GetNamed" for ThingDef, RecipeDef, etc.)
					method = GetMethodToGetDefByName(defType, e.DefName);
					_defDatabaseMethods[e.Type] = method;
				}

				// If we have the method, invoke it to get the Def.
				var targetDef = method?.Invoke(null, new object[] { e.DefName, true }) as Def;
				var researchDef = DefDatabase<ResearchProjectDef>.GetNamed(e.ResearchDefName);

				if (targetDef == null || researchDef == null)
				{
					Logger.LogNL($"[ExportXML] WARNING: Cannot find Def[{e.DefName}] or ResearchDef[{e.ResearchDefName}]. Skipping.");
					continue;
				}

				// 3) Export XML
				var defTypeXML = targetDef.GetType().Name;      // "ThingDef", "RecipeDef", ...
				var defName = targetDef.defName;			// defName in XML
				var research = researchDef.defName;			// string literal for XML

				switch (e.Action)
				{
					case ActionsLogger.ActionType.Add:
						{
							if (e.Legacy)
							{
								doc.Root!.Add(
									new XElement("Operation",
										new XAttribute("Class", "PatchOperationAdd"),
										new XElement("xpath", $"Defs/{defTypeXML}[defName=\"{defName}\"]"),
										new XElement("value",
											new XElement("researchPrerequisite", research)
										)
									)
								);
							}
							else
							{
								doc.Root!.Add(
									new XElement("Operation",
										new XAttribute("Class", "PatchOperationAdd"),
										new XElement("xpath", $"Defs/{defTypeXML}[defName=\"{defName}\"]/researchPrerequisites"),
										new XElement("value",
											new XElement("li", research)
										)
									)
								);
								// If parent node is missing, this won't apply. We keep it simple for now.
							}
							break;
						}
					case ActionsLogger.ActionType.Remove:
						{
							if (e.Legacy)
							{
								doc.Root!.Add(
									new XElement("Operation",
										new XAttribute("Class", "PatchOperationRemove"),
										new XElement("xpath",
											$"Defs/{defTypeXML}[defName=\"{defName}\"]/researchPrerequisite[text() = \"{research}\"]")
									)
								);
							}
							else
							{
								doc.Root!.Add(
									new XElement("Operation",
										new XAttribute("Class", "PatchOperationRemove"),
										new XElement("xpath",
											$"Defs/{defTypeXML}[defName=\"{defName}\"]/researchPrerequisites/li[. = \"{research}\"]")
									)
								);
							}
							break;
						}
				}

				// NOTE: We’re not *using* targetDef/researchDef yet for output, but they're ready for:
				// - Grouping files by targetDef?.modContentPack?.PackageId
				// - Adding <MayRequire> if researchDef?.modContentPack is non-vanilla, etc.
			}

			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
			{
				doc.Save(sw);
			}

#if DEBUG
			Logger.LogNL($"[ExportXML] Exported: {path}");
#endif
		}

		public static void ListRaw() => ActionsLogger.ListAll();

		// --- Helpers ----------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="defType"></param>
		/// <param name="defName"></param>
		/// <returns></returns>
		private static MethodInfo GetMethodToGetDefByName(Type defType, string defName)
		{
			if (defType == null || string.IsNullOrEmpty(defName)) return null;

			// defType is typeof(ThingDef), typeof(RecipeDef), etc.
			var dbType = typeof(DefDatabase<>).MakeGenericType(defType);

			// Now dbType is DefDatabase<ThingDef>, DefDatabase<RecipeDef>, etc.
			// We cannot use it as Database directly, but we can use reflection to get and invoke methods.

			// Get the method GetNamed to call it later.
			var method = dbType.GetMethod("GetNamed", BindingFlags.Public | BindingFlags.Static);

			if (method != null)
			{
				return method;
			}

			Logger.LogNL($"[ExportXML] WARNING: Could not reflect GetNamed on DefDatabase<{defType.Name}>.");
			Verse.Log.Error($"[{ResearchConnector.modName}: ExportXML] WARNING: Could not reflect GetNamed on DefDatabase<{defType.Name}>.");
			return null;
		}
	}
}
