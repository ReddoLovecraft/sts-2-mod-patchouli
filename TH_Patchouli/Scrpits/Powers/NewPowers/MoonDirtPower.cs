using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class MoonDirtPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

		public override Task AfterCardRetained(CardModel card)
		{
			if (card.Owner != Owner.Player)
			{
				return Task.CompletedTask;
			}

			int reduction = Math.Max(0, Amount);
			if (reduction <= 0)
			{
				return Task.CompletedTask;
			}

			Flash();
			card.EnergyCost.AddThisCombat(-reduction, reduceOnly: true);
			return Task.CompletedTask;
		}
	}
}
