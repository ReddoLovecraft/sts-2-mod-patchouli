using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class WitchTeaEndOfCombatPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override async Task AfterCombatEnd(CombatRoom room)
		{
			if (Owner.Player == null)
			{
				return;
			}

			Flash();
			await PlayerCmd.GainMaxPotionCount(Amount, Owner.Player);
		}
	}
}

