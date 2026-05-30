using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class DamoclesSwordPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/DSP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/DSP64.png";



		public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			return Trigger(choiceContext, card.Owner);
		}

		public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			return Trigger(context, cardPlay.Card.Owner);
		}

		private async Task Trigger(PlayerChoiceContext choiceContext, Player? owner)
		{
			if (CombatState == null || Owner.Player == null || owner != Owner.Player)
			{
				return;
			}

			var enemies = CombatState.HittableEnemies;
			if (enemies.Count <= 0)
			{
				return;
			}

			Rng rng = Owner.Player.RunState.Rng.CombatTargets;
			int idx = rng.NextInt(enemies.Count);
			Flash();
			await CreatureCmd.Damage(choiceContext, enemies[idx], Amount, ValueProp.Unpowered | ValueProp.Move, dealer: Owner, cardSource: null);
		}
	}
}
