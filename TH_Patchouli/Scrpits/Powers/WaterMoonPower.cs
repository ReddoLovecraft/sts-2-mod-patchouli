using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class WaterMoonPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

		private CardModel? _sourceCard;

		public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
		{
			_sourceCard = cardSource;
			return Task.CompletedTask;
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			if (Owner.Player == null || CombatState == null || cardPlay.Card.Owner != Owner.Player)
			{
				return;
			}
			if (_sourceCard != null && ReferenceEquals(cardPlay.Card, _sourceCard))
			{
				return;
			}

			int count = Math.Max(0, Amount);
			if (count <= 0)
			{
				return;
			}

			List<CardModel> generated = new List<CardModel>(count);
			for (int i = 0; i < count; i++)
			{
				CardModel clone = CombatState.CloneCard(cardPlay.Card);
				CardCmd.ApplyKeyword(clone, CardKeyword.Ethereal);
				generated.Add(clone);
			}

			Flash();
			await CardPileCmd.AddGeneratedCardsToCombat(generated, PileType.Hand, addedByPlayer: true);
		}

		public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side == Owner.Side)
			{
				await PowerCmd.Remove(this);
			}
		}
	}
}
