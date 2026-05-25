using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class WeakWitchPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

		public override decimal ModifyMaxEnergy(Player player, decimal amount)
    	{
        	if (player != base.Owner.Player)
        	{	
            return amount;
        	}
        	return amount + (decimal)base.Amount;
    	}
	}
}
