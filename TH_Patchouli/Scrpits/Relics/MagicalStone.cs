using BaseLib.Abstracts;
using BaseLib.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;


namespace TH_Patchouli.Relics
{
[Pool(typeof(PatchouliRelicPool))]
public class MagicalStone : CustomRelicModel
{
	private bool _isActivating;
	private int _elementsGained;

	public override string PackedIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Patchouli/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    public override RelicRarity Rarity => RelicRarity.Uncommon;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element"),HoverTipFactory.ForEnergy(this)];

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			int threshold = DynamicVars.Cards.IntValue;
			if (threshold <= 0)
			{
				return 0;
			}
			if (!IsActivating)
			{
				return ElementsGained % threshold;
			}
			return threshold;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CardsVar(10),
		new EnergyVar(1)
	];

	private bool IsActivating
	{
		get => _isActivating;
		set
		{
			AssertMutable();
			_isActivating = value;
			UpdateDisplay();
		}
	}

	[SavedProperty]
	public int ElementsGained
	{
		get => _elementsGained;
		set
		{
			AssertMutable();
			_elementsGained = value;
			UpdateDisplay();
		}
	}

	private void UpdateDisplay()
	{
		if (IsActivating)
		{
			Status = RelicStatus.Normal;
		}
		else
		{
			int threshold = DynamicVars.Cards.IntValue;
			Status = (threshold > 0 && ElementsGained % threshold == threshold - 1) ? RelicStatus.Active : RelicStatus.Normal;
		}

		InvokeDisplayAmountChanged();
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (amount <= 0m || Owner == null || power.Owner != Owner.Creature)
		{
			return;
		}

		if (!IsElementPower(power))
		{
			return;
		}

		ElementsGained += (int)amount;
		int threshold = DynamicVars.Cards.IntValue;
		if (CombatManager.Instance.IsInProgress && threshold > 0 && ElementsGained % threshold == 0)
		{
			TaskHelper.RunSafely(DoActivateVisuals());
			await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
		}
	}

	private static bool IsElementPower(PowerModel power)
	{
		return power is GoldElement or WoodElement or WaterElement or FireElement or DirtElement or SunElement or LunarElement;
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}

}
}

