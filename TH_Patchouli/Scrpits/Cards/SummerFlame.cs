using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class SummerFlame : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Fire };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4)];

		public SummerFlame() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int amount = DynamicVars.Cards.IntValue;
			await PowerCmd.Apply<IgnitePower>(cardPlay.Target, amount, Owner.Creature, this);
			if (cardPlay.Target.HasPower<IgnitePower>())
			{
				await PowerCmd.Apply<IgnitePower>(cardPlay.Target, amount, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
