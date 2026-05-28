using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class HydrogenousProminence : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Sun, ElementEnum.Water];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<FreezePower>(),
			HoverTipFactory.FromPower<TH_Patchouli.Scrpits.Powers.WaterElement>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public HydrogenousProminence() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			int multiplier = Math.Max(0, DynamicVars.Cards.IntValue);
			if (multiplier <= 0)
			{
				return;
			}

			int waterElement = Owner.Creature.GetPower<TH_Patchouli.Scrpits.Powers.WaterElement>()?.Amount ?? 0;
			int hitCount = 1 + Math.Max(0, waterElement);

			List<Creature> enemies = CombatState.HittableEnemies.ToList();
			foreach (Creature enemy in enemies)
			{
				var freeze = enemy.GetPower<FreezePower>();
				int freezeAmount = freeze?.Amount ?? 0;
				if (freeze != null)
				{
					await PowerCmd.Remove(freeze);
				}

				int damage = Math.Max(0, freezeAmount) * multiplier;
				if (damage <= 0)
				{
					continue;
				}

				await DamageCmd.Attack(damage).WithHitCount(hitCount).FromCard(this).Targeting(enemy).Execute(choiceContext);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
