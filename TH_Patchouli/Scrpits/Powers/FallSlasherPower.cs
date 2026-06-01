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
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class FallSlasherPower : CustomPowerModel
	{
		private const string FallSlasherProjectileScenePath = "res://TH_Patchouli/ArtWorks/VFX/fall_slasher_projectile.tscn";

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
			if (CombatState == null || Owner.Player == null)
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
			int idx = rng.NextInt(enemies.Count);
			Creature target = enemies[idx];

			this.Flash();
			await CreatureCmd.TriggerAnim(base.Owner, "Cast", base.Owner.Player.Character.CastAnimDelay);
			await PlayProjectileVfx(target);
			await CreatureCmd.Damage(context, target, Amount, ValueProp.Unpowered | ValueProp.Move, dealer: Owner, cardSource: null);
		}

		public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side == Owner.Side)
			{
				await PowerCmd.Remove(this);
			}
		}

		private async Task PlayProjectileVfx(Creature target)
		{
			Vector2? sourcePosition = PatchouliVfxManager.GetCreatureCenterPosition(Owner);
			Vector2? targetPosition = PatchouliVfxManager.GetCreatureCenterPosition(target);
			Node? container = PatchouliVfxManager.GetCombatVfxContainer();
			if (!sourcePosition.HasValue || !targetPosition.HasValue || container == null)
			{
				return;
			}

			Vector2 spawn = PatchouliVfxManager.GetTopSideSpawnPosition(Owner.Side, sourcePosition.Value.X) ?? (targetPosition.Value + new Vector2(Owner.Side == CombatSide.Player ? -140f : 140f, -260f));
			float totalDuration = 0.34f;

			PatchouliVfxManager.SpawnScene(FallSlasherProjectileScenePath, container, node =>
			{
				if (node is NFallSlasherProjectileVfx vfx)
				{
					vfx.SpawnGlobalPosition = spawn;
					vfx.TargetGlobalPosition = targetPosition.Value;
					totalDuration = vfx.TotalDurationSeconds;
				}
			});

			await PatchouliVfxManager.WaitSeconds(totalDuration);
		}
	}
}
