using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class GingerGust : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Gold, ElementEnum.Dirt };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Retain] : [CardKeyword.Retain, CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<StrengthPower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public GingerGust() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int mult = DynamicVars.Cards.IntValue;
			int strength = Owner.Creature.GetPower<StrengthPower>()?.Amount ?? 0;
			if (strength > 0)
			{
				await PowerCmd.Apply<StrengthPower>(Owner.Creature, strength * (mult - 1), Owner.Creature, this);
			}
			else
			{
				await PowerCmd.Apply<StrengthPower>(Owner.Creature, mult, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}
