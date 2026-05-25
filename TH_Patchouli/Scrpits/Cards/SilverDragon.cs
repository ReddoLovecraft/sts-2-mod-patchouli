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
	public sealed class SilverDragon : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Gold };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(14m, ValueProp.Move), new CardsVar(3)];

		public SilverDragon() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target);
			int strength = Owner.Creature.GetPower<StrengthPower>()?.Amount ?? 0;
			int multiplier = Math.Max(0, DynamicVars.Cards.IntValue);
			int extraStrengthDamage = Math.Max(0, strength * Math.Max(0, multiplier - 1));
			decimal damage = DynamicVars.Damage.BaseValue + extraStrengthDamage;
			await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
