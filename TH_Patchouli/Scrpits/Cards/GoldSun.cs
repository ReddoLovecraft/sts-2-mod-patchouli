using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class GoldSun : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Gold, ElementEnum.Sun };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new CalculationBaseVar(6m),
			new ExtraDamageVar(1m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateGoldBonus),
			new CardsVar(6),
		];

		public GoldSun() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.CalculationBase.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);

			Player player = Owner;
			int lose = Math.Max(0, DynamicVars.Cards.IntValue);
			if (lose > 0)
			{
				await PlayerCmd.LoseGold(lose, player, GoldLossType.Spent);
			}
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}

		private static decimal CalculateGoldBonus(CardModel card, Creature? _)
		{
			Player? owner = card.Owner;
			if (owner == null)
			{
				return 0m;
			}

			int gold = Math.Max(0, owner.Gold);
			int bonus = (int)Math.Floor(gold * 0.3m);
			return Math.Max(0, bonus);
		}
	}
}
