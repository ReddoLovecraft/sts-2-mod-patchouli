using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class PhotosynthesisPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this),HoverTipFactory.FromPower<RegenPower>()];
		public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
		{
			if (side != Owner.Side || Owner.Player?.PlayerCombatState == null)
			{
				return;
			}

			int maxEnergy = Owner.Player.PlayerCombatState.MaxEnergy;
			if (maxEnergy <= 0)
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<RegenPower>(Owner, maxEnergy, Owner, null);
		}

		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner || result.UnblockedDamage <= 0)
			{
				return;
			}

			var regens = Owner.GetPowerInstances<RegenPower>().ToList();
			if (regens.Count == 0)
			{
				return;
			}

			Flash();
			foreach (var regen in regens)
			{
				await PowerCmd.Remove(regen);
			}
		}
	}
}
