using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class GroundMouthpiece : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Dirt, ElementEnum.Sun];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

		public GroundMouthpiece() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int count = Math.Max(0, DynamicVars.Cards.IntValue);
			if (count <= 0)
			{
				return;
			}

			List<CardModel> drawCards = PileType.Draw.GetPile(Owner).Cards.ToList();
			if (drawCards.Count > 0)
			{
				int max = Math.Min(count, drawCards.Count);
				CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, max);
				List<CardModel> selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, drawCards, Owner, prefs)).ToList();
				foreach (CardModel card in selected)
				{
					await CardPileCmd.Add(card, PileType.Hand);
				}
			}

			List<CardModel> discardCards = PileType.Discard.GetPile(Owner).Cards.ToList();
			if (discardCards.Count > 0)
			{
				int max = Math.Min(count, discardCards.Count);
				CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, max);
				List<CardModel> selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, discardCards, Owner, prefs)).ToList();
				foreach (CardModel card in selected)
				{
					await CardPileCmd.Add(card, PileType.Hand);
				}
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
