using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Verse;

namespace ResearchConnector
{
	public class ExportXML
	{
		private const string _className = nameof(ExportXML);
		private static string OutputDir =>
			Path.Combine(GenFilePaths.SaveDataFolderPath, "DevOutput", "ResearchConnector", "Export");

		private const string OutputFileName = "Patch_Research.xml";

		// Cache for DefDatabase methods to avoid reflection overhead.
		private static readonly Dictionary<string, MethodInfo> _defDatabaseMethods = new Dictionary<string, MethodInfo>();

		public static void GenerateAll()
		{
#if DEBUG
			Logger.LogNL($"[{_className}] Generating XML for all actions (NewActionsLogger).");
#endif
			if (!ActionsLogger.HasItems)
			{
				Logger.LogNL($"[{_className}] Nothing to export.");
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
				// e.Item is DefKey, e.Research is ResearchKey
				var defTypeName = e.Item.DefType;       // e.g. "Verse.ThingDef"
														//var defName = e.Item.DefName;           // e.g. "Bed"
				var researchName = e.Research.ResearchDefName; // e.g. "Smithing"
				var legacy = e.Research.Legacy;

#if DEBUG
				Logger.LogNL($"[{_className}] {e.Item.DefName}[{e.Item.DefType}] Action: {e.Action}[{researchName}]" + (legacy ? "{legacy}" : ""));
#endif

				// Resolve the method to get the Def by name.
				if (!_defDatabaseMethods.TryGetValue(defTypeName, out var method))
				{
					// 1) Resolve def type (e.g., string "Verse.ThingDef" -> typeof(Verse.ThingDef))
					var defType = AccessTools.TypeByName(defTypeName);
					if (defType == null)
					{
						Logger.LogNL($"[{_className}] WARNING: Cannot resolve type '{defTypeName}'. Def[{e.Item.DefName}]");
						Verse.Log.Error($"[{ResearchConnector.modName}: {_className}] WARNING: Cannot resolve type '{defTypeName}'. Def[{e.Item.DefName}]");
						continue;
					}
					// 2) Get the method to get the Def by name (e.g., "GetNamed" for ThingDef, RecipeDef, etc.)
					method = GetMethodToGetDefByName(defType, e.Item.DefName);
					_defDatabaseMethods[defTypeName] = method;
#if DEBUG
					Logger.LogNL($"[{_className}] New DB method[{method}] for type[{defType.Name}]");
#endif
				}

				// If we have the method, invoke it to get the Def.
				var targetDef = method?.Invoke(null, new object[] { e.Item.DefName, true }) as Def;
				var researchDef = DefDatabase<ResearchProjectDef>.GetNamed(researchName);

				if (targetDef == null || researchDef == null)
				{
					Logger.LogNL($"[{_className}] WARNING: Cannot find Def[{e.Item.DefName}] or ResearchDef[{researchName}]. Skipping.");
					continue;
				}

				// 3) Export XML
				var defTypeXML = targetDef.GetType().Name;      // "ThingDef", "RecipeDef", ...
				var defName = targetDef.defName;
				var research = researchDef.defName;              // string literal for XML

				// <researchPrerequisites Inherit="False"> required for child nodes!!!

				switch (e.Action)
				{
					case ActionsLogger.ResearchAction.Add:
						{
							if (legacy)
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
					case ActionsLogger.ResearchAction.Remove:
						{
							if (legacy)
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
					case ActionsLogger.ResearchAction.None:
					default:
						break;
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
			Logger.LogNL($"[{_className}] Exported: {path}");
#endif
		}

		public static void ListRaw() => ActionsLogger.ListAll();

		// --- Helpers ----------------------------------------------------------

		private static MethodInfo GetMethodToGetDefByName(Type defType, string defName)
		{
			if (defType == null || string.IsNullOrEmpty(defName)) return null;

			// defType is typeof(Verse.ThingDef), typeof(Verse.RecipeDef), etc.
			var dbType = typeof(DefDatabase<>).MakeGenericType(defType);

			// Now dbType is DefDatabase<ThingDef>, DefDatabase<RecipeDef>, etc.
			// We cannot use it as Database directly, but we can use reflection to get and invoke methods.

			// Get the method GetNamed to call it later.
			var method = dbType.GetMethod("GetNamed", BindingFlags.Public | BindingFlags.Static);   // Public static method

			if (method != null)
			{
				return method;
			}

			Logger.LogNL($"[{_className}] WARNING: Could not reflect GetNamed on DefDatabase<{defType.Name}>.");
			Verse.Log.Error($"[{ResearchConnector.modName}: {_className}] WARNING: Could not reflect GetNamed on DefDatabase<{defType.Name}>.");
			return null;
		}
	}
}
