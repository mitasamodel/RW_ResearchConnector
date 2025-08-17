using HarmonyLib;
using RimWorld;
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
	public static class ExportXML
	{
		private const string _className = nameof(ExportXML);
		private static string OutputDir =>
			Path.Combine(GenFilePaths.SaveDataFolderPath, "DevOutput", "ResearchConnector", "Export");

		private const string OutputFileName = "Patch_Research.xml";

		private static Dictionary<string, string> mods = new Dictionary<string, string>();

		// Cache for DefDatabase methods to avoid reflection overhead.
		private static readonly Dictionary<string, MethodInfo> _defDatabaseMethods = new Dictionary<string, MethodInfo>();

		public static void Do()
		{
			if (Directory.Exists(Path.Combine(OutputDir, "ModPatches")))
				Directory.Delete(Path.Combine(OutputDir, "ModPatches"), true);

			foreach (var (defKey, actions) in ActionsLogger.EnumerateDefs())
			{
				Logger.LogNL($"[{_className}] Def: {defKey.DefName}[{defKey.DefType}]");
				Def targetDef = ResolveDef(defKey.DefType, defKey.DefName);
				if (targetDef == null)
				{
					Logger.LogNL($"Def not found: {defKey.DefName} of type {defKey.DefType}");
					continue;
				}

				ResearchProjectDef legacy = targetDef.GetLegacyResearchPrerequisite();
				List<ResearchProjectDef> researchDefs = targetDef.GetOrInitResearchPrerequisitesList();
				if (legacy != null)
					researchDefs.Insert(0, legacy);
				GenerateXML_DirectList(targetDef, researchDefs);
			}
			Generate_LoadFolders();
		}

		private static void Generate_LoadFolders()
		{
			string path = Path.Combine(OutputDir, "LoadFolders.xml");
			var doc = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XElement("loadFolders")
			);

			var velem = new XElement("v1.6");
			velem.Add(new XElement("li", "/"));
			foreach (var kv in mods.OrderBy(m => m.Value))
			{
				velem.Add(new XElement("li",
						new XAttribute("IfModActive", kv.Key),
						kv.Value
					)
				);
			}

			doc.Root!.Add(velem);
			doc.XML_SaveToFile(path);
		}

		/// <summary>
		/// Def will have direct researchPrerequisites List. Undependent from parent.
		/// </summary>
		/// <param name="def"></param>
		/// <param name="researchDefs"></param>
		public static void GenerateXML_DirectList(Def def, List<ResearchProjectDef> researchDefs)
		{
			if (def == null || researchDefs == null)    // researchDefs.Count can be 0 -> no prerequisites
			{
				Logger.LogNL($"[{_className}] GenerateXML_DirectList: No Def or ResearchDefs to export.");
				return;
			}
			Logger.LogNL($"[{_className}] Exporting Def: {def.defName} with {researchDefs.Count} research prerequisites.");

			var mod = (name: def.modContentPack?.Name ?? "0_Unknown", id: def.modContentPack?.PackageId ?? "unknown");
			mods[mod.id] = mod.name;

			// DevOutput/ResearchConnector/Export/ModPatches/Core/Patches/Core
			string outputPath = Path.Combine(OutputDir, "ModPatches", mod.name, "Patches", mod.name);
			Directory.CreateDirectory(outputPath);
			string fileName = $"ResearchPatch_{def.GetType().Name}.{def.defName}.xml";

			var path = Path.Combine(outputPath, fileName);

			var doc = new XDocument();
			string defPath = $"Defs/{def.GetType().Name}[defName=\"{def.defName}\"]";
			doc.XMLDoc_StartIfNeeded();
			doc.XMLDoc_ClearPrerequisites(defPath);
			if (researchDefs.Count > 0)
				doc.XMLDoc_AddResearchPrerequisites(defPath, researchDefs);

			doc.XML_SaveToFile(path);
			Logger.LogNL($"[{_className}] Exported: {path}");
		}

		public static void GenerateAll()
		{
#if DEBUG
			Logger.LogNL($"[{_className}] Generating XML for all actions.");
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

			foreach (var e in ActionsLogger.EnumerateAll())
			{
				var researchName = e.Research.ResearchDefName;
				var legacy = e.Research.Legacy;
#if DEBUG
				Logger.LogNL($"[{_className}] {e.Item.DefName}[{e.Item.DefType}] Action: {e.Action}[{researchName}]" + (legacy ? "{legacy}" : ""));
#endif

				Def targetDef = ResolveDef(e.Item.DefType, e.Item.DefName);
				ResearchProjectDef resDef = DefDatabase<ResearchProjectDef>.GetNamed(e.Research.ResearchDefName);

				if (targetDef == null || resDef == null)
				{
					Logger.LogNL($"[{_className}] WARNING: Cannot find Def[{e.Item.DefName}] or ResearchDef[{researchName}]. Skipping.");
					continue;
				}

				var defTypeXML = targetDef.GetType().Name;      // "ThingDef", "RecipeDef", ...
				var defName = targetDef.defName;
				var research = resDef.defName;              // string literal for XML

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

		private static MethodInfo ResolveGetNamedMethod(string defTypeName)
		{
			// Check the cache first.
			if (!_defDatabaseMethods.TryGetValue(defTypeName, out MethodInfo method))
			{
				// Resolve def type (e.g., string "Verse.ThingDef" -> typeof(Verse.ThingDef)).
				Type defType = AccessTools.TypeByName(defTypeName);
				if (defType == null)
				{
					Logger.LogError(_className, $"[ResolveGetNamedMethod] WARNING: Cannot resolve type '{defTypeName}'.");
					return null;
				}

				// defType is typeof(Verse.ThingDef), typeof(Verse.RecipeDef), etc.
				var dbType = typeof(DefDatabase<>).MakeGenericType(defType);

				// Now dbType is DefDatabase<ThingDef>, DefDatabase<RecipeDef>, etc.
				// We cannot use it as Database directly, but we can use reflection to get and invoke methods.

				// Get the method GetNamed to call it later.
				method = dbType.GetMethod("GetNamed", BindingFlags.Public | BindingFlags.Static);   // Public static method

				if (method == null)
				{
					Logger.LogError(_className, $"[ResolveGetNamedMethod] WARNING: Could not find GetNamed method on DefDatabase<{defType.Name}>.");
					return null;
				}

				_defDatabaseMethods[defTypeName] = method;
				Logger.LogNL($"[{_className}] New DB method[{method}] for type[{defType.Name}].");
			}

			return method;
		}

		/// <summary>
		/// Resolves a Def by its type name and name.
		/// </summary>
		/// <param name="defTypeName">string: "Verse.ThingDef"</param>
		/// <param name="defName">string: "Bed"</param>
		/// <returns></returns>
		private static Def ResolveDef(string defTypeName, string defName)
		{
			// Resolve GetNamed method for the specific def type
			MethodInfo method = ResolveGetNamedMethod(defTypeName);
			if (method == null) return null;

			// Resolve Def using the method for this defType
			Def def = method?.Invoke(null, new object[] { defName, true }) as Def;
#if DEBUG
			if (def == null)
				Logger.LogNL($"[{_className}: ResolveDef] Failed to resolve def for Type[{defTypeName}], Name[{defName}]");
#endif
			return def;
		}
	}
}