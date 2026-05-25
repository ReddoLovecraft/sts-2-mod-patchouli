using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class EmeraldCityPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override bool IsInstanced => true;

		public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
		{
			foreach (EmeraldCityPower existing in target.Powers.OfType<EmeraldCityPower>().ToList())
			{
				await PowerCmd.Remove(existing);
			}
		}

		public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (CombatState == null || target != Owner || amount <= 0 || dealer == null || dealer.Side == Owner.Side)
			{
				return;
			}

			Flash();
			await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, amount, ValueProp.Unpowered | ValueProp.Move, dealer: Owner, cardSource: null);
		}

		public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side == Owner.Side)
			{
				await PowerCmd.Remove(this);
			}
		}
	}
}
