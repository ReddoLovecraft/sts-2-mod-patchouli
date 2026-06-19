using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class MoonDirtPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/MDP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/MDP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

		public override Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
		{
			if (player != Owner.Player)
			{
				return Task.CompletedTask;
			}

			int reduction = Math.Max(0, Amount);
			if (reduction <= 0)
			{
				return Task.CompletedTask;
			}

			foreach (CardModel card in retainedCards)
			{
				if (card.Owner != Owner.Player)
				{
					continue;
				}

				Flash();
				card.EnergyCost.AddThisCombat(-reduction, reduceOnly: true);
			}
			return Task.CompletedTask;
		}
	}
}
