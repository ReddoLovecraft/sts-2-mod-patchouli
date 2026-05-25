using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class AutumnBlades : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Gold };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TH_Patchouli.Scrpits.Powers.GoldElement>()];

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new CalculationBaseVar(1m),
			new ExtraDamageVar(1m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateGoldElementBonus),
			new CardsVar(5),
		];

		public AutumnBlades() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this).Targeting(cardPlay.Target).WithHitCount(DynamicVars.Cards.IntValue).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}

		private static decimal CalculateGoldElementBonus(CardModel card, Creature? _)
		{
			AutumnBlades self = (AutumnBlades)card;
			int bonus = self.Owner.Creature.GetPower<global::TH_Patchouli.Scrpits.Powers.GoldElement>()?.Amount ?? 0;
			return Math.Max(0, bonus);
		}
	}
}
