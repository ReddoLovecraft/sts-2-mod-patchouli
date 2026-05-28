using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers.NewPowers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class BloodCircleMake : PatchouliCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [base.EnergyHoverTip];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5), new EnergyVar(1)];

		public BloodCircleMake() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(-boostAmount);
			DynamicVars.Cards.BaseValue = Math.Max(0, DynamicVars.Cards.BaseValue);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<BloodCircleMakePower>(Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(-2);
		}
	}
}
