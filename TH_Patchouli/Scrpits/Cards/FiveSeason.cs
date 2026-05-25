using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchoulib.Scrpits.Main;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class FiveSeason : PatchouliCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [] : [CardKeyword.Exhaust];
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
			[ Tools.GetStaticKeyword("Element"),base.EnergyHoverTip];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new EnergyVar(1)];
		protected override bool ShouldGlowGoldInternal => ToolBox.GetElementKinds(Owner.Creature) > 0;
		public FiveSeason() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
			DynamicVars.Energy.UpgradeValueBy(boostAmount);
		}
		protected override void OnUpgrade()
		{
			this.RemoveKeyword(CardKeyword.Exhaust);
		}
		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int kinds = ToolBox.GetElementKinds(Owner.Creature);
			await CardPileCmd.Draw(choiceContext, kinds * DynamicVars.Cards.IntValue, Owner);
			await PlayerCmd.GainEnergy(kinds * DynamicVars.Energy.IntValue, Owner);
		}
	}
}
