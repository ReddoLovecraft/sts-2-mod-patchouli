using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class SunRisePower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/DP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/DP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>()];

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (CombatState == null || player != Owner.Player)
			{
				return;
			}

			int times = Amount;
			if (times <= 0)
			{
				return;
			}

			for (int i = 0; i < times; i++)
			{
				bool triggered = false;
				foreach (Creature enemy in CombatState.HittableEnemies)
				{
					IgnitePower? ignite = enemy.GetPower<IgnitePower>();
					if (ignite == null)
					{
						continue;
					}
					triggered = true;
					await ignite.AfterSideTurnStart(enemy.Side, CombatState);
				}

				if (triggered)
				{
					Flash();
				}
			}
		}
	}
}
