using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers 
{
	public sealed class DoubleMagicPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/DMP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/DMP64.png";

		public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
		{
			if (card.Owner?.Creature != Owner)
			{
				return playCount;
			}
			return playCount + Amount;
		}

		public override Task AfterModifyingCardPlayCount(CardModel card)
		{
			Flash();
			return Task.CompletedTask;
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (side == Owner.Side)
			{
				await PowerCmd.Remove(this);
			}
		}
	}
}

