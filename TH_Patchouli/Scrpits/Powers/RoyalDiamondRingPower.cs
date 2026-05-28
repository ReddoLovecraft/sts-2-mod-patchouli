using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class RoyalDiamondRingPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
		{
			if (Owner.Player == null || card.Owner != Owner.Player || card.Pile?.Type != PileType.Hand || !card.IsUpgradable)
			{
				return Task.CompletedTask;
			}

			Flash();
			CardCmd.Upgrade(card);
			CardCmd.Preview(card);
			return Task.CompletedTask;
		}
	}
}
