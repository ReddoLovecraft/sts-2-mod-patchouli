using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class IgniteMark : CustomPowerModel
	{
		public override PowerType Type => PowerType.Debuff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._debuffAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/IM32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/IM64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>()];

		public IgniteMark()
		{
		}

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			PatchoulibEffectManager.OnPowerApplied(Owner, this);
			return Task.CompletedTask;
		}

		public override Task AfterRemoved(Creature oldOwner)
		{
			PatchoulibEffectManager.OnPowerRemoved(oldOwner, this);
			return Task.CompletedTask;
		}
	
		public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
		{
			if (target == Owner && canonicalPower is IgnitePower)
			{
				modifiedAmount = amount + Amount;
				return true;
			}
			modifiedAmount = amount;
			return false;
		}
	}
}
