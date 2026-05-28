using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class CircularLavaBeltPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>(),HoverTipFactory.Static(StaticHoverTip.Block)];

		public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
		{
			if (CombatState == null || creature != Owner || amount <= 0m)
			{
				return;
			}

			int ignite = (int)Math.Max(0m, amount);
			if (ignite <= 0)
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<IgnitePower>(CombatState.HittableEnemies, ignite, Owner, null);
		}
	}
}
