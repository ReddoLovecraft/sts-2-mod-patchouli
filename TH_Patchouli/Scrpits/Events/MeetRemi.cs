using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Relics;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scripts.Events;

public sealed class MeetRemi : CustomEventModel
{
	private readonly HpLossVar _hpLossVar = new HpLossVar(4);
	public override string? CustomInitialPortraitPath => "res://TH_Patchouli/ArtWorks/Events/rm.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => new[] { _hpLossVar };

	public override bool IsAllowed(IRunState runState)
	{
		return runState.CurrentActIndex is 0 or 1
			&& runState.Players.All((Player p) => p.Character is PatchouliCharacter);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		if (_hpLossVar.IntValue <= 0)
		{
			_hpLossVar.BaseValue = 4m;
		}

		return new EventOption[]
		{
			new EventOption(this, Refactor, $"{Id.Entry}.pages.INITIAL.options.REFACTOR", HoverTipFactory.Static(StaticHoverTip.Transform)).ThatDoesDamage(_hpLossVar.IntValue),
			new EventOption(this, Take, $"{Id.Entry}.pages.INITIAL.options.TAKE", HoverTipFactory.FromRelic<DestinyStone>()),
		};
	}

	private async Task Refactor()
	{
		Player owner = base.Owner!;
		await LoseHp(owner, _hpLossVar.IntValue);
		await TransformOne(owner);

		_hpLossVar.BaseValue += 1m;
		SetEventState(new LocString(LocTable, $"{Id.Entry}.pages.REFACTOR.description"), GenerateInitialOptions());
	}

	private async Task Take()
	{
		Player owner = base.Owner!;
		RelicModel relic = ModelDb.Relic<DestinyStone>().ToMutable();
		await RelicCmd.Obtain(relic, owner);
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.TAKE.description"));
	}

	private static Task LoseHp(Player owner, int amount)
	{
		return CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), owner.Creature, amount,ValueProp.Unpowered|ValueProp.Unblockable,null, null);
	}

	private async Task TransformOne(Player owner)
	{
		List<CardModel> deckCards = PileType.Deck.GetPile(owner).Cards
			.Where(c => c.IsTransformable && c.Type != CardType.Quest)
			.ToList();
		if (deckCards.Count <= 0)
		{
			return;
		}

		CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1, 1)
		{
			RequireManualConfirmation = true
		};

		CardModel selected = (await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), deckCards, owner, prefs)).FirstOrDefault();
		if (selected == null)
		{
			return;
		}

		List<CardTransformation> transforms = new List<CardTransformation> { new CardTransformation(selected) };
		List<CardPileAddResult> results = (await CardCmd.Transform(transforms, base.Rng)).ToList();
		if (results.Count > 0)
		{
			CardCmd.PreviewCardPileAdd(results, 2f);
		}
	}
}
