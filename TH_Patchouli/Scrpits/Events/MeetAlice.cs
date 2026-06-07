using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Cards;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scripts.Events;

public sealed class MeetAlice : CustomEventModel
{
	public override string? CustomInitialPortraitPath => "res://TH_Patchouli/ArtWorks/Events/meetalice.png";
	public override bool IsAllowed(IRunState runState)
	{
		return runState.Players.Any((Player p) => p.Character is PatchouliCharacter);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = base.Owner!;
		bool hasUpgradable = PileType.Deck.GetPile(owner).Cards.Any(c => c.IsUpgradable);

		EventOption talk = hasUpgradable
			? new EventOption(this, Talk, $"{Id.Entry}.pages.INITIAL.options.TALK")
			: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.THINK_LOCKED");

		return new EventOption[]
		{
			talk,
			new EventOption(this, Together, $"{Id.Entry}.pages.INITIAL.options.TOGETHER", HoverTipFactory.FromCard<DoubleSevenGroup>())
		};
	}

	private async Task Talk()
	{
		Player owner = base.Owner!;
		CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 2, 2)
		{
			RequireManualConfirmation = true
		};

		IEnumerable<CardModel> cards = await CardSelectCmd.FromDeckForUpgrade(owner, prefs);
		foreach (CardModel card in cards)
		{
			CardCmd.Upgrade(card);
		}
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.TALK.description"));
	}

	private async Task Together()
	{
		Player owner = base.Owner!;
		CardModel card = owner.RunState.CreateCard(ModelDb.Card<DoubleSevenGroup>(), owner);
		CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
		CardCmd.PreviewCardPileAdd(result, 2f);
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.TOGETHER.description"));
	}
}
