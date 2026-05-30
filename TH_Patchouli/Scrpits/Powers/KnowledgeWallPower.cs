using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class KnowledgeWallPower : CustomPowerModel
	{
		private int _reductionThisRound;
		public int LockedHandCount { get; private set; }
		public bool IsVfxLocked { get; private set; }

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override bool IsInstanced => true;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/KWP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/KWP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			if (CombatState != null)
			{
				PatchoulibEffectManager.OnPowerApplied(Owner, this);
			}
			return Task.CompletedTask;
		}

		public override Task AfterRemoved(Creature oldOwner)
		{
			PatchoulibEffectManager.OnPowerRemoved(oldOwner, this);
			return Task.CompletedTask;
		}

		public override Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side == Owner.Side)
			{
				int handCount = PileType.Hand.GetPile(Owner.Player).Cards.Count;
				LockedHandCount = handCount;
				IsVfxLocked = true;
				_reductionThisRound = handCount * Amount;
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
		{
			if (side == Owner.Side)
			{
				_reductionThisRound = 0;
				LockedHandCount = 0;
				IsVfxLocked = false;
			}
			return Task.CompletedTask;
		}
		public override Task AfterModifyingHpLostBeforeOsty()
		{
			Flash();
			return Task.CompletedTask;
		}
		public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner || _reductionThisRound <= 0 || amount <= 0 || dealer == null || dealer.Side == Owner.Side)
			{
				return 0m;
			}
			return -_reductionThisRound;
		}
	}
}
