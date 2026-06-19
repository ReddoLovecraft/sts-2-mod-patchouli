using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization.Formatters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TH_Patchouli.Relics;

namespace TH_Patchouli.Scripts.Main
{
	[HarmonyPatch(typeof(EnergyIconHelper), nameof(EnergyIconHelper.GetPrefix))]
	public static class PatchouliEnergyIconPrefixPatch
	{
		static void Postfix(AbstractModel model, ref string __result)
		{
			if (model is not PatchouliCardModel patchouli || !patchouli.HasElementVisuals)
			{
				return;
			}
			string? key = PatchouliCardModel.GetElementIconKey(patchouli.GetVisualElementNow());
			if (key != null)
			{
				__result = key;
			}
		}
	}

	[HarmonyPatch(typeof(EnergyIconHelper), nameof(EnergyIconHelper.GetPath), typeof(string))]
	public static class PatchouliEnergyIconPathPatch
	{
		static void Postfix(string prefix, ref string __result)
		{
			string lower = prefix?.ToLowerInvariant() ?? string.Empty;
			if (lower is "fire" or "gold" or "water" or "wood" or "dirt" or "lunar" or "sun")
			{
				__result = $"res://TH_Patchouli/ArtWorks/Character/{lower}_card_orb.png";
			}
		}
	}

	[HarmonyPatch(typeof(EnergyIconsFormatter), nameof(EnergyIconsFormatter.TryEvaluateFormat))]
	public static class PatchouliEnergyIconsFormatterPatch
	{
		static bool Prefix(object formattingInfo, ref bool __result)
		{
			if (formattingInfo == null)
			{
				return true;
			}

			object? currentValue = GetCurrentValue(formattingInfo);
			if (currentValue is not EnergyVar energyVar)
			{
				return true;
			}

			string prefix = energyVar.ColorPrefix?.ToLowerInvariant() ?? string.Empty;
			if (prefix is not ("fire" or "gold" or "water" or "wood" or "dirt" or "lunar" or "sun"))
			{
				return true;
			}

			int amount = Convert.ToInt32(energyVar.PreviewValue);
			string iconTag = "[img]res://TH_Patchouli/ArtWorks/Character/" + prefix + "_cost_orb.png[/img]";
			string output = (amount > 0 && amount < 4)
				? string.Concat(Enumerable.Repeat(iconTag, amount))
				: energyVar.ToHighlightedString(inverse: false) + iconTag;

			Write(formattingInfo, output);
			__result = true;
			return false;
		}

		private static object? GetCurrentValue(object formattingInfo)
		{
			PropertyInfo? prop = formattingInfo.GetType().GetProperty("CurrentValue", BindingFlags.Public | BindingFlags.Instance);
			return prop?.GetValue(formattingInfo);
		}

		private static void Write(object formattingInfo, string text)
		{
			MethodInfo? method = formattingInfo.GetType().GetMethod("Write", BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string) }, modifiers: null);
			method?.Invoke(formattingInfo, new object[] { text });
		}
	}

	[HarmonyPatch]
	public static class PatchouliCardDescriptionSuffixPatch
	{
		static MethodBase? TargetMethod()
		{
			Type? previewType = typeof(CardModel).GetNestedType("DescriptionPreviewType", BindingFlags.NonPublic);
			if (previewType == null)
			{
				return null;
			}
			return AccessTools.Method(typeof(CardModel), "GetDescriptionForPile", new[] { typeof(PileType), previewType, typeof(MegaCrit.Sts2.Core.Entities.Creatures.Creature) });
		}

		static void Postfix(CardModel __instance, ref string __result)
		{
			if (__instance is not PatchouliCardModel patchouli)
			{
				return;
			}
			string suffix = patchouli.GetElementSuffixText();
			if (string.IsNullOrEmpty(suffix))
			{
				return;
			}
			__result = string.IsNullOrEmpty(__result) ? suffix : __result.TrimEnd('\n') + "\n" + suffix;
		}
	}

	[HarmonyPatch(typeof(NCreature), nameof(NCreature.ShowHoverTips))]
	public static class DeepDarkFantasyHideIntentHoverTipsPatch
	{
		static void Prefix(NCreature __instance, ref IEnumerable<IHoverTip> hoverTips)
		{
			if (__instance?.Entity?.Monster == null)
			{
				return;
			}

			ICombatState? combatState = __instance.Entity.CombatState;
			if (combatState == null)
			{
				return;
			}

			Player? me = LocalContext.GetMe(combatState);
			if (me == null)
			{
				return;
			}

			if (me.GetRelic<DeepDarkFantasy>() == null)
			{
				return;
			}

			if (hoverTips == null)
			{
				return;
			}

			hoverTips = hoverTips.Where(t => !IsIntentHoverTip(t)).ToList();
		}

		private static bool IsIntentHoverTip(IHoverTip tip)
		{
			if (tip is not HoverTip ht || string.IsNullOrEmpty(ht.Id))
			{
				return false;
			}

			return ht.Id.Contains("Title=intents.", StringComparison.Ordinal);
		}
	}
}
