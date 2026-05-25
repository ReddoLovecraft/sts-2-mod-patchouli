using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class BookSea : PatchouliCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public BookSea() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];
		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			List<CardModel> discardCards = PileType.Discard.GetPile(Owner).Cards.ToList();
			if (discardCards.Count == 0)
			{
				return;
			}

			CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, DynamicVars.Cards.IntValue);
			List<CardModel> selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, discardCards, Owner, prefs)).ToList();
			if (selected.Count == 0)
			{
				return;
			}

			bool shouldZero = PileType.Draw.GetPile(Owner).Cards.Count > PileType.Discard.GetPile(Owner).Cards.Count;
			foreach (CardModel card in selected)
			{
				await CardPileCmd.Add(card, PileType.Hand);
				if (shouldZero)
				{
					card.EnergyCost.SetThisTurnOrUntilPlayed(0, reduceOnly: true);
				}
			}
		}

		protected override void OnUpgrade()
		{
			this.RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}
