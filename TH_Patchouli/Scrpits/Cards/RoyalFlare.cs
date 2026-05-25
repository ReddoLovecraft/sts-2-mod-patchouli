using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Patchouib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class RoyalFlare : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Fire };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(11)];

		public RoyalFlare() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			List<CardModel> toExhaust =
			[
				.. PileType.Hand.GetPile(Owner).Cards.Where(c => c.Type is CardType.Curse or CardType.Status),
				.. PileType.Draw.GetPile(Owner).Cards.Where(c => c.Type is CardType.Curse or CardType.Status),
				.. PileType.Discard.GetPile(Owner).Cards.Where(c => c.Type is CardType.Curse or CardType.Status),
			];

			int count = toExhaust.Count;
			for (int i = 0; i < toExhaust.Count; i++)
			{
				await CardCmd.Exhaust(choiceContext, toExhaust[i]);
			}

			if (count > 0)
			{
				await PowerCmd.Apply<IgnitePower>(CombatState.HittableEnemies, count * DynamicVars.Cards.IntValue, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
