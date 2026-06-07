using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Relics;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scripts.Events;

public sealed class Trismegistus : CustomEventModel
{
	public override string? CustomInitialPortraitPath => "res://TH_Patchouli/ArtWorks/Events/trismegistus.png";
	public override bool IsAllowed(IRunState runState)
	{
		return runState.CurrentActIndex is 1 or 2
			&& runState.Players.All((Player p) => p.Character is PatchouliCharacter);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = base.Owner!;

		EventOption inlay = owner.GetRelic<UnstablePhilosophersStone>() != null
			? new EventOption(
				this,
				Inlay,
				$"{Id.Entry}.pages.INITIAL.options.INLAY",
				HoverTipFactory.FromRelic<UnstablePhilosophersStone>().Concat(HoverTipFactory.FromRelic<SageStaff>()))
			: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.INLAY_LOCKED");

		return new EventOption[]
		{
			new EventOption(this, ReadTablet, $"{Id.Entry}.pages.INITIAL.options.READ", HoverTipFactory.FromRelic<EmeraldTablet>()),
			inlay,
			new EventOption(this, Search, $"{Id.Entry}.pages.INITIAL.options.SEARCH", HoverTipFactory.FromRelic<RedStone>()),
		};
	}

	private async Task ReadTablet()
	{
		Player owner = base.Owner!;
		RelicModel relic = ModelDb.Relic<EmeraldTablet>().ToMutable();
		await RelicCmd.Obtain(relic, owner);
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.READ.description"));
	}

	private async Task Inlay()
	{
		Player owner = base.Owner!;
		RelicModel? stone = owner.GetRelic<UnstablePhilosophersStone>();
		if (stone != null)
		{
			await RelicCmd.Remove(stone);
		}

		RelicModel relic = ModelDb.Relic<SageStaff>().ToMutable();
		await RelicCmd.Obtain(relic, owner);

		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.INLAY.description"));
	}

	private async Task Search()
	{
		Player owner = base.Owner!;
		RelicModel relic = ModelDb.Relic<RedStone>().ToMutable();
		await RelicCmd.Obtain(relic, owner);
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.SEARCH.description"));
	}
}
