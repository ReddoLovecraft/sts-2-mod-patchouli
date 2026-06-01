using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class EmeraldMegalithPower : CustomPowerModel
	{
		private static readonly Color EmeraldBoulderModulate = new Color(0.35f, 1.00f, 0.70f, 0.72f);

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/EMP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/EMP64.png";

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

			var choiceContext = new ThrowingPlayerChoiceContext();
			List<Task> damageTasks = new List<Task>();

			NRollingBoulderVfx vfx = NRollingBoulderVfx.Create(enemies, Amount);
			vfx.Modulate = EmeraldBoulderModulate;
			vfx.Connect(NRollingBoulderVfx.SignalName.HitCreature, Callable.From(delegate(NCreature c)
			{
				damageTasks.Add(DoDamage(choiceContext, c.Entity));
			}));

			Callable.From(delegate
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
				if (!vfx.IsInsideTree())
				{
					throw new InvalidOperationException("VFX is not inside tree after adding it to combat room!");
				}
			}).CallDeferred();

			await vfx.ToSignal(vfx, Node.SignalName.TreeExiting);
			await Task.WhenAll(damageTasks);
		}

		private Task DoDamage(PlayerChoiceContext choiceContext, Creature target)
		{
			return CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unpowered | ValueProp.Move, dealer: Owner, cardSource: null);
		}
	}
}
