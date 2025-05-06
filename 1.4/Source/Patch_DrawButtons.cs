using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Foxy.QuickGive {
	[HarmonyPatch("Verse.DebugWindowsOpener", "DrawButtons")]
	internal static class Patch_DrawButtons {
		private static readonly MethodInfo methodWidgetRowInit = AccessTools.Method("Verse.WidgetRow:Init");
		private static readonly FieldInfo fieldWidgetRow = AccessTools.Field("Verse.DebugWindowsOpener:widgetRow");
		private static readonly MethodInfo methodInjection = AccessTools.Method("Foxy.QuickGive.Patch_DrawButtons:Injection");

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if (methodWidgetRowInit == null) {
				Logger.Error("Method Verse.WidgetRow:Init not found");
				return instructions;
			}
			if (fieldWidgetRow == null) {
				Logger.Error("Field Verse.DebugWindowsOpener:widgetRow not found");
				return instructions;
			}
			if (methodInjection == null) {
				Logger.Error("Method Foxy.QuickGive.Patch_DrawButtons:Injection not found");
				return instructions;
			}

			var list = new List<CodeInstruction>(instructions);

			int index = list.FindIndex(x => x.Calls(methodWidgetRowInit));
			if (index < 0) {
				Logger.Error("Failed to find injection point.");
				return instructions;
			}

			list.Insert(++index, new CodeInstruction(OpCodes.Ldarg_0));
			list.Insert(++index, new CodeInstruction(OpCodes.Ldfld, fieldWidgetRow));
			list.Insert(++index, new CodeInstruction(OpCodes.Call, methodInjection));

			return list;
		}

		private static void Injection(WidgetRow row) {
			if (Current.ProgramState != ProgramState.Playing) return;
			Dialog_QuickGive dlg = Find.WindowStack.WindowOfType<Dialog_QuickGive>();
			string message = dlg == null ? Resources.strOpenMenu : Resources.strCloseMenu;
			if (!row.ButtonIcon(Resources.texOpenDebugGiveMenu, message)) return;

			if (dlg == null) {
				Find.WindowStack.Add(new Dialog_QuickGive());
			} else {
				dlg.Close();
			}
		}
	}
}
