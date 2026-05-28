using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class FireWind : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Fire, ElementEnum.Water };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<IgnitePower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public FireWind() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.AllEnemies)
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

			int mult = DynamicVars.Cards.IntValue;
			if (mult <= 1)
			{
				return;
			}

			foreach (Creature enemy in CombatState.HittableEnemies)
			{
				IgnitePower? ignite = enemy.GetPower<IgnitePower>();
				if (ignite == null || ignite.Amount <= 0)
				{
					continue;
				}

				int add = ignite.Amount * (mult - 1);
				if (add > 0)
				{
					await PowerCmd.Apply<IgnitePower>(enemy, add, Owner.Creature, this);
				}
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
