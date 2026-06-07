using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scripts.Events;

public sealed class GirlRoom : CustomEventModel
{
	 public override string? CustomInitialPortraitPath => "res://TH_Patchouli/ArtWorks/Events/girlroom.png";
	public override bool IsAllowed(IRunState runState)
	{
		return runState.CurrentActIndex is 0 or 1
			&& runState.Players.Any((Player p) => p.Character is PatchouliCharacter);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = base.Owner!;
		bool hasUpgradable = PileType.Deck.GetPile(owner).Cards.Any(c => c.IsUpgradable);

		EventOption think = hasUpgradable
			? new EventOption(this, Think, $"{Id.Entry}.pages.INITIAL.options.THINK")
			: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.THINK_LOCKED");

		return new EventOption[]
		{
			new EventOption(this, Search, $"{Id.Entry}.pages.INITIAL.options.SEARCH"),
			think,
			new EventOption(this, Sleep, $"{Id.Entry}.pages.INITIAL.options.SLEEP"),
		};
	}

	private async Task Search()
	{
		Player owner = base.Owner!;
		RelicModel relic = RelicFactory.PullNextRelicFromFront(owner).ToMutable();
		await RelicCmd.Obtain(relic, owner);
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.SEARCH.description"));
	}

	private async Task Think()
	{
		Player owner = base.Owner!;
		List<CardModel> upgradable = PileType.Deck.GetPile(owner).Cards.Where(c => c.IsUpgradable).ToList();
		if (upgradable.Count <= 0)
		{
			SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.THINK.description"));
			return;
		}

		Rng rng = owner.RunState.Rng.Niche;
		List<CardModel> selected = new List<CardModel>();
		int picks = System.Math.Min(2, upgradable.Count);
		for (int i = 0; i < picks; i++)
		{
			int idx = rng.NextInt(upgradable.Count);
			selected.Add(upgradable[idx]);
			upgradable.RemoveAt(idx);
		}

		foreach (CardModel card in selected)
		{
			CardCmd.Upgrade(card);
		}
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.THINK.description"));
	}

	private async Task Sleep()
	{
		Player owner = base.Owner!;
		await CreatureCmd.Heal(owner.Creature, 20);
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.SLEEP.description"));
	}
}
