using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Cards;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class EndlessMakePower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<BasicBody>(false)];

		public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
		{
			if (player != Owner.Player)
			{
				return;
			}

			int count = Math.Max(0, Amount);
			if (count <= 0)
			{
				return;
			}

			List<CardModel> cards = new();
			for (int i = 0; i < count; i++)
			{
				cards.Add(combatState.CreateCard(ModelDb.Card<BasicBody>(), player));
			}

			Flash();
			await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, addedByPlayer: true);
		}
	}
}
