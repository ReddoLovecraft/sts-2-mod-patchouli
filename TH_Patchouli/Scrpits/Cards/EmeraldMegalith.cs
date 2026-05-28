using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers.NewPowers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class EmeraldMegalith : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Gold, ElementEnum.Dirt };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
		HoverTipFactory.Static(StaticHoverTip.Block)
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5)];

		public EmeraldMegalith() : base(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<EmeraldMegalithPower>(Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
