using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Models;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Patchouib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using ElementPowers = TH_Patchouli.Scrpits.Powers;
using MegaCrit.Sts2.Core.Commands;

namespace TH_Patchouli.Scrpits.Multiplayer;

public static class PatchouliEnhancementRightClickSync
{
	private const uint EnhancementRightClickChoiceId = 4000000001u;

	private static PlayerChoiceSynchronizer? _lastSynchronizer;

	public static void EnsureSubscribed()
	{
		PlayerChoiceSynchronizer synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
		if (synchronizer == null)
		{
			return;
		}
		if (ReferenceEquals(_lastSynchronizer, synchronizer))
		{
			return;
		}
		if (_lastSynchronizer != null)
		{
			_lastSynchronizer.PlayerChoiceReceived -= OnPlayerChoiceReceived;
		}
		_lastSynchronizer = synchronizer;
		synchronizer.PlayerChoiceReceived += OnPlayerChoiceReceived;
	}

	public static Task DoLocalAndSync(Player player, PatchouliCardModel card, int boostAmount)
	{
		if (boostAmount <= 0)
		{
			return Task.CompletedTask;
		}

		EnsureSubscribed();

		if (RunManager.Instance.NetService.Type.IsMultiplayer())
		{
			uint cardId = NetCombatCardDb.Instance.GetCardId(card);
			PlayerChoiceMessage message = new PlayerChoiceMessage
			{
				choiceId = EnhancementRightClickChoiceId,
				result = PlayerChoiceResult.FromIndexes([(int)cardId, boostAmount]).ToNetData()
			};
			RunManager.Instance.NetService.SendMessage(message);
		}

		ApplyImmediate(player, card, boostAmount);
		return Task.CompletedTask;
	}

	private static void OnPlayerChoiceReceived(Player player, uint choiceId, NetPlayerChoiceResult result)
	{
		if (choiceId != EnhancementRightClickChoiceId)
		{
			return;
		}
		if (result.type != PlayerChoiceType.Index || result.indexes == null || result.indexes.Count < 2)
		{
			return;
		}

		int cardIndex = result.indexes[0];
		int boostAmount = result.indexes[1];
		if (cardIndex < 0 || boostAmount <= 0)
		{
			return;
		}
		if (!NetCombatCardDb.Instance.TryGetCard((uint)cardIndex, out CardModel? card) || card == null)
		{
			return;
		}
		if (!ReferenceEquals(card.Owner, player))
		{
			return;
		}
		if (card is not PatchouliCardModel pcm)
		{
			return;
		}

		ApplyImmediate(player, pcm, boostAmount);
	}

	private static void ApplyImmediate(Player player, PatchouliCardModel card, int boostAmount)
	{
		if (boostAmount <= 0)
		{
			return;
		}

		List<ElementEnum> elements = GetDistinctElementsOrdered(card.ElementTypes);
		for (int i = 0; i < elements.Count; i++)
		{
			ConsumeElement(player.Creature, elements[i], boostAmount);
		}
		card.BoostWhenElementEnhanced(boostAmount);
		CardCmd.Preview(card);
		if (card.Pile != null)
		{
			NCard.FindOnTable(card)?.UpdateVisuals(card.Pile.Type, CardPreviewMode.Normal);
		}
	}

	private static void ConsumeElement(Creature owner, ElementEnum element, int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		PowerModel? power = element switch
		{
			ElementEnum.Gold => owner.GetPower<ElementPowers.GoldElement>(),
			ElementEnum.Lunar => owner.GetPower<ElementPowers.LunarElement>(),
			ElementEnum.Sun => owner.GetPower<ElementPowers.SunElement>(),
			ElementEnum.Fire => owner.GetPower<ElementPowers.FireElement>(),
			ElementEnum.Water => owner.GetPower<ElementPowers.WaterElement>(),
			ElementEnum.Wood => owner.GetPower<ElementPowers.WoodElement>(),
			ElementEnum.Dirt => owner.GetPower<ElementPowers.DirtElement>(),
			_ => null
		};
		if (power == null)
		{
			return;
		}

		int next = power.Amount - amount;
		if (next <= 0)
		{
			power.RemoveInternal();
		}
		else
		{
			power.SetAmount(next);
		}
	}

	private static List<ElementEnum> GetDistinctElementsOrdered(List<ElementEnum>? elements)
	{
		List<ElementEnum> result = new List<ElementEnum>();
		if (elements == null || elements.Count == 0)
		{
			return result;
		}

		HashSet<ElementEnum> seen = new HashSet<ElementEnum>();
		for (int i = 0; i < elements.Count; i++)
		{
			ElementEnum e = elements[i];
			if (e != ElementEnum.None && seen.Add(e))
			{
				result.Add(e);
			}
		}
		result.Sort();
		return result;
	}
}

[HarmonyPatch(typeof(RunManager))]
[HarmonyPatch("InitializeShared")]
public static class PatchouliEnhancementRightClickSyncRunManagerPatch
{
	public static void Postfix()
	{
		PatchouliEnhancementRightClickSync.EnsureSubscribed();
	}
}
