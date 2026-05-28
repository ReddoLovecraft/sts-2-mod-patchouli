using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class MagicExpand : PatchouliCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate, CardKeyword.Exhaust];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move), new CardsVar(2)];

		public MagicExpand() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Block.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(2);
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
