using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class BloodCircleMakePower : CustomPowerModel
	{
		private readonly Dictionary<CardModel, int> _pendingHpLossByCard = new();

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override bool IsInstanced => true;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5)];

		public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (cardSource is PatchouliCardModel pcm)
			{
				DynamicVars.Cards.BaseValue = Math.Max(0, pcm.DynamicVars.Cards.IntValue);
			}
			return Task.CompletedTask;
		}

		public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			if (originalCost < 0m || Owner?.Player == null || card.Owner != Owner.Player || card.EnergyCost.CostsX)
			{
				modifiedCost = originalCost;
				return false;
			}

			int energy = Owner.Player.PlayerCombatState?.Energy ?? 0;
			int cost = (int)Math.Max(0m, originalCost);
			if (cost <= energy)
			{
				_pendingHpLossByCard.Remove(card);
				modifiedCost = originalCost;
				return false;
			}

			int baseCost = Math.Max(0, card.EnergyCost.GetWithModifiers(CostModifiers.None));
			_pendingHpLossByCard[card] = baseCost;

			modifiedCost = energy;
			return (int)modifiedCost != cost;
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			if (cardPlay.Card.Owner != Owner.Player)
			{
				return;
			}

			if (!_pendingHpLossByCard.TryGetValue(cardPlay.Card, out int baseCost))
			{
				return;
			}
			_pendingHpLossByCard.Remove(cardPlay.Card);

			int multiplier = Math.Max(0, DynamicVars.Cards.IntValue);
			int hpLoss = baseCost * multiplier;
			if (hpLoss <= 0)
			{
				return;
			}

			Flash();
			await CreatureCmd.Damage(context, Owner, hpLoss, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, dealer: null, cardSource: null);
		}
	}
}
