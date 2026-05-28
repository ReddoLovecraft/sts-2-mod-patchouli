using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class Swamp : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Water, ElementEnum.Dirt };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<SlowPower>(),
			HoverTipFactory.FromPower<WeakPower>(),
			HoverTipFactory.FromPower<VulnerablePower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public Swamp() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.AllEnemies)
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

			int amt = Math.Max(0, DynamicVars.Cards.IntValue);
			if (amt <= 0)
			{
				return;
			}

			await PowerCmd.Apply<SlowPower>(CombatState.HittableEnemies, amt, Owner.Creature, this);
			await PowerCmd.Apply<WeakPower>(CombatState.HittableEnemies, amt, Owner.Creature, this);
			await PowerCmd.Apply<VulnerablePower>(CombatState.HittableEnemies, amt, Owner.Creature, this);
			await PowerCmd.Apply<DebilitatePower>(CombatState.HittableEnemies, amt, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
