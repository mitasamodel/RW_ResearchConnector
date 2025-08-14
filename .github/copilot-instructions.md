# General information
This is a mod for the game Rimworld. Version 1.6.
It is written in C#, targeting LangVersion 8.0 and FrameworkVersion 4.8. Notify if it does not match the project's settings.
External references should not be included in the assembly output. Suggest a solution, such as creating a "Directory.Build.props" file.
Check naming against the C# naming conventions. Prefer the official convention over the Rimworld-specific one.
Some parts may be written using XML. Both direct definitions and Rimworld-specific patches allowed.

# Mod-specific information
This mod is a UI mod.
Some code sections may be executed every game frame.
If you detect potentially low-performance code in the frame rendering area, notify about it.
Identify rendering areas by inspecting surrounding code for typical instructions, such as "Widgets.Label" or other functions from RimWorld or Unity.
Provide a performance comparison for different enumerable types in two scenarios:
- Low element count (< 20 elements)
- High element count (>= 2000 elements)
