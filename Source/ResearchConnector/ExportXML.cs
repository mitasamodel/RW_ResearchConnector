using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Linq;
using Verse;

namespace ResearchConnector
{
	public static class ExportXML
	{
		private const string _className = nameof(ExportXML);
		private static string OutputDir =>
			Path.Combine(GenFilePaths.SaveDataFolderPath, "DevOutput", "ResearchConnector", "Export");

		// Mods for LoadFolders.xml.
		private static readonly Dictionary<string, string> mods = new Dictionary<string, string>();

		// Cache for DefDatabase methods to avoid reflection overhead.
		private static readonly Dictionary<string, MethodInfo> _defDatabaseMethods = new Dictionary<string, MethodInfo>();

		public static void Do()
		{
			if (Directory.Exists(Path.Combine(OutputDir, "ModPatches")))
				Directory.Delete(Path.Combine(OutputDir, "ModPatches"), true);

			foreach (var (defKey, actions) in ActionsLogger.EnumerateDefs())
			{
				Logger.LogNL($"[{_className}] Def: {defKey.DefName}[{defKey.DefType}]");
				// Resolve Def by strings: type and name.
				Def targetDef = ResolveDef(defKey.DefType, defKey.DefName);
				if (targetDef == null)
				{
					Logger.LogNL($"Def not found: {defKey.DefName} of type {defKey.DefType}");
					continue;
				}

				var mod = (name: targetDef.modContentPack?.Name ?? "0_Unknown", id: targetDef.modContentPack?.PackageId ?? "unknown");
				mods[mod.id] = mod.name;

				ResearchProjectDef legacy = targetDef.GetLegacyResearchPrerequisite();
				List<ResearchProjectDef> researchDefs = targetDef.GetOrInitResearchPrerequisitesList();

				// DevOutput/ResearchConnector/Export/ModPatches/Core/Patches/Core
				string outputPath = Path.Combine(OutputDir, "ModPatches", mod.name, "Patches", mod.name);
				Directory.CreateDirectory(outputPath);
				string fileName = $"ResearchPatch_{targetDef.GetType().Name}.{targetDef.defName}.xml";
				var path = Path.Combine(outputPath, fileName);

				var doc = new XDocument();
				string defPath = $"Defs/{targetDef.GetType().Name}[defName=\"{targetDef.defName}\"]";
				doc.XMLDoc_StartIfNeeded();
				if (targetDef.GetLegacyResearchPrerequisite() == null)
					doc.XMLDoc_ClearLegacyPrerequisite(defPath);
				else
					doc.XMLDoc_SetLegacyPrerequisite(defPath, legacy);
				doc.XMLDoc_ClearPrerequisitesList(defPath);
				if (researchDefs.Count > 0)
					doc.XMLDoc_AddResearchPrerequisites(defPath, researchDefs);

				doc.XML_SaveToFile(path);
				Logger.LogNL($"[{_className}] Exported: {path}");

				//if (legacy != null)
				//{
				//	targetDef.SetLegacyResearchPrerequisite(null); // Clear legacy prerequisite to avoid duplication
				//	researchDefs.Insert(0, legacy);
				//}
				//GenerateXML_DirectList(targetDef, researchDefs);
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
						$"ModPatches/{kv.Value}"
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
			if (def.GetLegacyResearchPrerequisite() == null)
				doc.XMLDoc_ClearLegacyPrerequisite(defPath);
			doc.XMLDoc_ClearPrerequisitesList(defPath);
			if (researchDefs.Count > 0)
				doc.XMLDoc_AddResearchPrerequisites(defPath, researchDefs);

			doc.XML_SaveToFile(path);
			Logger.LogNL($"[{_className}] Exported: {path}");
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