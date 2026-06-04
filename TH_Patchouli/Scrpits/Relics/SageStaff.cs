using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;


namespace TH_Patchouli.Relics
{
[Pool(typeof(PatchouliRelicPool))]
public class SageStaff : CustomRelicModel, IRightCilckable
{
	private bool _isEffectEnabled = true;
	private Dictionary<Creature, decimal> _pendingHpLossByTarget = new Dictionary<Creature, decimal>();

	public override string PackedIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Patchouli/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    public override RelicRarity Rarity => RelicRarity.Event;

	[SavedProperty]
	public bool IsEffectEnabled
	{
		get => _isEffectEnabled;
		set
		{
			AssertMutable();
			_isEffectEnabled = value;
			UpdateRelicStatus();
		}
	}

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_pendingHpLossByTarget = new Dictionary<Creature, decimal>();
	}

	public override Task AfterObtained()
	{
		UpdateRelicStatus();
		return Task.CompletedTask;
	}

	public override Task AfterRemoved()
	{
		_pendingHpLossByTarget.Clear();
		return Task.CompletedTask;
	}

	public Task OnRightClick(PlayerChoiceContext context)
	{
		bool isMultiplayer = RunManager.Instance?.NetService?.Type.IsMultiplayer() ?? false;
		if (isMultiplayer && Owner != null && !LocalContext.IsMe(Owner))
		{
			return Task.CompletedTask;
		}

		IsEffectEnabled = !IsEffectEnabled;
		if (IsEffectEnabled)
		{
			Flash();
		}
		return Task.CompletedTask;
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!IsEffectEnabled)
		{
			return 1m;
		}

		if (target == null || amount <= 0m || !IsDealerFromOwner(dealer) || target.Side == Owner.Creature.Side)
		{
			return 1m;
		}

		_pendingHpLossByTarget[target] = amount;
		return 0m;
	}

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (!IsEffectEnabled || !IsDealerFromOwner(dealer) || target.Side == Owner.Creature.Side)
		{
			return;
		}

		if (!_pendingHpLossByTarget.TryGetValue(target, out decimal hpLoss) || hpLoss <= 0m)
		{
			return;
		}

		_pendingHpLossByTarget.Remove(target);

		int hpLossInt = (int)Math.Min(hpLoss, 999999999m);
		if (hpLossInt <= 0)
		{
			return;
		}

		await LoseMaxHpAllowZero(choiceContext, target, hpLossInt, isFromCard: cardSource != null);
		await PlayerCmd.GainGold(hpLossInt, Owner);

		Status = RelicStatus.Active;
		Flash();
		Status = IsEffectEnabled ? RelicStatus.Normal : RelicStatus.Disabled;
	}

	private void UpdateRelicStatus()
	{
		Status = IsEffectEnabled ? RelicStatus.Normal : RelicStatus.Disabled;
	}

	private bool IsDealerFromOwner(Creature? dealer)
	{
		if (dealer == null || Owner == null)
		{
			return false;
		}

		if (dealer.Player == Owner)
		{
			return true;
		}

		return dealer.PetOwner == Owner;
	}

	private static async Task LoseMaxHpAllowZero(PlayerChoiceContext choiceContext, Creature creature, int amount, bool isFromCard)
	{
		if (amount <= 0)
		{
			return;
		}

		decimal newMaxHp = creature.MaxHp - amount;
		if (newMaxHp < creature.CurrentHp)
		{
			ValueProp props = isFromCard ? (ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move) : (ValueProp.Unblockable | ValueProp.Unpowered);
			await CreatureCmd.Damage(choiceContext, creature, creature.CurrentHp - newMaxHp, props, null, null);
		}

		await CreatureCmd.SetMaxHp(creature, Math.Max(0m, newMaxHp));
	}

}
}
