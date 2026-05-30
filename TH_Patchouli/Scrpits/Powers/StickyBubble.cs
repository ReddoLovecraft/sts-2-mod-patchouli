using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class StickyBubble : CustomPowerModel
	{
		public override PowerType Type => PowerType.Debuff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._debuffAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/SB32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/SB64.png";

		public StickyBubble()
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

		public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
		{
			if (dealer == Owner && target.IsPlayer)
			{
				await TriggerAndDecrement(choiceContext);
			}
		}

		public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side == Owner.Side)
			{
				await TriggerAndDecrement(choiceContext);
			}
		}

		private async Task TriggerAndDecrement(PlayerChoiceContext choiceContext)
		{
			if (Amount <= 0 || Owner.IsDead)
			{
				return;
			}

			Flash();
			await CreatureCmd.Damage(choiceContext, Owner, 10m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, dealer: null, cardSource: null);
			await PowerCmd.Decrement(this);
		}
	}
}
