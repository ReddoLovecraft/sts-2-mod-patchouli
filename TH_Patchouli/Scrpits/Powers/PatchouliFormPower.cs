using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class PatchouliFormPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/PFP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/PFP64.png";
		public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if(target==Owner)
			{
				decimal res=1m-this.Amount/100m;
				if(res<=0)
				{
					res=0;
				}
				return res;
			}
			if(dealer==Owner&&cardSource!=null&&props.IsPoweredAttack_()){
				decimal res=1m+this.Amount/100m;
				return res;
			}
			return base.Amount;
		}
		public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == base.Owner && dealer != null && (props.IsPoweredAttack() || cardSource is Omnislice))
        {
			Tools.Talk("姆Q~",Owner);
			SfxCmd.Play(PatchouliInit.ToModSfxPath("TH_Patchouli/ArtWorks/SFX/mq.wav"));
		}
	}	
	}
}
