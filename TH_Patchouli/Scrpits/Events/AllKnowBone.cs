using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.ValueProps;

namespace TH_Patchouli.Scripts.Events;

public sealed class AllKnowBone : CustomEventModel
{
	public override string? CustomInitialPortraitPath => "res://TH_Patchouli/ArtWorks/Events/allknowbone.png";
	private readonly DynamicVar _choiceOneVar = new DynamicVar("ChoiceOne", 6m);
	private readonly DynamicVar _choiceTwoVar = new DynamicVar("ChoiceTwo", 6m);
	private readonly DynamicVar _choiceThreeVar = new DynamicVar("ChoiceThree", 6m);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { _choiceOneVar, _choiceTwoVar, _choiceThreeVar };

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		if (_choiceOneVar.IntValue <= 0)
		{
			_choiceOneVar.BaseValue = 6m;
		}
		if (_choiceTwoVar.IntValue <= 0)
		{
			_choiceTwoVar.BaseValue = 6m;
		}
		if (_choiceThreeVar.IntValue <= 0)
		{
			_choiceThreeVar.BaseValue = 6m;
		}

		return new EventOption[]
		{
			new EventOption(this, Knowledge, $"{Id.Entry}.pages.INITIAL.options.KNOWLEDGE").ThatDoesDamage(_choiceOneVar.IntValue),
			new EventOption(this, Treasure, $"{Id.Entry}.pages.INITIAL.options.TREASURE").ThatDoesDamage(_choiceTwoVar.IntValue),
			new EventOption(this, Health, $"{Id.Entry}.pages.INITIAL.options.HEALTH").ThatDoesDamage(_choiceThreeVar.IntValue),
			new EventOption(this, Leave, $"{Id.Entry}.pages.INITIAL.options.LEAVE").ThatDoesDamage(6),
		};
	}

	private async Task Knowledge()
	{
		Player owner = base.Owner!;
		await LoseHp(owner, _choiceOneVar.IntValue);
		await GiveColorlessReward(owner);

		_choiceOneVar.BaseValue += 1m;
		SetEventState(new LocString(LocTable, $"{Id.Entry}.pages.KNOWLEDGE.description"), GenerateInitialOptions());
	}

	private async Task Treasure()
	{
		Player owner = base.Owner!;
		await LoseHp(owner, _choiceTwoVar.IntValue);
		await PlayerCmd.GainGold(100, owner);

		_choiceTwoVar.BaseValue += 1m;
		SetEventState(new LocString(LocTable, $"{Id.Entry}.pages.TREASURE.description"), GenerateInitialOptions());
	}

	private async Task Health()
	{
		Player owner = base.Owner!;
		await LoseHp(owner, _choiceThreeVar.IntValue);
		await CreatureCmd.GainMaxHp(owner.Creature, 5);

		_choiceThreeVar.BaseValue += 1m;
		SetEventState(new LocString(LocTable, $"{Id.Entry}.pages.HEALTH.description"), GenerateInitialOptions());
	}

	private async Task Leave()
	{
		Player owner = base.Owner!;
		await LoseHp(owner, 6);
		SetEventFinished(new LocString(LocTable, $"{Id.Entry}.pages.LEAVE.description"));
	}

	private static Task LoseHp(Player owner, int amount)
	{
		return CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), owner.Creature, amount,ValueProp.Unpowered|ValueProp.Unblockable,null, null);
	}

	private async Task GiveColorlessReward(Player owner)
	{
		CardPoolModel colorlessPool = ModelDb.CardPool<ColorlessCardPool>();
		CardCreationOptions options = CardCreationOptions.ForNonCombatWithDefaultOdds(new List<CardPoolModel>(1) { colorlessPool });
		CardReward reward = new CardReward(options, 3, owner);
		await RewardsCmd.OfferCustom(owner, new List<Reward>(1) { reward });
	}
}
