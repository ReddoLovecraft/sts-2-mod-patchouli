using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class WeakWitch : PatchouliCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<WeakPower>(),
			HoverTipFactory.FromPower<FrailPower>(),
			base.EnergyHoverTip
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new EnergyVar(1)];

		public WeakWitch() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(-boostAmount);
			DynamicVars.Energy.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<WeakPower>(Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
			await PowerCmd.Apply<FrailPower>(Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
			await PowerCmd.Apply<WeakWitchPower>(Owner.Creature, DynamicVars.Energy.IntValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}
