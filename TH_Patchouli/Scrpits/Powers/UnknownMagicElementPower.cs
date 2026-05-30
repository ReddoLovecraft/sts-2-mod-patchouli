using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class UnknownMagicElementPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/UMEP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/UMEP64.png";

		public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (power.Owner != Owner || amount >= 0m)
			{
				return;
			}
			if (power is not (GoldElement or WoodElement or WaterElement or FireElement or DirtElement or SunElement or LunarElement))
			{
				return;
			}

			int dec = (int)Math.Abs(amount);
			if (dec <= 0)
			{
				return;
			}

			int gain = Math.Max(0, Amount) * dec;
			if (gain <= 0 || Owner.Player == null)
			{
				return;
			}

			Flash();
			await PlayerCmd.GainEnergy(gain, Owner.Player);
		}
	}
}

