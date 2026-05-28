using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class EvadeWorldGirlPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
		{
			if (CombatState == null)
			{
				return;
			}

			Flash();
			await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
		}
	}
}

