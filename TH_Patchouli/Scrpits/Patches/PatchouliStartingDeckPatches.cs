using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Collections.Generic;

namespace TH_Patchouli.Scripts.Main
{
	[HarmonyPatch(typeof(Player), "PopulateStartingDeck")]
	public static class PatchouliStartingDeckCapturePatch
	{
		static void Prefix(Player __instance)
		{
			if (__instance.Character is PatchouliCharacter)
			{
				StartingDeckElementCapture.Begin();
			}
		}

		static void Postfix(Player __instance)
		{
			if (__instance.Character is PatchouliCharacter)
			{
				StartingDeckElementCapture.End();
			}
		}
	}

	[HarmonyPatch(typeof(CardModel), nameof(CardModel.ToMutable))]
	public static class PatchouliCardToMutablePatch
	{
		static void Postfix(CardModel __instance, ref CardModel __result)
		{
			if (!StartingDeckElementCapture.Active)
			{
				return;
			}
			if (__instance is not PatchouliCardModel)
			{
				return;
			}
			if (__result is not PatchouliCardModel patchouliResult)
			{
				return;
			}
			if (StartingDeckElementCapture.TryDequeue(out List<ElementEnum> elements))
			{
				patchouliResult.SetElementTypes(elements);
			}
		}
	}
}
