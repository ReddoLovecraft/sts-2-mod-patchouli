using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class SatelliteHimawariPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

		public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
		{
			if (side != Owner.Side || Owner.Player == null)
			{
				return;
			}

			Flash();
			await PlayerCmd.GainEnergy(Amount, Owner.Player);
			await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Amount, Owner.Player);
		}
	}
}
