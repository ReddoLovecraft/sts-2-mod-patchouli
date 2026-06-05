using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class SatelliteHimawariPower : CustomPowerModel
	{
		private const string SunDropScenePath = "res://TH_Patchouli/ArtWorks/VFX/satellite_himawari_sun_drop.tscn";

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/SHP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/SHP64.png";

		public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
		{
			if (side != Owner.Side || Owner.Player == null)
			{
				return;
			}

			Flash();
			TryPlaySunDropVfx();
		}
		 public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != base.Owner.Player)
        {
            return amount;
        }
        return amount + (decimal)base.Amount;
    }
	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner.Player)
		{
			return count;
		}
		return count + (decimal)base.Amount;
	}

		private void TryPlaySunDropVfx()
		{
			if (CombatState == null)
			{
				return;
			}

			var spawn = PatchouliVfxManager.GetCreatureHitboxCenterPosition(Owner);
			var hitboxSize = PatchouliVfxManager.GetCreatureHitboxSize(Owner);
			Node? container = PatchouliVfxManager.GetCombatVfxContainer(inFront: true);
			int count = Math.Max(0, (int)Amount);
			if (!spawn.HasValue || container == null || count <= 0)
			{
				return;
			}

			float jitterX = 0f;
			if (hitboxSize.HasValue)
			{
				jitterX = Mathf.Min(26f, hitboxSize.Value.X * 0.18f);
			}

			var rng = new RandomNumberGenerator();
			for (int i = 0; i < count; i++)
			{
				float x = spawn.Value.X + (jitterX <= 0f ? 0f : rng.RandfRange(-jitterX, jitterX));
				float y = spawn.Value.Y + rng.RandfRange(-8f, 8f);
				Vector2 p = new Vector2(x, y);

				PatchouliVfxManager.SpawnScene(SunDropScenePath, container, node =>
				{
					node.GlobalPosition = p;
				});
			}
		}
	}
}
