using BaseLib.Abstracts;
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
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class FallSlasherPower : CustomPowerModel
	{
		private bool _skipNext;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/FSP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/FSP64.png";



		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			_skipNext = true;
			return Task.CompletedTask;
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			if (_skipNext)
			{
				_skipNext = false;
				return;
			}

			if (cardPlay.Card.Owner != Owner.Player)
			{
				return;
			}

			var enemies = CombatState.HittableEnemies;
			if (enemies.Count <= 0)
			{
				return;
			}

			Rng rng = Owner.Player.RunState.Rng.CombatTargets;
			this.Flash();
			int idx = rng.NextInt(enemies.Count);
			await CreatureCmd.Damage(context, enemies[idx], Amount, ValueProp.Unpowered | ValueProp.Move, dealer: Owner, cardSource: null);
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
