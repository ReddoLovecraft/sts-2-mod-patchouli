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
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class KnowledgeWallPower : CustomPowerModel
	{
		private int _reductionThisRound;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override bool IsInstanced => true;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/GE32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/GE64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

		public override Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side == Owner.Side)
			{
				_reductionThisRound = PileType.Hand.GetPile(Owner.Player).Cards.Count * Amount;
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
		{
			if (side == Owner.Side)
			{
				_reductionThisRound = 0;
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
