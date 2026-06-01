using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class BookMountain : PatchouliCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new CalculationBaseVar(8m),
			new ExtraDamageVar(3m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateHandCount),
		];

		public BookMountain() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.CalculationBase.UpgradeValueBy(boostAmount);
			DynamicVars.ExtraDamage.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target);
			await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this).WithHitFx(PatchouliVfxManager.ToPatchouliVfxPath("moutain"), null, "blunt_attack.mp3")
			.WithHitVfxSpawnedAtBase().Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.CalculationBase.UpgradeValueBy(2);
			DynamicVars.ExtraDamage.UpgradeValueBy(1);
		}

		private static decimal CalculateHandCount(CardModel card, Creature? _)
		{
			if (card.Owner == null)
			{
				return 0m;
			}
			return PileType.Hand.GetPile(card.Owner).Cards.Count;
		}
	}
}
