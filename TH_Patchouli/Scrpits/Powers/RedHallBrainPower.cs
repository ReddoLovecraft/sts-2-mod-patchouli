using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class RedHallBrainPower : CustomPowerModel, BaseLib.Hooks.IMaxHandSizeModifier
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
		{
			if (!IsMutable)
			{
				return currentMaxHandSize;
			}
			if (player != Owner.Player)
			{
				return currentMaxHandSize;
			}
			return currentMaxHandSize + Amount;
		}
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/RHBP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/RHBP64.png";



		public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}
			Flash();
			await CardPileCmd.Draw(choiceContext, Amount, player);
		}
	}
}
