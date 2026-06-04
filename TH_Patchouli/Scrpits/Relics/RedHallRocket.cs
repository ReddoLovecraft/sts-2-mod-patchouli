using BaseLib.Abstracts;
using BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;
using MegaCrit.Sts2.Core.Map;


namespace TH_Patchouli.Relics
{
[Pool(typeof(PatchouliRelicPool))]
public class RedHallRocket : CustomRelicModel,IRightCilckable
{
	private const int TravelCooldownRooms = 5;

	private bool _isEffectEnabled = true;
	private int _travelCooldown;
	private int _lastMapCol = -1;
	private int _lastMapRow = -1;
    public override bool IsAllowed(IRunState runState)
	{
		return runState.Players.Count == 1;
	}

	public override string PackedIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Patchouli/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    public override RelicRarity Rarity => RelicRarity.Rare;
	public override bool ShowCounter => TravelCooldown > 0;
	public override int DisplayAmount => TravelCooldown;

	[SavedProperty]
	public bool IsEffectEnabled
	{
		get => _isEffectEnabled;
		set
		{
			AssertMutable();
			_isEffectEnabled = value;
			UpdateRelicStatus();
			UpdateMapDebugTravel();
		}
	}

	[SavedProperty]
	public int TravelCooldown
	{
		get => _travelCooldown;
		set
		{
			AssertMutable();
			int clamped = Math.Max(0, value);
			if (_travelCooldown == clamped)
			{
				return;
			}
			_travelCooldown = clamped;
			InvokeDisplayAmountChanged();
			UpdateRelicStatus();
			UpdateMapDebugTravel();
		}
	}

	[SavedProperty]
	public int LastMapCol
	{
		get => _lastMapCol;
		set
		{
			AssertMutable();
			_lastMapCol = value;
		}
	}

	[SavedProperty]
	public int LastMapRow
	{
		get => _lastMapRow;
		set
		{
			AssertMutable();
			_lastMapRow = value;
		}
	}

	public override Task AfterObtained()
	{
		UpdateRelicStatus();
		UpdateMapDebugTravel();
		return Task.CompletedTask;
	}

	public override Task AfterRemoved()
	{
		NMapScreen? mapScreen = NMapScreen.Instance;
		if (mapScreen != null && mapScreen.IsDebugTravelEnabled)
		{
			mapScreen.SetDebugTravelEnabled(enabled: false);
		}
		return Task.CompletedTask;
	}

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (!IsEffectEnabled)
		{
			return;
		}
		if (side == base.Owner.Creature.Side)
		{
		    Flash();
		    await ToolBox.GainElementRandomly(1,Owner.Creature);
		}
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		IRunState? runState = Owner?.RunState;
		if (runState == null)
		{
			return Task.CompletedTask;
		}

		MapCoord? toCoordOpt = runState.CurrentMapCoord;
		if (!toCoordOpt.HasValue)
		{
			UpdateMapDebugTravel();
			return Task.CompletedTask;
		}

		MapCoord toCoord = toCoordOpt.Value;
		if (LastMapCol >= 0 && LastMapRow >= 0)
		{
			MapCoord fromCoord = new MapCoord(LastMapCol, LastMapRow);
			if (IsEffectEnabled && TravelCooldown <= 0 && DidUseNonStandardTravel(runState, fromCoord, toCoord))
			{
				TravelCooldown = TravelCooldownRooms;
			}
			else if (TravelCooldown > 0)
			{
				TravelCooldown -= 1;
			}
		}
		else if (TravelCooldown > 0)
		{
			TravelCooldown -= 1;
		}

		LastMapCol = toCoord.col;
		LastMapRow = toCoord.row;

		UpdateMapDebugTravel();
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

	private void UpdateRelicStatus()
	{
		if (!IsEffectEnabled || TravelCooldown > 0)
		{
			Status = RelicStatus.Disabled;
			return;
		}
		Status = RelicStatus.Normal;
	}

	private void UpdateMapDebugTravel()
	{
		bool isMultiplayer = RunManager.Instance?.NetService?.Type.IsMultiplayer() ?? false;
		if (isMultiplayer && Owner != null && !LocalContext.IsMe(Owner))
		{
			return;
		}

		NMapScreen? mapScreen = NMapScreen.Instance;
		if (mapScreen == null)
		{
			return;
		}

		bool shouldEnable = IsEffectEnabled && TravelCooldown <= 0;
		if (mapScreen.IsDebugTravelEnabled != shouldEnable)
		{
			mapScreen.SetDebugTravelEnabled(shouldEnable);
		}
	}

	private static bool DidUseNonStandardTravel(IRunState runState, MapCoord from, MapCoord to)
	{
		MapPoint? fromPoint = runState.Map.GetPoint(from);
		if (fromPoint == null)
		{
			return false;
		}

		if (fromPoint.Children.Any(c => c.coord == to))
		{
			return false;
		}

		return true;
	}
}
}
