using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class SecretChamber : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Water, ElementEnum.Gold];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new CardsVar(4),
			new CalculationBaseVar(9m),
			new ExtraDamageVar(1m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateExtraDamage),
		];

		public SecretChamber() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.CalculationBase.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}
			await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.CalculationBase.UpgradeValueBy(1);
		}

		private static decimal CalculateExtraDamage(CardModel card, Creature? target)
		{
			if (target == null)
			{
				return 0m;
			}

			int per = Math.Max(0, card.DynamicVars.Cards.IntValue);
			if (per <= 0)
			{
				return 0m;
			}

			List<PowerModel> debuffs = target.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
			if (debuffs.Count == 0)
			{
				return 0m;
			}

			int count;
			if (card.IsUpgraded)
			{
				count = debuffs.Sum(p => Math.Max(1, p.Amount));
			}
			else
			{
				count = debuffs.Select(p => p.Id).Distinct().Count();
			}

			return Math.Max(0, count) * per;
		}
	}
}
