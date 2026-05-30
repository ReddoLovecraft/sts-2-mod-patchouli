using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class LavaCromlechPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/LCP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/LCP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>(),HoverTipFactory.Static(StaticHoverTip.Block)];

		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner || dealer == null || dealer.Side == Owner.Side || result.BlockedDamage <= 0 || !props.IsPoweredAttack_())
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<IgnitePower>(dealer, result.BlockedDamage, Owner, null);
		}

		public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
		{
			if (side == Owner.Side)
			{
				await PowerCmd.Decrement(this);
			}
		}
	}
}
