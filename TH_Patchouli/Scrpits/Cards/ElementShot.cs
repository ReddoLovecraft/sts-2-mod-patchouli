using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class ElementShot : PatchouliCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

		public ElementShot() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target);
			int kinds = ToolBox.GetElementKinds(Owner.Creature);
			int hits = Math.Max(1, 1 + kinds);
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitCount(hits).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3);
		}
	}
}
