using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class ElementManagerPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];

		public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (power.Owner != Owner)
			{
				return;
			}
			if (power is not (GoldElement or WoodElement or WaterElement or FireElement or DirtElement or SunElement or LunarElement))
			{
				return;
			}
			this.Flash();
			await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), CombatState.HittableEnemies, Amount, ValueProp.Unpowered | ValueProp.Move, Owner);
		}
	}
}
