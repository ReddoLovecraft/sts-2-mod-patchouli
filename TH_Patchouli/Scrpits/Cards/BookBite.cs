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
	public sealed class BookBite : PatchouliCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
		private int _bonusDamage;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move), new CardsVar(7)];

		public BookBite() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override void DeepCloneFields()
		{
			base.DeepCloneFields();
			_bonusDamage = CloneOf is BookBite other ? other._bonusDamage : 0;
		}

		public override Task AfterCardRetained(CardModel card)
		{
			if (card == this)
			{
				_bonusDamage += DynamicVars.Cards.IntValue;
			}
			return Task.CompletedTask;
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target);

			AttackCommand attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue + _bonusDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
			int unblocked = attack.Results.Sum(r => r.UnblockedDamage);
			if (unblocked > 0)
			{
				await CreatureCmd.LoseMaxHp(choiceContext, cardPlay.Target, unblocked, isFromCard: true);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3);
			DynamicVars.Cards.UpgradeValueBy(3);
		}
	}
}
