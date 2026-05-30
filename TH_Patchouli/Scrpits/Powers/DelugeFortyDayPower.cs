using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class DelugeFortyDayPower : CustomPowerModel
	{
		private const string DelugeWaterColumnScenePath = "res://TH_Patchouli/ArtWorks/VFX/deluge_water_column.tscn";

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/DFDP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/DFDP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FreezePower>()];

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player || CombatState == null)
			{
				return;
			}

			foreach (Creature enemy in CombatState.HittableEnemies)
			{
				TryPlayVfx(enemy);
				Flash();
				await PowerCmd.Apply<FreezePower>(enemy, Amount, Owner, null);
			}
		}

		private void TryPlayVfx(Creature enemy)
		{
			PatchouliVfxManager.PlayOnCreatureBase(enemy, DelugeWaterColumnScenePath, configure: (node, targetPos) =>
			{
				if (node is NDelugeWaterColumnVfx vfx)
				{
					vfx.GroundGlobalPosition = targetPos;
					return;
				}
				node.GlobalPosition = targetPos;
				PatchouliVfxManager.TrySetPropertyIfExists(node, "GroundGlobalPosition", targetPos);
			});
		}
	}
}
