using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Cards;

namespace TH_Patchouli.Scripts.Events;

public sealed class CursedBook : CustomEventModel
{
	public override string? CustomInitialPortraitPath => "res://TH_Patchouli/ArtWorks/Events/cursedbook.png";
	public override bool IsAllowed(IRunState runState)
	{
		return runState.CurrentActIndex == 2;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new EventOption[]
		{
			new EventOption(this, ReadBook, $"{Id.Entry}.pages.INITIAL.options.READ", HoverTipFactory.FromCard<Crazy>()),
			new EventOption(this, Leave, $"{Id.Entry}.pages.INITIAL.options.LEAVE"),
		};
	}

	private async Task ReadBook()
	{
		Player owner = base.Owner!;

		List<CardModel> deck = PileType.Deck.GetPile(owner).Cards.ToList();
		if (deck.Count > 0)
		{
			await CardPileCmd.RemoveFromDeck(deck);
		}

		List<CardPileAddResult> added = new List<CardPileAddResult>();
		foreach (CharacterModel character in ModelDb.AllCharacters)
		{
			List<CardModel> poolCards = character.CardPool
				.GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)
				.Where(c => c.ShouldShowInCardLibrary && c.Type != CardType.Quest)
				.ToList();
			if (poolCards.Count <= 0)
			{
				continue;
			}

			CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1, 1)
			{
				RequireManualConfirmation = true
			};

			CardModel selected = (await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), poolCards, owner, prefs)).FirstOrDefault();
			if (selected == null)
			{
				continue;
			}

			CardModel card = owner.RunState.CreateCard(selected, owner);
			added.Add(await CardPileCmd.Add(card, PileType.Deck));
		}

		CardModel curse = owner.RunState.CreateCard(ModelDb.Card<Crazy>(), owner);
		added.Add(await CardPileCmd.Add(curse, PileType.Deck));

		if (added.Count > 0)
		{
			CardCmd.PreviewCardPileAdd(added, 2f);
		}

		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.READ.description"));
	}

	private Task Leave()
	{
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.LEAVE.description"));
		return Task.CompletedTask;
	}
}
