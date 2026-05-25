using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class FallSlasher : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Gold };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(7)];

		public FallSlasher() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<FallSlasherPower>(Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
