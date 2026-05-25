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
	public sealed class BookStrike : PatchouliCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new EnergyVar(0),
			new CalculationBaseVar(0m),
			new ExtraDamageVar(1m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateDeckCardCount),
		];

		public BookStrike() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}
		protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target);

			await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}

		private static decimal CalculateDeckCardCount(CardModel card, Creature? _)
		{
			if (card.Owner == null)
			{
				return 0m;
			}
			return PileType.Deck.GetPile(card.Owner).Cards.Count;
		}
	}
}
