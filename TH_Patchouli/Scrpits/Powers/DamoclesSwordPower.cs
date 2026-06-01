using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class DamoclesSwordPower : CustomPowerModel
	{
		private const string DamoclesSwordProjectileScenePath = "res://TH_Patchouli/ArtWorks/VFX/damocles_sword_projectile.tscn";

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/DSP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/DSP64.png";

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

			List<Creature> enemies = [.. CombatState.HittableEnemies];
			if (enemies.Count <= 0)
			{
				return;
			}

			Flash();
			await CreatureCmd.TriggerAnim(base.Owner, "Cast", base.Owner.Player.Character.CastAnimDelay);
			await PlayProjectileVfx(enemies);
			await CreatureCmd.Damage(choiceContext, enemies, Amount, ValueProp.Unpowered | ValueProp.Move, dealer: Owner, cardSource: null);
		}

		private async Task PlayProjectileVfx(List<Creature> enemies)
		{
			Vector2? source = PatchouliVfxManager.GetCreatureCenterPosition(Owner);
			Node? container = PatchouliVfxManager.GetCombatVfxContainer();
			if (!source.HasValue || container == null || enemies.Count <= 0)
			{
				return;
			}

			float totalDuration = 0.52f;
			float mid = (enemies.Count - 1) * 0.5f;

			for (int i = 0; i < enemies.Count; i++)
			{
				Vector2? target = PatchouliVfxManager.GetCreatureCenterPosition(enemies[i]);
				if (!target.HasValue)
				{
					continue;
				}

				float offsetX = (i - mid) * 56f;
				float preferredX = source.Value.X + offsetX;
				Vector2 spawn = PatchouliVfxManager.GetTopSideSpawnPosition(Owner.Side, preferredX) ?? (source.Value + new Vector2(offsetX, -112f - (Mathf.Abs(i - mid) * 18f)));

				PatchouliVfxManager.SpawnScene(DamoclesSwordProjectileScenePath, container, node =>
				{
					if (node is NDamoclesSwordProjectileVfx vfx)
					{
						vfx.SpawnGlobalPosition = spawn;
						vfx.TargetGlobalPosition = target.Value;
						vfx.BodyLocalOffset = new Vector2(28f, -12f);
						totalDuration = Mathf.Max(totalDuration, vfx.TotalDurationSeconds);
					}
				});
			}

			await PatchouliVfxManager.WaitSeconds(totalDuration);
		}
	}
}
