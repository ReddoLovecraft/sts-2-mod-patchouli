using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class EmeraldMegalithPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
		{
			if (CombatState == null || creature != Owner || amount <= 0m)
			{
				return;
			}

			var enemies = CombatState.HittableEnemies;
			if (enemies.Count <= 0)
			{
				return;
			}

			Flash();
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), enemies, Amount, ValueProp.Unpowered | ValueProp.Move, dealer: Owner, cardSource: null);
		}
	}
}
