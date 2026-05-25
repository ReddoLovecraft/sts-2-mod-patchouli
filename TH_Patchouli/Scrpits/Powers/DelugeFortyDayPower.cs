using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class DelugeFortyDayPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/WE32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/WE64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FreezePower>()];

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player || CombatState == null)
			{
				return;
			}
			await PowerCmd.Apply<FreezePower>(CombatState.HittableEnemies, Amount, Owner, null);
		}
	}
}
