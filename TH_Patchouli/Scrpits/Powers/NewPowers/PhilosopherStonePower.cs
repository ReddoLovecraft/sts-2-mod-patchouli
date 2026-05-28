using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class PhilosopherStonePower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			if (Owner.Player == null || CombatState == null || cardPlay.Card.Owner != Owner.Player)
			{
				return;
			}

			int times = Math.Max(0, Amount);
			if (times <= 0)
			{
				return;
			}

			for (int i = 0; i < times; i++)
			{
				await TriggerElementEffectsOnce(context);
			}
		}

		private async Task TriggerElementEffectsOnce(PlayerChoiceContext context)
		{
			if (Owner.Player == null || CombatState == null)
			{
				return;
			}

			int gold = Owner.GetPower<GoldElement>()?.Amount ?? 0;
			if (gold > 0)
			{
				await PowerCmd.Apply<FlexPotionPower>(Owner, gold, Owner, null);
			}

			int water = Owner.GetPower<WaterElement>()?.Amount ?? 0;
			if (water > 0)
			{
				await PowerCmd.Apply<FreezePower>(CombatState.HittableEnemies, water, Owner, null);
			}

			int sun = Owner.GetPower<SunElement>()?.Amount ?? 0;
			int energy = sun / 2;
			if (energy > 0)
			{
				await PlayerCmd.GainEnergy(energy, Owner.Player);
			}

			int lunar = Owner.GetPower<LunarElement>()?.Amount ?? 0;
			int draw = lunar / 2;
			if (draw > 0)
			{
				await CardPileCmd.Draw(context, draw, Owner.Player);
			}

			int wood = Owner.GetPower<WoodElement>()?.Amount ?? 0;
			if (wood > 0)
			{
				await CreatureCmd.Heal(Owner, wood);
			}

			int dirt = Owner.GetPower<DirtElement>()?.Amount ?? 0;
			if (dirt > 0)
			{
				await CreatureCmd.GainBlock(Owner, dirt, ValueProp.Unpowered | ValueProp.Move, null);
			}

			Flash();
		}

		public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side == Owner.Side)
			{
				await PowerCmd.Remove(this);
			}
		}
	}
}
