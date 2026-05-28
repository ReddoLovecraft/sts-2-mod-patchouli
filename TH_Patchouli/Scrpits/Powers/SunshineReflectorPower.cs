using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class SunshineReflectorPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			if (Owner.Player == null || cardPlay.Card.Owner != Owner.Player)
			{
				return;
			}

			int baseCost = cardPlay.Card.EnergyCost.Canonical;
			if (baseCost <= 0 || cardPlay.Card.EnergyCost.CostsX)
			{
				return;
			}

			int gain = baseCost / 2;
			if (gain <= 0)
			{
				return;
			}

			Flash();
			await PlayerCmd.GainEnergy(gain, Owner.Player);
		}
	}
}
