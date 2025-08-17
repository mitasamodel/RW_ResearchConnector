using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Verse;

namespace ResearchConnector
{
	public static class XMLExtensions
	{
		/// <summary>
		/// Fill in the list of research prerequisites.
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="defPath"></param>
		/// <param name="researchDefs"></param>
		public static void XMLDoc_AddResearchPrerequisites(this XDocument doc, string defPath, List<ResearchProjectDef> researchDefs)
		{
			doc.XMLDoc_StartIfNeeded();
			doc.Root!.Add(
				new XElement("Operation",
					new XAttribute("Class", "PatchOperationAdd"),
					new XElement("xpath", $"{defPath}/researchPrerequisites"),
					new XElement("value",
						researchDefs.Select(r => new XElement("li", r.defName))
					)
				)
			);
		}

		/// <summary>
		/// Clear prerequisites and unbind them from parent.
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="defType"></param>
		/// <param name="defName"></param>
		public static void XMLDoc_ClearPrerequisites(this XDocument doc, string defPath)
		{
			doc.XMLDoc_StartIfNeeded();
			doc.Root!.Add(
				new XElement("Operation",
					new XAttribute("Class", "PatchOperationConditional"),
					new XElement("xpath", $"{defPath}/researchPrerequisite"),
					new XElement("match",
						new XAttribute("Class", "PatchOperationReplace"),
						new XElement("xpath", $"{defPath}/researchPrerequisite"),
						new XElement("value",
							new XElement("researchPrerequisite", new XAttribute("Inherit", "False"))
						)
					)
				)
			);
			doc.Root!.Add(
				new XElement("Operation",
					new XAttribute("Class", "PatchOperationConditional"),
					new XElement("xpath", $"{defPath}/researchPrerequisites"),
					new XElement("match",
						new XAttribute("Class", "PatchOperationReplace"),
						new XElement("xpath", $"{defPath}/researchPrerequisites"),
						new XElement("value",
							new XElement("researchPrerequisites", new XAttribute("Inherit", "False"))
						)
					),
					new XElement("nomatch",
						new XAttribute("Class", "PatchOperationAdd"),
						new XElement("xpath", $"{defPath}"),
						new XElement("value",
							new XElement("researchPrerequisites", new XAttribute("Inherit", "False")))
					)
				)
			);
		}

		public static void XMLDoc_StartIfNeeded(this XDocument doc)
		{
			doc.Declaration ??= new XDeclaration("1.0", "utf-8", "yes");

			if (doc.Root == null)
				doc.Add(new XElement("Patch"));
			else if (doc.Root.Name != "Patch")
				Logger.LogError(nameof(XMLExtensions), $"Expected root <Patch>, got <{doc.Root.Name}>.");
		}

		public static void XMLDoc_MergeFrom(this XDocument target, XDocument source)
		{
			target.XMLDoc_StartIfNeeded();
			source.XMLDoc_StartIfNeeded();

			// Move (clone) children from source root into target root
			var payload = source.Root!.Elements().ToList();
			if (payload.Count > 0)
				target.Root!.Add(payload);
		}

		public static void XML_AppendPatchSmart(this XDocument newDoc, string path)
		{
			XDocument doc;

			if (File.Exists(path))
			{
				// Load existing file and keep its declaration/standalone as-is
				using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				doc = XDocument.Load(fs, LoadOptions.SetLineInfo);
			}
			else
			{
				doc = new XDocument();
			}

			// Ensure single <Patch/> root
			doc.XMLDoc_StartIfNeeded();

			// Build new operations directly on the existing doc
			doc.XMLDoc_MergeFrom(newDoc);

			doc.XML_SaveToFile(path);
		}

		public static void XML_SaveToFile(this XDocument doc, string path)
		{
			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = false,
				Indent = true,
				IndentChars = "  ",                 // 2 spaces (pick what you like)
				NewLineChars = Environment.NewLine,
				NewLineHandling = NewLineHandling.Replace
			};

			using var ofs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			using var sw = new StreamWriter(ofs, new System.Text.UTF8Encoding(false));
			using var xw = XmlWriter.Create(sw, settings);
			doc.Save(xw);  // uniform pretty output
		}
	}
}
