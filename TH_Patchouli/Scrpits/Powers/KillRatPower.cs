using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Threading.Tasks;
using BaseLib.Extensions;

namespace TH_Patchouli.Scrpits.Powers 
{
	public sealed class KillRatPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Debuff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._debuffAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/KRP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/KRP64.png";



		public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target == Owner&&props.IsPoweredAttack_())
			{
				return Amount;
			}
			return 1m;
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (side != Owner.Side)
			{
				await PowerCmd.Remove(this);
			}
		}
	}
}

